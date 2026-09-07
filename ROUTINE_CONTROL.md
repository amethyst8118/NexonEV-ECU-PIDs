# RoutineControl (`0x31`) and actuator tests (`0x2F`)

`0x22` reads a value. `0x31` **makes the car do something** — run a self test, learn
a key, enter transport mode, lock a secret key into NVM. Some of it is harmless,
some of it will strand you in a car park with keys that no longer pair.

Everything below is read out of the TDS 20.0 / 21.0 ECU databases. **None of it has
been executed on a car.** Read the safety section before you send anything.

> **Nothing here is needed to read data.** If you only want sensor values or fault
> codes, use [`Nexon_EV_All_PIDs.md`](Nexon_EV_All_PIDs.md) and
> [`READING_DTCS.md`](READING_DTCS.md), and skip this file entirely.

## The service

```
Request    31 <sub> <RID hi> <RID lo> [ input data ]
Response   71 <sub> <RID hi> <RID lo> [ status / results ]
```

`<sub>` is the sub-function, and every ECU here agrees on it:

| `<sub>` | Meaning |
|---------|---------|
| `01` | **Start** the routine |
| `02` | **Stop** it |
| `03` | **Request results** — read status, does not start anything |

`<RID>` is a 2-byte routine identifier, the equivalent of a DID. Starting the BCM's
service self test is `31 01 20 00`; asking how it went is `31 03 20 00`.

The RID high byte follows the UDS convention and is a useful first filter:

| Range | Class |
|-------|-------|
| `20xx` | manufacturer self-test and vehicle-mode routines |
| `34xx` | assembly and configuration routines |
| `4Fxx` | security, key learning, immobiliser |
| `F0xx` `F1xx` | supplier-specific (bleeding, calibration) |
| `FF0x` | reprogramming — erase memory, check dependencies |

## Which ECUs actually have routines

TDS ships a `Routines Test` button flag per ECU, and it is the honest answer to
"does this ECU do routines". Only two are switched on:

| ECU | Header | `Routines Test` | Routines defined | Verdict |
|-----|--------|-----------------|-----------------:|---------|
| **BCM** | `0x701` | **Y** | 12 | real, and the tool exposes them |
| **PEPS** | `0x710` | **Y** | 21 | real, and the tool exposes them |
| IMMO | `0x704` | *(no button table)* | 7 | real, driven from the replacement flows |
| VECU | `0x7E3` | N | 2 | defined but not offered — see below |
| MCU | `0x783` | N | 10 | **not MCU routines at all** — see below |
| BMS | `0x785` | N | **0** | no routines exist |
| DCDC | `0x784` | N | **0** | no routines exist |
| OBC | `0x786` | N | **0** | no routines exist |

**The BMS has no routines.** Its `Routines`, `RoutinesService` and `RoutineStatus`
tables are all empty in both TDS 20.0 and 21.0, and the button is off. There is no
BMS cell-balancing trigger, no SOC reset, no pack recalibration routine to be found
here. If you were hoping `0x31` would let you force a balance cycle — it will not.
Same for the DC-DC converter and the on-board charger.

### The MCU table is contaminated

`MCU_DiagnosticsDB.sdf` has 10 rows in `tblRoutines`, and **none of them belong to a
motor controller**. They are `EvacAndFill`, `RepairBleed`, `WheelSpeedSensorTest`,
`SteeringAngleSensorCalibration` and four wheel-by-wheel brake-bleeding steps — an
**ABS** routine set copied in wholesale. Four independent signs:

- every language code is `ABS_0218`–`ABS_0230`, and **none of those ids exist** in
  the MCU's own string tables, so the tool could not render them if it tried;
- the table carries an `ABSVariant` column that no other MCU table has;
- the service conditions require *engine speed = 0*, on a car with no engine;
- `Routines Test` is `N`.

Do not send `31 01 F0 00` to `0x783` expecting anything. Treat the MCU as having
zero routines.

### The VECU pair, and why to leave it alone

The VECU database defines two routines, both security-key operations, both with
`Routines Test` switched off:

| RID | Name | Start | Stop | Result | Precondition |
|-----|------|:-----:|:----:|:------:|--------------|
| `0x0009` | AES-SK Locking | Y | N | Y | 9–16 V, extended session, **security access unlocked** |
| `0x4F0F` | AES-SK Unlocking | Y | N | Y | 9–16 V, extended session, **security access unlocked** |

