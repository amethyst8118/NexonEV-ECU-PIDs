# Nexon EV - Complete ECU Diagnostic PIDs Reference

Every diagnosable ECU on this car runs **500 kbps CAN with 11-bit addressing** and is
reachable from the standard OBD-II socket. Sections 0–3C are generated from the
**TDS 20.0** ECU databases, decrypted from the packed install; later sections are
older material kept for reference and marked as such.

- [`READING_DTCS.md`](READING_DTCS.md) — how to pull fault codes, with worked examples
- [`DECODE_TABLES.md`](DECODE_TABLES.md) — what every coded DID value means, 423 DIDs
- [`CELL_BALANCING.md`](CELL_BALANCING.md) — `$3479` in detail
- [`data/`](data/) — machine-readable DID definitions, 1,181 DIDs across eight ECUs
- [`carscanner/`](carscanner/) — importable CarScanner profiles, one folder per platform and one file per ECU ([zip](carscanner/CarScanner_Profiles.zip) — single files download as `.txt`)
- [`elm327-patch/`](elm327-patch/) — J2534 shim so TDS itself runs on an ELM327

---

## 0. ECU map and DTC reading

Every diagnosable ECU on this car answers on **11-bit addressing at 500 kbps**, and
the EV powertrain occupies a contiguous block. Addresses below are read straight
from each ECU's `DiagnocommSettings` table in the TDS 20.0 databases.

| ECU | Request | Response | Session | DIDs | DTCs |
|-----|---------|----------|---------|-----:|-----:|
| BCM — Body Control | `0x701` | `0x709` | `10 03` | 232 | 352 |
| IMMO — Immobiliser | `0x704` | `0x70C` | `10 03` | 41 | 25 |
| PEPS — Passive Entry/Start | `0x710` | `0x718` | `10 03` | 135 | 103 |
| **MCU** — Motor Control | `0x783` | `0x78B` | `10 03` | 27 | 89 |
| **DCDC** — DC-DC Converter | `0x784` | `0x78C` | `10 03` | 42 | 17 |
| **BMS** — Battery Management | `0x785` | `0x78D` | `10 03` | 114 | 256 |
| **OBC** — On Board Charger | `0x786` | `0x78E` | `10 01` | 75 | 31 |
| **VECU** — Vehicle Control | `0x7E3` | `0x7EB` | `10 01` | 515 | 285 |

The powertrain runs `783` MCU → `784` DCDC → `785` BMS → `786` OBC, response
always request + 8.

### Reading DTCs

Full guide with worked examples, code decoding and the status byte:
[`READING_DTCS.md`](READING_DTCS.md).

All eight use standard UDS `ReadDTCInformation`. From an ELM327:

```
ATSP6            # 11-bit, 500 kbps
ATSH785          # pick the ECU from the table above
ATCRA78D         # its response id
1003             # extended session (1001 for OBC and VECU)
1902FF           # report DTCs by status mask FF
```

| Purpose | Request | Notes |
|---------|---------|-------|
| Read DTCs | `19 02 FF` | status mask `FF` = every status bit |
| Read DTCs (BCM) | `19 02 09` | BCM uses mask `09` — confirmed + test-failed |
| Freeze frame | `19 04 <3-byte DTC> 01` | snapshot stored with the fault |
| Clear DTCs | `14 FF FF FF` | ⚠️ clears across all groups |

The reply is `59 02 <mask> <DTC hi> <mid> <status>` repeated per fault, and is
almost always multi-frame — send flow control (`30 00 00`) or let the adapter
handle it. Tata's codes are 3-byte: the `DTCMaster` tables in
[`data/`](data/) map them to text, e.g. BMS `P3069-1C` = static cell voltage
difference, first level.

`14 FF FF FF` erases stored faults and freeze frames. It does not fix anything, and
clearing an active fault only hides it until the next drive cycle.

---

## 1. BMS (Battery Management System) - Gotion ECU

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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

## 1B. BMS — Battery Management System

> Source: **TDS 20.0 `BMS_DB.sdf`**, decrypted from the packed install. This is
> the database the current factory tool ships.

### Bus

| Setting | Value |
|---------|-------|
| Request | **`0x785`** |
| Response | **`0x78D`** |
| Frame | 11-bit standard, 500 kbps |
| Session | `10 03` before reads |
| Padding | 8 bytes, `STMin` 10 |

Straight from the database's own `DiagnocommSettings` — `TesterAddress 0x785`,
`ECUAddress 0x78D`, `CanFrameFormat 1`.

### Scalar PIDs

`Value = Raw × Resolution − Offset`, big-endian, payload starts after `62 <hi> <lo>`.

| DID | Signal | B | Unit | Conversion |
|-----|--------|---|------|------------|
| `$3400` | BMS_BattCurVoltage | 2 | V | `raw × 0.1` |
| `$3401` | BMS_BattCurCurrent | 2 | A | `raw × 0.1 − 3200` |
| `$3402` | BMS_SOC | 2 | % | `raw × 0.1` |
| `$3403` | BMS_SOH | 2 | % | `raw × 0.1` |
| `$3406` | BMS Heartbeat Signal / Alive Counter | 1 | - | `raw` |
| `$3407` | BMS_MinTempBattPackSubSysNo | 1 | pos | `raw` |
| `$3408` | BMS_MaxTempBattPackSubSysNo | 1 | pos | `raw` |
| `$3409` | BMS_MaxPresentTemp | 1 | °C | `raw − 40` |
| `$340A` | BMS_MaxTempProbeNo | 1 | pos | `raw` |
| `$340B` | BMS_MinPresentTemp | 1 | °C | `raw − 40` |
| `$340C` | BMS_MinTempProbeNo | 1 | pos | `raw` |
| `$340D` | BMS_FLTRank | 1 | – | `raw` |
| `$3410` | BMS Battery Coolant Inlet Temperature | 1 | °C | `raw − 40` |
| `$3411` | BMS_OutletTemp | 1 | oC | `raw − 40` |
| `$3411` | BMS Battery Coolant Outlet Temperature | 1 | °C | `raw − 40` |
| `$3412` | BMS Battery Average Temperature | 1 | °C | `raw − 40` |
| `$3413` | BMS_InsulationResValue | 2 | KΩ | `raw` |
| `$3415` | BMS_MaxCellVolt | 2 | mV | `raw` |
| `$3416` | BMS_MaxCellVoltSubSysNo | 1 | mV | `raw` |
| `$3417` | BMS_MinCellVolt | 2 | mV | `raw` |
| `$3418` | BMS_MaxCellVoltSubSysNo | 1 | pos | `raw` |
| `$3419` | BMS Max Cell Voltage No. | 1 | Pos | `raw` |
| `$341A` | BMS Min Cell Voltage No. | 1 | pos | `raw` |
| `$347A` | BMS_OperMod | 1 | – | `raw` |
| `$347B` | BMS_MaxAllowContinusChrgCurr | 2 | A | `raw × 0.1` |
| `$347B` | BMS Max Allowed Charging Current | 2 | A | `raw × 0.1` |
| `$347C` | BMS_MaxAllowContinusDisChrgCurr | 2 | A | `raw × 0.1` |
| `$347D` | BMS_AllowedMaxContinusOutPower | 2 | W | `raw × 0.1` |
| `$347E` | BMS_AllowedMaxPeakOutPower | 2 | W | `raw × 0.1` |
| `$347E` | BMS Max Allowed Discharge Power | 2 | kW | `raw × 0.1` |
| `$347F` | BMS Max Allowed Regen Power | 2 | kW | `raw × 0.1` |
| `$3480` | BMS_AllowedMaxContinusFBPower | 2 | W | `raw × 0.1` |
| `$3481` | VeDATM_U_NegBusbarVolt1 | 2 | mV | `raw` |
| `$3482` | VeDATM_U_PosBusbarVolt1 | 2 | V | `raw × 0.1` |
| `$3483` | VCU_BMSModeReq | 1 | – | `raw` |
| `$3484` | VCU Busbar Voltage | 2 | V | `raw` |
| `$3485` | BMS_RealTime | 6 | – | `raw` |
| `$3490` | BatteryTV_V1 | 2 | – | `raw × 0.01` |
| `$3491` | BatteryTV_V5 | 2 | – | `raw × 0.1` |
| `$3492` | BMS LV Power Supply Voltage | 1 | – | `raw × 0.001` |
| `$3495` | BMS_MaxTempBoxNO | 1 | – | `raw` |
| `$3496` | BMS_MinTempBoxNo | 1 | – | `raw` |
| `$3497` | BMS_InletTemp2 | 1 | – | `raw − 50` |
| `$3498` | BMS_OutletTemp2 | 1 | – | `raw − 50` |
| `$34D5` | BMS_CellVolDiff | 2 | mV | `raw` |
| `$3527` | BMS_Crash_Signal | 1 | NA | `raw` |
| `$3528` | Fast Charging Positive relay side voltage | 2 | V | `raw × 0.01` |
| `$3529` | Battery Main Positive relay side voltage | 2 | V | `raw × 0.01` |
| `$352A` | Fast Charging Negative relay side voltage | 2 | V | `raw × 0.01` |
| `$352B` | Battery Main Negative relay side voltage | 2 | V | `raw × 0.01` |
| `$353B` | Thermal Runaway Reason | 1 | – | `raw` |
| `$353C` | BMS Fault Rolling Counter | 1 | – | `raw` |
| `$353D` | BMS SOC Calibration Flag | 1 | – | `raw` |
| `$353E` | Cummulative Charge Capacity | 4 | Ah | `raw` |
| `$353F` | Cummulative Discharge Capacity | 4 | Ah | `raw` |
| `$3540` | Aerosol Concentration Value | 2 | – | `raw` |
| `$3565` | BMS HVIL Hardwire Signal | 1 | – | `raw` |
| `$3566` | SOC Accumulation since last 100% Charge | 2 | – | `raw` |
| `$3567` | Mapped Ah Accumulation Value | 4 | – | `raw` |
| `$3568` | BMS Calculated SOC Reference | 2 | – | `raw × 0.1` |

