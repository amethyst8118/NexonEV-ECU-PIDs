# Cell balancing — `$3479` and everything around it

Cell balancing status is not a number you read off; it is one bit inside a packed
byte, and the byte's layout changed between database generations. This is the
complete picture for the **TDS 20.0** BMS (`0x785`).

## `$3479` — the flag byte

```
22 3479          request
62 34 79 XX      response, XX is the flag byte
```

Three signals share the byte:

| Mask | Bit | Signal |
|------|-----|--------|
| `0x01` | 0 | VCU HVIL Detect Signal |
| `0x02` | 1 | VCU Insulation Control Command |
| `0x04` | 2 | **BMS Cell Balance Status** |

Only three bits are defined here. Bits 3–7 have no definition in TDS 20.0 — but
bit 4 **has been seen set on a real car**, so the database is incomplete rather
than the upper bits being unused. See *Observed on the car* below.

## Every value

The byte can only take eight documented values. Read yours off this table:

| Value | Binary | HVIL | Insulation | **Balancing** |
|------:|--------|:----:|:----------:|:-------------:|
| `0` | `00000000` | – | – | – |
| `1` | `00000001` | ✓ | – | – |
| `2` | `00000010` | – | ✓ | – |
| `3` | `00000011` | ✓ | ✓ | – |
| `4` | `00000100` | – | – | **✓** |
| `5` | `00000101` | ✓ | – | **✓** |
| `6` | `00000110` | – | ✓ | **✓** |
| `7` | `00000111` | ✓ | ✓ | **✓** |

**Balancing is running whenever the value is 4 or higher** (4, 5, 6 or 7).

Anything above `7` means a bit the database does not define is set — which does
happen: a real car returned **16**. See the section below. Values above 7 are not
an error, they are the database being incomplete.

## Observed on the car: `16`

A real reading from a Nexon EV via CarScanner on `0x785` was **16**.

`16` = `0x10` = **bit 4**, and none of the three bits TDS 20.0 defines are set:

```
0x10 & 0x01 = 0   VCU HVIL Detect      : clear
0x10 & 0x02 = 0   Insulation Control   : clear
0x10 & 0x04 = 0   Cell Balance Status  : clear   <- not balancing
```

**Balancing was not running**, and that conclusion holds whichever bit map
applies — balancing is `0x04` on TDS 20.0 and `0x02` on 8.9S, and neither bit is
set in `16`. So the practical answer does not depend on resolving the layout.

Bit 4 itself is **not defined in TDS 20.0's database at all**. It *is* defined in
TDS 8.9S, as `VCU_HVILDetect` — the high-voltage interlock loop being monitored.

A value of exactly 16 under the 8.9S map reads as: HVIL monitoring active, no
derate, not balancing, not charging, no equalization requested. That is precisely
what a car sitting with ignition on and nothing plugged in should report, which is
good circumstantial support for the 8.9S map applying to this pack even though it
answers on the newer `0x785` address.

Note this also settles something the earlier bus sweep got wrong: **the BMS does
answer on `0x785` from the standard OBD port**. Only the BMS defines `$3479` —
no other ECU on the car could have produced this reading.

### Telling the two layouts apart, on the car

Plug into an AC charger and watch `$3479`:

| It becomes | Layout in force | Then balancing will read |
|---|---|---|
| **24** (`16 + 8`) | TDS 8.9S — `0x08` is `VCU_Charging_Flag` | **18** (`16 + 2`) |
| **stays 16** | TDS 20.0 — charging is not in this byte | **20** (`16 + 4`) |

The charging bit is the useful discriminator because it is the one thing you can
deliberately turn on. Once you know which map applies, the balancing bit follows
from the same table.

Two DIDs corroborate bit 4 being HVIL:

- **`$3565` BMS HVIL Hardwire Signal** on the BMS itself (`0x785`).
- **`$3561` Battery HVIL Sense PWM dutycycle** on the VECU (`0x7E3`).

If both show the interlock healthy while `$3479` reads 16, bit 4 is HVIL.

And to confirm which database generation the firmware matches, read **`$340E`
BMS Operation Mode**. It exists only in TDS 20.0; if the ECU answers with data
rather than a negative response, the firmware is the newer generation.

## Test with the mask, not equality

The database's `ResultByte` column is **inconsistent** on this DID:

| Mask | Stored "enable" value |
|------|----------------------|
| `0x01` | `1` |
| `0x02` | `2` — the masked value |
| `0x04` | `1` — *not* the masked value |

Two of the three use the masked value, one uses `1`. So comparing the byte against
`ResultByte` gives the wrong answer for at least one signal whichever convention
you assume.

```python
balancing = bool(raw & 0x04)      # correct
balancing = (raw == 4)            # wrong: misses 5, 6, 7
balancing = (raw & 0x04) == 1     # wrong: 0x04 & 4 is 4, never 1
```

Always `raw & mask != 0`.

## The other generation

The older TDS 8.9S database (`KPD_EV_BMS.inf`, 29-bit `0x1BDA96F1`) defines the
same DID completely differently:

| Mask | TDS 20.0 (`0x785`) | TDS 8.9S (29-bit) |
|------|--------------------|-------------------|
| `0x01` | VCU HVIL Detect | BMS Derate Flag |
| `0x02` | VCU Insulation Control | **BMS Cell Balance Status** |
| `0x04` | **BMS Cell Balance Status** | HSC BCM Leakage Enable |
| `0x08` | – | VCU Charging Flag |
| `0x10` | – | VCU HVIL Detect |
| `0x20` | – | VCU Equalization Trigger |

**Balancing moves from `0x02` to `0x04`**, and on the newer database `0x02` means
insulation control instead. Reading the wrong table does not fail visibly — it
reports a different signal with full confidence.

To tell which applies: **`22 340E` returns data only on TDS 20.0.** If it answers,
use the `0x04` table.

## Related DIDs

On TDS 20.0 several things that were bits inside `$3479` on the old database became
DIDs of their own, which is easier to read:

| DID | Signal | Type |
|-----|--------|------|
| `$340E` | BMS Operation Mode | enum, 8 states |
| `$340F` | BMS Derate Flag | `0x01` mask |
| `$3493` | VCU Flag — fast charge, charging enabled, slow charge | 3 bits |
| `$34D5` | Cell voltage difference | mV |
| `$3415` / `$3417` | Max / min cell voltage | mV |
| `$3419` / `$341A` | Which cell is highest / lowest | index |

`$340E` operating mode:

| Value | Meaning |
|------:|---------|
| `0` | Power-on self-test |
| `1` | Stand By |
| `2` | Precharge |
| `3` | Hvactive |
| `4` | HVPowerdown |
| `5` | PreChargeFailure |
| `6` | Fault |
| `0x0F` | Ready to Sleep |

`$3493` VCU Flag:

| Mask | Signal |
|------|--------|
| `0x01` | VCU Fast Charging Flag |
| `0x02` | Charging Enabled Status |
| `0x04` | VCU Slow Charging Flag |

Reading `$3479` and `$3493` together tells you both whether balancing is running
and whether the car is charging — the condition it normally runs under.

## What balancing actually does here

It is **passive (dissipative)**. Tata's BMS DTC manual states the BMS *"perform the
balancing by dissipating the heat through balancing resistor"*, which is why it
raises board temperature. Three consequences:

- it can only bleed **high** cells down, never raise a low one — a low outlier cell
  will never be corrected by balancing;
- it is slow, on the order of tens of milliamps, so closing a 50 mV spread takes
  hours rather than minutes;
- it runs during and after **charging**, so `$3479` bit 2 clear while you are
  driving is normal and not a fault.

The clearest official statement of its purpose is the healing condition Tata gives
for the dynamic cell-difference fault `P1208-1C`: *"vehicle slow charging over next
few ignition cycles"*. Balancing is what clears an imbalance, and it happens on
slow charge.

## Watching it work

Read `$34D5` (cell voltage difference) **at rest**. The manuals split the
difference faults into static (`P3069-1C`, at rest) and dynamic (`P1208-1C`, under
load) precisely because a large delta under load is mostly internal-resistance
spread between cells and shrinks once you stop.

A healthy balancing session looks like: plugged into AC, high state of charge,
`$3479 & 0x04` set, and `$34D5` shrinking over hours with `$3419` reporting the
same cell index as the highest throughout.

> The BMS has **no cell-balancing hardware fault code**. Gotion packs have
> `P3054-96` and Kratos has per-pack slave balancing faults; this one has neither.
> `$3479` bit 2 is your only visibility into whether the balancing hardware still
> works, which makes it worth logging rather than glancing at.