Two reasons not to touch these. First, they are gated behind `0x27` SecurityAccess,
whose seed/key algorithm is not in these databases — you cannot run them anyway.
Second, **the numbering looks wrong**: PEPS defines the same lock/unlock pair as
`0x4F0A` / `0x4F0B`, while `0x4F0F` on PEPS is *Start transponder authentication*.
A "locking" routine sitting at `0x0009`, in the `20xx` self-test range, fits no
convention. These two rows may be as miscopied as the MCU's. Flagged, not trusted.

Note also that the VECU uses `10 01` for data reading, but these routines demand the
**extended** session `10 03`.

## BCM routines — `0x701`

Twelve, all flagged for every BCM variant.

| RID | Routine | Start | Stop | Result | Risk |
|-----|---------|:-----:|:----:|:------:|------|
| `20 00` | Service Self Test Routine | Y | Y | Y | 🟡 noisy |
| `20 02` | **Activate** Transport Mode | Y | N | N | 🔴 |
| `20 02` | **Deactivate** Transport Mode | Y | N | N | 🔴 |
| `34 09` | Input Switch test | Y | Y | Y | 🟢 |
| `34 0C` | PEPS-BCM Encryption Counter Reset | Y | Y | Y | 🔴 |
| `34 0D` | PDC Configuration | Y | Y | Y | 🟡 |
| `34 0D` | FPAS Configuration | Y | Y | Y | 🟡 |
| `34 0E` | Remote Key Learning (1 Key) | Y | N | Y | 🔴 |
| `34 0E` | Remote Key Learning (2 Keys) | Y | N | Y | 🔴 |
| `34 0E` | Remote Key Learning (3 Keys) | Y | N | Y | 🔴 |
| `4F 0A` | IMMO Transponder Key **Erase** | Y | N | N | 🔴🔴 |
| `4F 0D` | IMMO Transponder Key Learning | Y | N | N | 🔴 |

**Transport mode takes an input byte.** The two `20 02` rows are the same RID,
distinguished only by their `InputData` column:

```
31 01 20 02 01     activate transport mode
31 01 20 02 00     deactivate transport mode
```

Transport mode is the dealer setting that suppresses body functions for shipping.
Turning it on is one byte; nothing in the databases describes what a car does while
it is on. `Input Switch test` similarly carries `InputData` `15`, presumably a
duration, but the schema documents it nowhere.

**Service Self Test drives 29 outputs**, and Input Switch test monitors 44 inputs —
both enumerated in `RoutineOutputList`. The self test cycles low beam, main beam,
front and rear fog, both indicators, brake and reverse lamps, all door locks, front
and rear washers and wipers, roof and ambient lighting, **the horn**, mirror fold,
battery saver, hazard LED and both DRLs. Not dangerous, but a car doing all of that
by itself in a residential street at night is a conversation you may not want.
Preconditions: extended session, doors and tailgate closed but unlocked, wipers
parked, all lamps off, Park selected.

`34 09` Input Switch test is the one genuinely safe entry — it watches inputs while
*you* operate switches, and drives nothing.

### BCM and PEPS status byte

Both decode the status in `71 03 <hi> <lo> <status>` as a **bitmask**, not an enum:

| Bit | Value | Meaning |
|----:|------:|---------|
| 0 | `0x01` | routine completed all requested functionality |
| 1 | `0x02` | aborted by the ECU — conditions not correct |
| 2 | `0x04` | aborted by the tester |
| 3 | `0x08` | routine currently active |
| 7 | `0x80` | routine created a DTC |

So `0x81` is "completed, and set a fault code", and `0x88` is "still running, and has
already set one". Test `value & mask`, never equality.

Key-learning progress is reported separately in `RKLRoutineStatus`, which is two
different enums sharing one table:

| Value | Key count (`KeyType 1`) | Pairing result (`KeyType 2`) |
|------:|-------------------------|------------------------------|
| `00` | no key paired yet | keys not paired |
| `01` | 1st key paired | keys paired |
| `02` | 2nd key paired | pairing failed — time out |
| `03` | 3rd key paired | pairing failed — same paired key ID |
| `04`–`06` | 4th–6th key paired | — |

## PEPS routines — `0x710`

Twenty-one, the largest set. `RoutineIdentifier` is stored as a full 2-byte hex
value, so it drops straight into the request.