### Coded PIDs

These return a code, not a number. Full value meanings are in
[`DECODE_TABLES.md`](DECODE_TABLES.md#bms--battery-management-system).

| DID | Signal | Signals in byte |
|-----|--------|-----------------|
| `$3404` | BMS Main Positive Relay Status | 3 |
| `$3405` | BMS Initialization State | 1 (plain enum) |
| `$340E` | BMS Operation Mode | 1 (plain enum) |
| `$340F` | BMS Derate Flag | 1 (plain enum) |
| `$3414` | BMS Insulation Enable/Disable Command | 1 (plain enum) |
| `$3479` | BMS Cell Balance Status Flag | 3 |
| `$3493` | VCU Flag | 3 |
| `$353A` | Smoke Sensor Logic | 1 (plain enum) |

**Cell balancing** has its own writeup — the bit moved between database
generations and the database's own compare values are inconsistent. See
[`CELL_BALANCING.md`](CELL_BALANCING.md).

### Faults

256 codes in `DTCMaster`, plus pack-specific `DTCMaster_K1AIO` / `_K2AIO` (72
each) matching the `BMS_K1AIO_DTC.pdf` service documents inside TDS 20.0. Read
with `19 02 FF` — see [`READING_DTCS.md`](READING_DTCS.md).

### Published thresholds

| Quantity | DIDs | Threshold | DTC |
|----------|------|-----------|-----|
| Pack temperature spread | `$3409` − `$340B` | > 6 °C after a 6-hour rest = replace pack | `P1220-22` |
| LV supply high | `$3492` | > 32 V | `P1233-17` |
| LV supply low | `$3492` | < 9 V; will not wake below 9 V | `P1234-16` |

Cell-voltage difference splits into static (`P3069-1C`, at rest) and dynamic
(`P1208-1C`, under load) — read `$34D5` with the car stopped.



## 2. VECU — Vehicle Control Unit

> Source: **TDS 20.0 `DB_VECU_DiagnosticsDB.mdb`** — 502 DIDs. Supersedes the 183 carved out of process memory in earlier revisions of this file, which were a partial copy with unreliable scaling.

| Setting | Value |
|---------|-------|
| Request | **`0x7E3`** |
| Response | **`0x7EB`** |
| Frame | 11-bit standard, 500 kbps |
| Session | `10 01` before reads |

### Scalar PIDs (298)

| DID | Signal | B | Unit | Conversion |
|-----|--------|---|------|------------|
| `$125B` | AES_SK Secret Key | 16 | - | `raw` |
| `$341D` | HV Battery Current | 2 | A | `raw × 0.1 − 600` |
| `$341E` | HV Battery Voltage | 2 | V | `raw × 0.1` |
| `$341F` | HV Battery Negative Contactor State | 1 | – | `raw` |
| `$3420` | HV Battery Positive Contactor State | 1 | – | `raw` |
| `$3421` | HV Battery SOC | 2 | – | `raw × 0.1` |
| `$3422` | HV Battery State of Health | 1 | % | `raw` |
| `$3423` | Battery Cell Temperature Max | 1 | °C | `raw − 50` |
| `$3424` | Battery Cell Temperature Min | 1 | °C | `raw − 50` |
| `$3425` | Compressor_Speed_rpm | 2 | rpm | `raw` |
| `$3426` | PDU +Ve Bus bar temperature | 1 | DegC | `raw − 40` |
| `$3427` | PDU -Ve Bus bar temperature | 1 | DegC | `raw − 40` |
| `$3428` | Charing current limit requested by VCU (Slow + Fast) | 1 | A | `raw − 40` |
| `$3429` | HV DC-DC converter input Voltage | 2 | V | `raw × 0.1` |
| `$342A` | LV DC-DC converter Output Voltage | 1 | V | `raw × 0.1` |
| `$342B` | Comp_HV_DC Bus Voltage | 1 | V | `raw × 2` |
| `$342C` | Comp_HV_DC Bus Current | 1 | A | `raw × 0.1` |
| `$342D` | HV Isolation Level | 2 | kΩ | `raw` |
| `$342E` | Accelerator pedal sensor 1 position | 1 | V | `raw × 0.01953125` |
| `$342F` | Accelerator pedal sensor 2 position | 1 | V | `raw × 0.01953125` |
| `$3430` | Accelerator pedal position | 1 | V | `raw × 0.02` |
| `$3431` | Brake pedal sensor 1 position | 1 | V | `raw × 0.01953125` |
| `$3432` | Brake pedal sensor 2 position | 1 | V | `raw × 0.01953125` |
| `$3433` | Brake pedal position | 1 | V | `raw × 0.02` |
| `$3436` | Charging Gun H bridge Parameters | 1 | – | `raw` |
| `$3437` | Battery cooling pump PWM Dutycycle | 1 | % | `raw` |
| `$3438` | Battery cooling pump PWM Freq | 1 | Hz | `raw` |
| `$3439` | Traction cooling pump PWM Dutycycle | 1 | % | `raw` |
| `$343A` | Traction cooling pump PWM Freq | 1 | Hz | `raw` |
| `$343B` | HV DC-DC converter input Current | 1 | A | `raw × 0.1` |
| `$343C` | HV DC-DC converter output current | 2 | A | `raw × 0.1` |
| `$343D` | Vehicle Ignition Status | 1 | – | `raw` |
| `$343E` | Crank Switch Status | 1 | – | `raw` |
| `$343F` | Park Brake Status | 1 | – | `raw` |
| `$3440` | Charger ON Sense Status | 1 | – | `raw` |
| `$3441` | Vehicle inlet Gun Lock feedback Value | 1 | – | `raw × 0.01953125` |
| `$3442` | PDU Cover Switch Status | 1 | – | `raw` |
| `$3443` | Brake pedal switch Status | 1 | – | `raw` |
| `$3444` | BMS Keep alive relay Enable Status | 1 | – | `raw` |
| `$3445` | AC linear Pressure Sensor | 1 | – | `raw × 0.01953125` |
| `$3446` | E-drive supply relay Enable Status | 1 | – | `raw` |
| `$3447` | Battery Key ON Supply relay Enable Status | 1 | – | `raw` |
| `$3448` | E-drive Ignition Enable Status | 1 | – | `raw` |
| `$3449` | Brake Vacuum Pump Relay Enable | 1 | – | `raw` |
| `$344B` | Charging Inlet Lock and unlock Command | 1 | – | `raw` |
| `$344C` | Cabin cooling solenoid valve On Cmd | 1 | – | `raw` |
| `$344D` | Battery cooling solenoid valve On Cmd | 1 | – | `raw` |
| `$344E` | Traction MCU Contactor On Cmd | 1 | – | `raw` |
| `$344F` | PTC Contactor ON Cmd | 1 | – | `raw` |
| `$3450` | DC Charging  +ve Coil Contactor ON Cmd | 1 | – | `raw` |
| `$3451` | DC Charging  -ve Coil Contactor ON Cmd | 1 | – | `raw` |
| `$3452` | Traction Cooling Fan fast Enable Command | 1 | – | `raw` |
| `$3453` | Traction Cooling Fan Slow Enable Command | 1 | – | `raw` |
| `$3454` | HV Battery Equalization Trigger Status (Cell Balancing) | 1 | – | `raw` |
| `$3455` | DCDC Hardware Wakeup relay Enable Cmd | 1 | – | `raw` |
| `$3456` | VCU- Park Brake Sensor Value | 1 | – | `raw` |
| `$3456` | Cooling Fan relay Enable Cmd | 1 | – | `raw` |
| `$3456` | VCU- Park Brake Sensor Value | 1 | – | `raw` |
| `$3456` | VCU- Park Brake Sensor Value | 1 | – | `raw` |
| `$345D` | Maximum reverse vehicle speed | 1 | km/h | `raw` |
| `$345E` | Vehicle HVIL Sense PWM dutycycle | 1 | % | `raw` |
| `$345F` | Vehicle HVIL Sense PWM Freq | 1 | Hz | `raw` |
| `$3460` | PDU HVIL Sense PWM dutycycle | 1 | % | `raw` |
| `$3461` | PDU HVIL Sense Value Freq | 1 | Hz | `raw` |
| `$3462` | Vehicle Speed | 2 | m/s | `raw × 0.03125` |
| `$3463` | Gear Selector PRND Switch | 1 | – | `raw` |
| `$3464` | Park brake dutycycle PWM | 1 | % | `raw` |
| `$3465` | Park brake frequency PWM | 1 | Hz | `raw` |
| `$3468` | Brake vacuum pressure sense Value | 1 | Kpa | `raw − 120` |
| `$3469` | Vehicle inlet PP signal | 1 | – | `raw × 0.01953125` |
| `$346A` | Vehicle inlet CP signal PWM Amplitude | 1 | V | `raw × 0.01953125` |
| `$346B` | Vehicle inlet CP signal PWM DutyCycle | 2 | % | `raw × 0.01` |
| `$346C` | Vehicle inlet CP signal PWM Freq | 2 | Hz | `raw` |
| `$346D` | CP S2 switch Enable | 1 | – | `raw` |
| `$346E` | CP S2 switch Enable Status | 1 | – | `raw` |
| `$346F` | Telematics ECU Wakeup | 1 | – | `raw` |
| `$3470` | Vehicle inlet Temp +ve sensor value | 1 | DegC | `raw − 40` |
| `$3471` | Vehicle inlet Temp -ve sensor value | 1 | DegC | `raw − 40` |
| `$3472` | Motor winding temperature value | 1 | DegC | `raw − 40` |
| `$3473` | Inverter Temperature Value | 1 | DegC | `raw − 40` |
| `$3474` | DC_DC Convererter_Temperature | 1 | DegC | `raw − 40` |
| `$3475` | Compressor Temperature | 1 | DegC | `raw − 40` |
| `$3476` | OBC Temperature | 1 | DegC | `raw − 40` |
| `$3477` | Battery Cell Voltage Max | 2 | V | `raw × 0.01` |
| `$3478` | Battery Cell Voltage Min | 2 | V | `raw × 0.01` |
| `$348A` | Valet Mode Speed Configuration | 1 | – | `raw` |
| `$348B` | Vehicle Forward Speed Limit | 1 | – | `raw` |
| `$348B` | Vehicle Forward Speed Limit | 1 | – | `raw` |
| `$348B` | Vehicle Forward Speed Limit | 1 | – | `raw` |
| `$348C` | MCU Contactor Weld Status | 1 | – | `raw` |
| `$348C` | Powertrain Config State 2 | 1 | – | `raw` |
| `$3499` | Fan Controller PWM Out freq | 2 | Hz | `raw` |
| `$3499` | Fan Controller PWM Out freq | 2 | Hz | `raw` |
| `$349A` | Fan Controller PWM Out dutycycle | 1 | % | `raw` |
| `$349A` | Fan Controller PWM Out dutycycle | 1 | % | `raw` |
| `$349B` | Crash PWM input dutycycle | 1 | % | `raw` |
| `$349C` | Crash PWM input on time | 2 | – | `raw` |
| `$349D` | Crash PWM input off time | 2 | – | `raw` |
| `$349E` | Crash PWM input freq | 1 | Hz | `raw` |
| `$349F` | Gun unlock fascia switch Status | 1 | – | `raw` |
| `$349F` | Gun unlock fascia switch Status | 1 | – | `raw` |
| `$34A0` | Gun lock feedback Status | 1 | – | `raw` |
| `$34A0` | Gun lock feedback Status | 1 | – | `raw` |
| `$34A1` | Gun unlock feedback Status | 1 | – | `raw` |
| `$34A1` | Gun unlock feedback Status | 1 | – | `raw` |
| `$34A2` | Fan controller feedback Status | 1 | – | `raw × 0.01953125` |
| `$34A2` | Fan controller feedback Status | 1 | – | `raw × 0.01953125` |
| `$34A3` | Reverse Lamp command | 1 | – | `raw` |
| `$34A3` | Reverse Lamp Command | 1 | – | `raw` |
| `$34A4` | Traction E drive Output Speed | 2 | rpm | `raw − 17000` |
| `$34A4` | Traction E drive Output Speed | 2 | rpm | `raw − 17000` |
| `$34A5` | Traction E drive Output Torque | 2 | Nm | `raw − 510` |
| `$34A5` | Traction E drive Output Torque | 2 | Nm | `raw − 510` |
| `$34A6` | Traction E drive HV Current | 2 | A | `raw × 0.5 − 510` |
| `$34A6` | Traction E drive HV Current | 2 | A | `raw × 0.5 − 510` |
| `$34A7` | Traction E drive HV Voltage | 2 | V | `raw × 0.5` |
| `$34A7` | Traction E drive HV Voltage | 2 | V | `raw × 0.5` |
| `$34A9` | Traction E drive Requested Torque | 2 | Nm | `raw − 510` |
| `$34A9` | Traction E drive Requested Torque | 2 | Nm | `raw − 510` |
| `$34AA` | Traction E drive Requested Speed | 2 | rpm | `raw − 17000` |
| `$34AA` | Traction E drive Requested Speed | 2 | rpm | `raw − 17000` |
| `$34AB` | Traction E drive Requested Torque Direction | 1 | – | `raw` |
| `$34AB` | Traction E drive Requested Torque Direction | 1 | – | `raw` |
| `$34AD` | HV Battery Busbar voltage | 1 | V | `raw` |
| `$34AD` | HV Battery Busbar voltage | 2 | V | `raw` |
| `$34AE` | OBC Active Control | 1 | – | `raw` |
| `$34AE` | OBC Active Control | 1 | – | `raw` |
| `$34AF` | OBC Sleep Control | 1 | – | `raw` |
| `$34AF` | OBC Sleep Control | 1 | – | `raw` |
| `$34B0` | OBC Charging Status | 1 | – | `raw` |
| `$34B0` | OBC Charging Status | 1 | – | `raw` |
| `$34B1` | OBC CP Status | 1 | – | `raw` |
| `$34B1` | OBC CP Status | 1 | – | `raw` |
| `$34B2` | OBC CC Status | 1 | – | `raw` |
| `$34B2` | OBC CC Status | 1 | – | `raw` |
| `$34B3` | OBC Cp Dutycycle | 1 | % | `raw` |
| `$34B3` | OBC Cp Dutycycle | 1 | % | `raw` |
| `$34B4` | OBC Cp Frequency | 2 | Hz | `raw` |
| `$34B4` | OBC Cp Frequency | 2 | Hz | `raw` |
| `$34BA` | Heating Power request from FATC | 1 | % | `raw` |
| `$34BA` | Heating Power request from FATC | 1 | % | `raw` |
| `$34BB` | Cooling Power request from FATC | 1 | % | `raw` |
| `$34BB` | Cooling Power request from FATC | 1 | % | `raw` |
| `$34BD` | Diagnostic Lamp Status | 1 | – | `raw` |
| `$34BD` | Diagnostic Lamp Status | 1 | – | `raw` |
| `$34BE` | VCU Wakeup Mode | 1 | – | `raw` |
| `$34BE` | VCU Wakeup Mode | 1 | – | `raw` |
| `$34BF` | VECU Supply Voltage | 1 | – | `raw × 0.1` |
| `$34BF` | VECU Supply Voltage | 1 | – | `raw × 0.1` |
| `$34C0` | Vacuum pressure value in brake booster | 1 | – | `raw × 0.01` |
| `$34C0` | Vacuum pressure value in brake booster | 1 | – | `raw × 0.01` |
| `$34C1` | Status of Immo operation | 1 | – | `raw` |
| `$34C1` | Status of Immo operation | 1 | – | `raw` |
| `$34C2` | VCU PP validity check status | 1 | – | `raw` |
| `$34C2` | VCU PP validity check status | 1 | – | `raw` |
| `$34C3` | VCU CP amplitude validity check status | 1 | – | `raw` |
| `$34C3` | VCU CP amplitude validity check status | 1 | – | `raw` |
| `$34C4` | VCU CP dutycycle validity check status | 1 | – | `raw` |
| `$34C4` | VCU CP dutycycle validity check status | 1 | – | `raw` |
| `$34C7` | OBC Wake Up request | 1 | – | `raw` |
| `$34C7` | OBC Wake Up request | 1 | – | `raw` |
| `$34CA` | Charging Shutdown Procedure Status | 1 | – | `raw` |
| `$34CB` | CCS Fast charging Gun Status | 1 | – | `raw` |
| `$34CC` | Fast Charging Diagnostic State | 1 | – | `raw` |
| `$34CD` | Slow Charging Diagnostic State | 1 | – | `raw` |
| `$34CF` | Slow Charging Sequence on vehicle | 1 | – | `raw` |
| `$34D7` | DC Charging  -ve Coil Contactor High side  ON Cmd | 1 | – | `raw` |
| `$34D8` | Traction MCU Contactor  High side On Cmd | 1 | – | `raw` |
| `$34D9` | DC Charging  +ve Coil Contactor High side  ON Cmd | 1 | – | `raw` |
| `$34DA` | PTC Contactor High side ON Cmd | 1 | – | `raw` |
| `$34DB` | AC Inlet Neutral line Temp Sensor Count Value | 1 | – | `raw` |
| `$34DC` | AC Inlet power line Temp Sensor Count Value | 1 | – | `raw` |
| `$34DD` | Fast charging voltage sensor analog input value | 1 | – | `raw` |
| `$34DE` | Cruise control switch analog input count value | 1 | – | `raw` |
| `$34DF` | Eco & Sports mode switches analog input count value | 1 | – | `raw` |
| `$34E0` | Regen control switch analog input count | 1 | – | `raw` |
| `$34E1` | 3 in 1 unit DC-DC Output voltage | 1 | V | `raw × 0.1` |
| `$34E2` | 3 in 1 unit DC-DC Input Current | 1 | A | `raw × 0.1` |
| `$34E3` | 3 in 1 unit DC-DC Input voltage | 2 | V | `raw × 0.02` |
| `$34E4` | 3 in 1 unit DC-DC Output Current | 2 | A | `raw × 0.01` |
| `$34E7` | 3 in 1 unit  DC-DC temperature | 1 | °C | `raw − 48` |
| `$34E8` | 3 in 1 unit OBC DC Voltage | 2 | V | `raw × 0.02` |
| `$34E9` | 3 in 1 unit OBC DC Current | 2 | A | `raw × 0.02` |
| `$34EB` | 3 in 1 unit OBC AC Current | 1 | A | `raw` |
| `$34EC` | 3 in 1 unit OBC AC Voltage | 2 | V | `raw × 0.02` |
| `$34EE` | VCU  to 3 in 1 unit DC-DC Output Voltage command | 1 | V | `raw × 0.1` |
| `$34F0` | VCU  to 3 in 1 unit Charging CP Voltage | 1 | V | `raw × 0.1` |
| `$34F1` | VCU  to 3 in 1 unit Charging CP Duty cycle | 1 | – | `raw` |
| `$34F2` | VCU  to 3 in 1 unit Charging CP Frequency | 2 | – | `raw` |
| `$34F4` | VCU  to 3 in 1 unit  OBC DC current command | 2 | A | `raw × 0.02` |
| `$34F5` | AC Inlet temperature sense value on neutral line | 1 | °C | `raw − 40` |
| `$34F6` | AC Inlet temperature sense value on positive line | 1 | °C | `raw − 40` |
| `$34F7` | Brake Pedal position from ESP | 1 | % | `raw × 0.5` |
| `$34F8` | Traget recuperation torque from ESP | 2 | – | `raw × 0.1` |
| `$34FD` | Brake Pedal sensor 2 value from ESP | 2 | volts | `raw × 0.01` |
| `$34FF` | Brake Pedal sensor 1 value from ESP | 2 | volts | `raw × 0.01` |
| `$3500` | Target Motor Torque Increase request from ESP | 2 | – | `raw × 0.1 − 500` |
| `$3501` | Target Motor torque limit fast request from ESP | 2 | – | `raw × 0.1 − 500` |
| `$350A` | PTC Temperature sensor value from FATC | 1 | °C | `raw − 40` |
| `$3515` | Desired Cruise Speed from VCU | 1 | – | `raw` |
| `$3516` | Max available Regeneration torque | 2 | Nm | `raw × 0.25 − 2048` |
| `$3517` | Driver Requested torque | 2 | Nm | `raw × 0.25 − 2048` |
| `$3519` | Recuperation Actual torque from VCU | 2 | Nm | `raw × 0.25 − 2048` |
| `$351F` | CCS Vehicle inlet voltage measured by ASW | 2 | V | `raw × 0.1` |
| `$3520` | CCS Charging current request to station | 2 | A | `raw × 0.03125` |
| `$3521` | CCS Charging voltage request to station | 2 | V | `raw × 0.1` |
| `$3522` | CCS Charging current measured from station | 2 | A | `raw × 0.03125` |
| `$3523` | CCS Charging voltage measured from station | 2 | V | `raw × 0.1` |
| `$352F` | Charging current limit requested by VCU (Slow + Fast) | 2 | A | `raw × 0.1` |
| `$3536` | FC -ve contactor line Voltage measured at EVSE side from BMS | 2 | V | `raw × 0.2` |
| `$3537` | HV battery Main +ve contactor Voltage measured at load side from BMS | 2 | V | `raw × 0.2` |
| `$3538` | FC +ve contactor line Voltage measured at EVSE side from BMS | 2 | V | `raw × 0.2` |
| `$3539` | HV battery Main -ve contactor Voltage measured at load side from BMS | 2 | V | `raw × 0.2` |
| `$3541` | Sports Switch Status | 1 | – | `raw` |
| `$3542` | Brake Complement Switch Status | 1 | – | `raw` |
| `$3543` | Eco Switch Status | 1 | – | `raw` |
| `$3544` | Cruise Control Switch Analog Input Count Value - SW 2 | 2 | – | `raw` |
| `$3545` | V2X Switch Status Analog Input | 1 | – | `raw` |
| `$3545` | V2X Switch Status analog input | 1 | % | `raw × 0.02` |
| `$3548` | VCU Wakeup Request To MSS | 1 | – | `raw` |
| `$354A` | 3in1 Bidirectional OBC Wakeup From VCU | 1 | – | `raw` |
| `$354B` | EEPROM Value initialization through VECU | 1 | – | `raw` |
| `$355F` | Mandatory Slow Charging | 1 | – | `raw` |
| `$3560` | Thermal Runaway from BMS | 1 | – | `raw` |
| `$3561` | Battery HVIL Sense PWM dutycycle | 1 | % | `raw` |
| `$3562` | Battery HVIL Sense PWM Freq | 1 | HZ | `raw` |
| `$3563` | BMS Crash Signal | 1 | – | `raw` |
| `$3564` | Mandatory Slow charging Distance | 2 | – | `raw` |
| `$356A` | ACC Torque Requested from ESP | 1 | – | `raw` |
| `$358F` | CAN frames enable/disable of VECU | 1 | – | `raw` |
| `$3590` | DTE EEPROM Write through VECU | 2 | – | `raw` |
| `$3597` | MCU Max Torque Info | 2 | Nm | `raw − 510` |
| `$3598` | MCU Min Torque Info | 2 | Nm | `raw − 510` |
| `$3599` | Tin1 PDU KL30 Voltage Value | 1 | V | `raw × 0.1` |
| `$359B` | MCU Idc Max Info | 2 | A | `raw × 0.5` |
| `$359C` | MCU Idc Min Info | 2 | A | `raw × 0.5 − 510` |
| `$35A1` | DTC Information from Hv Battery | 1 | – | `raw` |
| `$35A2` | HV Battery Insulation Low Fault | 1 | – | `raw` |
| `$35A3` | HV battery Cell Volt Sampling Fault | 1 | – | `raw` |
| `$35A4` | HV battery Pre charge Failure Fault | 1 | – | `raw` |
| `$35A5` | HV battery Power Supply low Alarm | 1 | – | `raw` |
| `$35A6` | HV battery Power Supply high Alarm | 1 | – | `raw` |
| `$35A7` | HV battery Cell Tempertaure DiffOver Alarm | 1 | – | `raw` |
| `$35A8` | HV battery Temperature sensor 1 | 1 | DegC | `raw − 40` |
| `$35A9` | HV battery Temperature sensor 2 | 1 | DegC | `raw − 40` |
| `$35AA` | HV battery Temperature sensor 3 | 1 | DegC | `raw − 40` |
| `$35AB` | HV battery Temperature sensor 4 | 1 | DegC | `raw − 40` |
| `$35AC` | HV battery Temperature sensor 5 | 1 | DegC | `raw − 40` |
| `$35AD` | HV battery Temperature sensor 6 | 1 | DegC | `raw − 40` |
| `$35AE` | HV battery Temperature sensor 7 | 1 | DegC | `raw − 40` |
| `$35AF` | HV battery Temperature sensor 8 | 1 | DegC | `raw − 40` |
| `$35B0` | HV battery Cummulative Charge Capacity | 2 | °C | `raw` |
| `$35B1` | HV battery Cummulative Disharge Capacity | 2 | °C | `raw` |
| `$35B2` | HV battery SmokeSensor Fail fault | 1 | – | `raw` |
| `$35B3` | HV battery fast charging +ve contactor weld detection | 1 | – | `raw` |
| `$35B4` | HV battery fast charging -ve contactor weld detection | 1 | – | `raw` |
| `$35B6` | OBC Output maximum power | 2 | Watt | `raw × 0.01` |
| `$35B7` | Compressor Inverter Temperature | 1 | Degc | `raw − 50` |
| `$35B8` | Compressor Speed | 1 | RPM | `raw × 50` |
| `$35BA` | Compressor  input Voltage | 1 | V | `raw × 2` |
| `$35BB` | Compressor Input current | 1 | mA | `raw × 0.5` |
| `$35BD` | Primary motor HV Current | 2 | mA | `raw × 0.5 − 100` |
| `$35BE` | Primary Motor Output Speed | 2 | RPM | `raw − 20000` |
| `$35BF` | Secondary motor maximum regeneration torque | 1 | nm | `raw` |
| `$35C0` | Primary motor maximum regeneration torque | 1 | nm | `raw` |
| `$35C1` | Secondary motor torque increase request from ESP | 2 | nm | `raw × 0.1 − 500` |
| `$35C9` | ABS  vehicle speed from ESP | 2 | RPM | `raw × 0.03125` |
| `$35CC` | Codriver PTC temp value from FATC to VECU | 1 | DegC | `raw − 40` |
| `$35CD` | Codriver heating power value from FATC to VECU | 1 | J | `raw` |
| `$35D3` | Codriver Heating Power feedback value from VCU | 1 | J | `raw` |
| `$35D4` | VCU AC Charge limit from VCU | 1 | % | `raw` |
| `$35D5` | VCU FC Charge Limit from VCU | 1 | % | `raw` |
| `$35D8` | Primary motor Voltage HV | 2 | V | `raw × 0.5` |
| `$35DA` | Secondary motor HV Current | 2 | mA | `raw × 0.5 − 1000` |
| `$35DB` | Secondary motor Output Speed | 2 | RPM | `raw − 20000` |
| `$35DC` | Secondary motor Output Torque | 2 | nm | `raw − 510` |
| `$35DE` | Secondary motor Voltage HV | 2 | V | `raw × 0.5` |
| `$35E1` | Secondary motor Maximum Torque | 2 | nm | `raw − 510` |
| `$35E2` | Secondary motor Minimum Torque | 2 | nm | `raw − 510` |
| `$35E3` | Secondary motor ElectricMachineTemp | 1 | DegC | `raw − 40` |
| `$35E4` | Secondary motor InverterTemperature | 1 | DegC | `raw − 40` |
| `$35E5` | Dual PTC1 Contactor low side enable | 1 | – | `raw` |
| `$35E6` | Dual PTC2 Contactor Low side enable | 1 | – | `raw` |
| `$35E7` | Dual PTC2 Contactor High side enable | 1 | – | `raw` |
| `$35E8` | Dual PTC1 Contactor High side enable | 1 | – | `raw` |
| `$35E9` | Front Edrive Power supply Relay Enable | 1 | – | `raw` |
| `$35EA` | Front Edrive Ignition enable | 1 | – | `raw` |
| `$360A` | MCU Rear HVIL Sense PWM DutyCycle | 1 | % | `raw` |
| `$360B` | MCU Rear HVIL Sense PWM Frequency | 1 | Hz | `raw` |
| `$360C` | MCU Front HVIL Sense PWM DutyCycle | 1 | % | `raw` |
| `$360D` | MCU Front HVIL Sense PWM Frequency | 1 | Hz | `raw` |
| `$3623` | EXV PT Sensor Pressure | 1 | Bar | `raw × 0.01 − 1` |
| `$3624` | EXV PT Sensor Temperature | 1 | °C | `raw − 40` |
| `$3625` | BLDC Fan controller | 1 | 1 | `raw` |
| `$4224` | No of AES-SK Write LeftOut | 1 | – | `raw` |
| `$4224` | No of AES-SK Write LeftOut | 1 | – | `raw` |
| `$4224` | No of AES-SK Write LeftOut | 1 | – | `raw` |
| `$7002` | Control Module Input Power "B" | 1 | V | `raw` |

### Coded PIDs (166)

Value meanings in [`DECODE_TABLES.md`](DECODE_TABLES.md#vecu--vehicle-control-unit).

| DID | Signal | Signals in byte |
|-----|--------|-----------------|
| `$3436` | Charging Gun Lock/Unlock Command | 1 |
| `$3436` | Charging Gun Lock/Unlock Command | 1 |
| `$3436` | Charging Gun Lock/Unlock Command | 1 |
| `$3457` | VCU Power Mode | 1 |
| `$3458` | HV Battery Operating Mode | 1 |
| `$3459` | Traction E drive Operating Mode | 1 |
| `$345A` | HV Battery Equalization Status Feedback (Cell Balancing) | 1 |
| `$345B` | Vehicle Status | 1 |
| `$345C` | Compressor Diagnostic Status 1 | 8 |
| `$345D` | Compressor Diagnostic Status 2 | 6 |
| `$3486` | Remote Valet Mode Status | 1 |
| `$3487` | DC Fast charging Contactor Status | 1 |
| `$3488` | Vehicle Status Post crash detect | 1 |
| `$3489` | Crash Detect Status | 1 |
| `$348D` | Powertrain Variant Configuration | 1 |
| `$348E` | VECU Features Variant Configuration | 12 |
| `$34A8` | Traction E drive Requested State | 1 |
| `$34A8` | Traction E drive Requested State | 1 |
| `$34AC` | HV Battery Requested operating mode | 1 |
| `$34AC` | HV Battery Requested operating mode | 1 |
| `$34B5` | HV Battery Fault Rank | 1 |
| `$34B5` | HV Battery Fault Rank | 1 |
| `$34B6` | MCU Fault Grade | 1 |
| `$34B6` | MCU Fault Grade | 1 |
| `$34B7` | OBC Fault  Status | 1 |
| `$34B7` | OBC Fault  Status | 1 |
| `$34B8` | DCDC System Status | 1 |
| `$34B8` | DCDC System Status | 1 |
| `$34B9` | AC request from FATC | 1 |
| `$34B9` | AC request from FATC | 1 |
| `$34BC` | HV Battery  cell equalization command | 1 |
| `$34C5` | HV Critical Alert indication on IPC | 1 |
| `$34C5` | HV Critical Alert indication on IPC | 1 |
| `$34C6` | Limphome Alert indication on IPC | 1 |
| `$34C6` | Limphome Alert indication on IPC | 1 |
| `$34C8` | Gun Lock/Unlock Feedback Status from BSW | 1 |
| `$34C8` | Gun Lock/Unlock Feedback Status from BSW | 1 |
| `$34C9` | Charging Shutdown Reason | 1 |
| `$34C9` | Charging Shutdown Reason | 1 |
| `$34CD` | Charing current limitation source (Slow charging) | 1 |
| `$34CE` | Fast Charging Sequence on vehicle | 1 |
| `$34CE` | Fast Charging Sequence on vehicle | 1 |
| `$34CF` | Slow Charging Sequence on vehicle | 1 |
| `$34CF` | Slow Charging Sequence on vehicle | 1 |
| `$34D0` | CCS Fast charging Relay Status | 1 |
| `$34D0` | CCS Fast charging Relay Status | 1 |
| `$34D1` | Remote Immobilizer Enable/ Disable | 1 |
| `$34D2` | Remote Immobilizer Function Status | 1 |
| `$34D3` | PEPS Auto learning feature request | 1 |
| `$34D4` | PEPS Auto-learning feature status | 1 |
| `$34D6` | Charging Socket Actuator Type | 1 |
| `$34E5` | 3 in 1 unit DC-DC Current Status | 1 |
| `$34E6` | 3 in 1 unit  checksum fault | 1 |
| `$34EA` | 3 in 1 unit OBC Current Status | 1 |
| `$34ED` | 3 in 1 unit OBC CRC Checksum Fault status | 1 |
| `$34EF` | VCU  to 3 in 1 unit DC-DC Enable command | 1 |
| `$34F3` | VCU  to 3 in 1 unit Charging system Operation command | 1 |
| `$34F9` | Auto-Park brake error state from ESP | 1 |
| `$34FA` | Auto Vehicle hold State from ESP | 1 |
| `$34FB` | Brake Pedal sensor 2 status from ESP | 1 |
| `$34FC` | Auto-Park brake state from ESP | 1 |
| `$34FE` | Brake Pedal sensor 1 status from ESP | 1 |
| `$3502` | ABS active status from ABS/ESP | 1 |
| `$3503` | TCS Active status from ESP | 1 |
| `$3504` | ESP Active status from ESP | 1 |
| `$3505` | ESP request to VCU for shut down cruise control function | 1 |
| `$3506` | Hill Hold control Active state from ESP | 1 |
| `$3507` | Hill Descent control Active state from ESP | 1 |
| `$3508` | Feedback signal status from ESP to enable sports mode | 1 |
| `$3509` | Feedback signal from ESP to enable sports mode | 1 |
| `$350B` | Gear Selection Switch State Status | 1 |
| `$350C` | Remote Immo Request from TCU | 1 |
| `$350D` | Cruise Switch (Acc/ Dec) selection State | 1 |
| `$350E` | Cruise state VCU (Disabled/ Not active/ Active) | 1 |
| `$350F` | Drive Mode Selection Switch State by driver | 1 |
| `$3510` | Regeneration level for Display from VCU | 1 |
| `$3511` | Drive Mode (City/Eco/Sport) for Display from VCU | 1 |
| `$3512` | Cruise function state of vehicle determined by VCU | 1 |
| `$3513` | Drive Mode selection (possible/not possible) | 1 |
| `$3514` | Regen Level selection (possible/not possible) | 1 |
| `$3518` | Recuperation brake state from VCU (Enable/Disable) | 1 |
| `$351A` | Co-driver door state from BCM | 1 |
| `$351B` | Driver door state from BCM | 1 |
| `$351C` | Rear right side door state from BCM | 1 |
| `$351D` | Rear left side door state from BCM | 1 |
| `$351E` | CCS Charge Permission (EV ready) | 1 |
| `$3524` | CCS Isolation check status from station | 1 |
| `$3525` | Powertrain VCU Function Mode | 1 |
| `$3526` | Compressor State | 1 |
| `$352C` | Regeneration selection switch state from IPC to VCU | 1 |
| `$352D` | Cruise Speed change state from IPC to VCU | 1 |
| `$352E` | Cruise Switch Selection State from IPC to VCU | 1 |
| `$3530` | Charging current limitation source (Slow Charging) | 1 |
| `$3531` | BMS Key On feedback to VCU | 1 |
| `$3532` | BMS FC +ve Relay feedback to VCU | 1 |
| `$3533` | BMS FC -ve Relay feedback to VCU | 1 |
| `$3534` | Fast charging Relay Close Cmd from VCU to BMS | 1 |
| `$3535` | Fast charging gun detection Flag signal from VCU to BMS | 1 |
| `$3546` | MSS Position Request To VCU | 1 |
| `$3547` | MSS Position Display From VCU To MSS | 1 |
| `$3549` | Regen Level Via Paddle Shifter | 1 |
| `$3569` | Adaptive Cruise Control Engaged from ESP | 1 |
| `$356B` | ACC Torque Request Enable from ESP | 1 |
| `$356C` | Adaptive Cruise Control State from VCU | 1 |
| `$356D` | Park Brake State from VCU | 1 |
| `$356E` | Slow Charging Full Required | 1 |
| `$356F` | DCFC Contactor State | 1 |
| `$3570` | Adaptive Cruise Control System State to VECU | 1 |
| `$3571` | ADAS Steering Switch State Ack to VECU | 1 |
| `$3591` | Diagnostic State from ESP to VECU | 1 |
| `$3592` | Regen Brake Lamp Request | 1 |
| `$3593` | VCU Cruise Mode to IPC | 1 |
| `$3596` | MCU  Inverter 12V supply | 1 |
| `$359A` | Tin1 Charging CC status | 1 |
| `$359E` | BMS Negative Contactor Weld Fault | 1 |
| `$359F` | BMS Positive Contactor Weld Fault | 1 |
| `$35A0` | BCM Crank Source | 1 |
| `$35B5` | 3in1 OBC operation mode | 1 |
| `$35B9` | Compressor status | 1 |
| `$35BC` | APA System State | 1 |
| `$35C3` | TP2 Status from ESP | 1 |
| `$35C4` | TP2 Active from ESP | 1 |
| `$35C5` | APA Torque Reqeust enable from ESP | 1 |
| `$35C6` | APA Interface from ESP | 1 |
| `$35C7` | Driving Direction Request from ESP | 1 |
| `$35C8` | VCU Minimum Maximum Mode from ESP | 1 |
| `$35CA` | Mode Detect signal from ESP | 1 |
| `$35CB` | Mode Detect signal from ESP Status | 1 |
| `$35CE` | Mode Detect signal from TAS | 1 |
| `$35CF` | Drive mode Display from VCU | 1 |
| `$35D0` | Apa Interface from VCU to ESP | 1 |
| `$35D1` | Electric Motor State from VCU | 1 |
| `$35D2` | TP2  Interface from VCU to ESP | 1 |
| `$35D6` | Mode Selection switch State from VCU | 1 |
| `$35D7` | Primary motor State | 1 |
| `$35D9` | Primary motor Active Short Circuit Status | 1 |
| `$35DD` | Secondary motor  State | 1 |
| `$35DF` | Secondary motor Faultgrade | 1 |
| `$35E0` | Secondary motor 12V SuppyLV | 1 |
| `$35EC` | Custom Terrain Mode acknowlegment from ESP | 1 |
| `$35ED` | Custom Steering Mode acknowlegment from EPAS | 1 |
| `$35EE` | Custom Mode HU request Type from HU  to VCU | 1 |
| `$35EF` | Custom Drive mode  user request from HU to  VCU | 1 |
| `$35F0` | Custom  terrain  Mode user request from HU to VCEU | 1 |
| `$35F1` | Custom steering Mode user request from HU to VCEU | 1 |
| `$35F2` | Custom Regen Mode user request from HU to  VCEU | 1 |
| `$35F3` | Custom Mode combined User request from HU to VCU | 1 |
| `$35F4` | Custom mode status request from VCU | 1 |
| `$35F5` | Custom steering mode request from VCU | 1 |
| `$35F6` | Custom terrain mode status request from VCU | 1 |
| `$35F7` | Custom Mode combined Request from VCU to Partner ECU | 1 |
| `$35F8` | Custom terrain mode current state from VCU | 1 |
| `$35F9` | Custom Drive mode current  state from VCU | 1 |
| `$35FA` | Custom steering mode current state from VCU | 1 |
| `$35FB` | Custom Regen mode current state from VCU | 1 |
| `$35FC` | Custom mode combined Current State from VCU | 1 |
| `$35FD` | Custom Mode saved settings in VECU for drive and regen mode | 8 |
| `$35FE` | Custom Mode saved settings in VECU for steering | 1 |
| `$35FF` | Custom Mode saved settings in VECU for AWD 4×4 | 1 |
| `$3600` | Custom Mode saved settings in VECU for RWD 4×2 | 1 |
| `$3601` | VCU-HU Autolearning Status of Custom Mode | 1 |
| `$3602` | Custom Mode Settings update State | 1 |
| `$3603` | Custom Mode Execution State | 1 |
| `$3604` | Custom Mode denied Reason | 1 |
| `$4223` | Status of AES-SK SecretKey | 1 |
| `$4223` | Status of AES-SK SecretKey | 1 |

252 fault codes — read with `19 02 FF`, see [`READING_DTCS.md`](READING_DTCS.md).


## 3. DCDC — DC-DC Converter

> Source: **TDS 20.0 `DCDC_EV.sdf`**.

| Setting | Value |
|---------|-------|
| Request | **`0x784`** |
| Response | **`0x78C`** |
| Frame | 11-bit standard, 500 kbps |
| Session | `10 03` before reads |

### Scalar PIDs (16)

| DID | Signal | B | Unit | Conversion |
|-----|--------|---|------|------------|
| `$1310` | DCDC Input Voltage | 2 | V | `raw × 0.1` |
| `$1311` | DCDC Input Current | 2 | A | `raw × 0.1` |
| `$1312` | DCDC Output Voltage | 1 | V | `raw × 0.1` |
| `$1313` | DCDC Output Current | 2 | A | `raw × 0.1` |
| `$1314` | DCDC Internal SR MOS Temperature | 1 | oC | `raw` |
| `$1315` | DCDC Internal MT MOS Temperature | 1 | oC | `raw` |
| `$1316` | DCDC Internal Pri MOS Temperature | 1 | oC | `raw` |
| `$1317` | 12V Power Supply Voltage | 1 | V | `raw × 0.1` |
| `$1318` | KL15 Wake Up Voltage | 1 | V | `raw × 0.1` |
| `$1327` | DCDC output current request | 2 | A | `raw × 0.01` |
| `$1328` | DCDC output power request | 2 | W | `raw` |
| `$F010` | EOL DataIdentifier | 8 | – | `raw` |
| `$F0F1` | DCDC internal status 1 | 2 | – | `raw` |
| `$F0F2` | DCDC internal status 2 | 2 | – | `raw` |
| `$F0F3` | DCDC internal status 3 | 2 | – | `raw` |
| `$F0F4` | DCDC Gateway Software Version | 2 | – | `raw` |

### Coded PIDs (5)

Value meanings in [`DECODE_TABLES.md`](DECODE_TABLES.md#dcdc--dc-dc-converter).

| DID | Signal | Signals in byte |
|-----|--------|-----------------|
| `$130E` | DCDC working states | 1 |
| `$130F` | DCDC Enable Command | 1 |
| `$1325` | DCDC derating mode | 1 |
| `$1326` | DCDC error flag | 1 |
| `$1329` | DCDC VCU CRC FAIL | 1 |

17 fault codes — read with `19 02 FF`, see [`READING_DTCS.md`](READING_DTCS.md).


## 3B. MCU — Motor Control Unit

> Source: **TDS 20.0 `MCU_DiagnosticsDB.sdf`**. Only 27 DIDs — most motor detail is reported by the VECU on `0x7E3` rather than by the MCU directly.

| Setting | Value |
|---------|-------|
| Request | **`0x783`** |
| Response | **`0x78B`** |
| Frame | 11-bit standard, 500 kbps |
| Session | `10 03` before reads |

### Scalar PIDs (15)

| DID | Signal | B | Unit | Conversion |
|-----|--------|---|------|------------|
| `$0506` | E drive maximum torque limit feedback | 2 | Nm | `raw` |
| `$0507` | E drive maximum regenerative(Brake torque) torque limit feedback | 2 | Nm | `raw` |
| `$0508` | E drive maximum speed limit feedback | 2 | Rpm | `raw` |
| `$0509` | E drive minimum speed limit. (Reverse direction) feedback. | 2 | Rpm | `raw` |
| `$0510` | Rotional speed of E drive | 2 | Rpm | `raw` |
| `$0511` | Output torque which is currently provided by the electric machine | 2 | Nm | `raw` |
| `$0512` | HV current drawn from the DC link by the E drive | 2 | A | `raw × 0.5` |
| `$0513` | HV voltage of the DC link measured by the E drive | 2 | V | `raw × 0.5` |
| `$0514` | The present mode of the E drive | 1 | - | `raw` |
| `$0515` | Current temperature of electric machine | 1 | °C | `raw` |
| `$0516` | Current temperature of inverter | 1 | °C | `raw` |
| `$0517` | Power consumed or generated by E Drive | 2 | Kw | `raw` |
| `$0518` | E drive phase A  current | 2 | A | `raw × 0.5` |
| `$0519` | E drive phase B  current | 2 | A | `raw × 0.5` |
| `$0520` | E drive phase C  current | 2 | A | `raw × 0.5` |

89 fault codes — read with `19 02 FF`, see [`READING_DTCS.md`](READING_DTCS.md).


## 3C. OBC — On Board Charger

> Source: **TDS 20.0 `OBC_EV.sdf`**.

| Setting | Value |
|---------|-------|
| Request | **`0x786`** |
| Response | **`0x78E`** |
| Frame | 11-bit standard, 500 kbps |
| Session | `10 01` before reads |

### Scalar PIDs (26)

| DID | Signal | B | Unit | Conversion |
|-----|--------|---|------|------------|
| `$0200` | Reprogramming counter (successfully reprogramming) | 2 | – | `raw` |
| `$0201` | Reprogramming attempt counter | 2 | – | `raw` |
| `$1000` | 12V power supply voltage | 1 | V | `raw` |
| `$1100` | Charger input AC frequency | 2 | Hz | `raw` |
| `$1101` | Chareger available power | 4 | W | `raw` |
| `$1102` | Primary board temperature | 1 | deg c | `raw` |
| `$1103` | Secondary board temperature | 1 | deg c | `raw` |
| `$1104` | Transformer temperature | 1 | deg c | `raw` |
| `$1105` | Input AC current | 2 | A | `raw × 0.1` |
| `$1106` | Input AC voltage | 2 | V | `raw` |
| `$131A` | Actual input  frequency  from AC Grid | 2 | Hz | `raw` |
| `$131B` | OBC CP Duty | 1 | % | `raw` |
| `$131C` | OBC CP Frequency | 2 | HZ | `raw` |
| `$131F` | OBC Charger available power | 2 | W | `raw` |
| `$132A` | OBC CP Voltage | 1 | V | `raw × 0.1` |
| `$2000` | CP voltage | 1 | V | `raw` |
| `$2001` | CP duty | 1 | – | `raw` |
| `$2002` | CP Frequency | 2 | Hz | `raw` |
| `$2003` | Output DC current | 2 | A | `raw × 0.01` |
| `$2004` | Output DC voltage | 2 | V | `raw × 0.1` |
| `$F0E1` | OBC Internal Status1 | 2 | – | `raw` |
| `$F0E2` | OBC Internal Status2 | 2 | – | `raw` |
| `$F0E3` | OBC Internal Status3 | 2 | – | `raw` |
| `$F0E4` | OBC Internal Status4 | 2 | – | `raw` |
| `$F0E5` | OBC Primary DSP Software Version | 3 | – | `raw` |
| `$F0E6` | OBC Secondary DSP Software Version | 3 | – | `raw` |

### Coded PIDs (23)

Value meanings in [`DECODE_TABLES.md`](DECODE_TABLES.md#obc--on-board-charger).

| DID | Signal | Signals in byte |
|-----|--------|-----------------|
| `$0210` | CP line status | 1 |
| `$0211` | Charger state | 1 |
| `$0212` | Internal CAN status | 1 |
| `$0213` | External CAN status | 1 |
| `$0214` | External CAN transceiver status | 1 |
| `$0215` | FEE status | 1 |
| `$0216` | EEPROM Write Error | 1 |
| `$0217` | Internal SCI communication FAIL | 1 |
| `$0218` | Internal SCI communication CRC FAIL | 1 |
| `$0219` | OBD sensors | 16 |
| `$1319` | internal CAN Communication states | 1 |
| `$131D` | HW wakeup output state | 1 |
| `$131E` | VCU CRC FAIL for OBC | 1 |
| `$1320` | OBC Internal fault | 1 |
| `$1321` | OBC External CAN status | 1 |
| `$1322` | OBC derating mode | 1 |
| `$1323` | OBC error flag | 1 |
| `$1324` | OBC CC ohm | 1 |
| `$1330` | VCU Charging/Discharge mode command | 1 |
| `$1331` | VCU Discharge gun connect status | 1 |
| `$1332` | OBC Operation Mode | 1 |
| `$1904` | Snapshot Data | 23 |
| `$2100` | HW wakeup output state | 1 |

31 fault codes — read with `19 02 FF`, see [`READING_DTCS.md`](READING_DTCS.md).



## 4. AC / HVAC (Climate Control)

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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

> ℹ️ **Legacy section.** Carried over from the original revision of this file and
> **not** regenerated from the TDS 20.0 databases — there is no TDS 20.0 source for
> this ECU/variant. Treat the values as unverified.


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
