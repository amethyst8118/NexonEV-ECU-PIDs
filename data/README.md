# Machine-readable PID definitions

Full DID definitions for every diagnosable ECU on the car — **1,181 DIDs across
eight ECUs**, exported from the Tata TDS ECU databases.

Checked against **TDS 21.0**: the BMS, MCU, DCDC and OBC databases are
content-identical to 20.0, and the VECU here is the 21.0 copy (515 DIDs, up from
502). Nothing in the update applies to a Nexon EV — see the note at the end.

| File | ECU | Header | DIDs | scaled | enums |
|------|-----|--------|-----:|-------:|------:|
| `TDS20_VECU_Unified_PIDs.json` | Vehicle Control | `0x7E3` | 515 | 338 | 174 |
| `TDS20_BCM_Unified_PIDs.json` | Body Control | `0x701` | 232 | 89 | 148 |
| `TDS20_PEPS_Unified_PIDs.json` | Passive Entry/Start | `0x710` | 135 | 85 | 48 |
| `TDS20_BMS_Unified_PIDs.json` | Battery Management | `0x785` | 114 | 105 | 9 |
| `TDS20_OBC_Unified_PIDs.json` | On Board Charger | `0x786` | 75 | 46 | 29 |
| `TDS20_DCDC_Unified_PIDs.json` | DC-DC Converter | `0x784` | 42 | 35 | 8 |
| `TDS20_IMMO_Unified_PIDs.json` | Immobiliser | `0x704` | 41 | 31 | 10 |
| `TDS20_MCU_Unified_PIDs.json` | Motor Control | `0x783` | 27 | 27 | 0 |

## Corrections

The databases contain a few authoring errors. The raw exports above are left
**exactly as Tata ships them**, so it stays clear what came from them and what came
from us. Corrections live in [`corrections.json`](corrections.json) and are applied
only to generated output — the CarScanner profiles and the tables in the docs.

Every scalar signal across all eight ECUs was audited. What was changed:

| DID | ECU | Database says | Corrected to | Why |
|-----|-----|---------------|--------------|-----|
| `$347D` `$347E` `$3480` | BMS | `W` | **`kW`** | the database lists `$347E` as both `W` and `kW`; at 0.1 resolution two bytes top out at 6553.5, so a 105 kW pack cannot be watts |
| `$35B0` `$35B1` | VECU | `C` | **`Ah`** | the BMS defines the identical signals at `$353E`/`$353F` as `Ah` — they are charge counters, not temperatures |

Units added where missing and unambiguous from the signal name: `$3421` and
`$3568` (`%`), `$3492` and `$34BF` (`V`), `$3497` and `$3498` (`°C`).

### A unit shown as `(X ?)`

Four signals look wrong but cannot be resolved from the files. They are **marked,
not altered** — a confident wrong value is worse than a visible doubt:

- **`$3428`** VECU charging current limit — one byte carrying a `40` offset, which
  is the coolant-temperature pattern. `$352F` is the same signal at 0.1 with no
  offset. **Prefer `$352F`**; the profiles already do.
- **`$3497` / `$3498`** BMS inlet/outlet temperature 2 — offset `50` where every
  other coolant temperature uses `40`. Read one against `$3410` on the car; if it
  is 10 °C high, the offset is 40.
- **`$3481`** BMS negative busbar — resolution 1 in mV where the positive busbar
  `$3482` is 0.1 V. The asymmetry is probably right, since the negative busbar sits
  near chassis potential. If it reads in the hundreds rather than near zero, the
  unit is wrong.

`corrections.json` carries the reasoning for each entry in machine-readable form.

## Schema

Each file is a JSON array of DID objects. The layout matches the `*_Unified_PIDs.json`
format TDS itself produces, so these drop into the same tooling:

```json
{
  "ParameterID": 17,
  "ServiceId": "0x22",
  "ServiceName": "ReadDataByIdentifier (0x22)",
  "DidHex": "0x3401",
  "ParameterName": "BMS_BattCurCurrent",
  "TotalByteLength": "2",
  "ReadPermission": "Y",
  "WritePermission": "N",
  "ValueParameters": [ ... ],
  "ByteParameters": [ ... ]
}
```

**`ValueParameters`** — scalar signals. Fields: `SubFunctionID`, `ParameterName`,
`BytePosition`, `ByteLength`, `Resolution`, `Offset`, `DivisionFactor`,
`AdditionFactor`, `Units`, `FormulaType`, `FormulaDescription`, `DecimalPrecision`,
`MinimumValue`, `MaximumValue`, `DataType`, `BitPosition`.

**`ByteParameters`** — enumerated and bit-packed signals. Fields: `SubFunctionID`,
`ParameterName`, `BytePosition`, `ByteLength`, `MaskingCondition`, `ResultByte`,
`ResultDescription`, `LanguageCode`. One row per possible value; a DID with several
distinct `ParameterName`s is carrying several signals in one byte, and
`MaskingCondition` is the bit mask for each.

## The conversion formula

Carried on every `ValueParameters` entry as `FormulaDescription`, and quoted
verbatim from TDS:

```
Value = (Raw * Resolution / DivisionFactor) + AdditionFactor + Offset
```

`Raw` is the big-endian integer assembled from `ByteLength` bytes starting at
`BytePosition` of the response (`BytePosition` 4 = the first payload byte after
`62 <hi> <lo>`). `DivisionFactor` and `AdditionFactor` are `1` and `0` throughout
these databases, so in practice it reduces to `Value = Raw × Resolution + Offset`.

Worked example — `$3401` pack current: `Resolution` `0.1`, `Offset` `-3200`, so
`Value = Raw × 0.1 − 3200`. At `Raw = 32000` that is 0 A, which matches the
independent `tata-ev-bms` reverse engineering of `(Raw − 32000) × 0.1`.

## Caveats

- **Scalings are supplier-specific.** These are the KPD/AIO pack values. Gotion,
  CESL and Kratos packs use different calibrations for identically-named signals —
  see the comparison table in the main reference. Do not mix them.
- **Some `Units` are wrong in the source.** `$347D`–`$3480` are declared `A` but are
  power in kW; the main reference documents the correction and the evidence.
- **Enum coverage is uneven.** `$347A BMS_OperMod` and `$3483 VCU_BMSModeReq` have no
  `ByteParameters` in any database, so their codes are undecoded.
- **`MinimumValue` / `MaximumValue` are raw bounds**, not engineering limits, and are
  sometimes just the datatype span.

## On TDS 21.0

TDS 21.0 was compared table-by-table against 20.0. For a Nexon EV nothing changed:

- **BMS, MCU, DCDC, OBC** — every table byte-identical. The files differ by md5 and
  size only because SQL Server Compact rewrites its page layout; the content is the
  same.
- **VECU** — 13 DIDs added (`$3673`, `$3675`–`$3680`, `$355E`, `$3615`), all
  flagged for a new **`NOVAMCE`** platform column and `BaseVariant` only. **None are
  flagged `K2` or `K3`**, so none apply to this car. They cover a "6in1" integrated
  powertrain unit — compressor and PTC interlocks, Xin1 KL30/KL15 rails, DC-DC
  precharge.
- **No existing DID row changed any field.**
- **DTCs** — the general VECU table grew 252 → 285, but `tblDTCMaster_K2` (195) and
  `tblDTCMaster_K3` (201) are unchanged. Those are the Nexon's.

The 13 new DIDs are included here because this folder is a faithful export of the
databases rather than a Nexon-only view. They are filtered out of the CarScanner
profiles, which is why those did not change.
