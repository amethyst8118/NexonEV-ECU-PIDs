# Nexon EV - Complete ECU Diagnostic PIDs Reference

Most ECUs use **500 kbps CAN, Extended Frame (29-bit)**. The **VECU** answers on **11-bit** addressing (`0x7E3` / `0x7EB`).

---

## 1. BMS (Battery Management System) - Gotion ECU

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request (Tester → ECU) | `0x1BDAF3F1` |
| Response (ECU → Tester) | `0x1BDAF1F3` |

### Identification PIDs (Service 0x22)
| DID | Name | Bytes |
|-----|------|-------|
| `$F18C` | Supplier ECU Serial Number | 4 |
| `$F190` | VIN | 17 |
| `$F191` | TML ECU Hardware Number | 15 |
| `$F192` | Supplier ECU Part Number | 4 |
| `$F19C` | Software Calibration ID | 16 |
| `$F188` | TML ECU Software Number | 15 |
| `$F198` | Programming Shop Code | 2 |
| `$F199` | Date of Last Programming | 4 |
| `$F1A0` | Vehicle Configuration Number | 15 |
| `$F1A1` | Parameter Part Number | 17 |
| `$F1A2` | Reprogramming Counter | 5 |
| `$F1A3` | Unique ID for Flashing/OTA | 20 |
| `$F1A4` | ECU ID | 20 |
| `$F1A6` | VCID | 20 |
| `$3000` | BMS Software Version | 4 |
| `$3002` | BMS Hardware Version | 4 |
| `$3063` | Supplier Inner Software Number | 4 |

### Live Data PIDs
| DID | Name | Bytes | Unit | Formula |
|-----|------|-------|------|---------|
| `$300D` | Battery Voltage | 2 | V | ×0.1 |
| `$300E` | Battery Current | 2 | A | ×0.1 - 600 |
| `$300F` | SOC | 2 | % | ×0.1 |
| `$3010` | SOH | 1 | % | direct |
| `$3011` | Max Cell Temperature | 1 | °C | -50 offset |
| `$3012` | Min Cell Temperature | 1 | °C | -50 offset |
| `$3013` | Fault Rank | 1 | - | direct |
| `$3014` | Inlet Temp 1 | 1 | °C | -50 offset |
| `$3015` | Outlet Temp 1 | 1 | °C | -50 offset |
| `$3016` | Insulation Value | 2 | kΩ | ×1 |
| `$3017` | Max Cell Voltage | 2 | V | ×0.01 |
| `$3018` | Min Cell Voltage | 2 | V | ×0.01 |
| `$3019` | Max Voltage Cell No | 1 | - | direct |
| `$301A` | Min Voltage Cell No | 1 | - | direct |
| `$301B` | V1 Sensing Voltage | 2 | V | ×0.01 |
| `$301C` | V3 Sensing Voltage | 2 | V | ×0.01 |
| `$301D` | Max Allow Peak Discharge Current | 2 | A | ×0.1 |
| `$301E` | Max Allow Peak Regen Current | 2 | A | ×0.1 |
| `$301F` | Max Allow Continuous Charge Current | 2 | A | ×0.1 |
| `$3020` | Max Allow Continuous Discharge Current | 2 | A | ×0.1 |
| `$3021` | Remaining Pack Energy | 1 | kWh | direct |
| `$3022` | Max Allow Peak Discharge Current (10s) | 2 | A | ×0.1 |
| `$3023` | Max Allow Peak Regen Current (10s) | 2 | A | ×0.1 |
| `$3024` | LV Power Supply | 2 | V | ×0.01 |
| `$3025` | SOC Calibration Counter | 2 | - | direct |
| `$302B` | Nominal Voltage | 2 | V | direct |
| `$302C` | Nominal Capacity | 1 | Ah | direct |
| `$302D` | Total Energy | 2 | kWh | direct |
| `$3030` | VCU Busbar Voltage | 2 | V | ×0.1 |
| `$3031` | RTC Seconds | 1 | s | direct |
| `$3032` | RTC Minutes | 1 | min | direct |
| `$3033` | RTC Hours | 1 | hr | direct |
| `$3034` | RTC Month | 1 | mon | direct |
| `$3035` | RTC Day | 1 | day | direct |
| `$3036` | RTC Year | 1 | yr | +1985 |
| `$3037` | RTC Local Minutes Offset | 1 | min | direct |
| `$3038` | RTC Local Hour Offset | 1 | hr | direct |
| `$303B` | VCU BMS No of Packs | 1 | - | direct |
| `$303C` | Max Temp Cell No | 1 | - | direct |
| `$303D` | Min Temp Cell No | 1 | - | direct |
| `$303E` | Total No of Cells | 1 | - | direct |
| `$3047` | Slave1 Power Supply | 1 | V | direct |
| `$302E` | Slave2 Power Supply | 1 | V | direct |
| `$3054` | Average Temperature | 1 | °C | -50 offset |
| `$3056` | PCB Temp Max | 1 | °C | direct |
| `$3058` | Cell SOC Min | 2 | % | ×0.1 |
| `$3059` | Cell SOC Max | 2 | % | ×0.1 |
| `$305B` | Cell Voltage Difference | 2 | mV | ×1 |
| `$305C` | Max Continuous Output Power | 2 | kW | ×0.1 |
| `$305D` | Max Peak Output Power | 2 | kW | ×0.1 |
| `$305E` | Max Peak Feedback Power | 2 | kW | ×0.1 |
| `$305F` | Max Continuous Feedback Power | 2 | kW | ×0.1 |
| `$3060` | Battery V2- | 2 | V | ×0.01 |
| `$3061` | Battery V3+ | 2 | V | ×0.1 |
| `$309C` | Accumulated kWh Consumed | 3 | kWh | ×1 |
| `$308B` | Aerosol Concentration | 2 | μg | ×1 |
| `$30D2` | Thermal Runaway Reason | 3 | - | direct |
| `$309F` | Crash Duty Cycle | 2 | % | ×0.1 |
| `$30D0` | Crash Frequency | 2 | Hz | ×0.1 |
| `$309E` | HVIL Duty Cycle | 2 | % | ×0.1 |
| `$30D1` | HVIL Frequency | 2 | Hz | ×0.1 |

