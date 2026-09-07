# CarScanner custom PID profiles

One folder per vehicle platform, one file per ECU. Grab the folder matching your
car and import all of it.

Import via **Settings → Custom PIDs → Import**.

## Downloading

**Grab [`CarScanner_Profiles.zip`](CarScanner_Profiles.zip)** and extract it — it
contains the whole tree.

GitHub serves any extension it does not recognise as `text/plain`, so downloading
a `.csp` on its own arrives as **`VECU.csp.txt`** and CarScanner will not offer it
in the import picker. Nothing is wrong with the file — rename it back to `.csp`.
On Windows you may need to turn off *Hide extensions for known file types* first.

## Which folder?

| Folder | Cars | VECU sensors |
|--------|------|-------------:|
| [`nexon-ev/`](nexon-ev/) | **Nexon EV** — start here | 233 |
| [`platforms/kanger-2.0/`](platforms/kanger-2.0/) | Nexon EV, Punch EV, Tiago EV, Curvv EV | 218 |
| [`platforms/kanger-3.0/`](platforms/kanger-3.0/) | Nexon EV, Punch EV, Curvv EV | 202 |
| [`platforms/osprey/`](platforms/osprey/) | Nexon EV, Punch EV, Tiago EV, Curvv EV | 138 |
| [`platforms/punch-40/`](platforms/punch-40/) | **Punch 40** (Nova MCE) | 215 |
| [`platforms/curvv/`](platforms/curvv/) | Curvv EV | 222 |
| [`platforms/eturna/`](platforms/eturna/) | Eturna | 315 |
| [`platforms/challenger-ev/`](platforms/challenger-ev/) | Challenger EV | 190 |
| [`platforms/row-a/`](platforms/row-a/) | export / rest-of-world build | 314 |
| [`platforms/nano/`](platforms/nano/) | Nano EV | 16 |
| [`platforms/mid-variant/`](platforms/mid-variant/) | mid-spec common set | 164 |
| [`platforms/base/`](platforms/base/) | common set, all platforms | 402 |

**Driving a Nexon EV? Use [`nexon-ev/`](nexon-ev/).** It merges KANGER 2.0 and 3.0
into one VECU file, adds a trimmed battery subset, and is the only set that has
actually been used against a car.

## What is in each folder

| File | ECU | Header | Response | Session |
|------|-----|--------|----------|---------|
| `VECU.csp` | Vehicle Control | `7E3` | `7EB` | `10 01` |
| `BMS.csp` | Battery Management | `785` | `78D` | `10 03` |
| `OBC.csp` | On Board Charger | `786` | `78E` | `10 01` |
| `DCDC.csp` | DC-DC Converter | `784` | `78C` | `10 03` |
| `MCU.csp` | Motor Control | `783` | `78B` | `10 03` |

`nexon-ev/` additionally has `VECU-battery.csp` — 23 sensors covering SOC, pack
voltage and current, cell min/max, temperatures, contactors and charging state.
**Start with that one**: it gives you the useful battery picture without the
polling cost of the full VECU set.

All 11-bit at 500 kbps. Sensor ids are unique within a folder, so importing
several files from the same folder never clashes.

## Only the VECU differs between platforms

`BMS.csp`, `MCU.csp`, `DCDC.csp` and `OBC.csp` are **identical in every folder** —
those ECUs have no platform variation in Tata's databases. They are duplicated
into each folder so a folder is self-contained rather than something you assemble
by hand.

The VECU is the exception, and the reason these are split at all: its database
lists **the same DID once per platform, and the meaning changes with it**.

| DID | On one platform | On another |
|-----|-----------------|------------|
| `$345D` | Maximum reverse vehicle speed | Compressor Diagnostic Status 2 |
| `$348C` | MCU Contactor Weld Status | Powertrain Config State 2 |
| `$3456` | Park Brake Sensor Value | Cooling Fan relay Enable Cmd |
| `$7205` | Calibration Version | Safety Secret Key |

Loading the wrong platform's VECU file does not fail visibly. It reports a
plausible number under the wrong name.

Why the other four do not vary is set out in
[`platforms/README.md`](platforms/README.md).

## Scaling lives in the formula, not in MUL/DIV/OFS

Worth knowing if you edit these or write your own.

CarScanner **ignores the `MUL`, `DIV` and `OFS` fields**. It evaluates only `FR`,
the formula string. A profile that puts scaling in `MUL`/`DIV`/`OFS` displays the
raw integer instead — pack voltage reading `3507` rather than `350.7`.

So the scaling is baked into `FR`:

| DID | Signal | `FR` |
|-----|--------|------|
| `$341E` | HV Battery Voltage | `(A*256+B)/10` |
| `$341D` | HV Battery Current | `(A*256+B)/10-600` |
| `$3478` | Cell Voltage Min | `(A*256+B)/100` |
| `$3423` | Cell Temperature Max | `A-50` |

`MUL`/`DIV`/`OFS` are left at identity so a build that *does* honour them cannot
double-scale.

## Coded sensors

Names ending `[code]` or `[bits]` return a **number that means something**, not a
measurement. `[bits]` means several flags share the byte — test `value & mask`,
never equality.

Value meanings for every coded DID are in
[`../DECODE_TABLES.md`](../DECODE_TABLES.md). For cell balancing,
**`$3479 & 0x04`** set means balancing is running — see
[`../CELL_BALANCING.md`](../CELL_BALANCING.md).

CarScanner's bit-extraction fields (`SBI`, `BIT`) were never confirmed against a
working example, so these ship as whole-byte readings rather than as guessed
per-bit switches that could silently show the wrong signal.

## Other limitations

- **Identification PIDs are omitted.** They return ASCII strings that CarScanner
  renders as meaningless numbers.
- **Units are in the sensor name**, e.g. `HV Battery Voltage (V)`. CarScanner's
  unit field is an internal enum whose mapping is undocumented.
- **A unit shown as `(X ?)`** is flagged as suspect in
  [`../CORRECTIONS.md`](../CORRECTIONS.md).
- **MIN/MAX are display bounds** derived from the raw range, not limits read from
  the car.
- **Only 1- and 2-byte signals are included.** Wider ones are strings, packed
  records or multi-frame, none of which render usefully as a gauge.
- **DIDs marked unreadable in the database are excluded**, so the profiles do not
  poll for data the ECU will never return.

## If a profile reads nothing

Load `VECU.csp` first — the VECU is the most reliably reachable ECU on the car. If
its sensors populate and another file's do not, the adapter and app are fine and
that ECU is simply not answering on its header.

**Only the Nexon EV profiles have been used against a real car.** The other
platforms rest on Tata's own platform flags. A wrong reading on a Punch, Tiago or
Curvv is a finding worth reporting, not a fact.
