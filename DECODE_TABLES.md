# Decode tables

What every enumerated and bit-packed DID value actually *means*, taken from the
`ByteParameters` rows of the TDS 20.0 ECU databases. These are the DIDs that
return a **code**, not a number — reading them as scalars gives you a bare
integer with no meaning.

Scalar DIDs (voltages, temperatures, currents) are not repeated here; they are in
[`data/`](data/) with their scaling, and the notable ones in
[`Nexon_EV_All_PIDs.md`](Nexon_EV_All_PIDs.md).

## How to read these

**Simple enum** — one signal, the whole byte is the code. Compare the value
directly.

**Bit-packed** — several independent signals in one byte, each with a mask. Test
with `byte & mask`, never equality. The database stores the *masked* value as the
match target, so a signal on mask `0x04` reads back as `4` when set, not `1`.

```
22 3479          request
62 34 79 05      response, flag byte = 0x05
0x05 & 0x01 = 1  -> VCU HVIL Detect     : enable
0x05 & 0x02 = 0  -> Insulation Control  : disable
0x05 & 0x04 = 4  -> Cell Balance Status : enable
```

## Coverage

| ECU | Header | Enumerated DIDs | Bit-packed |
|-----|--------|----------------:|-----------:|
| [VECU](#vecu--vehicle-control-unit) | `0x7E3` | 171 | 4 |
| [BMS](#bms--battery-management-system) | `0x785` | 9 | 3 |
| [OBC](#obc--on-board-charger) | `0x786` | 29 | 2 |
| [DCDC](#dcdc--dc-dc-converter) | `0x784` | 8 | 0 |
| [MCU](#mcu--motor-control-unit) | `0x783` | 0 | 0 |
| [BCM](#bcm--body-control-module) | `0x701` | 148 | 58 |
| [PEPS](#peps--passive-entry-passive-start) | `0x710` | 48 | 17 |
| [IMMO](#immo--immobiliser) | `0x704` | 10 | 4 |

---

## VECU — Vehicle Control Unit

Request `0x7E3` · response `0x7EB` · 171 enumerated DIDs, 4 of them bit-packed.

### `$3436` — Charging Gun Lock/Unlock Command

| Value | Meaning |
|-------|---------|
| `0` | Unlock Charging Gun |
| `1` | Lock Charging Gun |

### `$3436` — Charging Gun Lock/Unlock Command

| Value | Meaning |
|-------|---------|
| `0` | No Action |
| `1` | Lock Charging Gun |
| `2` | Unlock Charging Gun |
| `3` | Reserved |

### `$3436` — Charging Gun Lock/Unlock Command

| Value | Meaning |
|-------|---------|
| `0` | Unlock Charging Gun |
| `1` | Lock Charging Gun |
| `2` | Reserved |
| `3` | Reserved |

### `$3457` — VCU Power Mode

| Value | Meaning |
|-------|---------|
| `0` | Awake |
| `1` | Active |
| `2` | Crank |
| `3` | Normal run |
| `4` | Energy Recuperation |
| `5` | Charging mode |
| `6` | Limited power mode |

### `$3458` — HV Battery Operating Mode

| Value | Meaning |
|-------|---------|
| `4` | HV Powerdown |
| `5` | Pre-charge Fault |
| `6` | Operational Mode Fault |
| `0` | Initializing |
| `1` | Standby |
| `2` | PreCharge |
| `3` | HVActive |
| `7` | Ready to sleep |

### `$3459` — Traction E drive Operating Mode

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Initializing |
| `2` | Precharge |
| `3` | Standby |
| `4` | Torque Control |
| `5` | Speed Control |
| `6` | Discharge |
| `7` | Afterrun |
| `8` | Fault |

### `$345A` — HV Battery Equalization Status Feedback (Cell Balancing)

| Value | Meaning |
|-------|---------|
| `0` | No Cell Balance |
| `1` | Cell balancing ON |

### `$345B` — Vehicle Status

| Value | Meaning |
|-------|---------|
| `0` | Ready To Go |
| `1` | Not Ready to Go. Charger Connected |
| `2` | Not Ready to Go. HV System Not Ready |
| `3` | EV System Disengaged |

### `$345C` — Compressor Diagnostic Status 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Compressor Diagnostic Status 1- Hvolt_ERR | `0` | No Fault |
|  |  | `1` | Fault |
| `0x02` | Compressor Diagnostic Status 1- Temp_over_ERR | `0` | No Fault |
|  |  | `2` | Fault |
| `0x04` | Compressor Diagnostic Status 1- Power_over_ERR | `0` | No Fault |
|  |  | `4` | Fault |
| `0x08` | Compressor Diagnostic Status 1- Pha_cur_ERR | `0` | No Fault |
|  |  | `8` | Fault |
| `0x10` | Compressor Diagnostic Status 1- CAN_ERR | `0` | No Fault |
|  |  | `10` | Fault |
| `0x20` | Compressor Diagnostic Status 1- IN_Communi_ERR | `0` | No Fault |
|  |  | `20` | Fault |
| `0x40` | Compressor Diagnostic Status 1- Start_ERR | `0` | No Fault |
|  |  | `40` | Fault |
| `0x80` | Compressor Diagnostic Status 1- IN_PWR_ERR | `0` | No Fault |
|  |  | `80` | Fault |

### `$345D` — Compressor Diagnostic Status 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Compressor Diagnostic Status 2- Bus_limit | `0` | No Fault |
|  |  | `1` | Fault |
| `0x02` | Compressor Diagnostic Status 2- Bus_cur_ERR | `0` | No Fault |
|  |  | `2` | Fault |
| `0x04` | Compressor Diagnostic Status 2- Temp_limit | `0` | No Fault |
|  |  | `4` | Fault |
| `0x08` | Compressor Diagnostic Status 2- Pow_limit | `0` | No Fault |
|  |  | `8` | Fault |
| `0x10` | Compressor Diagnostic Status 2- Pha_cur_limit | `0` | No Fault |
|  |  | `10` | Fault |
| `0x20` | Compressor Diagnostic Status 2- LVolt_ERR | `0` | No Fault |
|  |  | `20` | Fault |
|  |  | `0` | No Fault |
|  |  | `40` | Fault |
|  |  | `0` | No Fault |
|  |  | `80` | Fault |

### `$3486` — Remote Valet Mode Status

| Value | Meaning |
|-------|---------|
| `0` | Valet Mode Disabled |
| `1` | Valet Mode Enabled |
| `2` | Reserved |
| `3` | Reserved |

### `$3487` — DC Fast charging Contactor Status

| Value | Meaning |
|-------|---------|
| `0` | Weld check not yet happened |
| `80` | DC FC Both Contactor Weld |
| `84` | DC FC Positive Contactor Wled |
| `81` | DC FC Negative Contactor Wled |
| `85` | DC FC Contactor Ok |
| `5` | Weld check by passed |
| `0xA4` | Reserved1 |

### `$3488` — Vehicle Status Post crash detect

| Value | Meaning |
|-------|---------|
| `0` | Status is 0 |
| `1` | Status is 1 |
| `2` | Status is 2 |
| `3` | Status is 3 |
| `0x0B` | Status is 11 |
| `5` | Status is 5 |
| `6` | Status is 6 |
| `7` | Status is 7 |
| `8` | Status is 8 |
| `9` | Status is 9 |
| `0x0A` | Status is 10 |
| `4` | Status is 4 |

### `$3489` — Crash Detect Status

| Value | Meaning |
|-------|---------|
| `0` | Crash Not detected |
| `1` | Crash  detected |
| `2` | Medium Crash detected |
| `3` | Reserved |

### `$348D` — Powertrain Variant Configuration

| Value | Meaning |
|-------|---------|
| `0` | India |
| `2` | Nepal |
| `0` | AIO BMS |
| `1` | GEN 3 BMS |

### `$348E` — VECU Features Variant Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | VECU Features Variant Configuration (Slow Fan, Fast Fan) | `0` | Slow Fan, Fast Fan |
| `0x01` | VECU Features Variant Configuration (PWM, Fast Relay) | `1` | PWM, Fast Relay |
| `0x02` | VECU Features Variant Configuration (Cruise Feature Unavailable) | `0` | Cruise Feature Unavailable |
| `0x02` | VECU Features Variant Configuration (Cruise Feature Available) | `2` | Cruise Feature Available |
| `0x04` | VECU Features Variant Configuration (Cruise Through Switch) | `0` | Cruise Through Switch |
| `0x04` | VECU Features Variant Configuration (Cruise Through CAN) | `4` | Cruise Through CAN |
| `0x08` | VECU Features Variant Configuration (Regen Level Change NA) | `0` | Regen Level Change NA |
| `0x08` | VECU Features Variant Configuration (Regen Level Change Available) | `8` | Regen Level Change Available |
| `0x10` | VECU Features Variant Configuration (Regen Through Switch) | `0` | Regen Through Switch |
| `0x10` | VECU Features Variant Configuration (Regen Through CAN) | `10` | Regen Through CAN |
| `0x20` | VECU Features Variant Configuration (Gear Selector - RNDS) | `0` | Gear Selector - RNDS |
| `0x20` | VECU Features Variant Configuration (Gear Selector - PRND) | `20` | Gear Selector - PRND |

### `$34A8` — Traction E drive Requested State

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Initializing |
| `2` | Precharge |
| `3` | Standby |
| `4` | Torque Control |
| `5` | Speed Control |
| `6` | Discharge |
| `7` | Afterrun |
| `8` | Fault |

### `$34A8` — Traction E drive Requested State

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Initializing |
| `2` | Precharge |
| `3` | Standby |
| `4` | Torque Control |
| `5` | Speed Control |
| `6` | Discharge |
| `7` | Afterrun |
| `8` | Fault |

### `$34AC` — HV Battery Requested operating mode

| Value | Meaning |
|-------|---------|
| `0` | Initializing |
| `1` | Standby |
| `2` | Reserved |
| `3` | HVActive |
| `4` | HV Power Down |
| `5` | LV Power Down |
| `6` | Crash Detect |

### `$34AC` — HV Battery Requested operating mode

| Value | Meaning |
|-------|---------|
| `0` | Initializing |
| `1` | Standby |
| `2` | Reserved |
| `3` | HVActive |
| `4` | HV Power Down |

### `$34B5` — HV Battery Fault Rank

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | First Level Fault |
| `2` | Second Level Fault |
| `3` | Third Level Fault |

### `$34B5` — HV Battery Fault Rank

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | First Level Fault |
| `2` | Second Level Fault |
| `3` | Third Level Fault |

### `$34B6` — MCU Fault Grade

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | First Level Fault |
| `2` | Second Level Fault |
| `3` | Third Level Fault |

### `$34B6` — MCU Fault Grade

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | First Level Fault |
| `2` | Second Level Fault |
| `3` | Third Level Fault |

### `$34B7` — OBC Fault  Status

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | Recoverable Fault |
| `2` | Un recoverable Fault |
| `3` | Reserved |

### `$34B7` — OBC Fault  Status

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | Recoverable Fault |
| `2` | Un recoverable Fault |
| `3` | Reserved |

### `$34B8` — DCDC System Status

| Value | Meaning |
|-------|---------|
| `0` | Initializing |
| `1` | Standby |
| `2` | Work |
| `3` | Reserved |
| `4` | Reserved |
| `5` | Sleep |
| `6` | Recoverable Fault |
| `7` | Lock Fault |
| `8` | Reserved |

### `$34B8` — DCDC System Status

| Value | Meaning |
|-------|---------|
| `0` | Initializing |
| `1` | Standby |
| `2` | Work |
| `3` | Reserved |
| `4` | Reserved |
| `5` | Sleep |
| `6` | Recoverable Fault |
| `7` | Lock Fault |
| `8` | Reserved |

### `$34B9` — AC request from FATC

| Value | Meaning |
|-------|---------|
| `0` | Deactivated |
| `1` | Activated |

### `$34B9` — AC request from FATC

| Value | Meaning |
|-------|---------|
| `0` | Deactivated |
| `1` | Activated |

### `$34BC` — HV Battery  cell equalization command

| Value | Meaning |
|-------|---------|
| `0` | No action |
| `1` | Cell Equilization Command |

### `$34C5` — HV Critical Alert indication on IPC

| Value | Meaning |
|-------|---------|
| `0` | Lamp OFF |
| `1` | Lamp ON due to HV battery fault |
| `2` | Lamp ON due to Motor fault |
| `3` | Lamp ON due to PDU safety logic |
| `4` | Lamp ON due to TGT safety logic |
| `5` | Lamp ON due to Diagnostic |

### `$34C5` — HV Critical Alert indication on IPC

| Value | Meaning |
|-------|---------|
| `0` | Lamp OFF |
| `1` | Lamp ON due to HV battery isolation fault |
| `2` | Lamp ON due to HV battery fault |
| `3` | Lamp ON due to Motor fault |
| `4` | Lamp ON due to PDU safety logic |
| `5` | Lamp ON due to TGT safety logic |
| `6` | Lamp ON due to Diagnostic |
| `7` | Reserve 1 |
| `8` | Reserve 2 |
| `9` | Reserve 3 |
| `0x0A` | Reserve 4 |

### `$34C6` — Limphome Alert indication on IPC

| Value | Meaning |
|-------|---------|
| `0` | Lamp OFF |
| `1` | Lamp ON due to BMS Derate flag |
| `2` | Lamp ON due to low BMS SOC |
| `3` | Lamp ON due to Diagnostic |

### `$34C6` — Limphome Alert indication on IPC

| Value | Meaning |
|-------|---------|
| `0` | Lamp OFF |
| `1` | Lamp ON due to low BMS SOC |
| `2` | Lamp ON due to Diagnostic |
| `3` | Lamp ON due to BMS Derate flag |
| `4` | Reserve 1 |
| `5` | Reserve 2 |
| `6` | Reserve 3 |
| `7` | Reserve 4 |
| `8` | Reserve 5 |
| `9` | Reserve 6 |
| `0x0A` | Reserve 7 |

### `$34C8` — Gun Lock/Unlock Feedback Status from BSW

| Value | Meaning |
|-------|---------|
| `0` | Gun Lock Open |
| `1` | Status Pending |
| `2` | Gun Lock Closed |
| `9` | Error |

### `$34C8` — Gun Lock/Unlock Feedback Status from BSW

| Value | Meaning |
|-------|---------|
| `0` | Gun Lock Open |
| `1` | Status Pending |
| `2` | Gun Lock Closed |
| `3` | Reserved |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |
| `8` | Reserved |
| `9` | Error |

### `$34C9` — Charging Shutdown Reason

| Value | Meaning |
|-------|---------|
| `0` | No Shutdown |
| `1` | Charging Timeout Normal Shutdown |
| `2` | Station Side Emergency Normal Shutdown |
| `3` | Station Side User Normal Shutdown |
| `4` | Vehicle Side Normal Shutdown |
| `5` | Battery Fully Charged Normal Shutdown |
| `6` | Wrong CP Failure Normal Shutdown |
| `7` | PLC Failure Normal Shutdown |
| `8` | HV Battery Failure Normal Shutdown |
| `9` | Park Brake Failure Normal Shutdown |
| `10` | Current Overshoot Failure Normal Shutdown |
| `11` | Isolation Failure Normal Shutdown |
| `12` | MCU Weld Check Failure Normal Shutdown |
| `13` | Invalid Weld Check (redo) Normal Shutdown |
| `14` | Wrong PP Failure Emergency Shutdown |
| `15` | Crash Detected Emergency Shutdown |

### `$34C9` — Charging Shutdown Reason

| Value | Meaning |
|-------|---------|
| `0` | No Shutdown |
| `1` | Charging Timeout Normal Shutdown |
| `2` | Station Side Emergency Normal Shutdown |
| `3` | Station Side User Normal Shutdown |
| `4` | Vehicle Side Normal Shutdown |
| `5` | Battery Fully Charged Normal Shutdown |
| `6` | Wrong CP Failure Normal Shutdown |
| `7` | PLC Failure Normal Shutdown |
| `8` | HV Battery Failure Normal Shutdown |
| `9` | Park Brake Failure Normal Shutdown |
| `10` | Current Overshoot Failure Normal Shutdown |
| `11` | Isolation Failure Normal Shutdown |
| `12` | MCU Weld Check Failure Normal Shutdown |
| `13` | Invalid Weld Check (redo) Normal Shutdown |
| `14` | Wrong PP Failure Emergency Shutdown |
| `15` | Crash Detected Emergency Shutdown |

### `$34CD` — Charing current limitation source (Slow charging)

| Value | Meaning |
|-------|---------|
| `0` | Undecisive / Fast Charing Ongoing |
| `1` | Charging Current Limited by HV Battery |
| `2` | Charging Current Limited by VCU for 3in1 Combo |
| `3` | Charging Current Limited by 3in1 Combo |
| `4` | Charging Current Limited by VCU for Home Charger for 3in1 Combo |
| `5` | Charging Current Limited by CP Dutycycle & PP Resistance Values |
| `6` | Charging Current Limited by CP Dutycycle |
| `7` | Charging Current Limited by VCU for Home Charger for OBC |
| `8` | Charging Current Limited by VCU for AC Fast Charger for OBC |

### `$34CE` — Fast Charging Sequence on vehicle

| Value | Meaning |
|-------|---------|
| `0` | No Fast Charging Gun |
| `1` | Fast Charging Gun Connected |
| `2` | Battery Contactor Close Req |
| `3` | Battery Contactors Closed |
| `4` | Charge Permission Given |
| `5` | Station Precharge Happening |
| `6` | Fast Charging Contactors Close Req |
| `7` | Charging Started |
| `8` | Charging Timeout |
| `9` | Normal Shutdown Detected |
| `10` | Current Derated |
| `11` | Normal Shutdown Initiated |
| `12` | Weld Check Happening |
| `13` | Weld Check Finish |
| `14` | Weld Check Bypass |
| `15` | Emergency Shutdown Detected |
| `16` | Open Battery Contactor Req |
| `17` | Battery Contactors Opened |
| `18` | Discharge DC Link Req |
| `19` | DC Link Discharged |
| `20` | Open Fast Charging Contcators Req |
| `21` | Fast Charging Gun Remove |

### `$34CE` — Fast Charging Sequence on vehicle

| Value | Meaning |
|-------|---------|
| `0` | No Fast Charging Gun |
| `1` | Fast Charging Gun Connected |
| `2` | Battery Contactor Close Req |
| `3` | Battery Contactors Closed |
| `4` | Charge Permission Given |
| `5` | Station Precharge Happening |
| `6` | Fast Charging Contactors Close Req |
| `7` | Charging Started |
| `8` | Charging Timeout |
| `9` | Normal Shutdown Detected |
| `10` | Current Derated |
| `11` | Normal Shutdown Initiated |
| `12` | Weld Check Happening |
| `13` | Weld Check Finish |
| `14` | Weld Check Bypass |
| `15` | Emergency Shutdown Detected |
| `16` | Open Battery Contactor Req |
| `17` | Battery Contactors Opened |
| `18` | Discharge DC Link Req |
| `19` | DC Link Discharged |
| `20` | Open Fast Charging Contcators Req |
| `21` | Fast Charging Gun Remove |

### `$34CF` — Slow Charging Sequence on vehicle

| Value | Meaning |
|-------|---------|
| `0` | Vehicle Cranked - Not Ready to Charge |
| `1` | Ready To Charge |
| `2` | Gun Connected |
| `3` | CP Dutycycle Received - Valid |
| `4` | OBC Awake |
| `5` | HV Battery Closing Requested |
| `6` | S2 Switch Open, HV Battery Closed |
| `7` | S2 Closed |
| `8` | OBC Active |
| `9` | PTC Discharged |
| `10` | Reserve1 |
| `11` | Reserve2 |
| `12` | Reserve3 |
| `13` | Reserve4 |
| `14` | Reserve5 |
| `15` | Reserve6 |

### `$34CF` — Slow Charging Sequence on vehicle

| Value | Meaning |
|-------|---------|
| `0` | Ready To Charge |
| `1` | Vehicle Cranked - Not Ready to Charge |
| `2` | Gun Connected |
| `3` | CP Signal Valid |
| `4` | OBC Wakeup Requested |
| `5` | OBC Awake |
| `6` | HV Battery Contactors Close Requested |
| `7` | S2 Switch Close Requested |
| `8` | S2 Switch Closed |
| `9` | Charge Permission Given |
| `10` | Charging Started |
| `11` | Charging Stopped |
| `12` | S2 Switch Open Requested |
| `13` | S2 Switch Open |
| `14` | HV Battery Contactors Open Requested |
| `15` | HV Battery Contactors Opened |

### `$34D0` — CCS Fast charging Relay Status

| Value | Meaning |
|-------|---------|
| `0` | Initialization in progress |
| `1` | Precharge in progress |
| `2` | Both contactors closed |
| `3` | Welding check in progress |
| `4` | Both contactors open |
| `5` | Only one contactor welded |
| `6` | Both contactors welded |
| `7` | Error state |

### `$34D0` — CCS Fast charging Relay Status

| Value | Meaning |
|-------|---------|
| `0` | Initialization in progress |
| `1` | Precharge in progress |
| `2` | Both contactors closed |
| `3` | Welding check in progress |
| `4` | Both contactors open |
| `5` | Only one contactor welded |
| `6` | Both contactors welded |
| `7` | Error state |
| `8` | Reserved1 |
| `9` | Reserved2 |

### `$34D1` — Remote Immobilizer Enable/ Disable

| Value | Meaning |
|-------|---------|
| `0` | Remote Immobilizer Mode Disabled |
| `1` | Remote Immobilizer Mode Enabled |
| `2` | Reserved |
| `3` | Reserved |

### `$34D2` — Remote Immobilizer Function Status

| Value | Meaning |
|-------|---------|
| `0` | Remote Immo Function  Disabled |
| `1` | Mobilized by TCU |
| `2` | Immobilized by TCU |
| `3` | Reserved |

### `$34D3` — PEPS Auto learning feature request

| Value | Meaning |
|-------|---------|
| `0` | Inactive |
| `1` | Active |

### `$34D4` — PEPS Auto-learning feature status

| Value | Meaning |
|-------|---------|
| `0` | Feedback not available |
| `1` | Not learnt |
| `2` | Auto-learning in process |
| `3` | Learn Complete |

### `$34D6` — Charging Socket Actuator Type

| Value | Meaning |
|-------|---------|
| `0` | Analog Feedback Type (Kiekart) |
| `1` | Digital Feedback Type (Yongsin) |

### `$34E5` — 3 in 1 unit DC-DC Current Status

| Value | Meaning |
|-------|---------|
| `0` | Standby |
| `1` | Work |
| `2` | Sleep |
| `3` | Fault |
| `4` | Reserved1 |
| `5` | Reserved2 |
| `6` | Reserved3 |
| `7` | Reserved4 |

### `$34E6` — 3 in 1 unit  checksum fault

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Abnormal |

### `$34EA` — 3 in 1 unit OBC Current Status

| Value | Meaning |
|-------|---------|
| `0` | Init State |
| `1` | Standby State |
| `2` | Working State |
| `3` | Reserved |
| `4` | Fault State |
| `5` | Sleep State |
| `6` | Reserved |
| `7` | Invalid |

### `$34ED` — 3 in 1 unit OBC CRC Checksum Fault status

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Abnormal |

### `$34EF` — VCU  to 3 in 1 unit DC-DC Enable command

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |
| `2` | Sleep |

### `$34F3` — VCU  to 3 in 1 unit Charging system Operation command

| Value | Meaning |
|-------|---------|
| `0` | Init State |
| `1` | Standby State |
| `2` | Working State |
| `3` | Reserved |
| `4` | Fault State |
| `5` | Sleep State |
| `6` | Reserved |
| `7` | Invalid |

### `$34F9` — Auto-Park brake error state from ESP

| Value | Meaning |
|-------|---------|
| `0` | Low Severity Error |
| `1` | No Error |
| `2` | High Severity Error |
| `3` | Diagnosis |

### `$34FA` — Auto Vehicle hold State from ESP

| Value | Meaning |
|-------|---------|
| `0` | AVH OFF |
| `1` | AVH Active |
| `2` | AVH Standby |
| `3` | Reserved |

### `$34FB` — Brake Pedal sensor 2 status from ESP

| Value | Meaning |
|-------|---------|
| `0` | Plausible |
| `1` | Reserved |
| `2` | Implausible |
| `3` | Signal Not Available |

### `$34FC` — Auto-Park brake state from ESP

| Value | Meaning |
|-------|---------|
| `0` | Static release not possible Brake not pressed |
| `1` | Released |
| `2` | Closed |
| `3` | Releasing |
| `4` | Locking |
| `5` | Dynamic APB Apply |
| `6` | APB cannot hold the vehicle - Slope Too High |
| `7` | No Action |
| `8` | APB Auto release not possible -  driver seat belt unbuckled |
| `9` | Unknown |

### `$34FE` — Brake Pedal sensor 1 status from ESP

| Value | Meaning |
|-------|---------|
| `0` | Plausible |
| `1` | Reserved |
| `2` | Implausible |
| `3` | Signal Not Available |

### `$3502` — ABS active status from ABS/ESP

| Value | Meaning |
|-------|---------|
| `0` | No ABS Intervention by ABS/ESP |
| `1` | ABS Intervention by ABS/ESP is active |

### `$3503` — TCS Active status from ESP

| Value | Meaning |
|-------|---------|
| `0` | No TCS Intervention by ESP system @MFL is OFF |
| `1` | TCS Intervention by ESP system @MFL is ON |

### `$3504` — ESP Active status from ESP

| Value | Meaning |
|-------|---------|
| `0` | No ESP Intervention by ESP system @MFL is OFF |
| `1` | ESP Intervention by ESP system is active @MFL is blinking |

### `$3505` — ESP request to VCU for shut down cruise control function

| Value | Meaning |
|-------|---------|
| `0` | Cruise control shut off is not requested by ESP |
| `1` | Cruise control shut off is requested by ESP |

### `$3506` — Hill Hold control Active state from ESP

| Value | Meaning |
|-------|---------|
| `0` | No HHC intervention by ESP |
| `1` | HHC intervention by ESP is active |

### `$3507` — Hill Descent control Active state from ESP

| Value | Meaning |
|-------|---------|
| `0` | No HDC intervention by ESP @HDC lamp is OFF |
| `1` | HDC Intervention by ESP is active @HDC lamp is blinking |

### `$3508` — Feedback signal status from ESP to enable sports mode

| Value | Meaning |
|-------|---------|
| `0` | Signal value is correct |
| `1` | Reserved |
| `2` | Signal value is implausible |
| `3` | Signal is not available |

### `$3509` — Feedback signal from ESP to enable sports mode

| Value | Meaning |
|-------|---------|
| `0` | Default |
| `1` | Sports |

### `$350B` — Gear Selection Switch State Status

| Value | Meaning |
|-------|---------|
| `0` | Info Is Correct |
| `1` | Reserved |
| `2` | Signal value is implausible |
| `3` | Signal is not available |

### `$350C` — Remote Immo Request from TCU

| Value | Meaning |
|-------|---------|
| `0` | No Request |
| `1` | Mobilize Request |
| `2` | Immobilize Request |
| `3` | Reserved |

### `$350D` — Cruise Switch (Acc/ Dec) selection State

| Value | Meaning |
|-------|---------|
| `0` | No Key Pressed |
| `1` | Resume / Cruise SPD + Key Pressed |
| `2` | Set /Cruise SPD - Pressed |
| `3` | Invalid Key Pressed |

### `$350E` — Cruise state VCU (Disabled/ Not active/ Active)

| Value | Meaning |
|-------|---------|
| `0` | Cruise Is Disabled |
| `1` | Cruise Is Enabled And Active |
| `2` | Cruise Is Enabled And Not Active |
| `3` | Reserved |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Information Initialization In Progress |

### `$350F` — Drive Mode Selection Switch State by driver

| Value | Meaning |
|-------|---------|
| `0` | City |
| `1` | Eco |
| `2` | Sport |
| `3` | Reserved |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$3510` — Regeneration level for Display from VCU

| Value | Meaning |
|-------|---------|
| `0` | Level 0 |
| `1` | Level 1 |
| `2` | Level 2 |
| `3` | Level 3 |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Information Initialization In Progress |

### `$3511` — Drive Mode (City/Eco/Sport) for Display from VCU

| Value | Meaning |
|-------|---------|
| `0` | City |
| `1` | Eco |
| `2` | Sport |
| `3` | Reserved |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Information Initialization In Progress |

### `$3512` — Cruise function state of vehicle determined by VCU

| Value | Meaning |
|-------|---------|
| `0` | Cruise OFF |
| `1` | Cruise ON |
| `2` | Cruise Activated |
| `3` | Cruise Resumed |
| `4` | Cruise Cancelled |
| `5` | Cruise Control Override |
| `6` | Cruise selection not possible |
| `7` | Maximum speed limit reached |
| `8` | Minimum speed limit reached |

### `$3513` — Drive Mode selection (possible/not possible)

| Value | Meaning |
|-------|---------|
| `0` | Mode Selection Not Possible |
| `1` | Mode Selection Possible |

### `$3514` — Regen Level selection (possible/not possible)

| Value | Meaning |
|-------|---------|
| `0` | Regen Level Selections Possible |
| `1` | Regen Level Selections Not Possible |
| `2` | Regen Level Max Limit Reached |
| `3` | Regen Level Min Limit Reached |

### `$3518` — Recuperation brake state from VCU (Enable/Disable)

| Value | Meaning |
|-------|---------|
| `0` | Recuperation Disabled |
| `1` | Recuperation Enabled |

### `$351A` — Co-driver door state from BCM

| Value | Meaning |
|-------|---------|
| `0` | Close |
| `1` | Open |

### `$351B` — Driver door state from BCM

| Value | Meaning |
|-------|---------|
| `0` | Close |
| `1` | Open |

### `$351C` — Rear right side door state from BCM

| Value | Meaning |
|-------|---------|
| `0` | Close |
| `1` | Open |

### `$351D` — Rear left side door state from BCM

| Value | Meaning |
|-------|---------|
| `0` | Close |
| `1` | Open |

### `$351E` — CCS Charge Permission (EV ready)

| Value | Meaning |
|-------|---------|
| `0` | Reserved |
| `1` | Reserved |
| `2` | HV Ready |
| `3` | Driving |
| `4` | Reserved |
| `5` | Charge Permission |
| `6` | Shutdown |

### `$3524` — CCS Isolation check status from station

| Value | Meaning |
|-------|---------|
| `0` | Isolation Status Invalid |
| `1` | Isolation Check Valid |

### `$3525` — Powertrain VCU Function Mode

| Value | Meaning |
|-------|---------|
| `0` | Awake |
| `1` | Active |
| `2` | Crank |
| `3` | Normal Run |
| `4` | Energy Recuperation |
| `5` | Charging Mode |
| `6` | Limited Power Mode |
| `7` | Remote AC |
| `8` | Remote Immo |
| `9` | Reserved |
| `0x0A` | Reserved |

### `$3526` — Compressor State

| Value | Meaning |
|-------|---------|
| `0` | Ready |
| `1` | Retention |
| `2` | Enable |
| `3` | Retention |
| `4` | PowerUp |
| `5` | Error |

### `$352C` — Regeneration selection switch state from IPC to VCU

| Value | Meaning |
|-------|---------|
| `0` | Level 0 |
| `1` | Level 1 |
| `2` | Level 2 |
| `3` | Level 3 |

### `$352D` — Cruise Speed change state from IPC to VCU

| Value | Meaning |
|-------|---------|
| `0` | No Change |
| `1` | Cruise Speed Increment |
| `2` | Cruise Speed Decrement |
| `3` | Reserved |

### `$352E` — Cruise Switch Selection State from IPC to VCU

| Value | Meaning |
|-------|---------|
| `0` | Cruise OFF |
| `1` | Cruise Set |
| `2` | Cruise Activated |
| `3` | Cruise Resumed |
| `4` | Cruise Cancelled |
| `5` | Cruise Selections not possible |

### `$3530` — Charging current limitation source (Slow Charging)

| Value | Meaning |
|-------|---------|
| `0` | Charging Inactive |
| `1` | Current limited by BMS |
| `2` | VCU Power Cap for WallMount for 3in1 |
| `3` | Power Cap by 3in1 Unit |
| `4` | VCU Power Cap for HomeCharger for 3in1 |
| `5` | Power cap by CP Dutycycle for 3in1 |
| `6` | Current limited by CP Dutycycle |
| `7` | VCU Power Cap for Homecharger for OBC |
| `8` | VCU Power Cap for WallMount for OBC |

### `$3531` — BMS Key On feedback to VCU

| Value | Meaning |
|-------|---------|
| `0` | Offline |
| `1` | Online |

### `$3532` — BMS FC +ve Relay feedback to VCU

| Value | Meaning |
|-------|---------|
| `0` | Open |
| `1` | Closed |

### `$3533` — BMS FC -ve Relay feedback to VCU

| Value | Meaning |
|-------|---------|
| `0` | Open |
| `1` | Closed |

### `$3534` — Fast charging Relay Close Cmd from VCU to BMS

| Value | Meaning |
|-------|---------|
| `0` | Open |
| `1` | Closed |

### `$3535` — Fast charging gun detection Flag signal from VCU to BMS

| Value | Meaning |
|-------|---------|
| `0` | Open |
| `1` | Closed |

### `$3546` — MSS Position Request To VCU

| Value | Meaning |
|-------|---------|
| `2` | Neutral |
| `3` | Drive |
| `4` | Sport |
| `5` | Park Repeat |
| `6` | Reverser Repeat |
| `7` | Neutral Repeat |
| `8` | Drive Repeat |
| `9` | Sport Repeat |
| `12` | Interrupt 1 |
| `13` | Interrupt 2 |
| `14` | Fault |
| `15` | Un-confirmed Position |
| `0` | Park |
| `1` | Reverse |

### `$3547` — MSS Position Display From VCU To MSS

Mask `0xFFFF`.

| Value | Meaning |
|-------|---------|
| `80` | Park |
| `100` | Reverse |
| `200` | Neutral |
| `400` | Drive |

### `$3549` — Regen Level Via Paddle Shifter

| Value | Meaning |
|-------|---------|
| `0` | Inactive |
| `1` | Up Active |
| `2` | Down Active |
| `3` | Up & Down Active |
| `4` | Invalid |

### `$3569` — Adaptive Cruise Control Engaged from ESP

| Value | Meaning |
|-------|---------|
| `1` | Off |
| `1` | On |

### `$356B` — ACC Torque Request Enable from ESP

| Value | Meaning |
|-------|---------|
| `0` | No Request |
| `1` | Enable ACC Torque Req |

### `$356C` — Adaptive Cruise Control State from VCU

| Value | Meaning |
|-------|---------|
| `0` | Inactive |
| `1` | Active |
| `2` | Fault |
| `3` | ACC Switch failure |

### `$356D` — Park Brake State from VCU

| Value | Meaning |
|-------|---------|
| `0` | Disengaged |
| `1` | Engaged |

### `$356E` — Slow Charging Full Required

| Value | Meaning |
|-------|---------|
| `0` | Slow chargingFull_Notrequired |
| `1` | Slow chargingFull_required |

### `$356F` — DCFC Contactor State

| Value | Meaning |
|-------|---------|
| `0` | No weld |
| `1` | Single weld |
| `2` | Double weld |
| `3` | Reserved |
| `0` | No weld |
| `1` | Single weld |
| `2` | Double weld |
| `3` | Reserved |

### `$3570` — Adaptive Cruise Control System State to VECU

| Value | Meaning |
|-------|---------|
| `0` | OFF |
| `1` | ACC_Enable |
| `2` | ACC_Engaged |
| `3` | ACC_Engaged_brakeonly |
| `4` | ACC_Cancle |
| `5` | NCC_Enabled |
| `6` | NCC_Engaged |
| `7` | NCC_Cancel |
| `8` | HAS_BSOC |
| `9` | Reserved |
| `0x0A` | Reserved |
| `0x0B` | Reserved |
| `0x0C` | Reserved |
| `0x0D` | Reserved |
| `0x0E` | Reserved |
| `0x0F` | SNA |

### `$3571` — ADAS Steering Switch State Ack to VECU

| Value | Meaning |
|-------|---------|
| `0` | No Action Required |
| `1` | ACC ON/OFF Pressed |
| `2` | ACC Cancel/Resume Pressed |
| `3` | SET+Short Press |
| `4` | SET+Long Press |
| `5` | SET-Short Press |
| `6` | SET-Long Press |
| `7` | ACC Timegap/Headway Gap short pressed |
| `8` | LKA Switch Pressed |
| `9` | ACC Timegap/Headway Gap  Long pressed |
| `0x0A` | Reserved |
| `0x0B` | Reserved |
| `0x0C` | Reserved |
| `0x0D` | Reserved |
| `0x0E` | Reserved |
| `0x0F` | Reserved |

### `$3591` — Diagnostic State from ESP to VECU

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Reserved |
| `2` | Dynomode or Diagnostic session running |
| `3` | Reserved |

### `$3592` — Regen Brake Lamp Request

| Value | Meaning |
|-------|---------|
| `0` | Brake Lamp Requested |
| `1` | Brake Lamp Not Requested |

### `$3593` — VCU Cruise Mode to IPC

Mask `0xFFFF`.

| Value | Meaning |
|-------|---------|
| `0` | PCC OFF for ADAS |
| `1` | PCC ON for ADAS |
| `2` | PCC ON for Non-ADAS |
| `3` | Reserved |

### `$3596` — MCU  Inverter 12V supply

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | Fault |

### `$359A` — Tin1 Charging CC status

| Value | Meaning |
|-------|---------|
| `0` | Invalid |
| `1` | Valid |

### `$359E` — BMS Negative Contactor Weld Fault

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | Third Fault Level(high) |

### `$359F` — BMS Positive Contactor Weld Fault

| Value | Meaning |
|-------|---------|
| `0` | No Fault |
| `1` | Third Fault Level(high) |

### `$35A0` — BCM Crank Source

| Value | Meaning |
|-------|---------|
| `0` | Don’t care |
| `1` | Driver inside vehicle(SSB) |
| `16` | Remote Crank with Keyfob |
| `17` | Remote Crank with Keyfob |

### `$35B5` — 3in1 OBC operation mode

| Value | Meaning |
|-------|---------|
| `0` | Init State |
| `1` | Charge State |
| `2` | VTOL Discharge state |
| `3` | VTOV Discharge state |

### `$35B9` — Compressor status

| Value | Meaning |
|-------|---------|
| `0` | Compressor off |
| `1` | Compressor on |
| `2` | Compressor limit |
| `3` | Pre-Heating |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Init |
| `7` | Failure of EAC |

### `$35BC` — APA System State

| Value | Meaning |
|-------|---------|
| `0` | Available |
| `1` | Not Available |
| `2` | Initialization |
| `3` | Erroneous |
| `4` | N/a |
| `5` | N/a |
| `6` | N/a |
| `7` | Reserved |

### `$35C3` — TP2 Status from ESP

| Value | Meaning |
|-------|---------|
| `0` | TP2 Function fully available@TP2 failure lamp is OFF. |
| `1` | TP2 Switched off due to system passive request or switch. |
| `2` | TP2 function switched off due to detected failure at ESP system@TP2. |
| `3` | TP2 detectivation due to vehicle speed more than 60 kmph. |

### `$35C4` — TP2 Active from ESP

| Value | Meaning |
|-------|---------|
| `0` | No TP2 intervention by ESP system @TP2 lamp is OFF |
| `1` | TP2 is active but control neccesary/TP2 Standby |
| `2` | TP2 is active and  in control |
| `3` | Signal not available |

### `$35C5` — APA Torque Reqeust enable from ESP

| Value | Meaning |
|-------|---------|
| `0` | No request |
| `1` | Enable APA torque request for park functions. |

### `$35C6` — APA Interface from ESP

| Value | Meaning |
|-------|---------|
| `0` | Idle |
| `1` | Enabled |
| `2` | control |
| `3` | Fault |

### `$35C7` — Driving Direction Request from ESP

| Value | Meaning |
|-------|---------|
| `0` | Neutral |
| `1` | Forward |
| `2` | Reverse |
| `3` | Park |

### `$35C8` — VCU Minimum Maximum Mode from ESP

| Value | Meaning |
|-------|---------|
| `0` | VCEU should not react to ESP torque requests. Only Driver should be respected |
| `1` | VCEU should realize maximum of driver and ESP requested torque |
| `2` | VCEU should realize minimum of driver and ESP requested torque |
| `3` | No Reaction on driver throttle. Only ESP requested torque should be Respected. |

### `$35CA` — Mode Detect signal from ESP

| Value | Meaning |
|-------|---------|
| `0` | Normal/City |
| `1` | Economy |
| `2` | Sports |
| `3` | Boost |
| `4` | Wet/Rain |
| `5` | Rough |
| `6` | Stand |
| `7` | Rock |
| `8` | Snow |
| `9` | Mud |
| `10` | Reserved |
| `11` | Reserved |
| `12` | Reserved |
| `13` | Reserved |
| `14` | Reserved |
| `15` | Reserved |

### `$35CB` — Mode Detect signal from ESP Status

| Value | Meaning |
|-------|---------|
| `0` | Signal value is correct |
| `1` | Reserved |
| `2` | Signal value is implausible |
| `3` | Signal is not available |

### `$35CE` — Mode Detect signal from TAS

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Wet/Rain |
| `2` | Rough |
| `3` | Sand |
| `4` | Rock |
| `5` | Snow |
| `6` | Mud |
| `7` | Reserved |

### `$35CF` — Drive mode Display from VCU

| Value | Meaning |
|-------|---------|
| `0` | Normal/City |
| `1` | Economy |
| `2` | Sports |
| `3` | Boost |
| `4` | Wet/Rain |
| `5` | Rough |
| `6` | Sand |
| `7` | Rock |
| `8` | Snow |
| `9` | Mud |
| `10` | Reserved |
| `11` | Drift Mode |
| `12` | Reserved |
| `13` | Reserved |
| `14` | Reserved |
| `15` | Reserved |

### `$35D0` — Apa Interface from VCU to ESP

| Value | Meaning |
|-------|---------|
| `0` | Idle |
| `1` | Enabled |
| `2` | Control |
| `3` | Fault |

### `$35D1` — Electric Motor State from VCU

| Value | Meaning |
|-------|---------|
| `0` | Torque request possible at both Motor,PM and SM |
| `1` | Torque request possible at PM but not in SM |
| `2` | Torque request possible at SM But not in PM |
| `3` | Torque request  not possible at both Motor,PM and SM due to failure |

### `$35D2` — TP2  Interface from VCU to ESP

| Value | Meaning |
|-------|---------|
| `0` | Default |
| `1` | Fault |
| `2` | Limp home due to SOC |
| `3` | Signal not available |

### `$35D6` — Mode Selection switch State from VCU

| Value | Meaning |
|-------|---------|
| `7` | Rock |
| `8` | Snow |
| `9` | Mud |
| `10` | Reserved |
| `11` | Reserved |
| `12` | Reserved |
| `13` | Reserved |
| `14` | Reserved |
| `15` | Reserved |
| `1` | Economy |
| `2` | Sports |
| `3` | Boost |
| `4` | Wet/Rain |
| `5` | Rough |
| `6` | Sand |

### `$35D7` — Primary motor State

| Value | Meaning |
|-------|---------|
| `0` | off |
| `1` | Initialization |
| `2` | Precharge |
| `3` | Standby |
| `4` | FWD Torque control |
| `5` | REV torque control |
| `6` | Speed control |
| `7` | Discharge |
| `8` | Afterrun |
| `9` | Fault |
| `10` | MCU Crash status |
| `11` | Chraging mode |
| `12` | Safe- state-IVN-VCU |
| `13` | Reserved |
| `14` | Reserved |
| `15` | MTrlnitAngleCali |

### `$35D9` — Primary motor Active Short Circuit Status

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | On |

### `$35DD` — Secondary motor  State

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Initialization |
| `2` | Precharge |
| `3` | Standby |
| `4` | FWD Torque control |
| `5` | REV Torque control |
| `6` | Speed control |
| `7` | Discharge |
| `8` | Afterrun |
| `9` | Fault |
| `10` | MCU Crash status |
| `11` | Charging Mode |
| `12` | Safe- state-IVN-VCU |
| `13` | Reserved |
| `14` | Reserved |
| `15` | MtrlnitAngleCali |

### `$35DF` — Secondary motor Faultgrade

| Value | Meaning |
|-------|---------|
| `0` | No fault |
| `1` | Level 1 fault |
| `2` | Level 2 fault |
| `3` | Level 3 fault |

### `$35E0` — Secondary motor 12V SuppyLV

| Value | Meaning |
|-------|---------|
| `0` | No fault |
| `1` | Fault |

### `$35EC` — Custom Terrain Mode acknowlegment from ESP

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Normal |
| `2` | Grass/Snow |
| `3` | Mudruts |
| `4` | Sand |
| `5` | Rock Crawl |
| `6` | Wet/Rain |
| `7` | Rough |
| `8` | Reserved |
| `9` | Reserved |
| `10` | Reserved |
| `11` | Reserved |
| `12` | Reserved |
| `13` | Reserved |
| `14` | Reserved |
| `15` | Reserved |

### `$35ED` — Custom Steering Mode acknowlegment from EPAS

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Comfort |
| `2` | Sporty |
| `3` | Reserved |

### `$35EE` — Custom Mode HU request Type from HU  to VCU

| Value | Meaning |
|-------|---------|
| `0` | Unused |
| `1` | User setting update |
| `2` | Reset to defaults |
| `3` | Config  query |
| `4` | Config Ack |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$35EF` — Custom Drive mode  user request from HU to  VCU

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Eco |
| `2` | City |
| `3` | Sports |
| `4` | Boost |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$35F0` — Custom  terrain  Mode user request from HU to VCEU

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Normal |
| `2` | Grass/Snow |
| `3` | Mudruts |
| `4` | Sand |
| `5` | Rock Crawl |
| `6` | Wet/Rain |
| `7` | Rough |
| `8` | Reserved |
| `9` | Reserved |
| `10` | Reserved |
| `11` | Reserved |
| `12` | Reserved |
| `13` | Reserved |
| `14` | Reserved |
| `15` | Reserved |

### `$35F1` — Custom steering Mode user request from HU to VCEU

| Value | Meaning |
|-------|---------|
| `0` | Don’t care |
| `1` | Comfort |
| `2` | Sporty |
| `3` | Reserved |

### `$35F2` — Custom Regen Mode user request from HU to  VCEU

| Value | Meaning |
|-------|---------|
| `0` | Don’t care |
| `1` | Off |
| `2` | Level1 |
| `3` | Level2 |
| `4` | Level3 |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$35F3` — Custom Mode combined User request from HU to VCU

| Value | Meaning |
|-------|---------|
| `3` | HuVcuCuDriveModeUserReq_DID |
| `4` | HuVcuCuTerrainModeUserReq_DID |
| `5` | HuVcuCuSteeringModeUserReq_DID |
| `6` | HuVcuCuRegenModeUserReq_DID |

### `$35F4` — Custom mode status request from VCU

| Value | Meaning |
|-------|---------|
| `0` | SNA |
| `1` | Plausible |
| `2` | Value is implausible |

### `$35F5` — Custom steering mode request from VCU

| Value | Meaning |
|-------|---------|
| `0` | SNA |
| `1` | Plausible |
| `2` | Value is implausible |
| `3` | Reserved |

### `$35F6` — Custom terrain mode status request from VCU

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Normal |
| `2` | Grass/Snow |
| `3` | Mudruts |
| `4` | Sand |
| `5` | Rock Crawl |
| `6` | Wet/Rain |
| `7` | Rough |

### `$35F7` — Custom Mode combined Request from VCU to Partner ECU

| Value | Meaning |
|-------|---------|
| `3` | VcuCuModeState + Status |
| `4` | VcuCuSteeringModeReqState_DID |
| `5` | VcuCuTerrainModeReqState_DID |

### `$35F8` — Custom terrain mode current state from VCU

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Normal |
| `2` | Grass/Snow |
| `3` | Mudruts |
| `4` | Sand |
| `5` | Rock Crawl |
| `6` | Wet/Rain |
| `7` | Rough |

### `$35F9` — Custom Drive mode current  state from VCU

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Eco |
| `2` | City |
| `3` | Sports |
| `4` | Boost |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$35FA` — Custom steering mode current state from VCU

| Value | Meaning |
|-------|---------|
| `0` | Don't Care |
| `1` | Comfort |
| `2` | Sporty |
| `3` | Reserved |

### `$35FB` — Custom Regen mode current state from VCU

| Value | Meaning |
|-------|---------|
| `0` | Don’t care |
| `1` | Off |
| `2` | Level1 |
| `3` | Level2 |
| `4` | Level3 |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$35FC` — Custom mode combined Current State from VCU

| Value | Meaning |
|-------|---------|
| `3` | VcuHuCuTerrainModeCurrState_DID |
| `4` | VcuHuCuDriveModeCurrState_DID |
| `5` | VcuHuCuSteeringModeCurrState_DID |
| `6` | VcuHuCuRegenModeCurrState_DID |

### `$35FD` — Custom Mode saved settings in VECU for drive and regen mode

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Regen off | `0` |  |
|  |  | `1` | Enabled |
| `0x02` | Regen 1 | `0` |  |
|  |  | `2` | Enabled |
| `0x04` | Regen 2 | `0` |  |
|  |  | `4` | Enabled |
| `0x08` | Regen 3 | `0` |  |
|  |  | `8` | Enabled |
| `0x10` | Normal/City | `0` |  |
|  |  | `10` | Enabled |
| `0x20` | ECO | `0` |  |
|  |  | `20` | Enabled |
| `0x40` | Sports | `0` |  |
|  |  | `40` | Enabled |
| `0x80` | Boost | `0` |  |
|  |  | `80` | Enabled |

### `$35FE` — Custom Mode saved settings in VECU for steering

| Value | Meaning |
|-------|---------|
| `0` | Comfort |
| `1` | Sporty |
| `2` | Reserved |
| `3` | Reserved |
| `4` | Reserved |

### `$35FF` — Custom Mode saved settings in VECU for AWD 4×4

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Grass/Snow |
| `2` | Mud |
| `3` | Rock |
| `4` | Sand |
| `5` | Reserved |

### `$3600` — Custom Mode saved settings in VECU for RWD 4×2

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` |  |
| `1` | Enabled |

### `$3601` — VCU-HU Autolearning Status of Custom Mode

| Value | Meaning |
|-------|---------|
| `0` | Start |
| `1` | In process |
| `2` | Completed |
| `3` | Failed |

### `$3602` — Custom Mode Settings update State

| Value | Meaning |
|-------|---------|
| `0` | Don’t Care |
| `1` | Save Successfully |
| `2` | Save failed |
| `3` | Reset ro default Successful |
| `4` | Reset ro default failed |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$3603` — Custom Mode Execution State

| Value | Meaning |
|-------|---------|
| `0` | Custom Mode Inactive |
| `1` | TAS request Recevied |
| `2` | pre-conditions Check |
| `3` | Command send to partner ECU |
| `4` | ESP ACK received |
| `5` | EPAS ACK received |
| `6` | Custom Mode activated |
| `7` | Reserved |

### `$3604` — Custom Mode denied Reason

| Value | Meaning |
|-------|---------|
| `0` | Custom Mode possible |
| `1` | Pre-conditions not satisfied |
| `2` | Ack not received by partner ECU |
| `3` | Powertrain internal fault |
| `4` | mode not applicable |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Reserved |

### `$4223` — Status of AES-SK SecretKey

| Value | Meaning |
|-------|---------|
| `0` | Unlocked |
| `1` | Locked |

### `$4223` — Status of AES-SK SecretKey

| Value | Meaning |
|-------|---------|
| `0` | Locked |
| `0` | Locked |

### `$720C` — Diagnostic Gateway State

| Value | Meaning |
|-------|---------|
| `0` | Inactive |
| `1` | Active |

### `$7211` — Communication Status

| Value | Meaning |
|-------|---------|
| `0` | RX and TX Enabled |
| `1` | RX Enabled and TX Disabled |
| `2` | RX Disabled and TX Enabled |
| `3` | RX Disabled and TX Disabled |

### `$7215` — Active Software Component

| Value | Meaning |
|-------|---------|
| `1` | Application |
| `2` | Bootloader |

### `$A006` — Vehicle Power mode

| Value | Meaning |
|-------|---------|
| `0` | Pre Stand By |
| `1` | Awake |
| `2` | Transport Park |
| `3` | KeyIn |
| `4` | Accessory |
| `5` | Accessory Delay |
| `6` | Active (Ignition ON) |
| `7` | Transport Drive |
| `8` | Run (Engine Run) |
| `9` | Start (Crank) |
| `0x0A` | Transport Drive Crank |
| `0x0B` | Transport Drive Run |

### `$F186` — Active Diagnostic Session

| Value | Meaning |
|-------|---------|
| `1` | Default Session |
| `2` | Programming Session |
| `3` | Extended Session |

## BMS — Battery Management System

Request `0x785` · response `0x78D` · 9 enumerated DIDs, 3 of them bit-packed.

### `$3404` — BMS Main Positive Relay Status

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | BDU Precharge Relay Status | `1` | Closed |
|  |  | `0` | Open |
| `0x02` | BDU Main Negative Relay Status | `2` | Closed |
|  |  | `0` | Open |
| `0x04` | BDU Main Postive Relay Status | `4` | Closed |
|  |  | `0` | Open |

### `$3405` — BMS Initialization State

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$340E` — BMS Operation Mode

| Value | Meaning |
|-------|---------|
| `0` | Power-on self-test |
| `1` | Stand By |
| `2` | Precharge |
| `3` | Hvactive |
| `4` | HVPowerdown |
| `5` | PreChargeFailure |
| `6` | Fault |
| `0x0F` | Ready to Sleep |

### `$340F` — BMS Derate Flag

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$3414` — BMS Insulation Enable/Disable Command

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `4` | Enable |

### `$3479` — BMS Cell Balance Status Flag

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | VCU HVIL Detect Signal | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | VCU Insulation Control Command | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | BMS Cell Balance Status Flag | `0` | Disable |
|  |  | `1` | Enable |

### `$3493` — VCU Flag

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | VCU Fast Charging Flag | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Charging Enabled Status | `2` | Enable |
|  |  | `0` | Disable |
| `0x04` | VCU Slow Charging Flag | `4` | Enable |
|  |  | `0` | Disable |

### `$353A` — Smoke Sensor Logic

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$7220` — Configuration Data - Tester Serial Number

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `10` | Enable |

## OBC — On Board Charger

Request `0x786` · response `0x78E` · 29 enumerated DIDs, 2 of them bit-packed.

### `$0210` — CP line status

| Value | Meaning |
|-------|---------|
| `0` | not connected |
| `1` | connected |
| `2` | Voltage/Frequency error |
| `3` | indeterminate |

### `$0211` — Charger state

| Value | Meaning |
|-------|---------|
| `1` | Init |
| `2` | Standby |
| `4` | Charging |
| `8` | Fault |
| `0x0B` | Shutdown |
| `0x0C` | Off |
| `0x0F` | Diagnostic |

### `$0212` — Internal CAN status

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0213` — External CAN status

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0214` — External CAN transceiver status

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0215` — FEE status

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0216` — EEPROM Write Error

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0217` — Internal SCI communication FAIL

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0218` — Internal SCI communication CRC FAIL

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Failed |

### `$0219` — OBD sensors

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0xFF` | 12V Primary sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | 3.3V Primary sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | 3.3V Secondary sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Tranasformer temperature sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Secondary board temperature sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output voltage sensor 3 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output voltage sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output current sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output current sensor 1 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Primary board temperature sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | VBOVS sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | VB PFC voltage sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | AC current sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | AC current sensor 1 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | PH2MEAS input AC voltage sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | PH2MEAS input AC voltage sensor 1 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |

### `$131` — VCU Charging Enable Command

| Value | Meaning |
|-------|---------|
| `0` | Init |
| `1` | Standby |
| `2` | Working |
| `5` | Sleep |

### `$1319` — internal CAN Communication states

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$131D` — HW wakeup output state

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$131E` — VCU CRC FAIL for OBC

| Value | Meaning |
|-------|---------|
| `1` | Error |
| `0` | Normal |

### `$132` — OBC Charging states

| Value | Meaning |
|-------|---------|
| `0` | Init |
| `1` | Standby |
| `2` | Working |
| `4` | Fault |
| `5` | Sleep |

### `$1320` — OBC Internal fault

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$1321` — OBC External CAN status

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$1322` — OBC derating mode

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `01` | Derating |

### `$1323` — OBC error flag

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$1324` — OBC CC ohm

| Value | Meaning |
|-------|---------|
| `00` | Reserved |
| `01` | not connected |
| `02` | semi-jion |
| `03` | 1500 Ohm |
| `04` | 680 Ohm |
| `05` | 220 Ohm |
| `06` | 100 Ohm |
| `07` | CC Ground |

### `$133` — VCU CC CP Connect states

| Value | Meaning |
|-------|---------|
| `0` | No Detected |
| `1` | Detected |

### `$1330` — VCU Charging/Discharge mode command

| Value | Meaning |
|-------|---------|
| `0` | init mode |
| `1` | Charge mode |
| `2` | VTOL Discharge mode |
| `3` | VTOV Discharge Mode |
| `4` | Reserved |
| `5` | Reserved |
| `6` | Reserved |
| `7` | Invalid |

### `$1331` — VCU Discharge gun connect status

| Value | Meaning |
|-------|---------|
| `0` | init |
| `1` | VTOV Discharge Gun Connect |
| `2` | VTOL Discharge Gun Connect |
| `3` | Discharge gun Disconnect |
| `4` | Discharge gun Half connect |

### `$1332` — OBC Operation Mode

| Value | Meaning |
|-------|---------|
| `0` | Init state |
| `1` | Charge state |
| `2` | VTOL Discharge state |
| `3` | VTOV Discharge state |

### `$134` — CP Duty Error States

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$135` — CP Frequency Error States

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$1904` — Snapshot Data

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0xFF` | Internal CAN status | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | External CAN status | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | External CAN transceiver status | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | FEE status | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | EEPROM Write Error | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | Internal SCI communication FAIL | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | Internal SCI communication CRC FAIL | `0` | Normal |
|  |  | `1` | Failed |
| `0xFF` | 12V Primary sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | 3.3V Primary sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | 3.3V Secondary sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Tranasformer temperature sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Secondary board temperature sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output voltage sensor 3 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output voltage sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output current sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Output current sensor 1 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | Primary board temperature sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | VBOVS sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | VB PFC voltage sensor | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | AC current sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | AC current sensor 1 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | PH2MEAS input AC voltage sensor 2 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |
| `0xFF` | PH2MEAS input AC voltage sensor 1 | `0` | Normal |
|  |  | `1` | Too low out of range |
|  |  | `2` | Too high out of range |
|  |  | `3` | Short to ground |
|  |  | `4` | Short to battery |

### `$2100` — HW wakeup output state

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | ON |

### `$F186` — ActiveDiagnosticSessionDataIdentifier

| Value | Meaning |
|-------|---------|
| `1` | Default Session |
| `2` | Programming Session |
| `3` | Extended Diagnostic Session |

## DCDC — DC-DC Converter

Request `0x784` · response `0x78C` · 8 enumerated DIDs, 0 of them bit-packed.

### `$04-` — Snapshot Data

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | DCDC_INIT_STATE = 0 |
| `1` | DCDC_STANDBY_STATE |
| `2` | DCDC_WORK_STATE |
| `5` | DCDC_SLEEP_STATE |
| `6` | DCDC_RECV_FAULT_STATE |
| `7` | DCDC_LOCK_FAULT_STATE |

### `$0C0` — DCDC work status

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | DCDC_INIT_STATE = 0 |
| `1` | DCDC_STANDBY_STATE |
| `2` | DCDC_WORK_STATE |
| `5` | DCDC_SLEEP_STATE |
| `6` | DCDC_RECV_FAULT_STATE |
| `7` | DCDC_LOCK_FAULT_STATE |

### `$0C2` — DCDC derating mode

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Derating mode |

### `$130E` — DCDC working states

| Value | Meaning |
|-------|---------|
| `0` | standby |
| `1` | Working |
| `2` | sleep |
| `3` | Fault |

### `$130F` — DCDC Enable Command

| Value | Meaning |
|-------|---------|
| `00` | Disable |
| `1` | Enable |
| `2` | Sleep |

### `$1325` — DCDC derating mode

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `01` | Derating |

### `$1326` — DCDC error flag

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

### `$1329` — DCDC VCU CRC FAIL

| Value | Meaning |
|-------|---------|
| `0` | Normal |
| `1` | Error |

## BCM — Body Control Module

Request `0x701` · response `0x709` · 148 enumerated DIDs, 58 of them bit-packed.

### `$4004` — Crash_State

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Inertia Switch | `0` | No |
|  |  | `1` | Digital Input |
| `0x02` | Impact Detected PWM Input | `0` | No |
|  |  | `2` | Digital Input |
| `0x04` | Impact Detected CAN Signal Input | `0` | No |
|  |  | `4` | Digital Input |

### `$4005` — Crash input configuration

Mask `0x07`.

| Value | Meaning |
|-------|---------|
| `0` | Digital |
| `1` | PWM |
| `2` | CAN |
| `3` | PWM + CAN |
| `4` | Digital + CAN |

### `$4201` — Central Locking 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Driver Door Lock Signal | `0` | Signal Absent |
|  |  | `1` | Signal Present |
| `0x02` | Driver Door Unlock Signal | `0` | Signal Absent |
|  |  | `2` | Signal Present |

### `$4202` — Central Locking 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Tailgate Lock Signal | `0` | Signal Absent |
|  |  | `1` | Signal Present |
| `0x02` | Tailgate Unlock Signal | `0` | Signal Absent |
|  |  | `1` | Signal Present |
| `0x03` | DoubleLockFeedbackSignal | `0` | Signal Absent |
|  |  | `1` | Signal Present |
| `0x04` | DoubleUnlockFeedbackSignal | `0` | Signal Absent |
|  |  | `1` | Signal Present |

### `$4203` — Central Locking 3

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Door Lock Relay | `0` | Off |
|  |  | `1` | On |
| `0x02` | Door Unlock Relay | `0` | Off |
|  |  | `2` | On |
| `0x04` | Door Double Lock Relay | `0` | Off |
|  |  | `4` | On |
| `0x08` | Door Double Unlock Relay | `0` | Off |
|  |  | `8` | On |
| `0x10` | Door Lock Relay Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Door Unlock Relay Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Door Double Lock Relay Masking | `0` | Not Supported |
|  |  | `40` | Supported |
| `0x80` | Door Double Unlock Relay Masking | `0` | Not Supported |
|  |  | `80` | Supported |

### `$4205` — Cabin Global Closing Signal

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Cabin Global Closing Signal | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Sun Roof Configuration | `0` | Not Present |
|  |  | `2` | Present |
| `0x0C` | HW Global Closing Pulse Type | `0` | Inactive |
|  |  | `4` | 20ms Active |
|  |  | `8` | 500ms Active |
|  |  | `0x0C` | Reserve |
| `0x10` | Sun Roof Tilt Function Enable/Disable | `0` | Not Present |
|  |  | `10` | Present |

### `$4207` — CDL Switch Input 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Bonnet Switch | `0` | Closed |
|  |  | `1` | Open |
| `0x02` | Passenger Door Switch | `0` | Closed |
|  |  | `2` | Open |
| `0x04` | Rear Left Door Switch | `0` | Closed |
|  |  | `4` | Open |
| `0x08` | Rear Right Door Switch | `0` | Closed |
|  |  | `8` | Open |
| `0x10` | Tailgate/Boot Switch | `0` | Closed |
|  |  | `10` | Open |

### `$4208` — Drivers Door Switch

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Closed |
| `1` | Open |

### `$420B` — CDL Switch Input 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Internal Lock Switch | `0` | Off |
|  |  | `1` | On |
| `0x02` | Internal Unlock Switch | `0` | Off |
|  |  | `2` | On |
| `0x04` | Fuel Flap Switch | `0` | Off |
|  |  | `4` | On |
| `0x10` | Internal Lock Switch Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Internal Unlock Switch Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Fuel Flap Switch Masking | `0` | Not Supported |
|  |  | `40` | Supported |

### `$420C` — Door other state

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Closed |
| `1` | Open |

### `$420E` — Vehicle Type Configuration For Auto Up

| Value | Meaning |
|-------|---------|
| `0` | Bolt/Zest/Tiago/Tigor |
| `1` | Hexa |
| `2` | Osprey |
| `3` | Q501 High End |
| `4` | Q501 Low End |
| `5` | X451 High End |
| `6` | X451 Low End |
| `7` | X445 High End |
| `8` | X445 Low End |
| `9` | Q502 -High End |
| `0x0A` | Q502 -Low End |
| `0x0B` | Nexon MCE |
| `0x0C` | Nexon EV |
| `0x0D` | Tiago/Tigor MCE |
| `0x0E` | X445_EV High End |
| `0x0F` | X445_EV Low End |
| `10` | Mid SUV High End |
| `11` | Mid SUV Low End |
| `12` | Nexon MCE2 High End |
| `13` | Nexon MCE2 Low End |
| `14` | Q501 Q502 MCE High End |
| `15` | Q501 Q502 MCE Low End |
| `16` | Mid SUV MID End |
| `17` | Nexon MCE2 MID End |
| `18` | Q501 Q502 MCE MID End |

### `$4220` — PEPS_BCM Encrypted Communication State (Readable,Snapshot relevant)

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4223` — AES-SK Lock Status

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `1` | Lock |
| `0` | Unlock |

### `$4226` — BCM with express down

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4234` — Tailgate Unlock Switch Configuration

| Value | Meaning |
|-------|---------|
| `0` | No Tailgate Unlock Switch |
| `1` | Internal Tailgate Unlock Switch |
| `2` | External Tailgate Unlock Switch |
| `3` | Can be used as both Internal & External Tailgate Unlock switch |

### `$4401` — Infotainment theft

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x0F` | Infotainment Theft Status 1 | `0` | No Theft |
|  |  | `1` | Theft Detected |
| `0x0F` | Infotainment Theft Status 2 | `0` | No Theft |
|  |  | `1` | Theft Detected |
| `0x0F` | Infotainment Theft Status 3 | `0` | No Theft |
|  |  | `1` | Theft Detected |

### `$4402` — Vehicle theft

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x0F` | Vehicle theft status 1 | `0` | No Trigger |
|  |  | `1` | Trigger from Driver Door |
|  |  | `2` | Trigger from Passenger Door |
|  |  | `3` | Trigger from left rear door |
|  |  | `4` | Trigger from right rear door |
|  |  | `5` | Trigger from Tailgate |
|  |  | `6` | Trigger from Bonnet |
| `0x0F` | Vehicle theft status 2 | `0` | No Trigger |
|  |  | `1` | Trigger from Driver Door |
|  |  | `2` | Trigger from Passenger Door |
|  |  | `3` | Trigger from left rear door |
|  |  | `4` | Trigger from right rear door |
|  |  | `5` | Trigger from Tailgate |
|  |  | `6` | Trigger from Bonnet |
| `0x0F` | Vehicle theft status 3 | `0` | No Trigger |
|  |  | `1` | Trigger from Driver Door |
|  |  | `2` | Trigger from Passenger Door |
|  |  | `3` | Trigger from left rear door |
|  |  | `4` | Trigger from right rear door |
|  |  | `5` | Trigger from Tailgate |
|  |  | `6` | Trigger from Bonnet |

### `$4403` — Infotainment theft Clear

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `1` | Clear Infotainment Theft record |

### `$4404` — High Security Audio 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | High Security Audio Status | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | BCM-HU pairing config Status | `0` | Disable |
|  |  | `2` | Enable |

### `$4406` — Key Authetication Status

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | No valid key detected since IGN ON or MCU request |
| `1` | Key detection running |
| `2` | Valid key no more available (after 30second validity time is expired). |
| `3` | A valid key was detected in the last 30 seconds |

### `$4407` — Horn Control

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | On |

### `$4408` — ArmingStatusIndicator Control

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | On |

### `$4409` — Alarm Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$440C` — Centrol Lock/Unlock Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Mislock Horn Warning configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Electrical Cancellation for Reverse cycling | `0` | Disable |
|  |  | `2` | Enable |

### `$440E` — Centrol Lock/Unlock Configuration 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Auto Relock | `0` | Disable |
|  |  | `1` | Enable |
| `0x01` | Locking Type configuration | `0` | Slam Lock |
|  |  | `1` | Reverse Cycling |
| `0x02` | RKE Lock/Unlock | `0` | Disable |
|  |  | `2` | Enable |
| `0x02` | Force Panic Horn configuration | `0` | With Horn |
|  |  | `2` | Without Horn |
| `0x04` | Vehicle seek | `0` | Disable |
|  |  | `4` | Enable |
| `0x04` | Bonnet State switch presence | `0` | Not Present |
|  |  | `4` | Present |
| `0x08` | Force Panic | `0` | Disable |
|  |  | `8` | Enable |
| `0x08` | Configuration of Alarm functions for mechanical key operation (supported with / without security transponder) | `0` | Without Securiy Alarm |
|  |  | `8` | With Security Alarm |
| `0x10` | Internal Facia Switches for CL | `0` | Not Available |
|  |  | `10` | Available |
| `0x10` | Configuration of alarm function for RKE lock unlock operations (supported with / without transponder in RKE) | `0` | Without Securiy Alarm |
|  |  | `10` | With Security Alarm |
| `0x20` | Unlock on CRASH detection | `0` | Disable |
|  |  | `20` | Enable |
| `0x20` | KeyIn sensor configuration | `0` | Not Present |
|  |  | `20` | Present |
| `0x40` | Door Knob | `0` | Not Present |
|  |  | `40` | Present |
| `0x80` | Engine RPM Check for Drive Away Lock | `0` | Disable |
|  |  | `80` | Enable |

### `$4415` — IMMO Status Indicator lamp

| Value | Meaning |
|-------|---------|
| `0` | OFF |
| `1` | ON |

### `$441F` — Drive Away Locking

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Single Stage |
| `2` | Multiple Stage |

### `$4420` — CKD vehicle or Non-CKD Vehicle

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Non CKD Vehicle |
| `1` | CKD Vehicle |

### `$4421` — PEPS function in BCM

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4428` — Secrete Key for RF Encryption

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Secret Key Not Written |
| `1` | Secret Key Written |

### `$442C` — Unlocking Type configuration

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Single Stage |
| `1` | Double Stage |
| `2` | Single Stage with tailgate unlock |
| `3` | Reserved |

### `$442D` — Status Of Wakeup Output And Reduced CAN Communication Mode

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$442E` — Internal Lock Switches

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Seperate switches for Lock and Unlock input |
| `1` | Common switch for Lock and Unlock input |

### `$442F` — Change Msg IDs Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Falcon |
| `1` | Eagle |

### `$4443` — BCM Lifecycle with EMS

| Value | Meaning |
|-------|---------|
| `000` | Blank |
| `001` | Paired |
| `010` | Last Authentication Failed |
| `011` | Last Authentication Passed |
| `100` | Anti-Scanning Mode |
| `101` | Not Authenticated |
| `110` | Remote Mobilized via TCU |
| `111` | Remote Immobilized via TCU |

### `$444E` — Automatic Transponder Pairing Configuration

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4458` — Remote immobilze function from TCU

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4602` — User Customization Settings 3

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | FMH Setting Display On HU | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Approach Lamp Setting Display On HU | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | Horn On Lock Setting Display On HU | `0` | Disable |
|  |  | `4` | Enable |
| `0x08` | RKESingleStageDoubleStageUnlockSettingDisplayOnHU | `0` | Disable |
|  |  | `8` | Enable |
| `0x10` | PKESingleStageDoubleStageUnlockSettingDisplayOnHU | `0` | Disable |
|  |  | `10` | Enable |
| `0x20` | DriveAwayLockingSettingDisplayOnHU | `0` | Disable |
|  |  | `20` | Enable |
| `0x40` | AutoRelockSettingDisplayOnHU | `0` | Disable |
|  |  | `40` | Enable |
| `0x80` | FactorySettingDisplayOnHU | `0` | Disable |
|  |  | `80` | Enable |

### `$4603` — Horn request on Lock

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$463E` — Engine Stop Start Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$463F` — Reverse Gear CAN Signal

Mask `0x08`.

| Value | Meaning |
|-------|---------|
| `0` | Disengaged |
| `8` | Engaged |

### `$4677` — Vehicle Functions Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Telematics Control Unit Function | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Express Cooling Function | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | Remote Climate Control & Engine Start Stop | `0` | Disable |
|  |  | `4` | Enable |
| `0x08` | Other Voice commands Function | `0` | Disable |
|  |  | `8` | Enable |

### `$470C` — BCM_PDC_Configuration

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | NO PDC |
| `1` | CAN Based PDC with HU |
| `2` | CAN Based PDC with Buzzer |
| `3` | CAN Based PDC with IC |
| `4` | LIN Based rear PDC with HU |
| `5` | LIN Based rear PDC with IC |
| `6` | LIN Based rear PDC with HU and IC |
| `7` | LIN Based rear PDC with Buzzer |
| `8` | LIN Based rear+front PDC with HU |
| `9` | LIN Based rear+front PDC with IC |
| `10` | LIN Based rear+front PDC with HU and IC |
| `11` | LIN Based rear+front PDC with Buzzer |
| `12` | Reserved |
| `13` | Reserved |
| `14` | Reserved |

### `$470C_` — RVC Camera Enable

| Value | Meaning |
|-------|---------|
| `1` | Enable RVC( Only When RVC is installed externally) |

### `$470F` — TPMS Configuration

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$471E` — PDC Vehicle ID

| Value | Meaning |
|-------|---------|
| `0` | Tiago |
| `1` | Tigor |
| `2` | Hexa |
| `3` | Nexon |
| `4` | X451 |
| `5` | Q501 |
| `6` | Q502 |
| `7` | Zest |
| `8` | X445 |

### `$4804` — Infotainment ECU Pairing Status

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Not Paired |
| `1` | Paired to this BCM |
| `2` | Paired to other BCM |

### `$4805` — User Customization Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4A28` — Interior Light 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Interior Light1 Luggage Compartment Light Masking | `0` | Not Supported |
|  |  | `1` | Supported |
| `0x02` | Interior Light1 Ignition Keyhole Illumination Signal Masking | `0` | Not Supported |
|  |  | `2` | Supported |
| `0x04` | Interior Light1 Front Courtesy Light Masking | `0` | Not Supported |
|  |  | `4` | Supported |
| `0x08` | Interior Light1 Middle Courtesy Light Masking | `0` | Not Supported |
|  |  | `8` | Supported |
| `0x10` | Interior Light1 Rear Courtesy Light Masking | `0` | Not Supported |
|  |  | `10` | Supported |

### `$4A29` — Interior Lamp Battery Saver Output

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Interior Lamp Deactivation Signal | `0` | Signal Absent |
|  |  | `1` | Signal Present |
| `0x10` | Interior Lamp Deactivation Signal Masking | `0` | Not Supported |
|  |  | `10` | Supported |

### `$4A2A` — Accessory Delay Output

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Accessory Delay Output Signal Status | `0` | Signal Absent |
|  |  | `1` | Signal Present |
| `0x10` | Accessory Delay Signal Masking | `0` | Not Supported |
|  |  | `10` | Supported |

### `$4A35` — Interior Light Configuration 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Front Roof lamp configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Cockpitt Illumination configuration | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | Key hole illumination configuration | `0` | Disable |
|  |  | `4` | Enable |

### `$4A39` — Hazard switch LED

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `1` | Off |
| `0` | On |

### `$4A7F` — Ambient Light LEDs Status/Control

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | RoofLampLED | `0` | Off |
|  |  | `1` | On |
| `0x02` | DoorPocketLED | `0` | Off |
|  |  | `2` | On |
| `0x04` | SpotLEDOut | `0` | Off |
|  |  | `4` | On |
| `0x08` | FootwellLEDOut | `0` | Off |
|  |  | `8` | On |
| `0x10` | RoofLampLEDMasking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | DoorPocketLEDMasking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | SpotLEDOutMasking | `0` | Not Supported |
|  |  | `40` | Supported |
| `0x80` | FootwellLEDOutMasking | `0` | Not Supported |
|  |  | `80` | Supported |

### `$4A81` — Ambient Light Mode

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Auto |
| `2` | On |

### `$4A82` — Ambient Light Intensity Level

| Value | Meaning |
|-------|---------|
| `0` | Intensity 1 |
| `1` | Intensity 2 |
| `2` | Intensity 3 |
| `3` | Intensity 4 |
| `4` | Intensity 5 |

### `$4A87` — ETC/ FATC Variant Coding

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | ETC Enable |
| `1` | FATC Enable |

### `$4A88` — Rear Defogger request

Mask `0x04`.

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `4` | On |

### `$4A9D` — Internal Power Supply Output configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Internal12PowerSupplyOutputConfiguration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | InternalPowerSupplyOutputConfiguration_1 | `0` | Disable |
|  |  | `2` | Enable |

### `$4AA1` — Cockpit Illumination PWM Control

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Non BCM driven cockpit illumination |
| `1` | BCM driven cockpit illumination |

### `$4AA3` — Mood Lighting configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | MoodLightState | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | FrontRHArea | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | FrontLHArea | `0` | Disable |
|  |  | `4` | Enable |
| `0x08` | FloorConsoleArea | `0` | Disable |
|  |  | `8` | Enable |
| `0x10` | Rear Area | `0` | Disable |
|  |  | `10` | Enable |

### `$4AA4` — Mood Lighting Colour Selection On HU

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | MoodLightingIceBlueColourSelectionOnHU | `0` | Not Present |
|  |  | `1` | Present |
| `0x01` | MoodLightingPurpleTaupeColourSelectionOnHU | `0` | Not Present |
|  |  | `1` | Present |
| `0x02` | MoodLightingCoolBlueColourSelectionOnHU | `0` | Not Present |
|  |  | `2` | Present |
| `0x02` | MoodLightingHotMagentaColourSelectionOnHU | `0` | Not Present |
|  |  | `2` | Present |
| `0x04` | MoodLightingPolarBrightWhiteColourSelectionOnHU | `0` | Not Present |
|  |  | `4` | Present |
| `0x04` | MoodLightingAeroBlueColourSelectionOnHU | `0` | Not Present |
|  |  | `4` | Present |
| `0x08` | MoodLightingIvoryOFFWhiteColourSelectionOnHU | `0` | Not Present |
|  |  | `8` | Present |
| `0x08` | MoodLightingTuscanRedColourSelectionOnHU | `0` | Not Present |
|  |  | `8` | Present |
| `0x10` | MoodLightingPurpleColourSelectionOnHU | `0` | Not Present |
|  |  | `10` | Present |
| `0x10` | MoodLightingGoldYellowColourSelectionOnHU | `0` | Not Present |
|  |  | `10` | Present |
| `0x20` | MoodLightingOrangeColourSelectionOnHU | `0` | Not Present |
|  |  | `20` | Present |
| `0x20` | MoodLightingFrenchLimeColourSelectionOnHU | `0` | Not Present |
|  |  | `20` | Present |
| `0x40` | MoodLightingFlourscentGreenColourSelectionOnHU | `0` | Not Present |
|  |  | `40` | Present |
| `0x40` | MoodLightingTealGreenColourSelectionOnHU | `0` | Not Present |
|  |  | `40` | Present |
| `0x80` | MoodLightingRubyRedColourSelectionOnHU | `0` | Not Present |
|  |  | `80` | Present |
| `0x80` | MoodLightingCyanBlueColourSelectionOnHU | `0` | Not Present |
|  |  | `80` | Present |

### `$4AA5` — Drive Mode Indication From ESP

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Present |
| `1` | Present |

### `$4AA6` — Drive Mode Indication From EMS

| Value | Meaning |
|-------|---------|
| `0` | No Mode Selection |
| `1` | 2 - Modes |
| `2` | 3 - Modes |

### `$4AA7` — Cockpit Illumination PWM Output

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | On |

### `$4AA8` — Mood Lighting LEDs

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | MoodLightingFrontRHZoneLEDs | `0` | Off |
|  |  | `1` | On |
| `0x02` | MoodLightingFrontLHZoneLEDs | `0` | Off |
|  |  | `2` | On |
| `0x04` | MoodLightingFloorConsoleZoneLEDs | `0` | Off |
|  |  | `4` | On |
| `0x08` | MoodLightingRearZoneLEDs | `0` | Off |
|  |  | `8` | On |
| `0x10` | MoodLightingFrontRHZoneLEDsMasking | `0` | Not Available |
|  |  | `10` | Available |
| `0x20` | MoodLightingFrontLHZoneLEDsMasking | `0` | Not Available |
|  |  | `20` | Available |
| `0x40` | MoodLightingFloorConsoleZoneLEDsMasking | `0` | Not Available |
|  |  | `40` | Available |
| `0x80` | MoodLightingRearZoneLEDsMasking | `0` | Not Available |
|  |  | `80` | Available |

### `$4AAB` — ORVM Fold Unfold Switch Input

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Latchable |
| `1` | Non Latchable |

### `$4AAC` — Drive Mode Indication From AT

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Present |
| `1` | Present |

### `$4AAD` — Drive Mode Indication From AMT

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Present |
| `1` | Present |

### `$4B1C` — ORVM Unfold Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | ORVM Unfold Configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | ORVM Auto Unfold Configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x04` | Proximity ORVM AutoUnfoldFold Configuration | `0` | Disable |
|  |  | `1` | Enable |

### `$4B24` — POT UCS Configuration Status - RKE & Buzzer & Gesture Control Feature

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Power Operated Tailgate RKE UCS Setting | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Power Operated Tailgate Gesture UCS setting | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | Power Operated Tailgate Buzzer UCS Setting | `0` | Disable |
|  |  | `4` | Enable |
| `0x08` | Tail Gate Latch Type Configuration | `0` | Disable |
|  |  | `8` | Enable |

### `$4B25` — POT Height Adjustment UCS Configuration Status

| Value | Meaning |
|-------|---------|
| `0` | No User Height Set |
| `1` | 50% Open |
| `2` | 60% Open |
| `3` | 70% Open |
| `4` | 80% Open |
| `5` | 90% Open |
| `6` | 100% Open |

### `$4B31` — HU Gen Type

| Value | Meaning |
|-------|---------|
| `0` | HU Gen2 |
| `1` | HU Gen3 |

### `$4B34` — AVAS Function Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C00` — Exterior light switches 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Front Fog Lamp Control Switch | `0` | Off |
|  |  | `1` | On |
| `0x02` | Rear Fog Lamp Control Switch | `0` | Off |
|  |  | `2` | On |
| `0x04` | Position Lamp Switch | `0` | Off |
|  |  | `4` | On |
| `0x08` | Headlamp On Signal | `0` | Off |
|  |  | `8` | On |

### `$4C01` — Exterior light switches 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Turn Left Indicator Switch | `0` | Off |
|  |  | `1` | On |
| `0x02` | Turn Right Indicator Switch | `0` | Off |
|  |  | `2` | On |
| `0x04` | Hazard Switch | `0` | Off |
|  |  | `4` | On |

### `$4C02` — Wiper/Washer Input

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Front Washer Switch | `0` | Off |
|  |  | `1` | On |
| `0x02` | Rear Washer Switch | `0` | Off |
|  |  | `2` | On |
| `0x04` | Front Wiper Intermittent Switch | `0` | Off |
|  |  | `4` | On |
| `0x08` | Rear Wiper Mode Switch | `0` | Off |
|  |  | `8` | On |
| `0x10` | Front Wiper Park Position Signal | `0` | Signal Absent |
|  |  | `10` | Signal Present |
| `0x20` | Rear Wiper Park Position Signal | `0` | Signal Absent |
|  |  | `20` | Signal Present |

### `$4C03` — Wiper/Washer Output

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Front Washer Pump | `0` | Off |
|  |  | `1` | On |
| `0x02` | Rear Washer Pump | `0` | Off |
|  |  | `2` | On |
| `0x04` | Wiper Low Speed Wipe | `0` | Off |
|  |  | `4` | On |
| `0x08` | Wiper High Speed Wipe | `0` | Off |
|  |  | `8` | On |
| `0x10` | Front Washer Pump Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Rear Washer Pump Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Wiper Low Speed Wipe Masking | `0` | Not Supported |
|  |  | `40` | Supported |
| `0x80` | Wiper High Speed Wipe Masking | `0` | Not Supported |
|  |  | `80` | Supported |

### `$4C04` — ORVM Fold Unfold

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | PassengerExternalMirrorFoldUnfoldSignal | `1` | Fold |
|  |  | `2` | Unfold |
| `0x02` | CombinedExternalMirrorFoldUnfoldSignal | `1` | Fold |
|  |  | `2` | Unfold |
|  |  | `3` | Folding |
|  |  | `6` | Unfolding |
| `0x03` | MirrorFoldUnfoldMasking | `1` | Driver & Passenger External Mirror Fold/Unfold |
|  |  | `2` | Combined External Mirror Fold/Unfold Control |
|  |  | `3` | High (1,2) |

### `$4C07` — Exterior Lamp Output 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Low Beam Relay | `0` | Off |
|  |  | `1` | On |
| `0x02` | Combined High Beam Circuit | `0` | Off |
|  |  | `2` | On |
| `0x04` | Left Stop Lamp | `0` | Off |
|  |  | `4` | On |
| `0x08` | Right Stop Lamp | `0` | Off |
|  |  | `8` | On |
| `0x10` | Low Beam Relay Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Combined High Beam Circuit Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Left Stop Lamp Masking | `0` | Not Supported |
|  |  | `40` | Supported |
| `0x80` | Right Stop Lamp Masking | `0` | Not Supported |
|  |  | `80` | Supported |

### `$4C08` — Exterior Lamp Output 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Left Position Lamps | `0` | Off |
|  |  | `1` | On |
| `0x02` | Right Position Lamps | `0` | Off |
|  |  | `2` | On |
| `0x04` | Left Turn Indicator | `0` | Off |
|  |  | `4` | On |
| `0x08` | Right Turn Indicator | `0` | Off |
|  |  | `8` | On |
| `0x10` | Left Position Lamps Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Right Position Lamps Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Left Turn Indicator Masking | `0` | Not Supported |
|  |  | `40` | Supported |
| `0x80` | Right Turn Indicator Masking | `0` | Not Supported |
|  |  | `80` | Supported |

### `$4C09` — Exterior Lamp Output 3

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Left Rear Fog Lamp | `0` | Off |
|  |  | `1` | On |
| `0x02` | Right Rear Fog Lamp | `0` | Off |
|  |  | `2` | On |
| `0x04` | Left Reverse Lamp | `0` | Off |
|  |  | `4` | On |
| `0x08` | Right Reverse Lamp | `0` | Off |
|  |  | `8` | On |
| `0x10` | Left Rear Fog Lamp Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Right Rear Fog Lamp Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Left Reverse Lamp Masking | `0` | Not Supported |
|  |  | `40` | Supported |
| `0x80` | Right Reverse Lamp Masking | `0` | Not Supported |
|  |  | `80` | Supported |

### `$4C0A` — Exterior Lamp Output 4

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Left Front Fog Lamp / Corner Lamp | `0` | Off |
|  |  | `1` | On |
| `0x02` | Right Front Fog Lamp / Corner Lamp | `0` | Off |
|  |  | `2` | On |
| `0x04` | Front Fog Lamp Relay | `0` | Off |
|  |  | `4` | On |
| `0x10` | Left Front Fog Lamp Corner Lamp Masking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | Right Front Fog Lamp Corner Lamp Masking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | Front Fog Lamp Relay Masking | `0` | Not Supported |
|  |  | `40` | Supported |

### `$4C0B` — WashLevel/ORVM/Defogger Switch Status

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | ScreenwashLevelSwitch | `0` | Off |
|  |  | `1` | On |
| `0x02` | MirrorFoldUnfoldSwitch | `0` | Fold |
|  |  | `2` | Unfold |
| `0x04` | RearDeFoggerSwitch | `0` | Off |
|  |  | `4` | On |
| `0x10` | ScreenwashLevelSwitchMasking | `0` | Not Supported |
|  |  | `10` | Supported |
| `0x20` | MirrorFoldUnfoldSwitchMasking | `0` | Not Supported |
|  |  | `20` | Supported |
| `0x40` | RearDeFoggerSwitchMasking | `0` | Not Supported |
|  |  | `40` | Supported |

### `$4C13` — Rear Defogger Output

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Rear Defogger Relay Control | `0` | Off |
|  |  | `1` | On |
| `0x02` | Heated Rear Window feedback from BCM | `0` | Off |
|  |  | `2` | On |

### `$4C19` — Rear Wiper

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Rear Wiper Relay | `0` | Off |
|  |  | `1` | On |
| `0x10` | Rear Wiper Relay Masking | `0` | Not Supported |
|  |  | `10` | Supported |

### `$4C1C` — Exterior Light Configuration 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Masterlight switch configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Position lamp Contact available at Headlamp On position | `0` | Not Available |
|  |  | `2` | Available |
| `0x04` | Hazard switch configuration | `0` | Latchable |
|  |  | `4` | Non Latchable |
| `0x08` | Front fog lamp switch configuration | `0` | Latchable |
|  |  | `8` | Non Latchable |
| `0x10` | Rear fog lamp switch configuration | `0` | Latchable |
|  |  | `10` | Non Latchable |
| `0x20` | Auto Fog lamp configuration | `0` | Disable |
|  |  | `20` | Enable |
| `0x40` | Combi Switch type | `0` | Low beam input availability only with headlamp ON position |
|  |  | `40` | Low beam input availability independent of head lamp ON position |
| `0x80` | Reverse Lamp Single Lamp configuration | `0` | Both Lamps |
|  |  | `80` | Single Lamps |

### `$4C1D` — Exterior Light Configuration 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Position lamp configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Low beam configuration | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | High Beam configuration | `0` | Disable |
|  |  | `4` | Enable |
| `0x08` | Auto headlamp configuration | `0` | Disable |
|  |  | `8` | Enable |
| `0x10` | Turn Signal flash configuration | `0` | Disable |
|  |  | `10` | Enable |
| `0x20` | Turn Tip Configuration | `0` | Disable |
|  |  | `20` | Enable |
| `0x40` | Reverse lamp configuration | `0` | Disable |
|  |  | `40` | Enable |
| `0x80` | Brake lamp configuration | `0` | Disable |
|  |  | `80` | Enable |

### `$4C1E` — Exterior Light Configuration 3

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Single/double chamber configuration | `0` | Single Chamber |
|  |  | `1` | Dual Chamber |
| `0x02` | Flash to pass configuration | `0` | FTP Function with IgnOn only |
|  |  | `2` | FTP Function with IgnOn and IgnOff |
| `0x04` | Fog lamp Telltale configuration | `0` | Telltale on Facia switch |
|  |  | `4` | Telltale on IC |
| `0x08` | PositionLamp operation when IgnOff & Headlamp switch ON | `0` | Disable |
|  |  | `8` | Enable |
| `0x10` | Rear fog Lamp Single Lamp Config | `0` | Both Lamps |
|  |  | `10` | Single Lamps |

### `$4C1F` — Exterior Light Configuration 4

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Rear fog lamp fitted | `0` | Not Fitted |
|  |  | `1` | Fitted |
| `0x0F` | Front fog lamp / Cornering lamp / Static bending configuration | `0` | No Front fog; No Cornering lamp; No Static bending |
|  |  | `1` | Front fog lamp only |
|  |  | `3` | Front fog with Cornering lamp only |
|  |  | `4` | Static bending only |
|  |  | `5` | Static bending with fog lamp |

### `$4C22` — Exterior Light Configuration 5

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Low beam Light |
| `1` | Low beam Light & Position Light |
| `2` | Low beam Light/ Position Light and Roof Light |

### `$4C24` — Wiper/Washer Configuration 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Front Low High Speed wiper configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Front washer configuration | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | Rear Wiper configuration | `0` | Disable |
|  |  | `4` | Enable |
| `0x07` | Front wiper INT/RLS-Auto/Speedbased-Auto configuration | `0` | No Intermittent/No RLS/No Speedbased Front Wiper |
|  |  | `1` | Front wiper Intermittent only |
|  |  | `2` | Front Wiper with RLS Auto configuration only |
|  |  | `4` | Speed base front wiper Auto configuration only |
| `0x08` | Rear washer configuration | `0` | Disable |
|  |  | `8` | Enable |
| `0x10` | Rear wiper fixed Intermittent configuration | `0` | Disable |
|  |  | `10` | Enable |
| `0x20` | Rear Wiper Tailgate Open/Close Inhibit | `0` | Disable |
|  |  | `20` | Enable |

### `$4C29` — ExteriorLight/Wiper Switches

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Low Beam Switch | `0` | Off |
|  |  | `1` | On |
| `0x02` | Main Beam Switch | `0` | Off |
|  |  | `2` | On |
| `0x04` | Headlamp Auto On Enable Switch | `0` | Off |
|  |  | `4` | On |
| `0x08` | Master Light Switch Fail Safe Signal | `0` | Off |
|  |  | `8` | On |
| `0x10` | Front Wiper Low Speed Switch | `0` | Off |
|  |  | `10` | On |
| `0x20` | Front Wiper High Speed Switch | `0` | Off |
|  |  | `20` | On |
| `0x40` | Rear Wiper Fixed Interval Signal | `0` | Off |
|  |  | `40` | On |

### `$4C2B` — Two/Three Button RKE

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Three Button RKE |
| `1` | Two Button RKE |

### `$4C2C` — Approach lamp mode configuration

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Low beam Light |
| `1` | Low beam Light & Position Light |
| `2` | Low beam Light/ Position Light and Roof Light |

### `$4C2D` — Day Time Running Lamp

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Day Time Running Lamp -Left | `0` | Off |
|  |  | `1` | On |
| `0x02` | Day Time Running Lamp - Right | `0` | Off |
|  |  | `2` | On |
| `0x10` | Day Time Running Lamp Masking | `0` | Not Supported |
|  |  | `10` | Supported |

### `$4C2E` — Daytime Running Lamp 1

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Daytime Running Lamp Feature | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | Dedicated DRL Configuration | `0` | DRL Combined with Posiiton Lamp |
|  |  | `2` | Dedicated DRL |

### `$4C2F` — Daytime Running Lamp 2

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | DRLONWhenIgnON | `0` | DRL comes ON with Engine Run only |
|  |  | `1` | DRL comes ON with Ign On and Engine Run |
| `0x02` | DRLDropOutRegulationDuringTurnIndicatorFuncitonIsON | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | DRLWithOrWithoutRampUpDown | `0` | Without Ramp Up/Down |
|  |  | `4` | With Ramp Up/Down |

### `$4C31` — External Mirror status

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Stall Not Detected |
| `1` | Stall Detected |

### `$4C32` — Intermittent Programmable Delay Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C33` — Brake Signal from ABS/ESP

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Available |
| `1` | Available |

### `$4C34` — Trailer Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | TrailerECUPresent | `0` | Not Present |
|  |  | `1` | Present |
| `0x02` | BCMBrakeLampsStateOnFunctionRequestWhenTrailerIsFitted | `0` | BCM shall not drive brake lamps |
|  |  | `2` | BCM shall drive brake lamps |
| `0x04` | BCMReverseLampsStateOnFunctionRequestWhenTrailerIsFitted | `0` | BCM shall not drive reverse lamps |
|  |  | `4` | BCM shall drive reverse lamps |
| `0x08` | BCMRearFogLampsStateOnFunctionRequestWhenTrailerIsFitted | `0` | BCM shall not drive rear fog lamps |
|  |  | `8` | BCM shall drive rear fog lamps |

### `$4C38` — Auto Light Switch configuration

| Value | Meaning |
|-------|---------|
| `0` | Auto Light is part of combi switch |
| `1` | Auto Light is part of MSL |
| `2` | Auto Light is part of facia switch bank |

### `$4C39` — Auto Light switch type

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Auto Light is part of combi switch |
| `1` | Auto Light is part of MSL |
| `2` | Auto Light is part of facia switch bank |

### `$4C40` — Rear defogger available

Mask `0x02`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `2` | Enable |

### `$4C42` — BCM to Trailer Signal Status

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | BrakeLampSignalToTrailer | `0` | Off |
|  |  | `1` | On |
| `0x02` | ReverseLampSignalToTrailer | `0` | Off |
|  |  | `2` | On |
| `0x04` | PositionLampSignalToTrailer | `0` | Off |
|  |  | `4` | On |
| `0x08` | RearFogLampSignalToTrailer | `0` | Off |
|  |  | `8` | On |
| `0x10` | RearLeftTurnLampSignalToTrailer | `0` | Off |
|  |  | `10` | On |
| `0x20` | RearRightTurnLampSignalToTrailer | `0` | Off |
|  |  | `20` | On |

### `$4C43` — Rain Sensor Hardware status

Mask `0x07`.

| Value | Meaning |
|-------|---------|
| `0` | No Fault detected |
| `1` | Fault detected |
| `2` | Fault Information not available |

### `$4C44` — Light Sensor Hardware status

Mask `0x07`.

| Value | Meaning |
|-------|---------|
| `0` | No Fault detected |
| `1` | Fault detected |
| `2` | Fault Information not available |

### `$4C45` — Configuration of Light sensitivity for Light Sensor

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Present |
| `1` | Present |

### `$4C50` — Vehicle type configuration for RLS

Mask `0x07`.

| Value | Meaning |
|-------|---------|
| `0` | Vehicle Type 0 |
| `1` | Vehicle Type 1 |
| `2` | Vehicle Type 2 |
| `3` | Vehicle Type 3 |
| `4` | Vehicle Type 4 |
| `5` | Vehicle Type 5 |
| `6` | Vehicle Type 6 |
| `7` | Vehicle Type 7 |

### `$4C51` — Configuration of Windscreen Type for RLS

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Windscreen Type 0 |
| `1` | Windscreen Type 1 |
| `2` | Windscreen Type 2 |
| `3` | Windscreen Type 3 |

### `$4C52` — Auto wipe configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C55` — Configuration of Wiper Type

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Wiper Type 1 |
| `1` | Wiper Type 2 |
| `2` | Wiper Type 3 |

### `$4C57` — Speed Light Function

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C58` — Auto Light On Sate LED output configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C59` — Transmission Control Module

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Present |
| `1` | Present |

### `$4C5B` — LED Based Rear Turn Indicator Lamp

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C5C` — LED Based Rear Turn Indicator Lamp Control

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | RearLeftTurnIndicatorLEDs | `0` | Off |
|  |  | `1` | On |
| `0x02` | RearRightTurnIndicatorLEDs | `0` | Off |
|  |  | `2` | On |
| `0x04` | RearLeftTurnIndicatorLEDsMasking | `0` | Not Available |
|  |  | `4` | Available |
| `0x08` | RearRightTurnIndicatorLEDsMasking | `0` | Not Available |
|  |  | `8` | Available |

### `$4C5D` — RLS Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x0F` | The Name Of RLS Manufacturer | `0` | VALEO RLS |
|  |  | `1` | HELLA RLS |
| `0x10` | Auto Wipe Switch Configuration | `0` | Latchable |
|  |  | `10` | Non Latchable |
| `0x20` | Auto Wipe And Auto Wipe Switch Configuration | `0` | One Detent Switch |
|  |  | `20` | Two Detent Switch |
| `0x40` | Auto Wipe And Auto Wipe Common Switch Configuration | `0` | Not Present |
|  |  | `40` | Present |

### `$4C5E` — Sensitivity values for Auto Light

| Value | Meaning |
|-------|---------|
| `0` | sensitivity 0 |
| `1` | sensitivity 1 |
| `2` | sensitivity 2 |
| `3` | sensitivity 3 |
| `4` | sensitivity 4 |

### `$4C5F` — Sensitivity values for Auto Wipe

| Value | Meaning |
|-------|---------|
| `0` | sensitivity 0 |
| `1` | sensitivity 1 |
| `2` | sensitivity 2 |
| `3` | sensitivity 3 |
| `4` | sensitivity 4 |

### `$4C60` — Configuration of Wiper sensitivity for Rain Sensor

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not Present |
| `1` | Present |

### `$4C66` — Cornering lamp activation input (Readable)

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Turn switch based cornering lamps |
| `1` | Steering input based cornering lamp |

### `$4C70` — Turn Lamp Voltage compensation configuration (Readable)

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Enable |
| `1` | Disable |

### `$4C71` — Welcome & Goodbye Feature Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disable |
| `1` | Enable |

### `$4C73` — Dual State Backlight Intensity Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Steering Wheel BackLight Illumination Configuration | `0` | Disable |
|  |  | `1` | Enable |
| `0xFF` | Steering Wheel BackLight Illumination Intensity Control | `0` | Intensity1 |
|  |  | `1` | Intensity2 |
|  |  | `2` | Intensity3 |
|  |  | `3` | Intensity4 |
|  |  | `4` | Intensity5 |

### `$4E00` — Reverse Gear Switch

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | ReverseGearSelectionSwitch | `0` | Off |
|  |  | `1` | On |
| `0x10` | ReverseGearSelectionSwitchMasking | `0` | Not Supported |
|  |  | `10` | Supported |

### `$4E07` — ORVM Configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | UnfoldingORVMOnRKEUnlock | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | MirrorAutoShutOffViaBCM | `2` | Enable |
| `0x02` | UnfoldingORVMOnMechanicalKeyUnlock | `0` | Disable |
|  |  | `2` | Enable |
| `0x04` | ORVMWithAutoShutOffOff | `0` | Disable |
|  |  | `4` | Enable |
| `0x08` | ORVMWithPTC | `0` | Disable |
|  |  | `8` | Enable |
| `0x10` | VehicleSpeedBasedMirrorUnfolding | `0` | Disable |
|  |  | `10` | Enable |
| `0x20` | UnfoldingORVMAtIGNON | `0` | Disable |
|  |  | `20` | Enable |
| `0x40` | FoldingORVMOnRKELOCK | `0` | Disable |
|  |  | `40` | Enable |
| `0x80` | FoldingORVMOnMechanicalKeyLOCK | `0` | Disable |
|  |  | `80` | Enable |

### `$4E11` — Vehicle Power Mode Local

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Pre-Stand By |
| `1` | Awake |
| `2` | Transport Park |
| `3` | Key In |
| `4` | Accessory |
| `5` | Accessory Delay |
| `6` | Active (Ignition ON) |
| `7` | Transport Drive |
| `8` | Run (Engine Run) |
| `9` | Start (Crank) |
| `10` | Transport Drive CRANK |
| `11` | Transport Drive RUN |

### `$6000` — Brake Switch

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Brake Pedal Switch "A" | `0` | Off |
|  |  | `1` | On |
| `0x02` | Brake Pedal Switch "B" | `0` | Off |
|  |  | `2` | On |

### `$7005` — Ignition Status

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Accessory |
| `2` | Ignition |
| `3` | Crank |

### `$7006` — Key In Status

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Key Out |
| `1` | Key In |

### `$700E` — Control Module Output Power Line

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Internal5V | `0` | Off |
|  |  | `1` | On |
| `0x02` | Internal12V | `0` | Off |
|  |  | `2` | On |

### `$700F` — Accessory Delay Output

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | With Acc Off |
| `1` | With Ign Off |

### `$7010` — Transport mode activation

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Transport Mode Function not available |
| `1` | Transport Mode Activation through Fuse Link |
| `2` | Transport Mode Activation through Diagnostic |
| `3` | Transport Mode Activation through Fuse Link + Diagnostic |

### `$7206` — Application Software Status

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Component Erased |
| `1` | Request Download Complete |
| `2` | Request Upload Complete |
| `3` | Transfer Data Complete |
| `4` | Transfer Exit Complete |
| `5` | Software Valid |
| `6` | Software Invalid |
| `15` | Virgin Component |

### `$7207` — Calibration Software Status

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Component Erased |
| `1` | Request Download Complete |
| `2` | Request Upload Complete |
| `3` | Transfer Data Complete |
| `4` | Transfer Exit Complete |
| `5` | Software Valid |
| `6` | Software Invalid |
| `15` | Virgin Component |

### `$7211` — Communication Status

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | RX & TX Enabled |
| `1` | RX Enabled & TX Disabled |
| `2` | RX Disabled & TX Enabled |
| `3` | RX Disabled & TX Disabled |

### `$7212` — Vehicle Mode

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `0` | Customer |
| `1` | Transport |
| `2` | Factory |
| `3` | Crash |

### `$7215` — Active Software Component

Mask `0x03`.

| Value | Meaning |
|-------|---------|
| `1` | Application |
| `2` | Primary Bootloader |
| `3` | Secondary Bootloader |

### `$7227` — Wakeup/Awake

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Stay Awake Input from hazard/crash | `0` | Inactive |
|  |  | `1` | Active |
| `0x02` | Stay Awake Input from roof lamp ON | `0` | Inactive |
|  |  | `2` | Active |
| `0x04` | Stay Awake Input from follow me home ON | `0` | Inactive |
|  |  | `4` | Active |
| `0x08` | Stay Awake Input from position lamp ON | `0` | Inactive |
|  |  | `8` | Active |
| `0x10` | Stay Awake Input from battery saver output ON | `0` | Inactive |
|  |  | `10` | Active |
| `0x19` | Wake up input 1 | `0` | No Input |
|  |  | `1` | Input from driver door open signal |
|  |  | `2` | Input from passenger door /other door open |
|  |  | `3` | Input from left rear door open signal |
|  |  | `4` | Input from right rear door open signal |
|  |  | `5` | Input from tailgate open signal |
|  |  | `6` | Input from bonnet open signal |
|  |  | `9` | Input from mechanical key lock signal |
|  |  | `10` | Input from mechanical key unlock signal |
|  |  | `11` | Input from infotainment removal detection |
|  |  | `12` | Input from brake lamp switch |
|  |  | `14` | Input from key RF signal |
|  |  | `15` | Input from interior lock signal |
|  |  | `16` | Input from interior unlock signal |
|  |  | `17` | Input from Key In Signal |
|  |  | `18` | Input from position lamp switch signal |
|  |  | `19` | Input from headlamp low beam signal |
|  |  | `20` | Input from headlamp high beam signal |
|  |  | `21` | Input from ignition position |
|  |  | `22` | Input from hazard switch signal |
|  |  | `23` | Input from headlamp ON signal |
|  |  | `24` | Input from accessory position |
|  |  | `25` | Input from PASE |
| `0x19` | Wake up input 2 | `0` | No Input |
|  |  | `1` | Input from driver door open signal |
|  |  | `2` | Input from passenger door /other door open |
|  |  | `3` | Input from left rear door open signal |
|  |  | `4` | Input from right rear door open signal |
|  |  | `5` | Input from tailgate open signal |
|  |  | `6` | Input from bonnet open signal |
|  |  | `9` | Input from mechanical key lock signal |
|  |  | `10` | Input from mechanical key unlock signal |
|  |  | `11` | Input from infotainment removal detection |
|  |  | `12` | Input from brake lamp switch |
|  |  | `14` | Input from key RF signal |
|  |  | `15` | Input from interior lock signal |
|  |  | `16` | Input from interior unlock signal |
|  |  | `17` | Input from Key In Signal |
|  |  | `18` | Input from position lamp switch signal |
|  |  | `19` | Input from headlamp low beam signal |
|  |  | `20` | Input from headlamp high beam signal |
|  |  | `21` | Input from ignition position |
|  |  | `22` | Input from hazard switch signal |
|  |  | `23` | Input from headlamp ON signal |
|  |  | `24` | Input from accessory position |
|  |  | `25` | Input from PASE |
| `0x19` | Wake up input 3 | `0` | No Input |
|  |  | `1` | Input from driver door open signal |
|  |  | `2` | Input from passenger door /other door open |
|  |  | `3` | Input from left rear door open signal |
|  |  | `4` | Input from right rear door open signal |
|  |  | `5` | Input from tailgate open signal |
|  |  | `6` | Input from bonnet open signal |
|  |  | `9` | Input from mechanical key lock signal |
|  |  | `10` | Input from mechanical key unlock signal |
|  |  | `11` | Input from infotainment removal detection |
|  |  | `12` | Input from brake lamp switch |
|  |  | `14` | Input from key RF signal |
|  |  | `15` | Input from interior lock signal |
|  |  | `16` | Input from interior unlock signal |
|  |  | `17` | Input from Key In Signal |
|  |  | `18` | Input from position lamp switch signal |
|  |  | `19` | Input from headlamp low beam signal |
|  |  | `20` | Input from headlamp high beam signal |
|  |  | `21` | Input from ignition position |
|  |  | `22` | Input from hazard switch signal |
|  |  | `23` | Input from headlamp ON signal |
|  |  | `24` | Input from accessory position |
|  |  | `25` | Input from PASE |

### `$722C` — Active Wake-Up output

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | On |

### `$722D` — DTC Masking (Writeable)

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | EnableDisableFault | `0` | Disable |
|  |  | `1` | Enable |
| `0x02` | LoadTypeConfiguration | `0` | LED load |
|  |  | `2` | Bulb load |
| `0x80` | SetToDefault | `0` | Don't care |
|  |  | `80` | Reset to default |

### `$7233` — Partner ECU Status Signal

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Engine speed signal | `0` | Signal Valid |
|  |  | `1` | Signal Invalid |
| `0x02` | Heater Rear Window Request From CCM | `0` | Signal Valid |
|  |  | `2` | Signal Invalid |
| `0x04` | Brake Light Signal Status | `0` | Signal Valid |
|  |  | `4` | Signal Invalid |
| `0x08` | Current gear TCU | `0` | Signal Valid |
|  |  | `8` | Signal Invalid |
| `0x10` | Vehicle Speed IPC | `0` | Signal Valid |
|  |  | `10` | Signal Invalid |
| `0x20` | Authentication Mechanical Key | `0` | Signal Valid |
|  |  | `20` | Signal Invalid |

### `$7239` — Basic Software Status

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `0` | Component Erased |
| `1` | Request Download Complete |
| `2` | Request Upload Complete |
| `3` | Transfer Data Complete |
| `4` | Transfer Exit Complete |
| `5` | Software Valid |
| `6` | Software Invalid |
| `15` | Virgin Component |

### `$7246` — Current Gear Status

Mask `0x0F`.

| Value | Meaning |
|-------|---------|
| `1` | Neutral |
| `2` | First Gear |
| `3` | Second Gear |
| `4` | Third Gear |
| `5` | Fourth Gear |
| `6` | Fifth Gear |
| `7` | Sixth Gear |
| `8` | Reverse Gear |
| `0` | Reserved |

### `$A006` — Vehicle Power Mode

| Value | Meaning |
|-------|---------|
| `0` | Pre Stand By |
| `1` | Awake |
| `2` | Transport Park |
| `3` | Key In |
| `4` | Accessory |
| `5` | Accessory Delay |
| `6` | Active |
| `7` | Transport Drive |
| `8` | Run |
| `9` | Start |
| `10` | Transport Drive Crank |
| `11` | Transport Drive Run |

### `$F186` — Active Diagnostic Session

| Value | Meaning |
|-------|---------|
| `1` | Default Session |
| `2` | Programming Session |
| `3` | Extended Session |
| `95` | Engineering Session |

## PEPS — Passive Entry / Passive Start

Request `0x710` · response `0x718` · 48 enumerated DIDs, 17 of them bit-packed.

### `$4015` — ESCL Lock Unlock State

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | LockStateESCL | `0` | No Lock |
|  |  | `1` | Lock |
| `0x02` | UnLockStateESCL | `0` | No Unlock |
|  |  | `2` | Unlock |

### `$420F` — Pairing Status of PEPS Enabled Key Lines

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | PairingStatusOfKeyLine01 | `0` | Not Paired |
|  |  | `1` | Paired |
| `0x02` | PairingStatusOfKeyLine02 | `0` | Not Paired |
|  |  | `2` | Paired |
| `0x04` | PairingStatusOfKeyLine03 | `0` | Not Paired |
|  |  | `4` | Paired |
| `0x08` | PairingStatusOfKeyLine04 | `0` | Not Paired |
|  |  | `8` | Paired |
| `0x10` | PairingStatusOfKeyLine05 | `0` | Not Paired |
|  |  | `10` | Paired |
| `0x20` | PairingStatusOfKeyLine06 | `0` | Not Paired |
|  |  | `20` | Paired |
| `0x40` | PairingStatusOfKeyLine07 | `0` | Not Paired |
|  |  | `40` | Paired |
| `0x80` | PairingStatusOfKeyLine08 | `0` | Not Paired |
|  |  | `80` | Paired |

### `$4210` — PEPS HW Inputs Status

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | DriverDoorHandleSwitch | `0` | Not Pressed |
|  |  | `1` | Pressed |
| `0x01` | StartStopSwitchInput1 | `0` | Not Pressed |
|  |  | `1` | Pressed |
| `0x02` | CoDriverDoorHandleSwitch | `0` | Not Pressed |
|  |  | `2` | Pressed |
| `0x02` | StartStopSwitchInput2 | `0` | Not Pressed |
|  |  | `2` | Pressed |
| `0x04` | RearRHDoorHandleSwitch | `0` | Not Pressed |
|  |  | `4` | Pressed |
| `0x04` | WakeupInputFromBCM | `0` | Off |
|  |  | `4` | On |
| `0x08` | RearLHDoorHandleSwitch | `0` | Not Pressed |
|  |  | `8` | Pressed |
| `0x10` | TailgateTrunkHandleSwitch | `0` | Not Pressed |
|  |  | `10` | Pressed |
| `0x20` | ClutchSwitch90PercPress | `0` | Not Pressed |
|  |  | `20` | Pressed |
| `0x40` | ClutchSwitch10PercPress | `0` | Not Pressed |
|  |  | `40` | Pressed |
| `0x80` | BrakeSwitch | `0` | Not Pressed |
|  |  | `80` | Pressed |

### `$4214` — RKE Command Received To PEPS

| Value | Meaning |
|-------|---------|
| `0` | Status Not Available |
| `1` | Lock |
| `2` | Unlock |
| `3` | Tailgate Unlock |
| `4` | Approach Light |
| `5` | Force Panic |
| `6` | Window Up |
| `7` | Window Down |

### `$4217` — Type of Door Handle Switch

| Value | Meaning |
|-------|---------|
| `0` | Push Button |
| `1` | Capacitive Sensor |

### `$4218` — Door Handle Sw Installed

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | DriverDoorHandleSwitch Installed | `0` | No |
|  |  | `1` | Yes |
| `0x02` | CoDriverDoorHandleSwitch Installed | `0` | No |
|  |  | `2` | Yes |

### `$4219` — Door States Received From BCM

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | DriverDoorState | `0` | Closed |
|  |  | `1` | Open |
| `0x02` | CoDriverDoorState | `0` | Closed |
|  |  | `2` | Open |

### `$421A` — Lock States Received From BCM

| Value | Meaning |
|-------|---------|
| `1` | Externally Locked |
| `2` | Internally Locked |
| `3` | Unlocked |
| `4` | Crash Unlocked |
| `5` | Reverse Unlocked |

### `$421B` — UnLock Type Configuration Received From BCM

| Value | Meaning |
|-------|---------|
| `0` | Single stage |
| `1` | Double stage |
| `2` | Single stage with Tailgate Unlock |

### `$421C` — PEPS Commands To BCM

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | VehicleLockCommand | `0` | No Lock |
|  |  | `1` | Lock |
| `0x01` | VehicleSeekCommand | `0` | Off |
|  |  | `1` | On |
| `0x02` | UnlockDriveDoorCommand | `0` | No Unlock |
|  |  | `2` | Unlock |
| `0x04` | UnlockAllDoorCommand | `0` | No Unlock |
|  |  | `4` | Unlock |
| `0x08` | UnlockTailgateCommand | `0` | No Unlock |
|  |  | `8` | Unlock |
| `0x10` | ApproachLightCommand | `0` | Off |
|  |  | `10` | On |
| `0x20` | ForcePanicCommand | `0` | Off |
|  |  | `20` | On |
| `0x40` | WindowDownCommand | `0` | No Window Down |
|  |  | `40` | Execute Window Down |
| `0x80` | WindowUpCommand | `0` | No Window Close |
|  |  | `80` | Execute Window Close |

### `$4415` — PEPS Status indicator lamp

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Active |
| `1` | Inactive |

### `$4418` — PEPS ECU Status

| Value | Meaning |
|-------|---------|
| `0` | Engine starting Disabled |
| `1` | Engine Stating Enabled |
| `2` | Key learning |

### `$441A` — PEPS EMS status

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | StatusOfAuthenticationBetweenPEPSAndEMS | `0` | Fail |
|  |  | `1` | Pass |
| `0x02` | AESSKKeyLearntStatusOfPEPS | `0` | Virgin ECU |
|  |  | `2` | Programmed ECU |

### `$441C` — Status of AES SK Key

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Locked |
| `1` | Unlocked |

### `$442A` — PEPS Modes

| Value | Meaning |
|-------|---------|
| `0` | PEPS status OK |
| `1` | Operational Fault in PEPS |
| `3` | Transponder Key Learning Mode |
| `4` | Anti-Scanning Mode |
| `6` | Transport Mode |

### `$4430` — Wakeup Reason

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | CANWakeup | `0` | Inactive |
|  |  | `1` | Active |
| `0x01` | RearLHDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `1` | Active |
| `0x02` | RKEWakeup | `0` | Inactive |
|  |  | `2` | Active |
| `0x02` | TailgateTrunkHandleSwitchWakeup | `0` | Inactive |
|  |  | `2` | Active |
| `0x04` | StartStopSwitch1Wakeup | `0` | Inactive |
|  |  | `4` | Active |
| `0x04` | ClutchSwitch90PercPressWakeup | `0` | Inactive |
|  |  | `4` | Active |
| `0x08` | StartStopSwitch2Wakeup | `0` | Inactive |
|  |  | `8` | Active |
| `0x08` | ClutchSwitch10PercPressWakeup | `0` | Inactive |
|  |  | `8` | Active |
| `0x10` | DriverDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `10` | Active |
| `0x10` | BrakeSwitchWakeup | `0` | Inactive |
|  |  | `10` | Active |
| `0x20` | CoDriverDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `20` | Active |
| `0x20` | BCMWakeup | `0` | Inactive |
|  |  | `20` | Active |
| `0x40` | RearRHDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `40` | Active |

### `$4433` — Reset Reason

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | PowerONReset | `0` | Inactive |
|  |  | `1` | Active |
| `0x02` | ExternalWatchdogReset | `0` | Inactive |
|  |  | `2` | Active |
| `0x04` | InternalWatchdogReset | `0` | Inactive |
|  |  | `4` | Active |
| `0x08` | SoftReset | `0` | Inactive |
|  |  | `8` | Active |
| `0x10` | HardReset | `0` | Inactive |
|  |  | `10` | Active |

### `$4434` — PEPS Clamp Control Outputs State

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | ACCRelayOutput | `0` | Off |
|  |  | `1` | On |
| `0x02` | IGNRelayOutput | `0` | Off |
|  |  | `2` | On |
| `0x04` | CrankRelayOutput | `0` | Off |
|  |  | `4` | On |

### `$4436` — Vehicle Steering Position LHD_RHD

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | RHD |
| `1` | LHD |

### `$4439` — PEPS Operations

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | PassiveEntryExit | `0` | Disabled |
|  |  | `1` | Enabled |
| `0x02` | PassiveStartStop | `0` | Disabled |
|  |  | `2` | Enabled |
| `0x04` | PEPSRKEOperation | `0` | Disabled |
|  |  | `4` | Enabled |

### `$443A` — Number Of LF Antennas Installed

| Value | Meaning |
|-------|---------|
| `0` | 3 - RH Door, Center Console & Trunk |
| `1` | 4 - RH Door, LH Door, Center Console & Trunk |

### `$443E` — PEPS RF-Config

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | RFPower | `0` | Power Setting 0 |
|  |  | `1` | Power Setting 1 |
| `0x02` | FFBTimeout | `0` | Timeout Setting 0 |
|  |  | `2` | Timeout Setting 1 |
| `0x04` | RFChannel3 | `0` | Off |
|  |  | `4` | On |
| `0x08` | RFChannel2 | `0` | Off |
|  |  | `8` | On |
| `0x10` | RFChannel1 | `0` | Off |
|  |  | `10` | On |

### `$443F` — Clutch Input Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Switch |
| `1` | Sensor |

### `$4442` — PEPS Lifecycle with UID

| Value | Meaning |
|-------|---------|
| `0` | Blank |
| `1` | Learned |
| `2` | Paired |

### `$4443` — PEPS Lifecycle with EMS

| Value | Meaning |
|-------|---------|
| `0` | Blank |
| `1` | Paired |
| `2` | Not Auth |
| `3` | Auth |
| `4` | Anti Scanning Mode |

### `$4444` — Configuration of Clutch Switch input 10% Press

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Not present |
| `1` | Present |

### `$4445` — PEPS Wakeup Output State

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Low |
| `1` | High |

### `$4446` — Automatic Odometer learning feature Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disabled |
| `1` | Enabled |

### `$4448` — Unlocking Type configuration

| Value | Meaning |
|-------|---------|
| `0` | Single stage |
| `1` | Double stage |
| `2` | Single stage with Tailgate Unlock |

### `$4450` — EMS Feature configuration

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | IACFeatureAvailable | `0` | Disabled |
|  |  | `1` | Enabled |
| `0x02` | ESSFeatureAvailable | `0` | Disabled |
|  |  | `2` | Enabled |

### `$4457` — TCU Immobilizer Feature configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disabled |
| `1` | Enabled |

### `$5296` — UID Battery Level Indication

| Value | Meaning |
|-------|---------|
| `0` | Normal/Full |
| `1` | Used 1 |
| `2` | Used 2 |
| `3` | Used 3/Empty |

### `$5298` — PEPS Clamp State

| Value | Meaning |
|-------|---------|
| `0` | Off |
| `1` | Accessory |
| `2` | Ignition |
| `3` | Crank |

### `$52A0` — Transmission Control Module Available

| Value | Meaning |
|-------|---------|
| `0` | Not present |
| `1` | Present |

### `$52A1` — PEPS Supervision Frame

| Value | Meaning |
|-------|---------|
| `0` | Disabled |
| `1` | Enabled |

### `$52A5` — Debug message feature Configuration

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disabled |
| `1` | Enabled |

### `$52F6` — PEPS support for Telematic features

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | Disabled |
| `1` | Enabled |

### `$7206` — Application Software Status

| Value | Meaning |
|-------|---------|
| `0` | Component Erased |
| `1` | Request Download Complete |
| `2` | Request Upload Complete |
| `3` | Transfer data Complete |
| `4` | Transfer Exit Complete |
| `5` | Software Valid |
| `6` | Software Invalid |
| `15` | Virgin Component |

### `$7207` — Calibration Software Status

| Value | Meaning |
|-------|---------|
| `0` | Component Erased |
| `1` | Request Download Complete |
| `2` | Request Upload Complete |
| `3` | Transfer data Complete |
| `4` | Transfer Exit Complete |
| `5` | Software Valid |
| `6` | Software Invalid |
| `15` | Virgin Component |

### `$7212` — Vehicle Mode

| Value | Meaning |
|-------|---------|
| `0` | Customer |
| `1` | Transport |
| `2` | Factory |
| `3` | Crash |

### `$7215` — Active Software Component

| Value | Meaning |
|-------|---------|
| `0` | Unknown |
| `1` | Application |
| `2` | Primary Bootloader |
| `3` | Secondary Bootloader |

### `$A006` — Vehicle Power mode

| Value | Meaning |
|-------|---------|
| `0` | Pre-Stand By |
| `1` | Awake |
| `2` | Transport Park |
| `3` | KeyIn |
| `4` | Accessory |
| `5` | Accessory Delay |
| `6` | Active (Ignition ON) |
| `7` | Transport Drive |
| `8` | Run (Engine Run) |
| `9` | Start (Crank) |
| `10` | Transport Drive CRANK |
| `11` | Transport Drive RUN |

### `$A00D` — Park Brake State

| Value | Meaning |
|-------|---------|
| `0` | Parking OFF |
| `1` | Parking ON |

### `$F186` — Active Diagnostic Session

| Value | Meaning |
|-------|---------|
| `1` | Default |
| `3` | Extended |

### `$F281` — PEPS_PeriodicDID_F281

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Vehicle Mode | `1` | Customer |
|  |  | `2` | Transport |
|  |  | `4` | Factory |
|  |  | `8` | Crash |
| `0x01` | CANWakeup | `0` | Inactive |
|  |  | `1` | Active |
| `0x01` | RearLHDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `1` | Active |
| `0x02` | ActiveDiagnosticSession | `2` | Default Session |
|  |  | `8` | Extended Session |
| `0x02` | RKEWakeup | `0` | Inactive |
|  |  | `2` | Active |
| `0x02` | TailgateTrunkHandleSwitchWakeup | `0` | Inactive |
|  |  | `2` | Active |
| `0x04` | StartStopSwitch1Wakeup | `0` | Inactive |
|  |  | `4` | Active |
| `0x04` | ClutchSwitch90PercPressWakeup | `0` | Inactive |
|  |  | `4` | Active |
| `0x08` | StartStopSwitch2Wakeup | `0` | Inactive |
|  |  | `8` | Active |
| `0x08` | ClutchSwitch10PercPressWakeup | `0` | Inactive |
|  |  | `8` | Active |
| `0x10` | DriverDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `10` | Active |
| `0x10` | BrakeSwitchWakeup | `0` | Inactive |
|  |  | `10` | Active |
| `0x20` | CoDriverDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `20` | Active |
| `0x20` | BCMWakeup | `0` | Inactive |
|  |  | `20` | Active |
| `0x40` | RearRHDoorHandleSwitchWakeup | `0` | Inactive |
|  |  | `40` | Active |

### `$F282` — PEPS_PeriodicDID_F282

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | PEPSClampState | `1` | Off |
|  |  | `2` | Accessory |
|  |  | `4` | Ignition |
|  |  | `8` | Crank |
| `0x01` | ACCRelayOutput | `0` | Off |
|  |  | `1` | On |
| `0x01` | Vehicle Power mode | `1` | Pre-Stand By |
|  |  | `2` | Awake |
|  |  | `4` | Transport Park |
|  |  | `8` | KeyIn |
|  |  | `10` | Accessory |
|  |  | `20` | Accessory Delay |
|  |  | `40` | Active (Ignition ON) |
|  |  | `80` | Transport Drive |
|  |  | `1` | Run (Engine Run) |
|  |  | `2` | Start (Crank) |
|  |  | `4` | Transport Drive CRANK |
|  |  | `8` | Transport Drive RUN |
| `0x02` | IGNRelayOutput | `0` | Off |
|  |  | `2` | On |
| `0x04` | CrankRelayOutput | `0` | Off |
|  |  | `4` | On |

### `$F283` — PEPS_PeriodicDID_F283

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | DriverDoorHandleSwitch | `0` | Not Pressed |
|  |  | `1` | Pressed |
| `0x01` | StartStopSwitchInput1 | `0` | Not Pressed |
|  |  | `1` | Pressed |
| `0x02` | CoDriverDoorHandleSwitch | `0` | Not Pressed |
|  |  | `2` | Pressed |
| `0x02` | StartStopSwitchInput2 | `0` | Not Pressed |
|  |  | `2` | Pressed |
| `0x04` | RearRHDoorHandleSwitch | `0` | Not Pressed |
|  |  | `4` | Pressed |
| `0x04` | WakeupInputFromBCM | `0` | Off |
|  |  | `4` | On |
| `0x08` | RearLHDoorHandleSwitch | `0` | Not Pressed |
|  |  | `8` | Pressed |
| `0x10` | TailgateTrunkHandleSwitch | `0` | Not Pressed |
|  |  | `10` | Pressed |
| `0x20` | ClutchSwitch90PercPress | `0` | Not Pressed |
|  |  | `20` | Pressed |
| `0x40` | ClutchSwitch10PercPress | `0` | Not Pressed |
|  |  | `40` | Pressed |
| `0x80` | BrakeSwitch | `0` | Not Pressed |
|  |  | `80` | Pressed |

### `$F284` — PEPS_PeriodicDID_F284

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | DriverDoorState | `0` | Closed |
|  |  | `1` | Open |
| `0x01` | PassiveEntryExit | `0` | Disabled |
|  |  | `1` | Enabled |
| `0x02` | CoDriverDoorState | `0` | Closed |
|  |  | `2` | Open |
| `0x02` | Lock States Received From BCM | `2` | Externally Locked |
|  |  | `4` | Internally Locked |
|  |  | `8` | Unlocked |
|  |  | `10` | Crash Unlocked |
|  |  | `20` | Reverse Unlocked |
| `0x02` | PassiveStartStop | `0` | Disabled |
|  |  | `2` | Enabled |
| `0x04` | RearRHDoorState | `0` | Closed |
|  |  | `4` | Open |
| `0x04` | PEPSRKEOperation | `0` | Disabled |
|  |  | `4` | Enabled |
| `0x08` | RearLHDoorState | `0` | Closed |
|  |  | `8` | Open |
| `0x10` | TailgateState | `0` | Closed |
|  |  | `10` | Open |

## IMMO — Immobiliser

Request `0x704` · response `0x70C` · 10 enumerated DIDs, 4 of them bit-packed.

### `$4410` — Auto Key Learning Functionality Status

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Status of Auto Key Learning Process Function | `0` | ENABLED |
|  |  | `1` | DISABLED |
| `0x02` | Status of Auto Key Learning between IMMO and Transponder | `0` | PASSED |
|  |  | `2` | FAILED |

### `$4412` — Status of Transponder Secret Code

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Status Of Transponder Secret Code | `0` | LOCKED |
|  |  | `1` | UnLOCKED |
| `0x01` | Status Of Transponder ID Code Learned By IMMO | `0` | LOCKED |
|  |  | `1` | UnLOCKED |

### `$4415` — IMMO Status Indicator Lamp

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | ACTIVE |
| `1` | IN-ACTIVE |

### `$4417` — Status of EMS-IMMO CAN communication

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | ENABLED |
| `1` | DISABLED |

### `$4418` — IMMO Status

| Value | Meaning |
|-------|---------|
| `0` | Engine Starting Disabled |
| `1` | Engine Starting Enabled |
| `2` | Key Learning |

### `$441A` — Status of Authentication between Transponder and IMMO

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | Status of Authentication between Transponder and IMMO | `0` | FAILED |
|  |  | `1` | PASSED |
| `0x01` | Status of Authentication between IMMO and EMS | `0` | FAILED |
|  |  | `1` | PASSED |
| `0x01` | Status of IMMO ECU | `0` | Virgin ECU |
|  |  | `1` | Programmed ECU |

### `$441C` — Status of AES SK Key

Mask `0x01`.

| Value | Meaning |
|-------|---------|
| `0` | LOCKED |
| `1` | UnLOCKED |

### `$442A` — IIMMO Modes

| Mask | Signal | Value | Meaning |
|------|--------|-------|---------|
| `0x01` | IMMO status | `0` | OK |
|  |  | `1` | NOT OK |
| `0x01` | Operational Fault on IMMO | `0` | NOT PRESENT |
|  |  | `1` | PRESENT |
| `0x01` | IMMO Transponder key  learning mode | `0` | IN-ACTIVE |
|  |  | `1` | ACTIVE |
| `0x01` | IMMO Antiscan mode | `0` | IN-ACTIVE |
|  |  | `1` | ACTIVE |
| `0x01` | IMMO Transport mode | `0` | IN-ACTIVE |
|  |  | `1` | ACTIVE |

### `$7211` — Communication Status

| Value | Meaning |
|-------|---------|
| `0` | RX and TX Enabled |
| `1` | RX Enabled and TX Disabled |
| `2` | RX Disabled and TX Enabled |
| `3` | RX Disabled and TX Disabled |

### `$A06` — Vehicle Power Mode

| Value | Meaning |
|-------|---------|
| `0` | Pre-Standby |
| `1` | Awake |
| `2` | Transport Park |
| `3` | Key In |
| `4` | Accessory |
| `5` | Accessory Delay |
| `6` | Active (Ignition ON) |
| `7` | Transport Drive |
| `8` | Run (Engine Run) |
| `9` | Start (Crank) |
| `10` | Transport Drive CRANK |
| `11` | Transport Drive RUN |
