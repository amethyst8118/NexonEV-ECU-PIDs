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

**TDS 8.9S** — the older generation. Kept because a few are *richer* than their
TDS 20.0 counterparts: the 8.9S MCU database has 164 DIDs against 27, and the BCS
has no TDS 20.0 equivalent at all.

| File | ECU | DIDs | scaled | enums |
|------|-----|-----:|-------:|------:|
| `KPD_EV_MCU_Unified_PIDs.json` | Motor Control Unit | 164 | 150 | 14 |
| `KPD_EV_BMS_Unified_PIDs.json` | BMS (29-bit variant) | 55 | 48 | 7 |
| `KPD_EV_BCS_Unified_PIDs.json` | Battery Cooling System | 40 | 25 | 15 |
| `KPD_EV_DCDC_Unified_PIDs.json` | DC-DC Converter | 38 | 38 | 0 |
| `KPD_EV_OBC_Unified_PIDs.json` | On Board Charger | 22 | 22 | 0 |

The two BMS files are **different database generations of the same ECU** and they
disagree on some bit-packed DIDs — `$3479`'s balancing bit is `0x04` on TDS 20.0
but `0x02` on 8.9S. Prefer the TDS 20.0 file; it is what the current tool ships
and it matches the `0x785` addressing. Note its `FormulaDescription` ends in
`- Offset`, not `+ Offset`: that database stores offset magnitudes to subtract
(temperature `40`, current `3200`) where 8.9S stored them signed.

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