| RID | Routine | Start | Stop | Result | Input | Risk |
|-----|---------|:-----:|:----:|:------:|:-----:|------|
| `2000` | Service Self-Test Routine | Y | N | Y | | 🟡 |
| `2001` | EOL Self-Test Routine | Y | Y | Y | | 🟡 |
| `2002` | Set Vehicle Mode (transport) | Y | N | N | ● | 🔴 |
| `2009` | Check Valid Software Component | Y | N | N | | 🟢 |
| `200A` | Activate Secondary Bootloader | Y | N | N | | 🔴🔴 |
| `200D` | Check Reprogramming Preconditions | Y | N | N | | 🟢 |
| `200E` | Read Masked DTCs Routine | Y | Y | Y | | 🟢 |
| `3408` | Current Draw Test (Without IBS) | Y | Y | Y | ● | 🟡 |
| `3409` | Input switch Test | Y | Y | Y | | 🟢 |
| `4F08` | Start LF-Antenna Diagnosis | Y | Y | Y | ● | 🟢 |
| `4F09` | Start RF-Antenna Diagnosis | Y | N | N | | 🟢 |
| `4F0A` | **Confirm and lock AES-SK** | Y | N | N | | 🔴🔴 |
| `4F0B` | Unlock AES-SK | Y | N | N | | 🔴 |
| `4F0D` | Transponder learning — initial | Y | N | Y | ● | 🔴🔴 |
| `4F0E` | Transponder learning — add | Y | N | Y | ● | 🔴 |
| `4F0F` | Start transponder authentication | Y | N | N | | 🟡 |
| `4F11` | UID Localization Test | Y | N | N | ● | 🟢 |
| `4F12` | Free Ignition ON count / reset | Y | N | N | ● | 🟡 |
| `4F13` | **Reset Odometer Value** | Y | N | N | | 🔴🔴 |
| `FF00` | Erase Memory | Y | N | N | ● | 🔴🔴 |
| `FF01` | CheckProgrammingDependencies | Y | N | N | | 🟢 |

Only three — `2000`, `4F0D`, `4F0E` — have `DisplayInTool = Y`. The rest are driven
internally by TDS's replacement and reflash flows.

Two deserve naming outright. **`4F0D` "transponder learning initial" deletes every
previously paired smart key before it learns a new one** — the database says so, at
step 3 of its own procedure. Run it without every key in hand and the car will not
start. And **`4F13` resets the PEPS odometer to zero**, which on a road car is
odometer tampering and illegal in most jurisdictions, India included.

`4F0A` locks the AES secret key into NVM. It is reversible only by `4F0B`, which
itself needs a working diagnostic session — if anything goes wrong in between, the
module needs replacing.

### PEPS routine results

PEPS is the only ECU with a full result-decoding schema: `PacketedRoutines`,
`RoutineDecoding_Bitmapped` (267 rows), `RoutineDecoding_StateEncoded` (64) and
`RoutineDecoding_Numeric` (10), mapping response bytes to named fields.

| Routine | Field | Meaning |
|---------|-------|---------|
| `200E` | DTC Masks | count of masked DTCs |
| `3408` | Total Loads | loads measured |
| `3409` | Number of switches present | |
| `4F08` | Number of antennas present | |
| `4F0D` `4F0E` | Pairing status of UID 1–6 | `0` blank, `1` learned, `2` paired |
| `4F0D` `4F0E` | PEPS life cycle UID | `0` blank, `1` not blank |
| `4F0F` | Authentication Status | `1` OK, `2` timeout, `4` NOK, `8` NvM error |
| `4F12` | Number of remaining free ignitions | |

The `BytePosition` values in these tables follow the same convention as the DID
tables — position 4 is the first byte after the 3-byte service echo — but a
`71 <sub> <hi> <lo>` reply echoes **four** bytes, and the schema is inconsistent
about whether the status sits at 4 or 5. **Confirm against a live reply before
trusting an offset.** It is one byte either way.

## IMMO routines — `0x704`

Seven. This is the immobiliser, so all of them are consequential.

| RID | Routine | Result | Risk |
|-----|---------|:------:|------|
| `20 00` | Service Self-Test Routine | Y | 🟢 |
| `20 02` | Set Vehicle Mode | N | 🔴 |
| `4F 00` | Start Authentication between IMMO and Transponder | N | 🟡 |
| `4F 01` | Transponder Key Learn Routine | Y | 🔴 |
| `4F 02` | **Transponder Key Erase Routine** | Y | 🔴🔴 |
| `4F 03` | Lock IMMO Pairing | Y | 🔴 |
| `4F 07` | Learn Single Transponder Key Routine | Y | 🔴 |

All seven are Start-only — no stop, so once sent you wait. All require ignition on,
engine not running.

The database's own warning on `4F 02` is unusually direct: *"All other keys, apart
from the key in the ignition barrel, will be erased / unlearnt from IMMO."* Two of
the seven (`4F 01`, `4F 02`) carry `IsWarningMsgToBeShown = Y`, and three tell the
operator to contact Tata Motors service before running them.

