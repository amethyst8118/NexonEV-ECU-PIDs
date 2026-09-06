# TDS ELM327 J2534 bridge — code review

Review of `ELM327_Bridge.cs` (1446 lines), the J2534 PassThru shim that lets TDS
drive an ELM327 instead of a Samtec VCI.

The design is sound: PassThru surface, ISO-TP reassembly, per-ECU header
switching, WiFi/serial/auto transport, telemetry export. TDS accepts it and reads
real data through it. What follows are correctness bugs inside a working design.

## First, a correction to my own earlier reading

I initially reported that every response arriving from `7EB` regardless of the
addressed ECU was a bridge bug. **It is not** — the adapter is a fake ELM327 that
ignores `ATSH` / `ATCRA` and answers from whatever the bus gives it. No amount of
bridge code fixes that, and none of the changes here try to work around it.

Consequently the log is only weak evidence about bridge behaviour: response
routing, the truncated `ATZ` banner, and the accepted-but-ineffective filter
commands are all attributable to the adapter. The findings below are from reading
the code, and are stated as such. Only the write timeouts and the log size are
directly evidenced by the log.

Clone-specific workarounds I had added — stripping spaces out of `ATSH`, `ATCRA`,
`ATFCSH` and `ATZ` — have been **removed**. The ELM327 datasheet specifies that
spaces are ignored, so a genuine adapter parses either form and the original
spelling is fine.

## Fixes

### Header and reassembly state survived a reconnect

`CloseConnection()` cleared the connection but left `s_currentTxCanId`,
`s_currentRxCanId` and the multi-frame accumulator at their previous values.

On the next connect, `InitElmDevice()` sends `ATZ` — which resets the *adapter's*
header to default — while `PassThruWriteMsgs` still skips `ATSH` whenever the
target equals `s_currentTxCanId`. The first request after a reconnect therefore
goes out on the default header rather than the intended one.

TDS opens and closes the device several times during startup, so this is reached
in normal use, on any adapter.

**Fixed** by clearing both CAN ids and the reassembly state in the `finally` block.

### Input buffer flushed while a multi-frame response was still arriving

`SendCommand` called `DiscardInBuffer()` unconditionally before every write. With a
first frame received and consecutive frames still in flight, those continuation
frames were discarded, and the reassembly then either timed out or completed
against the wrong data.

**Fixed** by skipping the flush while `s_expectedMultiLength >= 0`.

### Serial write timeout of 400 ms

`SerialPort.WriteTimeout = 400`. Bluetooth SPP routinely stalls longer than that
under load; the log's 40 `SendCommand Error: The write timed out` entries are the
symptom, and this is transport-level rather than adapter-specific.

**Fixed** to 2000 ms. `ReadTimeout` is deliberately left alone — reads are polled
through `BytesToRead`, so it barely participates.

### `ATZ` given only 1000 ms

`ATZ` is a full adapter reset and the slowest AT command; a real chip regularly
takes longer than a second to come back with its banner. The identification check
then falls through to accepting a bare `>` prompt as success.

**Fixed** to 3000 ms. The spelling `AT Z` is retained.

### Log grew without bound

110,913 lines / 8 MB in a single session, which makes the log useless for
diagnosis and adds a synchronous `File.AppendAllText` to every operation.

**Fixed** with rotation to `_prev.txt` past `LOG_MAX_MB` (default 8), size checked
every 200 writes rather than every line.

### Responses were queued without checking the sender

`ParseAndQueueResponses` reads the CAN id out of each frame and hands it to TDS
without verifying that it came from the ECU just addressed.

This is not the cause of your `7EB` readings — the adapter is — but it is why that
fault was **invisible**. With the check in place, an adapter that ignores `ATSH`
produces a log full of

```
Dropped frame from 0x7EB (VECU) - request went to 0x710, expected reply on 0x718
```

instead of silently reporting VECU data as PEPS data. On working hardware the
check never fires.

**Added** `IsExpectedResponder(rxCanId, txCanId)`: accepts the negotiated Rx id,
`txCanId + 8` per ISO 15765-4, or any id TDS registered through
`PassThruStartMsgFilter`; functional requests (`0x7DF`) accept anything. Rejects
are counted into `droppedForeignFrames` in the telemetry JSON.

`ACCEPT_ALL_RESPONDERS=1` in `elm327.ini` disables the check for anyone who would
rather see every frame.

## Considered and left alone

**Flow control.** `PassThruWriteMsgs` sets `ATFCSH` per ECU while `InitElmDevice`
leaves the adapter in `ATFCSM0`, where the ELM generates flow-control frames itself
and never consults `ATFCSH`. The per-ECU call is therefore inert.

I nearly "fixed" this to mode 1. That would have been a regression: mode 1 requires
`ATFCSD` to be set before the mode switch or the adapter rejects the command, and
`ATFCSH` is not set at init time. Mode 0 also demonstrably works — the log shows 22
first frames reassembled and 41 multi-frame responses completed. Left as mode 0
with a comment recording that `ATFCSH` is inert.

**`Regex.Replace(clean, @"\b[0-9A-Fa-f]:", "")`** strips line-number prefixes but
would also consume a legitimate hex digit followed by a colon. Harmless today,
because ELM output contains no other colons.

**NRC handling.** Only `0x78` (responsePending) is filtered and awaited. `0x21`
(busyRepeatRequest) is passed to TDS as a failure rather than retried — worth
adding if you see spurious read failures on a real adapter.

## Verification

Compiles clean under `csc /target:library /unsafe /optimize`, no warnings.
97 changed lines across 12 hunks; original line endings and encoding preserved so
the diff reviews cleanly. Not installed — `TDS_PassangerCars.exe` was running and
holding the DLL.

## Installing

1. Close TDS.
2. Back up `ELM327_Bridge.dll` (a copy of the original source is included as
   `ELM327_Bridge.cs.backup`).
3. Copy `ELM327_Bridge_new.dll` over `ELM327_Bridge.dll`, and `ELM327_Bridge.cs`
   alongside it.
4. Copy the updated `elm327.ini` — it adds `ACCEPT_ALL_RESPONDERS` and
   `LOG_MAX_MB`.

With a genuine adapter, expect no `Dropped frame` lines at all. Any that appear
point at a real routing problem rather than a scaling or parsing one.
