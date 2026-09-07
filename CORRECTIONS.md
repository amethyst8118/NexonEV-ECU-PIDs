# Corrections

The Tata databases contain authoring errors. Every scalar DID across all eight
ECUs — **818 signals** — was audited for unit/scaling anomalies. This is what was
found, what was changed, and why.

**The raw exports in [`data/`](data/) are left untouched**, so it stays clear what
came from Tata and what came from us. Corrections live in
[`data/corrections.json`](data/corrections.json) and are applied to everything
generated downstream: the CarScanner profiles and the tables in the reference docs.

A unit shown as `(X ?)` in a profile means the signal is flagged below as suspect.

## Corrected

### Power DIDs are kW, not W or A

| DID | ECU | Database says | Corrected to |
|-----|-----|---------------|--------------|
| `$347D` | BMS | `W` | **`kW`** |
| `$347E` | BMS | `W` *and* `kW` | **`kW`** |
| `$347F` | BMS | `kW` | `kW` (already right) |
| `$3480` | BMS | `W` | **`kW`** |

The TDS 20.0 database contradicts itself — `$347E` appears twice, once as `W` and
once as `kW`. Arithmetic settles it: at resolution 0.1 with two bytes the maximum
representable value is 6553.5. A 105 kW pack cannot be expressed in watts at
0.1 W/bit, but 105 kW is 1050 counts at 0.1 kW/bit.

The signal names settle any remaining doubt: `OutPower` and `FBPower`.

### Cumulative capacity is Ah, not °C

| DID | ECU | Database says | Corrected to |
|-----|-----|---------------|--------------|
| `$35B0` | VECU | `C` | **`Ah`** |
| `$35B1` | VECU | `C` | **`Ah`** |

The BMS defines the identical signals at `$353E` / `$353F` as **`Ah`**. These are
charge throughput counters, not temperatures.

## Units added

Missing from the database, unambiguous from the signal name:

| DID | ECU | Signal | Unit |
|-----|-----|--------|------|
| `$3421` | VECU | HV Battery SOC | `%` |
| `$3568` | BMS | Calculated SOC Reference | `%` |
| `$3492` | BMS | LV Power Supply Voltage | `V` |
| `$34BF` | VECU | VECU Supply Voltage | `V` |
| `$3497` | BMS | Inlet Temp 2 | `°C` |
| `$3498` | BMS | Outlet Temp 2 | `°C` |

## Flagged, not changed

These look wrong but cannot be resolved from the files alone. They are marked `?`
rather than altered, because a confident wrong value is worse than a visible doubt.

### `$3428` — VECU charging current limit

One byte, unit `A`, **offset 40**. A 40 offset is the coolant-temperature scaling
pattern found throughout these databases, and makes no sense on a current limit.

The VECU carries a second DID, **`$352F`**, named almost identically — *"Charging
current limit requested by VCU"* — at two bytes, resolution 0.1, no offset. That
one is sane. **Prefer `$352F`.**

### `$3497` / `$3498` — BMS inlet/outlet temperature 2

**Offset 50**, where `$3410`, `$3411` and every other coolant temperature in every
Tata database use **40**. Either these sensors genuinely have a different range, or
someone typed 50.

To settle it: read `$3497` against `$3410` on the car. If `$3497` reads 10 °C
higher than it should, the offset is 40.

### `$3481` — BMS negative busbar voltage

Declared as resolution 1 in **mV**, where the neighbouring positive busbar `$3482`
is 0.1 V. The asymmetry is easy to misread as an error but is physically right: the
negative busbar sits close to chassis potential, so a millivolt scale suits it,
while `$3482` carries pack potential and needs volts.

Left as declared. Worth a sanity check on the car all the same — if `$3481` reads
in the hundreds rather than near zero, the unit is wrong.

## Checked and found correct

Things the audit flagged that turned out to be fine, recorded so they are not
re-litigated:

- **Bidirectional current without an offset** (19 signals). DCDC input/output
  current, OBC AC current, motor phase currents and charge-current limits are all
  magnitudes. Only genuinely bidirectional signals need a midpoint offset, and
  those — pack current `$3401` and `$341D` — have one.
- **Motor speed and torque offsets** (`$34A4`, `$34AA`, `$35BE`, `$35DB` at 17000
  or 20000; `$3500`, `$3501` at 500). These are legitimate bipolar encodings — the
  motor turns both ways and torque goes both directions.
- **Two-byte voltages spanning to 6553.5 V.** That is headroom in the raw range,
  not a scaling error.
- **AC voltage at resolution 1** (`$1106`, `$1904` on the OBC). Whole volts is
  appropriate for a ~230 V mains reading.
- **Busbar voltage at resolution 1 V** (`$3484`, `$34AD`). A 350 V busbar reads
  350 — fine.

## Method

The audit script checks each scalar signal for: temperature-style offsets on
non-temperature signals, temperature units with no offset, offsets on unitless or
percentage signals, high-voltage signals read 1:1, millivolt units with fractional
resolution, bidirectional-sounding signals with no midpoint, and engineering spans
beyond anything physical.

Every hit was then cross-checked against the same signal in the other seven
databases before being called an error. Most were not.
