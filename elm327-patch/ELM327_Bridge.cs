using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ELM327Core
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafePassThruMsg
    {
        public uint ProtocolID;
        public uint RxStatus;
        public uint TxFlags;
        public uint Timestamp;
        public uint DataSize;
        public uint ExtraDataIndex;
        public fixed byte Data[4128];
    }

    public class ReceivedMessage
    {
        public uint ProtocolID;
        public uint CanID;
        public byte[] Payload;
        public uint Timestamp;

        public ReceivedMessage(uint protocolId, uint canId, byte[] payload)
        {
            ProtocolID = protocolId;
            CanID = canId;
            Payload = payload ?? new byte[0];
            Timestamp = (uint)Environment.TickCount;
        }
    }

    // =========================================================================
    // Connection Interface & Implementations (Serial and WiFi TCP)
    // =========================================================================

    public interface IElmConnection : IDisposable
    {
        bool IsOpen { get; }
        string ConnectionType { get; }      // "WIFI" or "SERIAL"
        string Endpoint { get; }            // e.g. "192.168.0.10:35000" or "COM3 @ 38400 baud"
        void DiscardInBuffer();
        void DiscardOutBuffer();
        void Write(string text);
        string ReadExisting();
        int BytesToRead { get; }
        void Close();
    }

    public class SerialElmConnection : IElmConnection
    {
        private SerialPort _port;
        private readonly string _portName;
        private readonly int _baud;

        public SerialElmConnection(string portName, int baud)
        {
            _portName = portName;
            _baud = baud;
            _port = new SerialPort(portName, baud, Parity.None, 8, StopBits.One);
            _port.ReadTimeout = 400;
            // Bluetooth SPP can stall well past 400 ms under load. Reads are
            // polled via BytesToRead so ReadTimeout barely matters, but a short
            // WriteTimeout aborts perfectly good writes.
            _port.WriteTimeout = 2000;
            _port.DtrEnable = true;
            _port.RtsEnable = true;
            _port.Open();
        }

        public bool IsOpen
        {
            get { return _port != null && _port.IsOpen; }
        }

        public string ConnectionType { get { return "SERIAL"; } }
        public string Endpoint { get { return string.Format("{0} @ {1} baud", _portName, _baud); } }

        public void DiscardInBuffer()
        {
            try { if (_port != null && _port.IsOpen) _port.DiscardInBuffer(); } catch { }
        }

        public void DiscardOutBuffer()
        {
            try { if (_port != null && _port.IsOpen) _port.DiscardOutBuffer(); } catch { }
        }

        public void Write(string text)
        {
            if (_port != null && _port.IsOpen)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                _port.Write(bytes, 0, bytes.Length);
            }
        }

        public string ReadExisting()
        {
            if (_port != null && _port.IsOpen)
            {
                return _port.ReadExisting();
            }
            return "";
        }

        public int BytesToRead
        {
            get
            {
                try { return (_port != null && _port.IsOpen) ? _port.BytesToRead : 0; } catch { return 0; }
            }
        }

        public void Close()
        {
            try
            {
                if (_port != null)
                {
                    if (_port.IsOpen) _port.Close();
                    _port.Dispose();
                }
            }
            catch { }
            finally { _port = null; }
        }

        public void Dispose() { Close(); }
    }

    public class TcpElmConnection : IElmConnection
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _ip;
        private readonly int _port;

        public TcpElmConnection(string ip, int port, int timeoutMs = 1200)
        {
            _ip = ip;
            _port = port;
            _client = new TcpClient();
            _client.NoDelay = true;
            _client.ReceiveTimeout = 400;
            _client.SendTimeout = 400;

            IAsyncResult ar = _client.BeginConnect(ip, port, null, null);
            bool success = ar.AsyncWaitHandle.WaitOne(timeoutMs);
            if (!success || !_client.Connected)
            {
                try { _client.Close(); } catch { }
                throw new Exception("Timeout connecting to WiFi ELM327 at " + ip + ":" + port);
            }
            _client.EndConnect(ar);
            _stream = _client.GetStream();
        }

        public bool IsOpen
        {
            get { return _client != null && _client.Connected && _stream != null; }
        }

        public string ConnectionType { get { return "WIFI"; } }
        public string Endpoint { get { return string.Format("{0}:{1}", _ip, _port); } }

        public void DiscardInBuffer()
        {
            try
            {
                if (_stream != null && _client.Available > 0)
                {
                    byte[] discard = new byte[_client.Available];
                    _stream.Read(discard, 0, discard.Length);
                }
            }
            catch { }
        }

        public void DiscardOutBuffer()
        {
            try { if (_stream != null) _stream.Flush(); } catch { }
        }

        public void Write(string text)
        {
            if (_stream != null)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                _stream.Write(bytes, 0, bytes.Length);
                _stream.Flush();
            }
        }

        public string ReadExisting()
        {
            if (_stream == null || _client == null || _client.Available == 0)
                return "";

            try
            {
                int avail = _client.Available;
                byte[] buf = new byte[avail];
                int read = _stream.Read(buf, 0, avail);
                if (read > 0)
                {
                    return Encoding.ASCII.GetString(buf, 0, read);
                }
            }
            catch { }
            return "";
        }

        public int BytesToRead
        {
            get
            {
                try { return (_client != null && _client.Connected) ? _client.Available : 0; }
                catch { return 0; }
            }
        }

        public void Close()
        {
            try { if (_stream != null) _stream.Close(); } catch { }
            try { if (_client != null) _client.Close(); } catch { }
            _stream = null;
            _client = null;
        }

        public void Dispose() { Close(); }
    }

    // =========================================================================
    // Core PassThru Bridge Implementation
    // =========================================================================

    public static class Bridge
    {
        private static readonly object s_lock = new object();
        private static IElmConnection s_connection = null;
        private static bool s_isOpen = false;
        private static int s_deviceId = 1;
        private static int s_channelId = 1;
        private static int s_filterId = 1;

        public static bool IsOpen { get { return s_isOpen; } }

        // Configuration
        private static string s_configuredType = "AUTO"; // AUTO, WIFI, SERIAL
        private static string s_configuredPort = "AUTO";
        private static int s_configuredBaud = 38400;
        private static string s_wifiIp = "192.168.0.10";
        private static int s_wifiPort = 35000;
        private static int s_wifiTimeoutMs = 1200;
        private static int s_timeoutMs = 2000;
        private static bool s_loggingEnabled = true;
        private static bool s_telemetryEnabled = true;
        private static bool s_openFilter = false;

        private static string s_logFilePath = null;
        private static string s_telemetryFilePath = null;
        private static uint s_currentTxCanId = 0;
        private static uint s_currentRxCanId = 0;
        private static long s_droppedForeignFrames = 0;
        private static int s_logMaxMb = 8;
        private static long s_logWriteCounter = 0;
        // Set from elm327.ini ACCEPT_ALL_RESPONDERS - disables the responder
        // check for anyone who would rather see every frame than lose one.
        private static bool s_openFilterAcceptAll = false;
        private static bool s_isConnectedToHardware = false;
        private static string s_connectionType = "DISCONNECTED";
        private static string s_connectionEndpoint = "None";

        // Filter Mapping: Tx CAN ID -> Rx CAN ID
        private static readonly Dictionary<uint, uint> s_filterMap = new Dictionary<uint, uint>();

        // Protocol message queues
        private static readonly Queue<ReceivedMessage> s_rxQueue = new Queue<ReceivedMessage>();
        private static readonly List<byte> s_multiFramePayload = new List<byte>();
        private static uint s_multiFrameRxId = 0;
        private static int s_expectedMultiLength = -1;

        // Telemetry & Monitoring State
        private static int s_lastLatencyMs = 0;
        private static int s_lastTxTick = 0;
        private static long s_totalTxPackets = 0;
        private static long s_totalRxPackets = 0;
        private static string s_activeEcuName = "None";
        private static readonly List<string> s_recentPackets = new List<string>();
        private static int s_lastSaveTick = 0;

        // J2534 Error Codes
        public const int STATUS_NOERROR = 0;
        public const int ERR_NOT_SUPPORTED = 1;
        public const int ERR_INVALID_CHANNEL_ID = 2;
        public const int ERR_INVALID_PROTOCOL_ID = 3;
        public const int ERR_NULL_PARAMETER = 4;
        public const int ERR_FAILED = 7;
        public const int ERR_DEVICE_NOT_CONNECTED = 8;
        public const int ERR_TIMEOUT = 9;
        public const int ERR_BUFFER_EMPTY = 16;
        public const int ERR_BUFFER_FULL = 17;

        // Protocol IDs
        public const int PROTOCOL_CAN = 5;
        public const int PROTOCOL_ISO15765 = 6;

        static Bridge()
        {
            try
            {
                string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(baseDir))
                {
                    baseDir = AppDomain.CurrentDomain.BaseDirectory;
                }
                s_logFilePath = Path.Combine(baseDir, "elm327_log.txt");
                s_telemetryFilePath = Path.Combine(baseDir, "live_telematics.json");

                LoadConfiguration(baseDir);
                SaveTelemetry();

                Log("==================================================================");
                Log("ELM327 J2534 PassThru Bridge Initialized (Multi-ECU + WiFi/Serial).");
                Log("BaseDir: " + baseDir);
                Log(string.Format("Config: Type={0}, Port={1}, Baud={2}, WiFi={3}:{4}",
                    s_configuredType, s_configuredPort, s_configuredBaud, s_wifiIp, s_wifiPort));
            }
            catch (Exception ex)
            {
                Log("Bridge Static Init Error: " + ex);
            }
        }

        public static void Initialize()
        {
            // Trigger static constructor
        }

        private static void LoadConfiguration(string baseDir)
        {
            try
            {
                string iniPath = Path.Combine(baseDir, "elm327.ini");
                if (!File.Exists(iniPath))
                {
                    string parentIni = Path.Combine(Directory.GetParent(baseDir).FullName, "elm327.ini");
                    if (File.Exists(parentIni))
                    {
                        iniPath = parentIni;
                    }
                }

                if (File.Exists(iniPath))
                {
                    string[] lines = File.ReadAllLines(iniPath);
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                            continue;

                        int eq = trimmed.IndexOf('=');
                        if (eq > 0)
                        {
                            string key = trimmed.Substring(0, eq).Trim().ToUpperInvariant();
                            string val = trimmed.Substring(eq + 1).Trim();

                            if (key == "TYPE") s_configuredType = val.ToUpperInvariant();
                            else if (key == "PORT") s_configuredPort = val.ToUpperInvariant();
                            else if (key == "BAUD") int.TryParse(val, out s_configuredBaud);
                            else if (key == "WIFI_IP") s_wifiIp = val;
                            else if (key == "WIFI_PORT") int.TryParse(val, out s_wifiPort);
                            else if (key == "WIFI_TIMEOUT") int.TryParse(val, out s_wifiTimeoutMs);
                            else if (key == "TIMEOUT") int.TryParse(val, out s_timeoutMs);
                            else if (key == "LOG") s_loggingEnabled = (val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase));
                            else if (key == "TELEMETRY") s_telemetryEnabled = (val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase));
                            else if (key == "OPEN_FILTER") s_openFilter = (val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("LoadConfiguration Error: " + ex);
            }
        }

        public static void Log(string msg)
        {
            if (!s_loggingEnabled) return;
            try
            {
                string entry = string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] {1}\r\n", DateTime.Now, msg);
                if (string.IsNullOrEmpty(s_logFilePath)) return;

                // Roll the log once it passes the cap. One scan session produced
                // 8 MB / 110k lines, which is unusable for diagnosis. Checked
                // every 200 writes so this is not a stat() per line.
                if (s_logMaxMb > 0 && ++s_logWriteCounter % 200 == 0)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(s_logFilePath);
                        if (fi.Exists && fi.Length > (long)s_logMaxMb * 1024L * 1024L)
                        {
                            string prev = Path.Combine(
                                Path.GetDirectoryName(s_logFilePath),
                                Path.GetFileNameWithoutExtension(s_logFilePath) + "_prev.txt");
                            if (File.Exists(prev)) File.Delete(prev);
                            File.Move(s_logFilePath, prev);
                        }
                    }
                    catch { }
                }

                File.AppendAllText(s_logFilePath, entry);
            }
            catch { }
        }

        // =========================================================================
        // Hardware Connection Management
        // =========================================================================

        private static bool ConnectHardware()
        {
            lock (s_lock)
            {
                if (s_connection != null && s_connection.IsOpen && s_isConnectedToHardware)
                {
                    return true;
                }

                CloseConnection();

                string mode = s_configuredType.ToUpperInvariant();
                Log("Initiating ELM327 connection (Mode=" + mode + ")...");

                // 1. WiFi connection attempt (WIFI or AUTO)
                if (mode == "WIFI" || mode == "AUTO")
                {
                    Log("Probing WiFi ELM327 at " + s_wifiIp + ":" + s_wifiPort + " (timeout=" + s_wifiTimeoutMs + "ms)...");
                    if (TryConnectWifi(s_wifiIp, s_wifiPort, s_wifiTimeoutMs))
                    {
                        if (InitElmDevice())
                        {
                            s_isConnectedToHardware = true;
                            s_connectionType = "WIFI";
                            s_connectionEndpoint = s_connection.Endpoint;
                            Log("Successfully connected to WiFi ELM327 adapter at " + s_connectionEndpoint);
                            SaveTelemetry();
                            return true;
                        }
                        CloseConnection();
                    }

                    if (s_wifiPort != 23)
                    {
                        Log("Probing alternate WiFi port 23 at " + s_wifiIp + ":23...");
                        if (TryConnectWifi(s_wifiIp, 23, 800))
                        {
                            if (InitElmDevice())
                            {
                                s_isConnectedToHardware = true;
                                s_connectionType = "WIFI";
                                s_connectionEndpoint = s_connection.Endpoint;
                                Log("Successfully connected to WiFi ELM327 adapter at " + s_connectionEndpoint);
                                SaveTelemetry();
                                return true;
                            }
                            CloseConnection();
                        }
                    }

                    if (mode == "WIFI")
                    {
                        Log("Error: Could not connect to WiFi ELM327 at " + s_wifiIp + ":" + s_wifiPort + ".");
                        s_connectionType = "DISCONNECTED";
                        s_connectionEndpoint = "None";
                        SaveTelemetry();
                        return false;
                    }
                }

                // 2. Serial connection attempt (SERIAL or AUTO fallback)
                if (mode == "SERIAL" || mode == "AUTO")
                {
                    if (s_configuredPort != "AUTO" && !string.IsNullOrEmpty(s_configuredPort))
                    {
                        if (TryConnectSerial(s_configuredPort, s_configuredBaud))
                        {
                            if (InitElmDevice())
                            {
                                s_isConnectedToHardware = true;
                                s_connectionType = "SERIAL";
                                s_connectionEndpoint = s_connection.Endpoint;
                                Log("Successfully connected to configured serial port " + s_connectionEndpoint);
                                SaveTelemetry();
                                return true;
                            }
                            CloseConnection();
                        }
                        Log("Failed to open configured port " + s_configuredPort + ". Checking available COM ports...");
                    }

                    // Scan available ports
                    string[] ports = SerialPort.GetPortNames();
                    Log("Scanning available COM ports: " + string.Join(", ", ports));

                    List<int> baudList = new List<int>();
                    if (s_configuredBaud > 0) baudList.Add(s_configuredBaud);
                    foreach (int b in new int[] { 38400, 115200, 500000, 230400 })
                    {
                        if (!baudList.Contains(b)) baudList.Add(b);
                    }

                    Array.Sort(ports);

                    foreach (string port in ports)
                    {
                        bool portFailedToOpen = false;
                        foreach (int baud in baudList)
                        {
                            if (portFailedToOpen) break;

                            Log("Probing " + port + " @ " + baud + "...");
                            if (TryConnectSerial(port, baud))
                            {
                                if (InitElmDevice())
                                {
                                    s_configuredPort = port;
                                    s_configuredBaud = baud;
                                    s_isConnectedToHardware = true;
                                    s_connectionType = "SERIAL";
                                    s_connectionEndpoint = s_connection.Endpoint;
                                    Log("Successfully connected to ELM327 on " + s_connectionEndpoint);
                                    SaveTelemetry();
                                    return true;
                                }
                                CloseConnection();
                            }
                            else
                            {
                                portFailedToOpen = true;
                            }
                        }
                    }
                }

                s_connectionType = "DISCONNECTED";
                s_connectionEndpoint = "None";
                SaveTelemetry();
                Log("Notice: No active ELM327 (WiFi or Serial) answered. Running in ready/virtual mode.");
                return false;
            }
        }

        private static bool TryConnectWifi(string ip, int port, int timeoutMs)
        {
            try
            {
                CloseConnection();
                s_connection = new TcpElmConnection(ip, port, timeoutMs);
                return s_connection.IsOpen;
            }
            catch (Exception ex)
            {
                Log("TryConnectWifi(" + ip + ":" + port + ") failed: " + ex.Message);
                CloseConnection();
                return false;
            }
        }

        private static bool TryConnectSerial(string portName, int baud)
        {
            try
            {
                CloseConnection();
                s_connection = new SerialElmConnection(portName, baud);
                return s_connection.IsOpen;
            }
            catch (Exception ex)
            {
                Log("TryConnectSerial(" + portName + ", " + baud + ") failed: " + ex.Message);
                CloseConnection();
                return false;
            }
        }

        private static void CloseConnection()
        {
            try
            {
                if (s_connection != null)
                {
                    s_connection.Close();
                    s_connection.Dispose();
                }
            }
            catch { }
            finally
            {
                s_connection = null;
                s_isConnectedToHardware = false;
                // The adapter is reset on the next connect (ATZ), which clears
                // its header. Forget ours too, or PassThruWriteMsgs will think
                // ATSH is still current and skip re-sending it.
                s_currentTxCanId = 0;
                s_currentRxCanId = 0;
                s_multiFramePayload.Clear();
                s_multiFrameRxId = 0;
                s_expectedMultiLength = -1;
            }
        }

        private static string SendCommand(string cmd, int timeoutMs = 800)
        {
            if (s_connection == null || !s_connection.IsOpen)
                return "";

            try
            {
                // Flushing mid-reassembly discards the consecutive frames we are
                // still waiting for.
                if (s_expectedMultiLength < 0)
                {
                    s_connection.DiscardInBuffer();
                }
                s_connection.DiscardOutBuffer();

                Log("TX -> " + cmd);
                s_connection.Write(cmd + "\r");

                StringBuilder sb = new StringBuilder();
                int start = Environment.TickCount;
                while ((Environment.TickCount - start) < timeoutMs)
                {
                    if (s_connection.BytesToRead > 0)
                    {
                        string chunk = s_connection.ReadExisting();
                        sb.Append(chunk);
                        if (chunk.Contains(">"))
                        {
                            break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(5);
                    }
                }

                string resp = sb.ToString().Replace("\r", " ").Replace("\n", " ").Trim();
                Log("RX <- " + resp);
                return resp;
            }
            catch (Exception ex)
            {
                Log("SendCommand Error: " + ex.Message);
                return "";
            }
        }

        private static bool InitElmDevice()
        {
            try
            {
                // Wake up and test AT Z
                s_connection.Write("\r\r\r");
                Thread.Sleep(50);
                s_connection.DiscardInBuffer();

                // ATZ is a full adapter reset and the slowest AT command there is;
                // 1000 ms can truncate the version banner before it arrives.
                string r = SendCommand("AT Z", 3000);
                if (string.IsNullOrEmpty(r) || (!r.Contains("ELM") && !r.Contains("OBD") && !r.Contains(">")))
                {
                    r = SendCommand("AT", 400);
                    if (!r.Contains("OK") && !r.Contains(">"))
                    {
                        return false;
                    }
                }

                SendCommand("ATE0");   // Echo Off
                SendCommand("ATL0");   // Linefeeds Off
                SendCommand("ATS0");   // Spaces Off
                SendCommand("ATH1");   // Headers On (shows CAN ID in response)
                SendCommand("ATV0");   // Variable DLC Off: ALWAYS send 8 bytes (padded with 00, required by Tata ECUs)
                SendCommand("ATSTFF"); // Maximum timeout (~1020ms) for slower ECUs (BMS, BCM, Airbag)
                SendCommand("ATAT2");  // Adaptive Timing 2 (fast when ECU answers, generous when slow)
                SendCommand("ATCAF1"); // CAN Auto Formatting on (automatic ISO-TP flow control & reassembly)
                // Flow Control Mode 0: the adapter generates flow-control frames
                // itself, deriving the header from the frame it is replying to.
                // Verified working - multi-frame reassembly succeeds in the logs.
                //
                // Note this makes the per-ECU ATFCSH in PassThruWriteMsgs inert:
                // ATFCSH is only consulted in mode 1. It is left in place because
                // it is harmless, and mode 1 would additionally require ATFCSD to
                // be set before the mode switch or the adapter rejects it.
                SendCommand("ATFCSM0");
                SendCommand("ATSP6");  // ISO 15765-4 CAN (11 bit ID, 500 kbaud) - Tata Passenger Car standard

                if (s_openFilter)
                {
                    SendCommand("ATCRA"); // Open filter: receive from all CAN IDs
                }

                return true;
            }
            catch (Exception ex)
            {
                Log("InitElmDevice Exception: " + ex.Message);
                return false;
            }
        }

        // =========================================================================
        // Telemetry Engine & Streamlined JSON State
        // =========================================================================

        private static void AddRecentPacket(string line)
        {
            lock (s_recentPackets)
            {
                s_recentPackets.Add(line);
                if (s_recentPackets.Count > 25)
                {
                    s_recentPackets.RemoveAt(0);
                }
            }
        }

        private static string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
        }

        private static void SaveTelemetry()
        {
            if (!s_telemetryEnabled || string.IsNullOrEmpty(s_telemetryFilePath)) return;

            int now = Environment.TickCount;
            if (Math.Abs(now - s_lastSaveTick) < 60) return; // Debounce ~16 Hz
            s_lastSaveTick = now;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{\n");
                sb.AppendFormat("  \"timestamp\": \"{0:yyyy-MM-dd HH:mm:ss.fff}\",\n", DateTime.Now);
                sb.AppendFormat("  \"connectionType\": \"{0}\",\n", EscapeJson(s_connectionType));
                sb.AppendFormat("  \"endpoint\": \"{0}\",\n", EscapeJson(s_connectionEndpoint));
                sb.AppendFormat("  \"isHardwareConnected\": {0},\n", s_isConnectedToHardware ? "true" : "false");
                sb.AppendFormat("  \"protocol\": \"ISO 15765-4 CAN (500k 11-bit)\",\n");
                sb.AppendFormat("  \"activeEcuId\": \"0x{0:X3}\",\n", s_currentTxCanId);
                sb.AppendFormat("  \"droppedForeignFrames\": {0},\n", s_droppedForeignFrames);
                sb.AppendFormat("  \"activeEcuName\": \"{0}\",\n", EscapeJson(s_activeEcuName));
                sb.AppendFormat("  \"latencyMs\": {0},\n", s_lastLatencyMs);
                sb.AppendFormat("  \"totalTxPackets\": {0},\n", s_totalTxPackets);
                sb.AppendFormat("  \"totalRxPackets\": {0},\n", s_totalRxPackets);
                sb.Append("  \"recentPackets\": [\n");

                lock (s_recentPackets)
                {
                    for (int i = 0; i < s_recentPackets.Count; i++)
                    {
                        sb.AppendFormat("    \"{0}\"{1}\n", EscapeJson(s_recentPackets[i]), i == s_recentPackets.Count - 1 ? "" : ",");
                    }
                }

                sb.Append("  ]\n");
                sb.Append("}\n");

                string tmp = s_telemetryFilePath + ".tmp";
                File.WriteAllText(tmp, sb.ToString());
                File.Copy(tmp, s_telemetryFilePath, true);
            }
            catch { }
        }

        private static string GetEcuName(uint canId)
        {
            switch (canId)
            {
                case 0x785: case 0x78D: return "BMS (Battery Mgmt)";
                case 0x783: case 0x78B: return "MCU (Motor Controller)";
                case 0x784: case 0x78C: return "DCDC (DC Converter)";
                case 0x786: case 0x78E: return "OBC (On-Board Charger)";
                case 0x701: case 0x709: return "BCM (Body Controller)";
                case 0x702: case 0x70A: return "IPC (Cluster / Airbag)";
                case 0x704: case 0x70C: return "IMMO (Immobilizer)";
                case 0x710: case 0x718: return "PEPS (Keyless Entry)";
                case 0x7EB: case 0x7F3: case 0x7E3: return "VECU (Vehicle ECU)";
                case 0x7E0: case 0x7E8: return "EMS (Engine Mgmt)";
                case 0x7DF: return "Broadcast / All ECUs";
                default: return string.Format("ECU 0x{0:X3}", canId);
            }
        }

        private static string GetUdsServiceDescription(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return "Empty";
            byte sid = payload[0];
            switch (sid)
            {
                case 0x10:
                    string sub = payload.Length > 1 ? string.Format("0x{0:X2}", payload[1]) : "";
                    return "DiagSession " + sub;
                case 0x11: return "ECUReset";
                case 0x14: return "ClearDTC";
                case 0x19: return "ReadDTCInfo";
                case 0x22:
                    if (payload.Length >= 3)
                    {
                        int did = (payload[1] << 8) | payload[2];
                        if (did == 0xF190) return "Read VIN (0xF190)";
                        if (did == 0xF191) return "Read Cal ID (0xF191)";
                        if (did == 0x3400) return "Read HV Voltage (0x3400)";
                        if (did == 0x3401) return "Read HV Current (0x3401)";
                        if (did == 0x3402) return "Read SOC (0x3402)";
                        if (did == 0x3403) return "Read SOH (0x3403)";
                        if (did == 0x3421) return "Read Speed (0x3421)";
                        if (did == 0x3441) return "Read Motor RPM (0x3441)";
                        return string.Format("Read DID 0x{0:X4}", did);
                    }
                    return "ReadDataById";
                case 0x27: return "SecurityAccess";
                case 0x2E: return "WriteDataById";
                case 0x2F: return "ActuatorTest (IOControl)";
                case 0x31: return "RoutineControl";
                case 0x3E: return "TesterPresent";
                case 0x01: return "OBD2 Mode 01 Live Data";
                case 0x03: return "OBD2 Mode 03 Read DTCs";
                case 0x04: return "OBD2 Mode 04 Clear DTCs";
                default: return string.Format("SID 0x{0:X2}", sid);
            }
        }

        private static string GetUdsResponseDescription(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return "Empty";
            byte sid = payload[0];
            switch (sid)
            {
                case 0x50: return "DiagSession OK";
                case 0x54: return "ClearDTC OK";
                case 0x59: return "ReadDTC Response";
                case 0x62:
                    if (payload.Length >= 3)
                    {
                        int did = (payload[1] << 8) | payload[2];
                        if (did == 0xF190) return "VIN Data";
                        if (did == 0xF191) return "Cal ID Data";
                        if (did == 0x3400) return "HV Voltage Data";
                        if (did == 0x3401) return "HV Current Data";
                        if (did == 0x3402) return "SOC Data";
                        if (did == 0x3403) return "SOH Data";
                        if (did == 0x3421) return "Speed Data";
                        if (did == 0x3441) return "Motor RPM Data";
                        return string.Format("DID 0x{0:X4} Data", did);
                    }
                    return "ReadData OK";
                case 0x67: return "SecurityAccess OK";
                case 0x6F: return "Actuator OK";
                case 0x71: return "RoutineControl OK";
                case 0x7E: return "TesterPresent OK";
                case 0x7F:
                    string reqSid = payload.Length > 1 ? string.Format("0x{0:X2}", payload[1]) : "??";
                    string nrc = payload.Length > 2 ? string.Format("0x{0:X2}", payload[2]) : "??";
                    return string.Format("NRC {0} (SID {1})", nrc, reqSid);
                default: return string.Format("Resp 0x{0:X2}", sid);
            }
        }

        // =========================================================================
        // J2534 API Implementation
        // =========================================================================

        public static unsafe int PassThruOpen(IntPtr pName, int* pDeviceID)
        {
            Log("PassThruOpen() called.");
            lock (s_lock)
            {
                if (pDeviceID == null) return ERR_NULL_PARAMETER;

                try
                {
                    ConnectHardware();
                }
                catch { }

                *pDeviceID = s_deviceId;
                s_isOpen = true;
                Log(string.Format("PassThruOpen() SUCCESS. DeviceID={0} (Type={1}, Endpoint={2}, HardwareConnected={3})",
                    s_deviceId, s_connectionType, s_connectionEndpoint, s_isConnectedToHardware));
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruClose(int deviceId)
        {
            Log("PassThruClose(DeviceID=" + deviceId + ") called.");
            lock (s_lock)
            {
                s_isOpen = false;
                CloseConnection();
                s_rxQueue.Clear();
                s_multiFramePayload.Clear();
                s_expectedMultiLength = -1;
                s_filterMap.Clear();
                s_connectionType = "DISCONNECTED";
                s_connectionEndpoint = "None";
                SaveTelemetry();
                Log("PassThruClose() SUCCESS.");
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruConnect(int deviceId, int protocolId, int flags, int baudRate, int* pChannelId)
        {
            Log(string.Format("PassThruConnect(Dev={0}, Proto={1}, Flags=0x{2:X}, Baud={3})", deviceId, protocolId, flags, baudRate));
            lock (s_lock)
            {
                if (pChannelId == null) return ERR_NULL_PARAMETER;

                if (!s_isConnectedToHardware)
                {
                    ConnectHardware();
                }

                if (s_isConnectedToHardware)
                {
                    SendCommand("ATSP6");   // ISO 15765-4 11-bit CAN @ 500kbps
                    SendCommand("ATCAF1");  // Auto formatting on
                    SendCommand("ATV0");    // Variable DLC Off (8-byte padding)
                    SendCommand("ATFCSM0"); // Flow Control Mode 0
                }

                *pChannelId = s_channelId;
                Log("PassThruConnect() SUCCESS. ChannelID=" + s_channelId);
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruDisconnect(int channelId)
        {
            Log("PassThruDisconnect(Channel=" + channelId + ") called.");
            lock (s_lock)
            {
                s_rxQueue.Clear();
                s_multiFramePayload.Clear();
                s_expectedMultiLength = -1;
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruStartMsgFilter(int channelId, int filterType, UnsafePassThruMsg* pMaskMsg, UnsafePassThruMsg* pPatternMsg, UnsafePassThruMsg* pFlowControlMsg, int* pFilterId)
        {
            Log(string.Format("PassThruStartMsgFilter(Channel={0}, FilterType={1})", channelId, filterType));
            lock (s_lock)
            {
                if (pFilterId == null) return ERR_NULL_PARAMETER;

                uint rxCanId = 0;
                uint txCanId = 0;

                if (pPatternMsg != null && pPatternMsg->DataSize >= 4)
                {
                    rxCanId = ((uint)pPatternMsg->Data[0] << 24) |
                              ((uint)pPatternMsg->Data[1] << 16) |
                              ((uint)pPatternMsg->Data[2] << 8) |
                              ((uint)pPatternMsg->Data[3]);
                }

                if (pFlowControlMsg != null && pFlowControlMsg->DataSize >= 4)
                {
                    txCanId = ((uint)pFlowControlMsg->Data[0] << 24) |
                              ((uint)pFlowControlMsg->Data[1] << 16) |
                              ((uint)pFlowControlMsg->Data[2] << 8) |
                              ((uint)pFlowControlMsg->Data[3]);
                }

                Log(string.Format("Filter Configured: Target Rx CAN ID=0x{0:X}, FlowControl Tx CAN ID=0x{1:X}", rxCanId, txCanId));

                // Save dynamic mapping for multi-ECU routing
                if (txCanId > 0 && rxCanId > 0)
                {
                    s_filterMap[txCanId] = rxCanId;
                }

                if (rxCanId > 0)
                {
                    s_currentRxCanId = rxCanId;
                    if (s_isConnectedToHardware && !s_openFilter)
                    {
                        SendCommand(string.Format("ATCRA {0:X3}", rxCanId));
                    }
                }

                if (txCanId > 0)
                {
                    s_currentTxCanId = txCanId;
                    s_activeEcuName = GetEcuName(txCanId);
                    if (s_isConnectedToHardware)
                    {
                        SendCommand(string.Format("ATSH {0:X3}", txCanId));
                        SendCommand(string.Format("ATFCSH {0:X3}", txCanId));
                    }
                }

                SaveTelemetry();
                *pFilterId = s_filterId++;
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruStartPassBlockMsgFilter(int channelId, int filterType, UnsafePassThruMsg* pMaskMsg, UnsafePassThruMsg* pPatternMsg, int nada, int* pFilterId)
        {
            return PassThruStartMsgFilter(channelId, filterType, pMaskMsg, pPatternMsg, null, pFilterId);
        }

        public static unsafe int PassThruStopMsgFilter(int channelId, int filterId)
        {
            Log("PassThruStopMsgFilter(Filter=" + filterId + ") called.");
            lock (s_lock)
            {
                if (s_isConnectedToHardware)
                {
                    SendCommand("ATCRA"); // Reset CAN address filter to open
                }
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruWriteMsgs(int channelId, UnsafePassThruMsg* pMsg, int* pNumMsgs, int timeout)
        {
            lock (s_lock)
            {
                if (pMsg == null || pNumMsgs == null) return ERR_NULL_PARAMETER;
                if (*pNumMsgs < 1) return STATUS_NOERROR;

                uint dataSize = pMsg->DataSize;
                if (dataSize < 5)
                {
                    Log("PassThruWriteMsgs: DataSize too small: " + dataSize);
                    return ERR_FAILED;
                }

                // First 4 bytes: CAN ID (big-endian)
                uint txCanId = ((uint)pMsg->Data[0] << 24) |
                               ((uint)pMsg->Data[1] << 16) |
                               ((uint)pMsg->Data[2] << 8) |
                               ((uint)pMsg->Data[3]);

                // Remaining bytes: UDS payload
                int payloadLen = (int)dataSize - 4;
                byte[] payload = new byte[payloadLen];
                for (int i = 0; i < payloadLen; i++)
                {
                    payload[i] = pMsg->Data[4 + i];
                }

                StringBuilder hex = new StringBuilder();
                for (int i = 0; i < payloadLen; i++)
                {
                    hex.Append(payload[i].ToString("X2"));
                }

                // Telemetry tracking
                s_totalTxPackets++;
                s_lastTxTick = Environment.TickCount;
                string svcDesc = GetUdsServiceDescription(payload);
                s_activeEcuName = GetEcuName(txCanId);

                AddRecentPacket(string.Format("[{0:HH:mm:ss.fff}] TX -> 0x{1:X3} ({2}): {3} ({4})",
                    DateTime.Now, txCanId, s_activeEcuName, hex.ToString(), svcDesc));

                Log(string.Format("PassThruWriteMsgs: CAN ID=0x{0:X} ({1}), Payload={2} ({3} bytes)",
                    txCanId, s_activeEcuName, hex.ToString(), payloadLen));

                if (s_isConnectedToHardware)
                {
                    // Multi-ECU synchronization:
                    // If target ECU changed, re-sync ATSH, ATFCSH (flow control), and ATCRA (receive filter)!
                    if (txCanId != s_currentTxCanId && txCanId > 0)
                    {
                        s_currentTxCanId = txCanId;

                        // Find expected Rx CAN ID:
                        uint targetRx = 0;
                        if (!s_filterMap.TryGetValue(txCanId, out targetRx) || targetRx == 0)
                        {
                            if (txCanId == 0x7DF) targetRx = 0; // Broadcast
                            else targetRx = txCanId + 8;         // Standard Tata ISO 15765-4 physical address (0x785->0x78D, 0x701->0x709, etc.)
                        }

                        s_currentRxCanId = targetRx;

                        SendCommand(string.Format("ATSH {0:X3}", txCanId));
                        SendCommand(string.Format("ATFCSH {0:X3}", txCanId)); // Send flow control to target ECU

                        if (!s_openFilter)
                        {
                            if (targetRx > 0)
                            {
                                SendCommand(string.Format("ATCRA {0:X3}", targetRx)); // hardware receive filter
                            }
                            else
                            {
                                SendCommand("ATCRA"); // Broadcast: accept all incoming CAN IDs
                            }
                        }
                    }

                    string rawResp = SendCommand(hex.ToString(), timeout > 0 ? timeout : s_timeoutMs);
                    ParseAndQueueResponses(rawResp, txCanId);
                }
                else
                {
                    Log("Simulating write (hardware not connected).");
                }

                SaveTelemetry();
                *pNumMsgs = 1;
                return STATUS_NOERROR;
            }
        }

        public static unsafe int PassThruReadMsgs(int channelId, IntPtr pMsgs, int* pNumMsgs, int timeout)
        {
            lock (s_lock)
            {
                if (pMsgs == IntPtr.Zero || pNumMsgs == null) return ERR_NULL_PARAMETER;
                if (*pNumMsgs < 1) return STATUS_NOERROR;

                int waitLimit = timeout > 0 ? timeout : 100;
                int start = Environment.TickCount;

                while (s_rxQueue.Count == 0 && (Environment.TickCount - start) < waitLimit)
                {
                    if (s_connection != null && s_connection.IsOpen && s_connection.BytesToRead > 0)
                    {
                        string chunk = s_connection.ReadExisting();
                        ParseAndQueueResponses(chunk, s_currentTxCanId);
                        if (s_rxQueue.Count > 0)
                            break;
                    }
                    Thread.Sleep(10);
                }

                if (s_rxQueue.Count == 0)
                {
                    *pNumMsgs = 0;
                    return ERR_BUFFER_EMPTY;
                }

                ReceivedMessage rx = s_rxQueue.Dequeue();
                UnsafePassThruMsg* pTarget = (UnsafePassThruMsg*)pMsgs.ToPointer();

                pTarget->ProtocolID = rx.ProtocolID;
                pTarget->RxStatus = 0;
                pTarget->TxFlags = 0;
                pTarget->Timestamp = rx.Timestamp;
                pTarget->ExtraDataIndex = 0;

                uint fullSize = (uint)(4 + rx.Payload.Length);
                pTarget->DataSize = fullSize;

                // Write 4-byte CAN ID (big-endian)
                pTarget->Data[0] = (byte)((rx.CanID >> 24) & 0xFF);
                pTarget->Data[1] = (byte)((rx.CanID >> 16) & 0xFF);
                pTarget->Data[2] = (byte)((rx.CanID >> 8) & 0xFF);
                pTarget->Data[3] = (byte)(rx.CanID & 0xFF);

                // Write payload
                for (int i = 0; i < rx.Payload.Length; i++)
                {
                    pTarget->Data[4 + i] = rx.Payload[i];
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < rx.Payload.Length; i++)
                {
                    sb.Append(rx.Payload[i].ToString("X2")).Append(" ");
                }
                Log(string.Format("PassThruReadMsgs: Returning CAN ID=0x{0:X}, DataSize={1}, Payload=[{2}]", rx.CanID, fullSize, sb.ToString().Trim()));

                *pNumMsgs = 1;
                return STATUS_NOERROR;
            }
        }

        private static void ParseAndQueueResponses(string raw, uint requestTxCanId)
        {
            if (string.IsNullOrEmpty(raw)) return;

            string clean = raw.Replace(">", "").Replace("SEARCHING...", "").Replace("STOPPED", "").Trim();
            // Remove line-numbering prefixes (e.g. "0: ", "1: ", "0:", "1:")
            clean = Regex.Replace(clean, @"\b[0-9A-Fa-f]:", "");

            string[] tokens = clean.Split(new char[] { '\r', '\n', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawToken in tokens)
            {
                string token = rawToken.Trim();
                if (string.IsNullOrEmpty(token) || token.StartsWith("NO") || token.StartsWith("?") || token.StartsWith("OK"))
                    continue;

                if (!Regex.IsMatch(token, "^[0-9A-Fa-f]+$"))
                    continue;

                uint canId = 0;
                string payloadHex = token;

                // Support 29-bit CAN ID (8 hex digits, e.g. 18DA...) or 11-bit CAN ID (3 hex digits)
                if (token.Length >= 10 && (token.StartsWith("18DA", StringComparison.OrdinalIgnoreCase) || token.StartsWith("18DB", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        canId = Convert.ToUInt32(token.Substring(0, 8), 16);
                        payloadHex = token.Substring(8);
                    }
                    catch { }
                }
                else if (token.Length >= 5 && IsHex(token.Substring(0, 3)))
                {
                    try
                    {
                        canId = Convert.ToUInt32(token.Substring(0, 3), 16);
                        payloadHex = token.Substring(3);
                    }
                    catch { }
                }

                if (canId == 0)
                {
                    canId = s_currentRxCanId > 0 ? s_currentRxCanId : (requestTxCanId + 8);
                }
                else if (!IsExpectedResponder(canId, requestTxCanId))
                {
                    // A frame from an ECU we did not just address. Almost always a
                    // late reply to an earlier request still draining out of the
                    // adapter. Accepting it would report one ECU's data as
                    // another's, which is worse than dropping it.
                    s_droppedForeignFrames++;
                    Log(string.Format(
                        "Dropped frame from 0x{0:X3} ({1}) - request went to 0x{2:X3}, expected reply on 0x{3:X3}",
                        canId, GetEcuName(canId), requestTxCanId, s_currentRxCanId));
                    continue;
                }

                byte[] rawBytes = HexStringToBytes(payloadHex);
                if (rawBytes.Length == 0) continue;

                s_totalRxPackets++;
                if (s_lastTxTick > 0)
                {
                    int lat = Environment.TickCount - s_lastTxTick;
                    if (lat >= 0 && lat < 10000) s_lastLatencyMs = lat;
                }

                byte pci = rawBytes[0];
                int pciType = (pci >> 4) & 0x0F;

                // Case 1: PCI Single Frame (0x0L [L bytes payload])
                if (pciType == 0)
                {
                    int len = pci & 0x0F;
                    if (len > 0 && len <= (rawBytes.Length - 1))
                    {
                        byte[] sfPayload = new byte[len];
                        Array.Copy(rawBytes, 1, sfPayload, 0, len);

                        // Filter UDS NRC 0x78 (Response Pending: 7F <SID> 78)
                        if (sfPayload.Length >= 3 && sfPayload[0] == 0x7F && sfPayload[2] == 0x78)
                        {
                            Log(string.Format("Filtered ResponsePending (7F {0:X2} 78) from 0x{1:X} - awaiting final payload", sfPayload[1], canId));
                            continue;
                        }

                        string rxDesc = GetUdsResponseDescription(sfPayload);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < Math.Min(12, sfPayload.Length); i++) sb.Append(sfPayload[i].ToString("X2")).Append(" ");
                        if (sfPayload.Length > 12) sb.Append("...");
                        AddRecentPacket(string.Format("[{0:HH:mm:ss.fff}] RX <- 0x{1:X3} ({2}): {3}({4})",
                            DateTime.Now, canId, GetEcuName(canId), sb.ToString(), rxDesc));

                        s_rxQueue.Enqueue(new ReceivedMessage(PROTOCOL_ISO15765, canId, sfPayload));
                        Log(string.Format("Queued Single Frame response from 0x{0:X}: {1} bytes", canId, len));
                    }
                }
                // Case 2: PCI First Frame (0x1L LL [payload])
                else if (pciType == 1)
                {
                    if (rawBytes.Length >= 2)
                    {
                        s_expectedMultiLength = ((pci & 0x0F) << 8) | rawBytes[1];
                        s_multiFrameRxId = canId;
                        s_multiFramePayload.Clear();
                        for (int i = 2; i < rawBytes.Length; i++)
                        {
                            s_multiFramePayload.Add(rawBytes[i]);
                        }
                        Log(string.Format("Received First Frame from 0x{0:X}: Expected={1} bytes, initial={2} bytes",
                            canId, s_expectedMultiLength, s_multiFramePayload.Count));
                    }
                }
                // Case 3: PCI Consecutive Frame (0x2S [payload])
                else if (pciType == 2)
                {
                    for (int i = 1; i < rawBytes.Length; i++)
                    {
                        if (s_expectedMultiLength > 0 && s_multiFramePayload.Count >= s_expectedMultiLength)
                            break;
                        s_multiFramePayload.Add(rawBytes[i]);
                    }

                    if (s_expectedMultiLength > 0 && s_multiFramePayload.Count >= s_expectedMultiLength)
                    {
                        uint finalRxId = s_multiFrameRxId > 0 ? s_multiFrameRxId : canId;
                        byte[] fullPayload = s_multiFramePayload.ToArray();

                        string rxDesc = GetUdsResponseDescription(fullPayload);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < Math.Min(12, fullPayload.Length); i++) sb.Append(fullPayload[i].ToString("X2")).Append(" ");
                        if (fullPayload.Length > 12) sb.Append("...");
                        AddRecentPacket(string.Format("[{0:HH:mm:ss.fff}] RX <- 0x{1:X3} ({2}): {3}({4})",
                            DateTime.Now, finalRxId, GetEcuName(finalRxId), sb.ToString(), rxDesc));

                        s_rxQueue.Enqueue(new ReceivedMessage(PROTOCOL_ISO15765, finalRxId, fullPayload));
                        Log(string.Format("Queued Multi-Frame response from 0x{0:X}: {1} bytes (complete)",
                            finalRxId, s_multiFramePayload.Count));
                        s_multiFramePayload.Clear();
                        s_expectedMultiLength = -1;
                    }
                }
                // Case 4: Already Reassembled UDS Response (0x50..0x7F) or Raw Payload from genuine ELM327
                else
                {
                    string rxDesc = GetUdsResponseDescription(rawBytes);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < Math.Min(12, rawBytes.Length); i++) sb.Append(rawBytes[i].ToString("X2")).Append(" ");
                    if (rawBytes.Length > 12) sb.Append("...");
                    AddRecentPacket(string.Format("[{0:HH:mm:ss.fff}] RX <- 0x{1:X3} ({2}): {3}({4})",
                        DateTime.Now, canId, GetEcuName(canId), sb.ToString(), rxDesc));

                    s_rxQueue.Enqueue(new ReceivedMessage(PROTOCOL_ISO15765, canId, rawBytes));
                    Log(string.Format("Queued UDS / Raw response from 0x{0:X}: {1} bytes", canId, rawBytes.Length));
                }

                SaveTelemetry();
            }
        }

        /// <summary>
        /// True when <paramref name="rxCanId"/> is a plausible responder for a
        /// request sent to <paramref name="txCanId"/>. Accepts the negotiated Rx
        /// id, the ISO 15765-4 convention of request+8, and any id registered by
        /// TDS through PassThruStartMsgFilter. Functional requests (0x7DF) accept
        /// anything, since that is the point of them.
        /// </summary>
        private static bool IsExpectedResponder(uint rxCanId, uint txCanId)
        {
            if (s_openFilterAcceptAll) return true;
            if (txCanId == 0x7DF || txCanId == 0) return true;
            if (s_currentRxCanId != 0 && rxCanId == s_currentRxCanId) return true;
            if (rxCanId == txCanId + 8) return true;
            uint mapped;
            if (s_filterMap.TryGetValue(txCanId, out mapped) && mapped != 0 && rxCanId == mapped)
                return true;
            return false;
        }

        private static bool IsHex(string s)
        {
            foreach (char c in s)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                    return false;
            }
            return true;
        }

        private static byte[] HexStringToBytes(string hex)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i + 1 < hex.Length; i += 2)
            {
                try
                {
                    bytes.Add(Convert.ToByte(hex.Substring(i, 2), 16));
                }
                catch { }
            }
            return bytes.ToArray();
        }

        public static unsafe int PassThruStartPeriodicMsg(int channelId, UnsafePassThruMsg* pMsg, int* pMsgId, int timeInterval)
        {
            if (pMsgId != null) *pMsgId = 1;
            return STATUS_NOERROR;
        }

        public static unsafe int PassThruStopPeriodicMsg(int channelId, int msgId)
        {
            return STATUS_NOERROR;
        }

        public static unsafe int PassThruSetProgrammingVoltage(int deviceId, int pinNumber, int voltage)
        {
            return ERR_NOT_SUPPORTED;
        }

        public static unsafe int PassThruReadVersion(int deviceId, IntPtr pFirmware, IntPtr pDll, IntPtr pApi)
        {
            Log("PassThruReadVersion() called.");
            try
            {
                byte[] fw = Encoding.ASCII.GetBytes("ELM327 Multi-ECU / Tata J2534 Bridge\0");
                byte[] dll = Encoding.ASCII.GetBytes("1.2.0\0");
                byte[] api = Encoding.ASCII.GetBytes("04.04\0");

                if (pFirmware != IntPtr.Zero) Marshal.Copy(fw, 0, pFirmware, fw.Length);
                if (pDll != IntPtr.Zero) Marshal.Copy(dll, 0, pDll, dll.Length);
                if (pApi != IntPtr.Zero) Marshal.Copy(api, 0, pApi, api.Length);

                return STATUS_NOERROR;
            }
            catch
            {
                return ERR_FAILED;
            }
        }

        public static unsafe int PassThruGetLastError(IntPtr pError)
        {
            try
            {
                if (pError != IntPtr.Zero)
                {
                    byte[] err = Encoding.ASCII.GetBytes("STATUS_NOERROR\0");
                    Marshal.Copy(err, 0, pError, err.Length);
                }
                return STATUS_NOERROR;
            }
            catch
            {
                return ERR_FAILED;
            }
        }

        public static unsafe int PassThruIoctl(int channelId, int ioctlID, IntPtr input, IntPtr output)
        {
            Log("PassThruIoctl(Channel=" + channelId + ", IoctlID=" + ioctlID + ") called.");
            lock (s_lock)
            {
                try
                {
                    switch (ioctlID)
                    {
                        case 1: // READ_VBATT
                            {
                                int millivolts = 12600;
                                if (s_isConnectedToHardware)
                                {
                                    string resp = SendCommand("ATRV", 400);
                                    Match m = Regex.Match(resp, @"(\d+\.?\d*)V?", RegexOptions.IgnoreCase);
                                    if (m.Success)
                                    {
                                        double volts = 0;
                                        if (double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out volts))
                                        {
                                            millivolts = (int)(volts * 1000);
                                        }
                                    }
                                }

                                if (output != IntPtr.Zero)
                                {
                                    Marshal.WriteInt32(output, millivolts);
                                }
                                Log("READ_VBATT returning " + millivolts + " mV");
                                SaveTelemetry();
                                return STATUS_NOERROR;
                            }

                        case 7: // CLEAR_TX_BUFFER
                            return STATUS_NOERROR;

                        case 8: // CLEAR_RX_BUFFER
                            s_rxQueue.Clear();
                            s_multiFramePayload.Clear();
                            s_expectedMultiLength = -1;
                            if (s_connection != null && s_connection.IsOpen)
                            {
                                s_connection.DiscardInBuffer();
                            }
                            return STATUS_NOERROR;

                        case 10: // CLEAR_MSG_FILTERS
                            s_rxQueue.Clear();
                            return STATUS_NOERROR;

                        default:
                            return STATUS_NOERROR;
                    }
                }
                catch (Exception ex)
                {
                    Log("PassThruIoctl Error: " + ex.Message);
                    return STATUS_NOERROR;
                }
            }
        }
    }
}