### Status PIDs
| DID | Name | Values |
|-----|------|--------|
| `$3003` | Pack Status | - |
| `$3004` | HVIL Status | 0=Not OK, 1=OK |
| `$3005` | Pack Lockout Status | - |
| `$3006` | HV Positive Relay | 0=Open, 1=Close |
| `$3007` | HV Negative Relay | 0=Open, 1=Close |
| `$3008` | HV Pre-charge Relay | 0=Open, 1=Close |
| `$3009` | VCU BMS Enable | 0=Open, 1=Close |
| `$300A` | VCU BMS EPO | 0=Open, 1=Close |
| `$300B` | VCU BMS Main Contactor Close | 0=Open, 1=Close |
| `$300C` | VCU BMS Insulation Enable | 0=Open, 1=Close |
| `$3039` | E2P Status | - |
| `$303A` | Main Contactor Status | 0=No Transfer, 1=In Process, 2=Complete, 3=Error |
| `$303F` | Emergency Power Off | - |
| `$3040` | Charging Done | 0=No, 1=Yes |
| `$3041` | Emergency Charge Ack | - |
| `$3042` | SOC Calibration Flag | - |
| `$3043` | Balancing Enabled | - |
| `$3049` | VCU Slow Charging Flag | - |
| `$304B` | BMS Pack Ready | 0=No, 1=Yes |
| `$304C` | BMS Key On | 0=No, 1=Yes |
| `$304F` | Operating Mode | 0=Init, 1=Standby, 2=PreCharge, 3=HVActive, 4=EPO, 5=PreChargeFail, 6=Fault, 7=ReadySleep |
| `$3050` | Derate Flag | - |
| `$3051` | Cell Balancing Status | - |
| `$3052` | SOC No Full Charge Flag | - |
| `$3053` | VCU HVIL Detect | - |
| `$3055` | Insulation Status | - |
| `$3057` | Mandatory Slow Full Charging | - |
| `$305A` | Aerosol Com Fault | - |

### DTCs (Read: `19 02 FF`, Clear: `14 FF FF FF`)
| DTC | Description |
|-----|-------------|
| P3002-17 | Max cell over voltage 1st level |
| P3001-17 | Max cell over voltage 2nd level |
| P3000-17 | Max cell over voltage 3rd level |
| P3005-16 | Min cell under voltage 1st level |
| P3004-16 | Min cell under voltage 2nd level |
| P3003-16 | Min cell under voltage 3rd level |
| P3155-1C | Cell voltage difference 1st level |
| P3156-1C | Cell voltage difference 2nd level |
| P3060-17 | Battery over voltage 2nd level |
| P3019-17 | Battery over voltage 3rd level |
| P3061-16 | Battery under voltage 2nd level |
| P3020-16 | Battery under voltage 3rd level |
| P3107-98 | Battery temp high 1st level |
| P3108-98 | Battery temp high 2nd level |
| P3109-98 | Battery temp high 3rd level |
| P3110-21 | Battery under temp 1st level |
| P3111-21 | Battery under temp 2nd level |
| P3112-21 | Battery under temp 3rd level |
| P3113-22 | Cell temp diff over 1st level |
| P3114-22 | Cell temp diff over 2nd level |
| P3116-19 | Pack over discharge current 1st level |
| P3117-19 | Pack over discharge current 2nd level |
| P3118-19 | Pack over discharge current 3rd level |
| P3119-19 | Pack over regen current 1st level |
| P3120-19 | Pack over regen current 2nd level |
| P3121-19 | Pack over regen current 3rd level |
| P3125-19 | Battery over AC current 2nd level |
| P3124-19 | Battery over AC current 3rd level |
| P3126-1A | Battery insulation 1st level |
| P3127-1A | Battery insulation 2nd level |
| P3128-1A | Battery insulation 3rd level |
| P3129-98 | PCB over temp 2nd level |
| P3130-22 | SOC jump 1st level |
| P3076-21 | SOC low 1st level |
| P3131-22 | Cell SOC diff 1st level |
| P3050-21 | SOH low 1st level |
| P3132-16 | KL30 under 2nd level |
| P3133-17 | KL30 over 2nd level |
| P3049-16 | Pre-charge failure 3rd level |
| P3134-71 | Pre-charge HW invalid 3rd level |
| P3135-96 | Neg Contactor HW fault 3rd level |
| P3136-96 | Pos Contactor HW fault 3rd level |
| P3036-73 | Neg Contactor weld 3rd level |
| P3034-73 | Pos Contactor weld 3rd level |
| P3033-13 | Pos Contactor open 3rd level |
| P3141-13 | HVIL fault 2nd level |
| P3094-13 | HVIL fault 1st level |
| U1300-87 | BMS Slave Daisy Chain 3rd level |
| U1306-87 | BMS Slave Daisy Chain 2nd level |
| P3056-22 | Thermal Runaway 4th level |
| P3142-02 | Crash signal 4th level |
| P3089-96 | Temp sensor fault 2nd level |
| P3037-1C | Cell voltage sampling 3rd level |
| P3093-1C | Cell voltage sampling 2nd level |
| U1307-87 | BMS-VCU CAN comm fault |
| P3104-96 | BMS HW fault 3rd level |
| P3105-96 | BMS HW fault 2nd level |
| P3100-44 | E2P read/write fail 1st level |
| P3101-44 | Ext E2P read/write fail 1st level |
| U1309-87 | Aerosol Sensor comm 1st level |
| P3102-96 | Aerosol Sensor fault 1st level |
| P3163-96 | Shunt Sensor 2nd level |
| P3164-96 | Shunt Sensor 3rd level |
| U1310-88 | CAN0 bus off 2nd level |
| P3096-1C | Pack Voltage mismatch 2nd level |
| P3165-96 | Cell balancing HW fault |
| P3166-16 | KL15 IGN fault |
| P3057-41 | VCU request CRC failure |
| P3169-16 | KL30 under 3rd level |
| P3170-17 | KL30 over 3rd level |
| P3220-13 | Crash signal open 1st level |
| P3400-13 | KL30 LV lost 1st level |
| P3025-98 | Coolant High Temp 1st level |

