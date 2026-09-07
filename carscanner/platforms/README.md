# Per-platform VECU profiles

CarScanner profiles for the **Vehicle Control Unit** on every Tata EV platform,
generated from the TDS 21.0 `VECU_DiagnosticsDB.mdb`.

| Profile | Sensors | Platform | Cars |
|---------|--------:|----------|------|
| `VECU_K2.csp` | 218 | KANGER 2.0 | Nexon EV, Punch EV, Tiago EV, Curvv EV |
| `VECU_K3.csp` | 202 | KANGER 3.0 | Nexon EV, Punch EV, Curvv EV |
| `VECU_OSPREY.csp` | 138 | Osprey | Nexon EV, Punch EV, Tiago EV, Curvv EV |
| `VECU_CURVV.csp` | 222 | Curvv | Curvv EV |
| `VECU_ETURNA.csp` | 315 | Eturna | Eturna |
| `VECU_ROWA.csp` | 314 | ROW-A | export / rest-of-world build |
| `VECU_ChallengerEV.csp` | 190 | Challenger | Challenger EV |
| `VECU_NOVAMCE.csp` | 215 | Nova MCE | 6-in-1 integrated powertrain |
| `VECU_MidVariant.csp` | 164 | Mid variant | mid-spec common set |
| `VECU_NanoVariant.csp` | 16 | Nano | Nano EV |
| `VECU_BaseVariant.csp` | 402 | Base | the common set, all platforms |

All on header `0x7E3`, response `0x7EB`, 11-bit at 500 kbps, session `10 01`.

## Why these are separate files

The VECU database lists **the same DID once per platform**, each row carrying a
Y/N column for every platform. That is not redundancy — **the same DID often means
a different signal on a different car**:

| DID | On one platform | On another |
|-----|-----------------|------------|
| `$345D` | Maximum reverse vehicle speed | Compressor Diagnostic Status 2 |
| `$348C` | MCU Contactor Weld Status | Powertrain Config State 2 |
| `$3456` | Park Brake Sensor Value | Cooling Fan relay Enable Cmd |
| `$7205` | Calibration Version | Safety Secret Key |

Loading the wrong platform's profile does not fail visibly. It reports a
plausible-looking value under the wrong name, which is worse than reading nothing.

**Pick the one profile matching your car and ignore the rest.**

## Which one do I want?

Look your model up in the table above. If two apply — a Nexon EV is listed under
both KANGER 2.0 and 3.0 — start with **`VECU_K2.csp`**, and if a sensor reads
nothing, try the K3 file. The two overlap heavily; the difference is which
generation of the control unit is fitted.

For a **Nexon EV** specifically, the curated set in the [parent folder](..) is a
better starting point: it merges K2 and K3 and includes the BMS, MCU, DCDC and
OBC alongside.

`VECU_BaseVariant.csp` is the union of DIDs common to everything. It is the widest
net and the least precise — useful for probing an unknown car, not for daily use.

## Only the VECU is here

The other EV ECUs do not meaningfully vary by platform, so one profile serves all
of them and they live in the [parent folder](..):

- **BMS** — no platform columns at all. The DID set, scalings and permissions are
  identical for every variant. Only the *fault codes* differ by pack
  (`K1AIO`/`K2AIO` vs `Limber`), and K1AIO and K2AIO are themselves identical.
- **PEPS, BCM** — no platform columns.
- **DCDC** — has `BaseVariant` and `K2` columns, but they are identical: the same
  26 DIDs flagged in both.
- **OBC** — does differ, but 30 of its 39 `K2` rows are write-only configuration
  DIDs. Only 3 readable sensors are K2-specific, so a separate file would be
  misleading rather than useful.

## Caveats

These carry the same properties as every profile in this repo:

- scaling is baked into the `FR` formula, because CarScanner ignores
  `MUL`/`DIV`/`OFS`;
- sensors named `[code]` or `[bits]` return a value to look up in
  [`../../DECODE_TABLES.md`](../../DECODE_TABLES.md), not a measurement;
- identification DIDs are omitted — they return ASCII that renders as junk;
- a unit shown as `(X ?)` is flagged in [`../../CORRECTIONS.md`](../../CORRECTIONS.md);
- DIDs marked unreadable in the database are excluded.

**These are unverified on anything but a Nexon EV.** The platform flags come from
Tata's own database, but only the Nexon profiles have been used against a real
car. Treat a reading that looks wrong on another model as a finding worth
reporting, not as a fact.
