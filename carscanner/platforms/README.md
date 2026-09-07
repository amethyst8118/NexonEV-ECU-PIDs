# Platform folders

One folder per Tata EV platform. Each is self-contained — five ECU profiles,
import the lot.

| Folder | Platform | Cars | VECU sensors |
|--------|----------|------|-------------:|
| [`kanger-2.0/`](kanger-2.0/) | KANGER 2.0 | Nexon EV, Punch EV, Tiago EV, Curvv EV | 218 |
| [`kanger-3.0/`](kanger-3.0/) | KANGER 3.0 | Nexon EV, Punch EV, Curvv EV | 202 |
| [`osprey/`](osprey/) | Osprey | Nexon EV, Punch EV, Tiago EV, Curvv EV | 138 |
| [`punch-40/`](punch-40/) | Nova MCE | **Punch 40** | 215 |
| [`curvv/`](curvv/) | Curvv | Curvv EV | 222 |
| [`eturna/`](eturna/) | Eturna | Eturna | 315 |
| [`challenger-ev/`](challenger-ev/) | Challenger | Challenger EV | 190 |
| [`row-a/`](row-a/) | ROW-A | export / rest-of-world build | 314 |
| [`nano/`](nano/) | Nano | Nano EV | 16 |
| [`mid-variant/`](mid-variant/) | Mid variant | mid-spec common set | 164 |
| [`base/`](base/) | Base | common set, all platforms | 402 |

For a **Nexon EV**, use [`../nexon-ev/`](../nexon-ev/) instead — it merges KANGER
2.0 and 3.0 and adds a trimmed battery subset.

Every folder contains `VECU.csp`, `BMS.csp`, `MCU.csp`, `DCDC.csp` and `OBC.csp`.

## Only `VECU.csp` differs between folders

The other four are byte-identical everywhere. They are copied into each folder so
a folder is something you grab whole, not something you assemble.

That is not a shortcut — it is what the databases say. Each ECU was checked:

- **BMS** — no platform columns at all. Identical DIDs, scalings and permissions
  on every variant. Only the *fault codes* differ by pack (`K1AIO`/`K2AIO` vs
  `Limber`), and K1AIO and K2AIO are themselves identical to each other.
- **PEPS, BCM** — no platform columns.
- **MCU** — one `BaseVariant` column, no per-platform split.
- **DCDC** — has `BaseVariant` *and* `K2` columns, but they flag **the same 26
  DIDs**. Splitting would produce two identical files.
- **OBC** — genuinely differs (39 rows flagged `K2`), but **30 of those are
  write-only configuration DIDs**. Only 3 readable sensors are K2-specific, so a
  separate OBC file would be misleading rather than useful.

## Why the VECU has to be split

Its database lists **the same DID once per platform**, with a Y/N column for each.
That is not redundancy — the same DID often carries a different signal:

| DID | On one platform | On another |
|-----|-----------------|------------|
| `$345D` | Maximum reverse vehicle speed | Compressor Diagnostic Status 2 |
| `$348C` | MCU Contactor Weld Status | Powertrain Config State 2 |
| `$3456` | Park Brake Sensor Value | Cooling Fan relay Enable Cmd |
| `$7205` | Calibration Version | Safety Secret Key |

Load the wrong folder and CarScanner shows a plausible number under the wrong
name, with nothing to warn you.

## Picking between two that both list your car

A Nexon EV appears under KANGER 2.0, KANGER 3.0 *and* Osprey. KANGER covers the
powertrain and Osprey the body electronics, and 2.0 versus 3.0 is which generation
of control unit is fitted.

Start with `kanger-2.0`. If a sensor stays blank, try `kanger-3.0` — they overlap
heavily. Or just use [`../nexon-ev/`](../nexon-ev/), which merges both.

`base/` is the union of DIDs common to everything: the widest net and the least
precise. Useful for probing an unknown car, not for daily use.

## Caveats

Same as every profile here — scaling baked into `FR`, `[code]`/`[bits]` sensors
return values to look up in [`../../DECODE_TABLES.md`](../../DECODE_TABLES.md),
identification DIDs omitted, unreadable DIDs excluded. Full notes in
[`../README.md`](../README.md).

**Only the Nexon EV profiles have been used against a real car.** Everything here
rests on Tata's own platform flags, which are good evidence but not a test. Treat
a wrong-looking reading on another model as a finding, not a fact.
