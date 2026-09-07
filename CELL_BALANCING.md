# Cell balancing — `$3479` and everything around it

Balancing status is not a number you read off. It is one bit inside a packed byte,
and TDS 20.0's own BMS database describes that byte **twice, inconsistently**. This
is what the database actually contains, and how to read it without guessing.

Source throughout: `BMS_DB.sdf` from TDS 20.0. Request `0x785`, response `0x78D`.

## `$3479` — the flag byte

```
22 3479          request
62 34 79 XX      response, XX is the flag byte
```

The live definition (`ParameterId` 109) gives three signals:

| Mask | Bit | Signal |
|------|-----|--------|
| `0x01` | 0 | VCU HVIL Detect Signal |
| `0x02` | 1 | VCU Insulation Control Command |
| `0x04` | 2 | **BMS Cell Balance Status** |

Balancing is **`0x04`**. Bits 3–7 have no definition on this row.

## Values under the live definition

| Value | HVIL | Insulation | **Balancing** |
|------:|:----:|:----------:|:-------------:|
| `0` | – | – | – |
| `1` | ✓ | – | – |
| `2` | – | ✓ | – |
| `3` | ✓ | ✓ | – |
| `4` | – | – | **✓** |
| `5` | ✓ | – | **✓** |
| `6` | – | ✓ | **✓** |
| `7` | ✓ | ✓ | **✓** |

`raw & 0x04` is the balancing test. A value **above 7** means a bit outside this
definition is set — which is expected rather than anomalous, for the reason below.

## The database contradicts itself

`BMS_DB.sdf` contains a **second, orphaned definition** of the same concept.
`ParameterId` 86 has a complete set of decode rows but **no matching row in
`DataIdentifiers`** — the DID it belonged to was deleted from the table while its
decode rows were left behind. `ParameterId` 85, 87 and 90 are missing the same way;
87 is the relay-status equivalent.

What the orphaned rows define:

| Mask | Bit | Signal |
|------|-----|--------|
| `0x01` | 0 | Battery Derate Status |
| `0x02` | 1 | **BMS_CellBalanceStatus** |
| `0x04` | 2 | Insulation Measurement Enable |
| `0x08` | 3 | Charging Enabled Status |
| `0x20` | 5 | Initiation of cell balancing by vehicle |

Set against the live definition:

| Bit | Live (`$3479`, pid 109) | Orphaned (pid 86) |
|----:|-------------------------|-------------------|
| 0 | VCU HVIL Detect | Battery Derate Status |
| 1 | VCU Insulation Control | **Cell Balance Status** |
| 2 | **Cell Balance Status** | Insulation Measurement Enable |
| 3 | – | Charging Enabled Status |
| 5 | – | Initiation of balancing by vehicle |

**Balancing and insulation are swapped between the two.** That is the trap: read
the wrong one and you get "insulation" where you meant "balancing", or the reverse,
with nothing to warn you.

The orphaned set also fills in bits the live definition leaves blank — bit 3
(charging) and bit 5 (a balancing request from the vehicle). So a byte value above
7 is not corruption; it is a bit the live row simply does not describe.

The same pattern shows on `$3404`: the live definition puts the relays on
`0x01`/`0x02`/`0x04`, while the orphaned pid 87 rows put them on
`0x02`/`0x04`/`0x08`.

## Reading it safely

With two candidate maps, the reliable approach is to trust only what they agree on
and treat the rest as unknown until it is measured.

```python
raw = read_did(0x3479)

balancing_live   = bool(raw & 0x04)   # live definition
balancing_orphan = bool(raw & 0x02)   # orphaned definition

if balancing_live and balancing_orphan:
    state = "balancing"          # both agree
elif not balancing_live and not balancing_orphan:
    state = "not balancing"      # both agree
else:
    state = "unresolved"         # the maps disagree - do not guess
```

Always test with `raw & mask`, never equality. The database's own `ResultByte`
column is inconsistent on this DID — `0x02` stores its masked value (`2`) while
`0x04` stores `1` — so comparing the byte against `ResultByte` gives the wrong
answer for at least one signal whichever convention you assume.

```python
balancing = bool(raw & 0x04)      # correct
balancing = (raw == 4)            # wrong: misses 5, 6, 7
balancing = (raw & 0x04) == 1     # wrong: 0x04 & 4 is 4, never 1
```

## Settling it on the car

Bit 3 is the discriminator, because charging is the one condition you can switch on
deliberately.

**Read `$3479` plugged into an AC charger, then again unplugged.**

- If a `+8` appears while charging and clears when you unplug, **bit 3 is the
  charging flag** — this firmware follows the orphaned definition, and balancing is
  therefore `0x02`.
- If nothing changes, charging is not in this byte, and the live definition
  (balancing on `0x04`) stands.

Two DIDs corroborate independently:

- **`$3565`** BMS HVIL Hardwire Signal, on the BMS itself.
- **`$3561`** Battery HVIL Sense PWM duty cycle, on the VECU (`0x7E3`).

And **`$3493` VCU Flag** carries charging state as its own DID — `0x01` fast
charging, `0x02` charging enabled, `0x04` slow charging. Reading it alongside
`$3479` tells you whether the car is charging without needing to decide what bit 3
means.

## Related DIDs

Several things that share the packed byte also exist as DIDs of their own, which
are unambiguous and worth preferring:

| DID | Signal | Type |
|-----|--------|------|
| `$340E` | BMS Operation Mode | enum, 8 states |
| `$340F` | BMS Derate Flag | `0x01` mask |
| `$3493` | VCU Flag — fast / enabled / slow charging | 3 bits |
| `$34D5` | Cell voltage difference | mV |
| `$3415` / `$3417` | Max / min cell voltage | mV |
| `$3419` / `$341A` | Which cell is highest / lowest | index |
| `$3565` | BMS HVIL Hardwire Signal | 1 byte |

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

## What balancing actually does here

It is **passive (dissipative)**. Tata's BMS DTC manual states the BMS *"perform the
balancing by dissipating the heat through balancing resistor"*, which is why it
raises board temperature. Three consequences:

- it can only bleed **high** cells down, never raise a low one — a low outlier cell
  will never be corrected by balancing;
- it is slow, on the order of tens of milliamps, so closing a 50 mV spread takes
  hours rather than minutes;
- it runs during and after **charging**, so a clear balancing bit while you are
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

A healthy balancing session: plugged into AC, high state of charge, the balancing
bit set, and `$34D5` shrinking over hours with `$3419` reporting the same cell
index as the highest throughout.

> The BMS has **no cell-balancing hardware fault code**. Other Tata packs do —
> Gotion has `P3054-96` and Kratos has per-pack slave balancing faults — but this
> one has neither. `$3479` is your only visibility into whether the balancing
> hardware still works, which makes it worth logging rather than glancing at.