Result strings live in `tblResource_Routines` and are worth reading as documentation
of what failure looks like: *"Routine completed successfully. But, keys are not
erased / unlearnt from IMMO"*, *"Routine not completed due to time out"*,
*"Transponder key learning failed"*.

## Actuator tests — `0x2F` on the VECU

The VECU has no usable routines, but its `Test Actuators` button **is** enabled, and
that is a different service: `InputOutputControlByIdentifier`.

```
Request    2F <DID hi> <DID lo> <control option> [ state ]
```

Seven controllable outputs, all in the `34xx` DID range, all 4 bytes:

| DID | Actuator | Type | Range |
|-----|----------|------|-------|
| `$3439` | Traction Cooling Pump | value | 0–100 % |
| `$3437` | Battery Cooling Pump | value | 0–100 % |
| `$3452` | Traction Cooling Fan Fast | bit `0x01` | Off / On |
| `$3453` | Traction Cooling Fan Slow | bit `0x02` | Off / On |
| `$344C` | Cabin Cooling Solenoid Valve | bit `0x03` | Closed / Open |
| `$344D` | Battery Cooling Solenoid Valve | bit `0x03` | Closed / Open |
| `$3436` | **Charging Gun Lock/Unlock Command** | bit `0x03` | — |

All seven support On and Off; **none reports status** (`IOLI_Status = N`), so you
command blind and read the effect back through the ordinary `0x22` sensors.

`$3436` is the one to be careful with. Releasing the charging gun lock on a car that
is mid-session on a DC fast charger means unlatching a connector carrying hundreds
of volts. Do not.

The pumps and fans are less dramatic but still override the thermal management of a
live traction battery. Forcing the battery cooling pump to 0 % during a fast charge
is a real way to damage a pack.

## Negative responses

A refused routine comes back as `7F 31 <NRC>`. The ones you will actually see:

| NRC | Meaning | What it usually means here |
|-----|---------|----------------------------|
| `11` | Service not supported | ECU has no routines — BMS, DCDC, OBC, MCU |
| `12` | Sub-function not supported | routine has no Stop, or no Request-Results |
| `13` | Incorrect message length | routine wanted input data you did not send |
| `22` | Conditions not correct | preconditions unmet — voltage, speed, session |
| `23` | **Routine not completed** | asked for results before it finished |
| `24` | Request sequence error | results requested before a start |
| `31` | Request out of range | that RID does not exist on this ECU |
| `33` | Security access denied | needs `0x27` first — the VECU pair, reflash routines |
| `78` | Response pending | normal; the ECU is working, keep waiting |
| `7E` | Sub-function not supported in active session | you are in `10 01`, it wants `10 03` |
| `7F` | Service not supported in active session | same, for the whole service |

`0x23` "Routine not completed" is specific to this service and is not an error — it
means "still running, ask again".

## Safety

Ranked by what goes wrong, worst first.

**🔴🔴 Can leave the car undriveable, or is illegal.**
`BCM 4F 0A` and `IMMO 4F 02` erase paired keys. `PEPS 4F 0D` erases every paired
smart key before learning. `PEPS 4F 0A` locks the AES secret key into NVM.
`PEPS 4F 13` zeroes the odometer — tampering, and illegal on a road car.
`PEPS FF00` erases ECU memory and `PEPS 200A` activates a bootloader; both belong to
a reflash sequence, and are how a module gets bricked outside one.

**🔴 Changes persistent vehicle state.**
Transport mode on the BCM (`20 02`) and on PEPS/IMMO (`2002` / `20 02`). Key
learning of any kind. The PEPS-BCM encryption counter reset (`34 0C`), which exists
for the case where PEPS has been replaced and nothing else.

**🟡 Visible, noisy or transient.**
Self tests that drive lamps, wipers and the horn. Configuration routines. Current
draw tests. Nothing persistent, but the car will do things on its own.

**🟢 Read-only or input-monitoring.**
`31 03 <RID>` on a routine that is not running just returns status. Input switch
tests, antenna diagnostics, preconditions checks, `PEPS 200E` masked-DTC read.

Three rules that hold across all of it:

1. **`31 03` is the safe sub-function.** Requesting results never starts anything.
   If you are exploring, explore with `03`.
2. **Never run a routine on a moving or charging car.** Every database precondition
   assumes vehicle speed 0, and several assume the car is not on a charger.
3. **Have every key in hand before touching anything in the `4Fxx` range.** That
   range is entirely immobiliser and key pairing, and several routines delete
   existing pairings as their *first* step, before checking the new one works.

None of this has been executed on a car as part of this project. The tables say what
TDS believes; a real ECU is the only thing that can confirm it.