---

## 1B. BMS — Nexon EV Max (KPD / K1AIO-K2AIO)

> The Nexon EV Max battery pack. Distinct from the Gotion BMS above: `34xx` DID
> block, target `0x96`. Extracted from `KPD_EV_BMS.inf` (TDS 8.9S) — all three DID
> tables (identifiers, value-type, byte-type) — and cross-verified against the
> community `tata-ev-bms` project (2023 Nexon EV Max).

### Bus / session
| Setting | Value |
|---------|-------|
| Request (Tester → ECU) | `0x1BDA96F1` |
| Response (ECU → Tester) | `0x1BDAF196` |
| Frame format | **29-bit extended** |
| Bit rate | 500 kbps |
| Physical bus | **Powertrain CAN — OBD pins 3 (H) / 11 (L)** |
| Session | `10 03` (extended) before reads |
| Service | `22` ReadDataByIdentifier |

> ⚠️ Not reachable from OBD pins 6/14 — the gateway does not route diagnostics
> to the BMS. Verified by live sweep: only the VECU (`0x7E3`) answers there.

### Key live values

The 19 worth logging. `raw` = the big-endian integer from the response payload.

| DID | Signal | B | Unit | Conversion |
|-----|--------|---|------|------------|
| `3400` | BMS_BattCurVoltage | 2 | V | `raw × 0.1` |
| `3401` | BMS_BattCurCurrent | 2 | A | `raw × 0.1 − 3200` |
| `3402` | BMS_SOC | 2 | % | `raw × 0.1` |
| `3403` | BMS_SOH | 2 | % | `raw × 0.1` |
| `3409` | BMS_MaxPresentTemp | 1 | °C | `raw − 40` |
| `340B` | BMS_MinPresentTemp | 1 | °C | `raw − 40` |
| `3410` | BMS_InletTemp | 1 | °C | `raw − 40` |
| `3411` | BMS_OutletTemp | 1 | °C | `raw − 40` |
| `3412` | BMS_BattPresentAverageTemp | 1 | °C | `raw − 40` |
| `3413` | BMS_InsulationResValue | 2 | kΩ | `raw` |
| `3415` | BMS_MaxCellVolt | 2 | mV | `raw` |
| `3417` | BMS_MinCellVolt | 2 | mV | `raw` |
| `3419` | BMS_MaxCellVoltNo | 1 | – | `raw` |
| `341A` | BMS_MinCellVoltNo | 1 | – | `raw` |
| `3492` | BMS_PowerSupply voltage | 2 | V | `raw × 0.001` |
| `34D5` | BMS_CellVolDiff | 2 | mV | `raw` |

### All scalar PIDs

| DID | Signal | B | Unit | Conversion | Range |
|-----|--------|---|------|------------|-------|
| `3400` | BMS_BattCurVoltage | 2 | V | `raw × 0.1` | 0 … 6553.5 |
| `3401` | BMS_BattCurCurrent | 2 | A | `raw × 0.1 − 3200` | -3200 … 3353.5 |
| `3402` | BMS_SOC | 2 | % | `raw × 0.1` | 0 … 6553.5 |
| `3403` | BMS_SOH | 2 | % | `raw × 0.1` | 0 … 6553.5 |
| `3406` | BMS_HeartbeatSignal | 1 | – | `raw` | 0 … 255 |
| `3409` | BMS_MaxPresentTemp | 1 | °C | `raw − 40` | -40 … 215 |
| `340A` | BMS_MaxTempProbeNo | 1 | – | `raw` | 0 … 255 |
| `340B` | BMS_MinPresentTemp | 1 | °C | `raw − 40` | -40 … 215 |
| `340C` | BMS_MinTempProbeNo | 1 | – | `raw` | 0 … 255 |
| `340D` | BMS_FltRank | 1 | – | `raw` | 0 … 255 |
| `3410` | BMS_InletTemp | 1 | °C | `raw − 40` | -40 … 215 |
| `3411` | BMS_OutletTemp | 1 | °C | `raw − 40` | -40 … 215 |
| `3412` | BMS_BattPresentAverageTemp | 1 | °C | `raw − 40` | -40 … 215 |
| `3413` | BMS_InsulationResValue | 2 | kΩ | `raw` | 0 … 65535 |
| `3415` | BMS_MaxCellVolt | 2 | mV | `raw` | 0 … 65535 |
| `3417` | BMS_MinCellVolt | 2 | mV | `raw` | 0 … 65535 |
| `3419` | BMS_MaxCellVoltNo | 1 | – | `raw` | 0 … 255 |
| `341A` | BMS_MinCellVoltNo | 1 | – | `raw` | 0 … 255 |
| `341B` | BMS_TotalTempProbeNumber | 1 | – | `raw` | 0 … 255 |
| `347A` | BMS_OperMod | 1 | – | `raw` | 0 … 255 |
| `347B` | BMS_MaxAllowContinusChrgCurr | 2 | A | `raw × 0.1` | 0 … 6553.5 |
| `347C` | BMS_MaxAllowContinusDisChrgCurr | 2 | A | `raw × 0.1` | 0 … 6553.5 |
| `347D` | BMS_AllowedMaxContinusOutPower ¹ | 2 | kW | `raw × 0.1` | 0 … 6553.5 |
| `347E` | BMS_AllowedMaxPeakOutPower ¹ | 2 | kW | `raw × 0.1` | 0 … 6553.5 |
| `347F` | BMS_AllowedMaxPeakFBPower ¹ | 2 | kW | `raw × 0.1` | 0 … 6553.5 |
| `3480` | BMS_AllowedMaxContinusFBPower ¹ | 2 | kW | `raw × 0.1` | 0 … 6553.5 |
| `3481` | VeDATM_U_NegBusbarVolt1 | 2 | V | `raw × 0.1` | 0 … 6553.5 |
| `3482` | VeDATM_U_PosBusbarVolt1 | 2 | V | `raw × 0.1` | 0 … 6553.5 |
| `3483` | VCU_BMSModeReq | 1 | – | `raw` | 0 … 255 |
| `3484` | MCU_Vdc | 2 | V | `raw` | 0 … 65535 |
| `3485` | BMS_RealTime | 6 | – | 6-byte packed (date/time) | – |
| `3492` | BMS_PowerSupply voltage | 2 | V | `raw × 0.001` | 0 … 65.535 |
| `34D5` | BMS_CellVolDiff | 2 | mV | `raw` | 0 … 65535 |

