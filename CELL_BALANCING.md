# Cell balancing — `$3479` and everything around it

Balancing status is not a number you read off — it is one bit inside a packed byte.
The database contains a second, older description of that byte which assigns the
bit differently, and it is easy to mistake for an alternative reading. It is not:
it is a retired ancestor. **Balancing is `0x04`.**

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

`raw & 0x04` is the balancing test. A value **above 7** means a bit nothing in the
current database describes — see below, and treat it as a genuine unknown.

## The second definition is retired, not an alternative

`BMS_DB.sdf` contains a second set of decode rows for a similar byte, under
`ParameterId` 86, which puts balancing on `0x02` instead. Three things establish
that it is superseded rather than a parallel map.

**It has no DID row at all.** Every live decode group points at a row in
`DataIdentifiers`; `ParameterId` 86 and 87 do not. Their DID rows were deleted
outright, which is a stronger retirement than the `None` permission used elsewhere
in the same table.

**Its ParameterId places it a generation earlier.** Every live enumerated DID sits
at `ParameterId` 109–131. The orphans are 86 and 87. That matches the pattern
visible throughout this database, where a revised row is appended with a higher id
and the original is disabled or removed.

**Its signals have named successors.** The retired byte was not reinterpreted — it
was split into four separate DIDs:

| Retired (pid 86) | Now lives at |
|------------------|--------------|
| `0x01` Battery Derate Status | **`$340F`** BMS Derate Flag, mask `0x01` |
| `0x02` BMS_CellBalanceStatus | **`$3479`** BMS Cell Balance Status Flag, mask **`0x04`** |
| `0x04` Insulation Measurement Enable | **`$3414`** BMS Insulation Function Enable/Disable |
| `0x08` Charging Enabled Status | **`$3493`** VCU Flag, mask `0x02` |
| `0x20` Initiation of cell balancing by vehicle | no successor in the current table |

Two of those carry the **identical signal name** across the move — *Battery Derate
Status* and *Charging Enabled Status* — which is what makes this a documented
migration rather than a guess.

The relay byte went the same way: retired `ParameterId` 87 held the three
contactors on `0x02`/`0x04`/`0x08`, and the current `$3404` holds the same three on
`0x01`/`0x02`/`0x04`. Re-laid-out, not reinterpreted.

### What this means for the upper bits

Because charging moved to `$3493` and derate moved to `$340F`, bits 3–7 of `$3479`
carry **no known meaning at all** on the current firmware. A value above 7 is not
the old map showing through — the old map's signals are elsewhere now. If you see
one, it is genuinely undocumented, and worth reporting.

## Reading it safely

```python
balancing = bool(read_did(0x3479) & 0x04)
```

Test with `raw & mask`, never equality. The database's `ResultByte` column is
inconsistent on this DID — `0x02` stores its masked value (`2`) while `0x04` stores
`1` — so comparing the byte against `ResultByte` gives the wrong answer for at
least one signal whichever convention you assume.

```python
balancing = bool(raw & 0x04)      # correct
balancing = (raw == 4)            # wrong: misses 5, 6, 7
balancing = (raw & 0x04) == 1     # wrong: 0x04 & 4 is 4, never 1
```

## Cross-checking on the car

`$3479` no longer carries charging state, so read **`$3493` VCU Flag** alongside it:
`0x01` fast charging, `0x02` charging enabled, `0x04` slow charging. Together they
tell you whether balancing is running *and* whether the car is in the condition
that normally triggers it.

Useful confirmations:

- **`$340E`** BMS Operation Mode — should read `3` (Hvactive) or `2` (Precharge)
  while the pack is live.
- **`$340F`** BMS Derate Flag — the signal that used to share the packed byte.
- **`$3565`** BMS HVIL Hardwire Signal, and **`$3561`** on the VECU (`0x7E3`).

## The VECU's own view

`$3479` is the BMS's answer, on `0x785`. The **VECU** carries its own cell-balancing
signals on `0x7E3`, and unlike `$3479` they are plain booleans rather than bits in a
packed byte.

| DID | Signal | Read | Values |
|-----|--------|:----:|--------|
| `$3454` | HV Battery Equalization **Trigger Status** | **Y** | `0` no balance, `1` balancing ON |
| `$34BC` | HV Battery cell equalization **command** | **Y** | `0` no action, `1` equalization command |
| `$345A` | HV Battery Equalization **Status Feedback** | N | `0` no balance, `1` balancing ON |

`$345A` is the row that defines the meanings, and it is the one you cannot read —
`ReadPermission = N`, so `22 345A` returns NRC `0x31`. `$3454` is the same signal
with read access, and `$34BC` is the command that causes it rather than the state
that results.

```
22 3454          request       (header 7E3, response 7EB, session 10 01)
62 34 54 01      balancing ON
```

### These are not flagged for a Nexon

All three are `BaseVariant = Y` with **`K2 = N` and `K3 = N`** — the two platform
columns that cover a Nexon EV. Tata does not list them for this car.

That flag is not noise. Every VECU DID confirmed working on a real Nexon — `$3421`
SOC, `$341D`/`$341E` pack current and voltage, `$3477`/`$3478` cell voltage max and
min, `$3462` speed — is flagged on **all eleven** platform columns, K2 and K3
included. 223 readable VECU DIDs sit in the `BaseVariant`-only bucket, and the
working ones are not among them.

They are nonetheless **included in the Nexon profiles**, for two reasons: no other
VECU DID reuses `$3454` or `$34BC`, so there is no risk of reading a different
signal under this name, and an unsupported DID simply answers NRC `0x31`. The cost
of being wrong is a blank sensor. The cost of omitting them is not knowing.

**Treat a reading here as unconfirmed** until it agrees with the BMS. If `$3454`
reads `1` while `$3479 & 0x04` is `0`, believe `$3479` — it is the BMS's own
account of its own hardware, and it is the one that has been seen working.

### Which to prefer

| Want | Use |
|------|-----|
| Is the pack balancing right now | `$3479 & 0x04` on the BMS — verified |
| A second opinion from the VCU | `$3454` on the VECU — unverified on K2/K3 |
| Whether the VCU is *asking* for balancing | `$34BC` — unverified on K2/K3 |

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
