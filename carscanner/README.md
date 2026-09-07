# CarScanner custom PID profiles

Import via **Settings → Custom PIDs → Import**. Files are plain JSON, editable in
any text editor.

## Downloading

**Grab [`NexonEV_CarScanner_Profiles.zip`](NexonEV_CarScanner_Profiles.zip)** and
extract it. That is the reliable route.

GitHub serves any extension it does not recognise as `text/plain`, so downloading
a `.csp` on its own arrives as **`NexonEV_BMS.csp.txt`** and CarScanner will not
offer it in the import picker. Nothing is wrong with the file — rename it back to
`.csp` and it imports fine. On Windows you may need to turn off *Hide extensions
for known file types* in Explorer's View options to do that.

The zip sidesteps it entirely.

| File | Sensors | Header | Response | ECU |
|------|--------:|--------|----------|-----|
| `NexonEV_BMS.csp` | 42 | `785` | `78D` | Battery Management |
| `NexonEV_VECU.csp` | 233 | `7E3` | `7EB` | Vehicle Control |
| `NexonEV_OBC.csp` | 29 | `786` | `78E` | On Board Charger |
| `NexonEV_DCDC.csp` | 20 | `784` | `78C` | DC-DC Converter |
| `NexonEV_MCU.csp` | 15 | `783` | `78B` | Motor Control |
| `NexonEV_VECU_battery.csp` | 23 | `7E3` | `7EB` | VECU, battery subset |

All 11-bit at 500 kbps, generated from the **TDS 20.0** databases.

Every profile is filtered to DIDs this car will actually answer:

- **Permission.** A DID whose permission column reads `None` is not exposed by the
  ECU. 20 such DIDs were being polled on the BMS and 18 on the OBC; they would
  simply never return data.
- **Platform.** The VECU database lists the same DID once per vehicle platform,
  with a Y/N column for each. A Nexon EV is KANGER 2.0 / 3.0, so only rows flagged
  `K2` or `K3` apply — 174 of the VECU's rows belong to CURVV, ETURNA, ROWA, Nano,
  OSPREY or ChallengerEV and describe different signals on those cars.
- **Duplicates.** What the per-platform rows collapse into once filtered. Where a
  DID still appears twice, the later row wins: it carries the current name and
  unit, and the earlier one is a superseded entry left in the table.

That took the VECU profile from 462 sensors to 233 and the BMS from 65 to 42,
which also makes round-robin polling roughly twice as fast. `BCM` is set to
the session command each ECU's database specifies (`1003`, or `1001` for VECU and
OBC), so the session is entered before every read.

> **`NexonEV_VECU.csp` has 462 sensors.** CarScanner polls every *visible* sensor
> round-robin, so enabling all of them makes each update crawl. Put the handful you
> want on a dashboard page and leave the rest hidden.

## Scaling lives in the formula, not in MUL/DIV/OFS

This is worth knowing if you edit these or write your own.

CarScanner **ignores the `MUL`, `DIV` and `OFS` fields** in a `.csp`. It evaluates
only `FR`, the formula string. A profile that puts the scaling in `MUL`/`DIV`/`OFS`
displays the raw integer instead — pack voltage reads `3507` rather than `350.7`,
temperatures read `84` rather than `34`.

So the scaling is baked into `FR`:

| DID | Signal | `FR` |
|-----|--------|------|
| `$341E` | HV Battery Voltage | `(A*256+B)/10` |
| `$341D` | HV Battery Current | `(A*256+B)/10-600` |
| `$3478` | Cell Voltage Min | `(A*256+B)/100` |
| `$3423` | Cell Temperature Max | `A-50` |
| `$3426` | PDU +Ve bus bar temp | `A-40` |

`MUL`/`DIV`/`OFS` are left at identity (`1`/`1`/`0`) so that a build which *does*
honour them cannot double-scale the value.

## Coded sensors

Names ending `[code]` or `[bits]` return a **number that means something**, not a
measurement. `[bits]` means several flags share the byte — test `value & mask`,
never equality.

Value meanings for every coded DID are in
[`../DECODE_TABLES.md`](../DECODE_TABLES.md).

For cell balancing specifically: **`$3479 & 0x04`** set means balancing is running,
so any of 4, 5, 6 or 7. The database also contains an older, retired description of
that byte putting balancing on `0x02`; it is superseded and should not be used —
[`../CELL_BALANCING.md`](../CELL_BALANCING.md) sets out the evidence.

CarScanner's bit-extraction fields (`SBI`, `BIT`) were never confirmed against a
working example, so these ship as whole-byte readings rather than as guessed
per-bit switches that could silently show the wrong signal.

## Other limitations

- **Identification PIDs are omitted.** They return ASCII strings that CarScanner
  renders as meaningless numbers.
- **Units are in the sensor name**, e.g. `HV Battery Voltage (V)`. CarScanner's
  unit field is an internal enum whose mapping is undocumented, so it is left at 0.
- **MIN/MAX are display bounds** derived from the raw range, not limits read from
  the car. A gauge will peg rather than show an out-of-range reading.
- **Only 1- and 2-byte signals are included.** Wider ones are strings, packed
  records or multi-frame, and none of those render usefully as a gauge.

## If a profile reads nothing

Load `NexonEV_VECU.csp` first — the VECU is the most reliably reachable ECU on the
car. If its sensors populate and another profile's do not, the adapter and app are
fine and the problem is that ECU not answering on its header.

Full DID definitions, including everything excluded from these profiles, are in
[`../data/`](../data/).