¹ DB declares the unit as `A`, but the signal names say *power*. Read as **kW**
  (0.1 kW/bit) and sanity-check against pack V × A on the car.

### State / enumerated PIDs

These decode to text, not numbers — they are why a plain scalar dump looks wrong.

**`3404` BMS_MaiRlyNClsd** (1 byte)

| Signal | Value | Meaning |
|--------|-------|---------|
| BMS_MaiRlyNClsd | `0` | open |
| BMS_MaiRlyNClsd | `1` | close |
| BMS_MaiRlyPClsd | `0` | open |
| BMS_MaiRlyPClsd | `1` | close |
| BMS_PreRlyClsd | `0` | open |
| BMS_PreRlyClsd | `1` | close |

> ⚠️ The DB lists all three relay signals on this one DID with mask `FF`.
> That is almost certainly an authoring slip — in the real frame they are
> bit 0 / bit 1 / bit 2 of the single byte. Verify on the car before trusting.

**`3405` BMS_InitState** (1 byte)

| Value | Meaning |
|-------|---------|
| `0` | initialing |
| `1` | init OK |

**`3493` BMS_SOC_CalibrationFlag** (1 byte)

| Value | Meaning |
|-------|---------|
| `0` | not reached |
| `1` | reached |

**`3494` BMS_SOCCalActFlag** (1 byte)

| Value | Meaning |
|-------|---------|
| `0` | Not Calibrated |
| `1` | SOC 100% calibration |
| `2` | SOC 99% calibration |
| `3` | SOC 95% calibration |
| `4` | SOC 0% calibration |

**`3414` BMS_Insulation_Enable** (1 byte)

| Value | Meaning |
|-------|---------|
| `0` | disable |
| `1` | enable |

**`341C` BMS_ChargingPortConnectionStatus** (1 byte)

| Value | Meaning |
|-------|---------|
| `0` | connect |
| `1` | disconnect |

> ⚠️ Note the polarity: **`0` = connected**, `1` = disconnected.

**`3479` BMS_CellBalanceStatus** (1 byte — packed bit flags)

| Mask | Bit | Signal | 0 | 1 |
|------|-----|--------|---|---|
| `0x01` | 0 | BMS_DerateFlag | disable | enable |
| `0x02` | 1 | BMS_CellBalanceStatus | disable | enable |
| `0x04` | 2 | HSC_BCM_LEAKAGE_ENA | disable | enable |
| `0x08` | 3 | VCU_Charging_Flag | disable | enable |
| `0x10` | 4 | VCU_HVILDetect | disable | enable |
| `0x20` | 5 | VCU_EqualizationTrigger | disable | enable |

> One read of `22 3479` gives you all six flags. Six separate signals were
> collapsed into this byte — earlier extracts reported only the DID name.

### Identification PIDs

| DID | Name | Bytes |
|-----|------|-------|
| `F18C` | Supplier ECU Serial Number | 8 |
| `F192` | Supplier ECU part number | 8 |
| `F191` | TML ECU hardware number | 15 |
| `F187` | TML Container Part Number ( Assembly No) | 15 |
| `F19C` | TML Software calibration identification Number | 16 |
| `F188` | TML ECU software number | 15 |
| `F198` | Reprogramming Counter | 2 |
| `F199` | Date of Last Programming in the format [ DD - MM - YYYY ] | 4 |
| `F190` | Vehicle Identification Number | 17 |
| `F197` | Variant Dataset Identification Number (Variant Coding) | 5 |
| `F1A0` | Vehicle configuration Number | 15 |
| `F1A1` | Parameter Part Number | 17 |
| `F1A2` | Programming Shop Code | 5 |
| `F1A3` | Unique ID for Flashing / OTA | 20 |
| `F1A4` | Reserved for Future Use | 20 |

`22 F197` returns the variant-coding string — use it to confirm you are talking
to the BMS and not another ECU.


## 2. VECU (Vehicle Control Unit) — Nexon EV (VECU_R15.2)

> **11-bit addressing** (not 29-bit): this ECU answers on the diagnostic CAN.
> Extracted from TDS 20.0 (VECU database) + scaling cross-checked against the
> MCU / OBC / DCDC / BMS databases. Verified live: this address self-identifies
> as `VECU_R15.2` via `22 F197`.

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request (Tester → ECU) | `0x7E3` (11-bit) |
| Response (ECU → Tester) | `0x7EB` (11-bit) |

*Enter extended session (`10 03`) before reads.*

