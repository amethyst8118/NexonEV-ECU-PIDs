# Nexon EV - Complete ECU Diagnostic PIDs Reference

All ECUs use **500 kbps CAN, Extended Frame (29-bit)**

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

## 2. EVCU (Electric Vehicle Control Unit)

### CAN IDs
| Direction | CAN ID |
|-----------|--------|
| Request | `0x1BDA00F1` |
| Response | `0x1BDAF100` |

### Key PIDs
| DID | Name | Bytes |
|-----|------|-------|
| `$F18C` | Supplier ECU Serial Number | 10 |
| `$F190` | VIN | 17 |
| `$F191` | ECU HW Number | 15 |
| `$F192` | Supplier Part Number | 8 |
| `$F186` | Active Session | 1 |
| `$0102` | Road Speed Limit | 1 |
| `$0105` | BMS Battery Voltage | 2 |
| `$012C` | BMS Inlet Temperature | 2 |
| `$0134` | BMS Outlet Temperature / Software Number | 2 |
| `$0159` | BMS Insulation Resistance | 2 |
| `$015A` | Compressor Speed | 2 |
| `$015D` | DCDC Output Voltage | 2 |
| `$0161` | DCDC Output Current | 2 |
| `$0164` | VCU LV Supply Voltage | 2 |
| `$0166` | Vehicle Speed | 2 |
| `$017A` | Electric Machine Speed | 2 |
| `$0181` | OBC DC Current | 2 |
| `$0182` | BMS DC Current | 2 |
| `$01A0` | BMS Battery SOC | 2 |
| `$01D2` | HVAC Enable On/Off | 1 |
| `$01D6` | BCS Set Temperature | 1 |
| `$01D7` | BCS Self Circulating Temperature | 1 |
| `$01CF` | Torque Map Selection | 1 |

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
