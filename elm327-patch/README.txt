=====================================================================
  TDS ELM327 OBD2 PATCH  v2.0
  SAE J2534 PassThru shim for Tata Diagnostic System 20.0.0
=====================================================================

Lets TDS talk to an ELM327 (USB, Bluetooth or WiFi) instead of the
factory Samtec HS Light II VCI, by presenting itself to TDS as a
J2534 PassThru driver and translating to ELM327 AT/OBD commands.

Upgrades cleanly over v1. Read "UPGRADING" below.


---------------------------------------------------------------------
INSTALL
---------------------------------------------------------------------

  1. Extract this folder anywhere.
  2. Right-click Apply_Patch.bat -> Run as administrator.
     (It elevates itself if you forget.)
  3. Launch with the "Launch_TDS_ELM327" desktop shortcut.

The installer finds TDS automatically, closes it if running, backs up
every file it is about to replace, installs, merges your config, and
registers the J2534 driver.


---------------------------------------------------------------------
UPGRADING FROM v1
---------------------------------------------------------------------

Just run Apply_Patch.bat. It detects the existing install and:

  - backs up all current files to ELM327_backup_<timestamp>\
  - replaces every patch file, in the TDS root AND in Extracted\
    if that folder exists (a stale copy there can otherwise be loaded
    instead of the new one)
  - MERGES elm327.ini instead of overwriting it

That last point matters. v1's installer overwrote elm327.ini outright,
resetting PORT, BAUD, TYPE and the WiFi settings back to defaults. v2
keeps every value you had, adds only the new keys, and preserves any
key of your own that it does not recognise.

To undo: run Rollback_Patch.bat and pick a backup.


---------------------------------------------------------------------
WHAT CHANGED IN v2
---------------------------------------------------------------------

Bug fixes in ELM327_Bridge.cs, all adapter-agnostic:

  * Header and reassembly state survived a reconnect.
    CloseConnection() left s_currentTxCanId set, but the next connect
    sends ATZ which resets the adapter's header. PassThruWriteMsgs
    then skipped ATSH, so the first request after a reconnect went out
    on the wrong header. TDS opens and closes the device several times
    during startup, so this was reached in normal use.

  * Input buffer was flushed mid-reassembly.
    SendCommand called DiscardInBuffer() before every write. With a
    first frame received and consecutive frames still in flight, those
    continuation frames were thrown away.

  * Serial write timeout raised from 400 ms to 2000 ms.
    Bluetooth SPP stalls longer than 400 ms under load; this was the
    cause of "SendCommand Error: The write timed out".

  * ATZ given 3000 ms instead of 1000 ms.
    ATZ is a full adapter reset and the slowest AT command. At 1000 ms
    the version banner was being truncated and the identification
    check fell through to accepting a bare prompt.

  * Responses are now checked against the ECU that was addressed.
    Frames from an unexpected sender are dropped and logged rather
    than handed to TDS as the answer. On working hardware this never
    fires. On an adapter that ignores ATSH/ATCRA it turns silent
    wrong-ECU data into visible "Dropped frame from 0x..." lines.
    Set ACCEPT_ALL_RESPONDERS=1 to disable.

  * elm327_log.txt now rotates.
    It previously grew without limit - one session produced 8 MB and
    110,000 lines, with a synchronous file append per operation.

Installer improvements: backups before replacing, Extracted\ kept in
sync, config merged rather than clobbered, verification step, version
marker, and a rollback script.

Full write-up with the evidence for each: ANALYSIS.md


---------------------------------------------------------------------
CONFIGURATION - elm327.ini
---------------------------------------------------------------------

  TYPE          AUTO | SERIAL | WIFI
  PORT          AUTO, or COM3 / COM4 ...
  BAUD          38400 (Bluetooth), 115200 (CH340/FTDI/CP2102)
  WIFI_IP       192.168.0.10
  WIFI_PORT     35000
  TIMEOUT       response timeout, ms
  OPEN_FILTER   0 = per-ECU hardware filter, 1 = accept all + filter
                in software
  LOG           1 = write elm327_log.txt
  TELEMETRY     1 = write live_telematics.json

  New in v2:
  ACCEPT_ALL_RESPONDERS   0 = drop frames from an ECU that was not
                          addressed (recommended). 1 = accept every
                          frame; expect cross-contaminated readings.
  LOG_MAX_MB              roll the log past this size. 0 = never.


---------------------------------------------------------------------
FILES
---------------------------------------------------------------------

  Apply_Patch.bat            installer / upgrader
  Rollback_Patch.bat         restore a previous backup
  merge_ini.ps1              config merge used by the installer
  ELM327_Bridge.dll          the bridge (compiled)
  ELM327_Bridge.cs           its source
  ELM327_PassThru.dll        native J2534 entry points
  ELM327_PassThru.reg        registry registration
  register_elm327_j2534.ps1  registration script
  Start_TDS_ELM327.bat       launcher
  Test_OBD_Connection.bat    adapter self-test
  test_connection.ps1        self-test script
  Live_Telematics.bat/.ps1   live telemetry viewer
  ANALYSIS.md                code review behind v2
  README.txt                 this file


---------------------------------------------------------------------
A NOTE ON ADAPTERS
---------------------------------------------------------------------

Counterfeit ELM327s are common and many ignore ATSH and ATCRA - they
accept the command, reply OK, and then answer from whatever ECU the
bus offers. TDS will show one ECU's data under another's name.

v2 makes that visible rather than silent: you will see repeated
"Dropped frame from 0xNNN - request went to 0xMMM" in elm327_log.txt.
No bridge code can fix it. A genuine ELM327 v1.4+, or an STN-based
adapter (OBDLink SX / MX+), is the actual remedy.

Note also that the Nexon EV BMS sits on the Powertrain CAN at OBD
pins 3 and 11, not the 6/14 pair a standard adapter is wired to.