### Live Data PIDs (Service 0x22)
Scaled values below are sourced from the MCU/OBC/DCDC/BMS databases for the same
signal; `direct` = raw integer (scaling not in a verified source — calibrate).
| DID | Name | Bytes | Unit | Formula |
|-----|------|-------|------|---------|
| `$3436` | Charging Gun Lock/Unlock Command | 1 | - | direct |
| `$3456` | VCU- Park Brake Sensor Value | 1 | - | direct |
| `$348B` | Vehicle Forward Speed Limit | 1 | - | direct |
| `$3499` | Fan Controller PWM Out freq | 2 | Hz | direct |
| `$349A` | Fan Controller PWM Out dutycycle | 1 | - | direct |
| `$349C` | Crash PWM input on time | 1 | - | direct |
| `$349E` | Crash PWM input freq | 1 | - | direct |
| `$349F` | Gun unlock fascia switch Status | 1 | - | direct |
| `$34A0` | Gun lock feedback Status | 1 | - | direct |
| `$34A1` | Gun unlock feedback Status | 1 | - | direct |
| `$34A2` | Fan controller feedback Status | 1 | - | direct |
| `$34A3` | Reverse Lamp command | 1 | - | direct |
| `$34A4` | Traction E drive Output Speed | 2 | RPM | -15000 offset |
| `$34A5` | Traction E drive Output Torque | 2 | Nm | -510 offset |
| `$34A6` | Traction E drive HV Current | 2 | A | ×0.5 - 510 |
| `$34A7` | Traction E drive HV Voltage | 2 | V | ×0.5 |
| `$34A8` | Traction E drive Requested State | 1 | - | direct |
| `$34A9` | Traction E drive Requested Torque | 2 | Nm | -510 offset |
| `$34AA` | Traction E drive Requested Speed | 2 | RPM | -15000 offset |
| `$34AB` | Traction E drive Requested Torque Direction | 1 | - | direct |
| `$34AC` | HV Battery Requested operating mode | 1 | - | direct |
| `$34AD` | HV Battery Busbar voltage | 2 | V | ×0.1 |
| `$34AE` | OBC Active Control | 1 | - | direct |
| `$34AF` | OBC Sleep Control | 1 | - | direct |
| `$34B0` | OBC Charging Status | 1 | - | direct |
| `$34B1` | OBC CP Status | 1 | - | direct |
| `$34B2` | OBC CC Status | 1 | - | direct |
| `$34B3` | OBC Cp Dutycycle | 1 | - | direct |
| `$34B4` | OBC Cp Frequency | 2 | - | direct |
| `$34B5` | HV Battery Fault Rank | 1 | - | direct |
| `$34B6` | MCU Fault Grade | 1 | - | direct |
| `$34B7` | OBC Fault  Status | 1 | - | direct |
| `$34B8` | DCDC System Status | 1 | - | direct |
| `$34B9` | AC request from FATC | 1 | - | direct |
| `$34BA` | Heating Power request from FATC | 1 | % | direct |
| `$34BB` | Cooling Power request from FATC | 1 | % | direct |
| `$34BC` | HV Battery  cell equalization command | 1 | - | direct |
| `$34BD` | Diagnostic Lamp Status | 1 | - | direct |
| `$34BE` | VCU Wakeup Mode | 1 | - | direct |
| `$34BF` | VECU Supply Voltage | 1 | - | direct |
| `$34C0` | Vacuum pressure value in brake booster | 1 | - | ×0.01 |
| `$34C1` | Status of Immo operation | 1 | - | direct |
| `$34C2` | VCU PP validity check status | 1 | - | direct |
| `$34C3` | VCU CP amplitude validity check status | 1 | - | direct |
| `$34C4` | VCU CP dutycycle validity check status | 1 | - | direct |
| `$34C5` | HV Critical Alert indication on IPC | 1 | - | direct |
| `$34C6` | Limphome Alert indication on IPC | 1 | - | direct |
| `$34C7` | OBC Wake Up request | 1 | - | direct |
| `$34C8` | Gun Lock/Unlock Feedback Status from BSW | 1 | - | direct |
| `$34C9` | Charging Shutdown Reason | 1 | - | direct |
| `$34CA` | Charging Shutdown Procedure Status | 1 | - | direct |
| `$34CB` | CCS Fast charging Gun Status | 1 | - | direct |
| `$34CD` | Slow Charging Diagnostic State | 1 | - | direct |
| `$34CE` | Fast Charging Sequence on vehicle | 1 | - | direct |
| `$34CF` | Slow Charging Sequence on vehicle | 1 | - | direct |
| `$34D0` | CCS Fast charging Relay Status | 1 | - | direct |
| `$34D1` | Remote Immobilizer Enable/ Disable | 1 | - | direct |
| `$34D2` | Remote Immobilizer Function Status | 1 | - | direct |
| `$34D3` | PEPS Auto learning feature request | 1 | - | direct |
| `$34D4` | PEPS Auto-learning feature status | 1 | - | direct |
| `$34DC` | AC Inlet power line Temp Sensor Count Value | 1 | - | direct |
| `$34DD` | Fast charging voltage sensor analog input value | 1 | - | direct |
| `$34DE` | Cruise control switch analog input count value | 1 | - | direct |
| `$34DF` | Eco & Sports mode switches analog input count value | 1 | - | direct |
| `$34E0` | Regen control switch analog input count | 1 | - | direct |
| `$34E1` | 3 in 1 unit DC-DC Output voltage | 2 | V | ×0.1 |
| `$34E2` | 3 in 1 unit DC-DC Input Current | 2 | A | ×0.1 |
| `$34E3` | 3 in 1 unit DC-DC Input voltage | 2 | V | ×0.1 |
| `$3545` | V2X Switch Status analog input | 1 | - | direct |
| `$354B` | EEPROM Value initialization through VECU | 1 | - | direct |
| `$355F` | Mandatory Slow Charging | 1 | - | direct |
| `$3560` | Thermal Runaway from BMS | 1 | - | direct |
| `$3561` | Battery HVIL Sense PWM dutycycle | 1 | - | direct |
| `$3562` | Battery HVIL Sense PWM Freq | 1 | - | direct |
| `$3563` | BMS Crash Signal | 1 | - | direct |
| `$3569` | Adaptive Cruise Control Engaged from ESP | 1 | - | direct |
| `$356A` | ACC Torque Requested from ESP | 2 | - | direct |
| `$356B` | ACC Torque Request Enable from ESP | 1 | - | direct |
| `$356D` | Park Brake State from VCU | 1 | - | direct |
| `$3570` | Adaptive Cruise Control System State to VECU | 1 | - | direct |
| `$3571` | ADAS Steering Switch State Ack to VECU | 1 | - | direct |
| `$35A1` | DTC Information from Hv Battery | 1 | - | direct |
| `$35A2` | HV Battery Insulation Low Fault | 1 | - | direct |
| `$35A3` | HV battery Cell Volt Sampling Fault | 1 | - | direct |
| `$35A4` | HV battery Pre charge Failure Fault | 1 | - | direct |
| `$35A5` | HV battery Power Supply low Alarm | 1 | - | direct |
| `$35A6` | HV battery Power Supply high Alarm | 1 | - | direct |
| `$35A7` | HV battery Cell Tempertaure DiffOver Alarm | 1 | - | direct |
| `$35A8` | EEPROM Value initialization through VECU | 1 | °C | -40 offset |
| `$35A9` | HV battery Temperature sensor 2 | 1 | °C | -40 offset |
| `$35AA` | HV battery Temperature sensor 3 | 1 | °C | -40 offset |
| `$35AB` | HV battery Temperature sensor 4 | 1 | °C | -40 offset |
| `$35AC` | HV battery Temperature sensor 5 | 1 | °C | -40 offset |
| `$35AD` | HV battery Temperature sensor 6 | 1 | °C | -40 offset |
| `$35AE` | HV battery Temperature sensor 7 | 1 | °C | -40 offset |
| `$35AF` | HV battery Temperature sensor 8 | 1 | °C | -40 offset |
| `$35B0` | HV battery Cummulative Charge Capacity | 2 | - | direct |
| `$35B1` | HV battery Cummulative Disharge Capacity | 2 | - | direct |
| `$35B2` | HV battery SmokeSensor Fail fault | 1 | - | direct |
| `$35B3` | HV battery fast charging +ve contactor weld detection | 1 | - | direct |
| `$35B4` | HV battery fast charging -ve contactor weld detection | 1 | - | direct |
| `$35B5` | 3in1 OBC operation mode | 1 | - | direct |
| `$35B6` | OBC Output maximum power | 2 | - | direct |
| `$35B7` | Compressor Inverter Temperature | 1 | - | -50 offset |
| `$35B8` | Compressor Speed | 1 | - | ×50 |
| `$35B9` | Compressor status | 1 | - | direct |
| `$35BA` | Battery HVIL Sense PWM dutycycle | 1 | V | ×2 |
| `$35BB` | Compressor Input current | 1 | - | direct |
| `$35BC` | APA System State | 1 | - | direct |
| `$35BD` | Primary motor HV Current | 2 | A | ×0.5 - 510 |
| `$35BE` | Primary Motor Output Speed | 2 | - | direct |
| `$35BF` | Secondary motor maximum regeneration torque | 1 | - | direct |
| `$35C0` | Primary motor maximum regeneration torque | 1 | - | direct |
| `$35C1` | Secondary motor torque increase request from ESP | 2 | - | ×0.1 - 500 |
| `$35C2` | secondary motor torque limit fast  request from ESP | 5 | - | ×0.1 - 500 |
| `$35C3` | TP2 Status from ESP | 1 | - | direct |
| `$35C4` | TP2 Active from ESP | 1 | - | direct |
| `$35C5` | APA Torque Reqeust enable from ESP | 1 | - | direct |
| `$35C6` | APA Interface from ESP | 1 | - | direct |
| `$35C7` | Driving Direction Request from ESP | 1 | - | direct |
| `$35C8` | VCU Minimum Maximum Mode from ESP | 1 | - | direct |
| `$35C9` | ABS  vehicle speed from ESP | 2 | - | direct |
| `$35CA` | Mode Detect signal from ESP | 1 | - | direct |
| `$35CB` | Mode Detect signal from ESP Status | 1 | - | direct |
| `$35CC` | Codriver PTC temp value from FATC to VECU | 1 | °C | -40 offset |
| `$35CD` | Codriver heating power value from FATC to VECU | 1 | - | direct |
| `$35CF` | Drive mode Display from VCU | 1 | - | direct |
| `$35D0` | Apa Interface from VCU to ESP | 1 | - | direct |
| `$35D1` | Electric Motor State from VCU | 1 | - | direct |
| `$35D2` | TP2  Interface from VCU to ESP | 1 | - | direct |
| `$35D3` | Codriver Heating Power feedback value from VCU | 1 | - | direct |
| `$35D4` | VCU AC Charge limit from VCU | 1 | - | direct |
| `$35D5` | VCU FC Charge Limit from VCU | 1 | - | direct |
| `$35D6` | Mode Selection switch State from VCU | 1 | - | direct |
| `$35D7` | Primary motor State | 1 | - | direct |
| `$35D8` | Primary motor Voltage HV | 2 | V | ×0.5 |
| `$35D9` | Primary motor Active Short Circuit Status | 1 | - | direct |
| `$35DA` | Secondary motor HV Current | 2 | A | ×0.5 - 510 |
| `$35DB` | Secondary motor Output Speed | 2 | - | direct |
| `$35DC` | Secondary motor Output Torque | 2 | - | -510 offset |
| `$35DD` | Secondary motor  State | 1 | - | direct |
| `$35DE` | Secondary motor Voltage HV | 2 | V | ×0.5 |
| `$35DF` | Secondary motor Faultgrade | 1 | - | direct |
| `$35E0` | Secondary motor 12V SuppyLV | 1 | - | direct |
| `$35E1` | Secondary motor Maximum Torque | 2 | - | direct |
| `$35E2` | Secondary motor Minimum Torque | 2 | - | direct |
| `$35E3` | Secondary motor ElectricMachineTemp | 1 | - | direct |
| `$35E4` | Secondary motor InverterTemperature | 1 | - | direct |
| `$35E5` | Dual PTC1 Contactor low side enable | 1 | - | direct |
| `$35E6` | Dual PTC2 Contactor Low side enable | 1 | - | direct |
| `$35E7` | Dual PTC2 Contactor High side enable | 1 | - | direct |
| `$35E8` | Dual PTC1 Contactor High side enable | 1 | - | direct |
| `$35E9` | Front Edrive Power supply Relay Enable | 1 | - | direct |
| `$35EA` | Front Edrive Ignition enable | 1 | - | direct |
| `$35EC` | Custom Terrain Mode acknowlegment from ESP | 1 | - | direct |
| `$35ED` | Custom Steering Mode acknowlegment from EPAS | 1 | - | direct |
| `$35EE` | Custom Mode HU request Type from HU  to VCU | 1 | - | direct |
| `$35EF` | Custom Drive mode  user request from HU to  VCU | 1 | - | direct |
| `$35F0` | Custom  terrain  Mode user request from HU to VCEU | 1 | - | direct |
| `$35F1` | Custom steering Mode user request from HU to VCEU | 1 | - | direct |
| `$35F2` | Custom Regen Mode user request from HU to  VCEU | 1 | - | direct |
| `$35F3` | Custom Mode combined User request from HU to VCU | 1 | - | direct |
| `$35F4` | Custom mode status request from VCU | 1 | - | direct |
| `$35F5` | Custom steering mode request from VCU | 1 | - | direct |
| `$35F6` | Custom terrain mode status request from VCU | 1 | - | direct |
| `$35F7` | Custom Mode combined Request from VCU to Partner ECU | 1 | - | direct |
| `$35F8` | Custom terrain mode current state from VCU | 1 | - | direct |
| `$35F9` | Custom Drive mode current  state from VCU | 1 | - | direct |
| `$35FA` | Custom steering mode current state from VCU | 1 | - | direct |
| `$35FB` | Custom Regen mode current state from VCU | 1 | - | direct |
| `$35FC` | Custom mode combined Current State from VCU | 1 | - | direct |
| `$35FD` | Custom Mode saved settings in VECU for drive and regen mode | 1 | - | direct |
| `$35FE` | Custom Mode saved settings in VECU for steering | 1 | - | direct |
| `$35FF` | Custom Mode saved settings in VECU for AWD | 1 | - | direct |
| `$3600` | Custom Mode saved settings in VECU for RWD | 1 | - | direct |
| `$3601` | VCU-HU Autolearning Status of Custom Mode | 1 | - | direct |
| `$3602` | Custom Mode Settings update State | 1 | - | direct |
| `$3603` | Custom Mode Execution State | 1 | - | direct |
| `$3604` | Mandatory Slow charging Distance | 1 | - | direct |
| `$4223` | Status of AES-SK SecretKey | 1 | - | direct |
| `$4224` | No of AES-SK Write LeftOut | 1 | - | direct |
| `$727F` | FOTA Variant coding 1 | 2 | - | direct |
| `$7280` | FOTA Variant coding 2 | 4 | - | direct |

