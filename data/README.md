# Machine-readable PID definitions

Full DID definitions for every diagnosable ECU on the car, exported from the Tata
TDS ECU databases — 1,168 DIDs across eight ECUs from TDS 20.0, plus five older
TDS 8.9S databases kept where they carry more detail.

| File | ECU | DIDs | with scaling | with enums |
|------|-----|-----:|-------------:|-----------:|
**TDS 20.0** — the current factory tool's databases, decrypted from the packed
install. Prefer these.

| File | ECU | Header | DIDs | scaled | enums |
|------|-----|--------|-----:|-------:|------:|
| `TDS20_VECU_Unified_PIDs.json` | Vehicle Control | `0x7E3` | 502 | 328 | 171 |
| `TDS20_BCM_Unified_PIDs.json` | Body Control | `0x701` | 232 | 89 | 148 |
| `TDS20_PEPS_Unified_PIDs.json` | Passive Entry/Start | `0x710` | 135 | 85 | 48 |
| `TDS20_BMS_Unified_PIDs.json` | Battery Management | `0x785` | 114 | 105 | 9 |
| `TDS20_OBC_Unified_PIDs.json` | On Board Charger | `0x786` | 75 | 46 | 29 |
| `TDS20_DCDC_Unified_PIDs.json` | DC-DC Converter | `0x784` | 42 | 35 | 8 |
| `TDS20_IMMO_Unified_PIDs.json` | Immobiliser | `0x704` | 41 | 31 | 10 |
| `TDS20_MCU_Unified_PIDs.json` | Motor Control | `0x783` | 27 | 27 | 0 |

**TDS 8.9S** — only the two that are *not* superseded are kept. The rest were
removed: their TDS 20.0 equivalents are newer, larger and correctly addressed.

| File | ECU | DIDs | Why kept |
|------|-----|-----:|----------|
| `KPD_EV_MCU_Unified_PIDs.json` | Motor Control Unit | 164 | TDS 20.0's MCU database has only 27 |
| `KPD_EV_BCS_Unified_PIDs.json` | Battery Cooling System | 40 | no TDS 20.0 equivalent exists |


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
