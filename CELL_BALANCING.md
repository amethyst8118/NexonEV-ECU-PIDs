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

Only three bits are defined. Bits 3–7 have no definition in the database.

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

Anything above `7` means a bit the database does not define is set. That is worth
reporting rather than ignoring — it is new information, not an error.

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