---

## 3. DCDC (DC-DC Converter)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request | `0x1BDA8FF1` |
| Response | `0x1BDAF18F` |

### Key PIDs
| DID | Name | Bytes | Unit |
|-----|------|-------|------|
| `$130F` | DCDC Enable Command | 1 | - |
| `$1310` | DCDC Input Voltage | 2 | V |
| `$1311` | DCDC Input Current | 1 | A |
| `$1312` | DCDC Output Voltage | 2 | V |
| `$1313` | DCDC Output Current | 2 | A |
| `$1314` | DCDC Internal Temperature | 1 | °C |
| `$1317` | 24V Power Supply Voltage | 2 | V |
| `$1318` | KL15 Wake Up Voltage | 1 | V |
| `$1325` | DCDC Derating Mode | 1 | - |
| `$1326` | DCDC Error Flag | 1 | - |
| `$1329` | DCDC VCU Communication | 1 | - |
| `$F0F1` | DCDC Internal Status | 1 | - |

---

## 4. AC / HVAC (Climate Control)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request | `0x1BDA19F1` |
| Response | `0x1BDAF119` |

### Key PIDs
| DID | Name | Bytes | Unit |
|-----|------|-------|------|
| `$F031` | Left Compressor Speed | 2 | rpm |
| `$F032` | Right Compressor Speed | 2 | rpm |
| `$F033` | Left Compressor Voltage | 1 | V |
| `$F034` | Right Compressor Voltage | 1 | V |
| `$F035` | Left Compressor Current | 1 | A |
| `$F036` | Right Compressor Current | 1 | A |
| `$F037` | Blower Fan Speed Level | 1 | - |
| `$F038` | Blower Fan Voltage | 1 | V |
| `$F039` | Condenser Fan Voltage | 1 | V |
| `$F03A` | Condenser Fan Current | 1 | A |
| `$F03B` | Blower Fan Current | 1 | A |
| `$F03C` | AC Status | 1 | - |
| `$F03D` | DCDC Temp | 1 | °C |
| `$F03E` | DCDC Output Voltage | 1 | V |
| `$F03F` | DCDC Power | 1 | W |
| `$F040` | AC Total Consumption | 1 | kWh |
| `$F041` | Bus AC Fault Code | 1 | - |
| `$F042` | Left Compressor Fault Code | 1 | - |
| `$4043` | Right Compressor Fault Code | 1 | - |
| `$4044` | PTC1 Voltage | 2 | V |
| `$4045` | PTC2 Voltage | 2 | V |
| `$4046` | PTC1 Current | 1 | A |
| `$4047` | PTC2 Current | 1 | A |
| `$4048` | PTC1 Consumption | 1 | kWh |
| `$4049` | PTC2 Consumption | 1 | kWh |
| `$4051` | PTC1 Switch Status | 1 | - |
| `$4052` | PTC2 Switch Status | 1 | - |
| `$4053` | Left Four-way Valve Status | 1 | - |
| `$4054` | Right Four-way Valve Status | 1 | - |
| `$4055` | Condenser Fan Consumption | 1 | kWh |
| `$4056` | Blower Fan Consumption | 1 | kWh |
| `$4057` | Right Compressor Consumption | 1 | kWh |
| `$4058` | Left Compressor Consumption | 1 | kWh |

