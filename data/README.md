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
