# Machine-readable PID definitions

Full DID definitions for five Nexon EV powertrain ECUs, exported from the Tata TDS
ECU databases (`TDS 8.9S`, `DB/KPD_EV_*.inf` — Access/Jet files despite the `.inf`
extension).

| File | ECU | DIDs | with scaling | with enums |
|------|-----|-----:|-------------:|-----------:|
| `KPD_EV_BMS_Unified_PIDs.json` | Battery Management System | 55 | 48 | 7 |
| `KPD_EV_MCU_Unified_PIDs.json` | Motor Control Unit | 164 | 150 | 14 |
| `KPD_EV_BCS_Unified_PIDs.json` | Battery Cooling System | 40 | 25 | 15 |
| `KPD_EV_DCDC_Unified_PIDs.json` | DC-DC Converter | 38 | 38 | 0 |
| `KPD_EV_OBC_Unified_PIDs.json` | On Board Charger | 22 | 22 | 0 |

These are the raw definitions. For addressing, bus wiring, corrections and the
caveats that matter in practice, read
[`../Nexon_EV_All_PIDs.md`](../Nexon_EV_All_PIDs.md) — several DB fields are wrong
or incomplete and are corrected there, not here.

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