---

## 5. BCS (Battery Cooling System)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request (BCS1) | `0x1BDA82F1` |
| Response (BCS1) | `0x1BDAF182` |
| Request (BCS2) | `0x1BDA3AF1` |
| Response (BCS2) | `0x1BDAF13A` |

### Key PIDs
| DID | Name | Bytes | Unit |
|-----|------|-------|------|
| `$FD00` | Water Inlet Temperature | 1 | °C |
| `$FD01` | Water Outlet Temperature | 1 | °C |
| `$FD02` | Pressure Suction Side | 1 | bar |
| `$FD03` | Pressure Discharge Side | 1 | bar |
| `$FD04` | Coolant Availability Status | 1 | - |
| `$FD05` | LV Battery Voltage | 1 | V |
| `$FD06` | HV Battery Voltage | 2 | V |
| `$FD07` | Cooling Fan 1 Duty Cycle | 1 | % |
| `$FD08` | Cooling Fan 2 Duty Cycle | 1 | % |
| `$FD09` | Coolant Pump Duty Cycle | 1 | % |
| `$FD0A` | Compressor Speed Now | 1 | rpm |
| `$FD0B` | Compressor Voltage | 2 | V |
| `$FD0C` | Compressor Current | 2 | A |
| `$FD0D` | Compressor Motor Temp | 1 | °C |
| `$FD0E` | PTC BCS Status | 1 | - |
| `$FD0F` | PTC Output Temp | 1 | °C |
| `$FD10` | PTC High Voltage | 1 | V |
| `$FD11` | PTC Power | 1 | W |
| `$FDA0` | Set Temp Cooling Start | 1 | °C |
| `$FDA1` | Set Temp Cooling Off | 1 | °C |
| `$FDA2` | Set Temp Heater Start | 1 | °C |
| `$FDA3` | Set Temp Heater Off | 1 | °C |

---

## 6. AUX (Auxiliary Inverter / MCU)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request (AUX1) | `0x1BDA80F1` |
| Response (AUX1) | `0x1BDAF180` |
| Request (AUX2) | `0x1BDA81F1` |
| Response (AUX2) | `0x1BDAF181` |

### Key PIDs
| DID | Name | Bytes | Unit |
|-----|------|-------|------|
| `$1201` | Motor Speed (Measured) | 2 | rpm |
| `$1202` | Motor Temperature | 1 | °C |
| `$1203` | Junction Temperature | 2 | °C |
| `$1204` | Max Forward Speed | 2 | rpm |
| `$1205` | DC Link Voltage | 2 | V |
| `$1206` | Torque Measured | 2 | Nm |
| `$1207` | AC Output Voltage | 2 | V |
| `$1208` | AC Output Current | 2 | A |
| `$1209` | Inverter Status | 2 | - |
| `$1210` | Torque Limit Code | 2 | - |

---

## 7. TCP (Thermal Coolant Pump)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request | `0x1BDA90F1` |
| Response | `0x1BDAF190` |

### Key PIDs
| DID | Name | Bytes | Unit |
|-----|------|-------|------|
| `$0F60` | Water Pump Fault Status | 1 | - |
| `$0F61` | LV Power Supply Voltage | 1 | V |
| `$0F62` | Water Pump Motor Current | 1 | A |
| `$0F66` | EWP-B Pump Control Instruction | 1 | - |

---

## 8. EBS3 (Electronic Braking System)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request | `0x1BDA0BF1` |
| Response | `0x1BDAF10B` |

### Key PIDs
| DID | Name | Bytes | Unit |
|-----|------|-------|------|
| `$0201` | Wheel Speed Front Left | 2 | km/h |
| `$0202` | Wheel Speed Front Right | 2 | km/h |
| `$0203` | Wheel Speed Rear Left | 2 | km/h |
| `$0204` | Wheel Speed Rear Right | 2 | km/h |
| `$021B` | Front Axle Voltage | 2 | V |
| `$021C` | Terminal 30a Voltage | 2 | V |
| `$021D` | Terminal 30b Voltage | 2 | V |
| `$021E` | Terminal 15 Voltage | 2 | V |
| `$021F` | Brake Lining Front Left | 2 | % |
| `$0220` | Brake Lining Front Right | 2 | % |
| `$0221` | Brake Lining Rear Left | 2 | % |
| `$0222` | Brake Lining Rear Right | 2 | % |
| `$0229` | Actual Pressure Front Axle | 2 | bar |
| `$022B` | Actual Pressure Rear Left | 1 | bar |
| `$022C` | Actual Pressure Rear Right | 1 | bar |
| `$022F` | Brake Pedal Position | 2 | % |
| `$0230` | Desired Deceleration | 2 | m/s² |
| `$0236` | Tow Vehicle Speed | 2 | km/h |
| `$0238` | Steering Wheel Angle | 2 | deg |
| `$0239` | Yaw Rate | 2 | °/s |
| `$023A` | Lateral Acceleration | 2 | m/s² |

---

## 9. BMS Multi-Pack (CESL EV - 4 pack variant)

For vehicles with multiple battery packs (buses/commercial):

| Pack | Request CAN ID | Response CAN ID |
|------|---------------|-----------------|
| BMS1 | `0x1BDAF3F1` | `0x1BDAF1F3` |
| BMS2 | `0x1BDAF4F1` | `0x1BDAF1F4` |
| BMS3 | `0x1BDAF5F1` | `0x1BDAF1F5` |
| BMS4 | `0x1BDAF6F1` | `0x1BDAF1F6` |

---

## Quick Command Reference

| Action | Request |
|--------|---------|
| Start Extended Session | `10 03` |
| Read Data by ID | `22 [DID_H] [DID_L]` |
| Read DTCs | `19 02 FF` |
| Clear DTCs | `14 FF FF FF` |
| Tester Present | `3E 00` |
| Security Access Seed | `27 01` |
| Security Access Key | `27 02 [key]` |
| ECU Reset | `11 01` |
| Read Active Session | `22 F1 86` |

### All ECUs Common DID
| DID | Name |
|-----|------|
| `$F18C` | Supplier ECU Serial Number |
| `$F190` | VIN |
| `$F191` | ECU Hardware Number |
| `$F192` | Supplier Part Number |
| `$F197` | Variant Coding |
| `$F19C` | Software Calibration ID |
| `$F188` | ECU Software Number |
| `$F198` | Programming Shop Code |
| `$F199` | Date of Last Programming |
| `$F1A0` | Vehicle Config Number |
| `$F1A1` | Parameter Part Number |
| `$F1A2` | Reprogramming Counter |
| `$F1A3` | Unique ID Flashing/OTA |
| `$F1A4` | ECU ID |
| `$F1A6` | VCID |
| `$F186` | Active Diagnostic Session |
