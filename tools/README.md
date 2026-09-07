# tools

| File | What it does |
|------|--------------|
| [`vecu_actuator_test.py`](vecu_actuator_test.py) | Drives the VECU's seven `0x2F` actuators over a Bluetooth ELM327, and reads each one back |
| [`sim_elm327.py`](sim_elm327.py) | A fake ELM327 + VECU, so you can rehearse the above without a car |

Only dependency is `pyserial`:

```bash
pip install pyserial
```

## The actuators

The VECU is the one EV ECU whose `Test Actuators` function is enabled in TDS.
It exposes seven outputs through UDS `InputOutputControlByIdentifier` (`0x2F`) —
see [`../ROUTINE_CONTROL.md`](../ROUTINE_CONTROL.md) for where they come from.

| `--actuator` | DID | Output | Range |
|--------------|-----|--------|-------|
| `traction-pump` | `$3439` | Traction Cooling Pump | 0–100 % |
| `battery-pump` | `$3437` | Battery Cooling Pump | 0–100 % |
| `fan-fast` | `$3452` | Traction Cooling Fan Fast | 0 / 1 |
| `fan-slow` | `$3453` | Traction Cooling Fan Slow | 0 / 1 |
| `cabin-valve` | `$344C` | Cabin Cooling Solenoid Valve | 0 closed / 1 open |
| `battery-valve` | `$344D` | Battery Cooling Solenoid Valve | 0 closed / 1 open |
| `gun-lock` | `$3436` | Charging Gun Lock/Unlock | gated, see below |

### It does not have to be blind

The Tata database sets `IOLI_Status = N` on all seven, which is why TDS commands
these without showing you a result. That flag only means *there is no status
sub-function on `0x2F`* — **every one of the seven DIDs is separately readable
with `0x22`**, under the same DID number.

So the script reads the DID back while it holds control, alongside the sensors
that should move with it:

| Actuator | Read back | Should also move |
|----------|-----------|------------------|
| `traction-pump` | `$3439` | `$343A` pump PWM freq, `$3473` inverter temp |
| `battery-pump` | `$3437` | `$3438` pump PWM freq, `$3423`/`$3424` cell temps |
| `fan-fast` / `fan-slow` | `$3452` / `$3453` | `$34BB` cooling power request |
| `cabin-valve` | `$344C` | `$34BB` |
| `battery-valve` | `$344D` | `$3423`/`$3424` |
| `gun-lock` | `$3436` | `$3441` gun lock feedback, `$349F` fascia switch |

Scaling comes from [`../carscanner/nexon-ev/VECU.csp`](../carscanner/nexon-ev/VECU.csp),
so the numbers match what CarScanner shows.

## Connecting

Pair the ELM327 in Windows Bluetooth settings first. It then appears as an
outgoing COM port:

```bash
python vecu_actuator_test.py --list-ports
```

**BLE-only adapters will not work.** An "ELM327 v2.1 BLE" never gets a COM port,
and this script speaks serial. A classic Bluetooth SPP adapter is what you want.

## Using it

Nothing is actuated without `--arm`. Start by just looking:

```bash
python vecu_actuator_test.py --port COM5 --scan
```

That reads all seven actuator DIDs plus SOC, cell temperatures, contactor states
and charging status, and exits. It is a safe thing to run any time the ignition
is on.

Preview what a command would look like, still sending nothing:

```bash
python vecu_actuator_test.py --port COM5 --actuator fan-slow
```

Actually do it:

```bash
python vecu_actuator_test.py --port COM5 --actuator fan-slow --level 1 --arm
```

The script then reads a baseline, sends `2F 34 53 03 01`, holds for `--hold`
seconds while polling the readback DIDs and keeping `3E 00` tester-present
alive, sends `2F 34 53 00` to hand control back, and re-reads to confirm the ECU
took its output back.

`--actuator all` works for previewing but is refused with `--arm` — one
`--level` cannot be right for both a 0–100 % pump and a 0/1 valve, and they
carry different risks. Actuate one at a time.

## Safety

These are real outputs on a live traction-battery system.

- **The script refuses to actuate unless the car is stationary and off charge.**
  It checks `$3462` vehicle speed, `$3440` charger-on sense, `$34B0` OBC
  charging status, `$34CB` CCS gun status and `$34CC` fast-charge state, and it
  re-checks speed on every poll during the hold. `--force` overrides this; there
  is very little reason to use it.
- **Prefer raising a pump over lowering one.** Forcing the battery cooling pump
  to 0 % on a warm or charging pack is a real way to damage it.
- **`$3436` is the charging gun lock** and needs `--allow-gun-lock`. Releasing it
  mid-session unlatches a connector carrying hundreds of volts. The database has
  no decode rows for this DID at all, so its value encoding is a guess. This is
  the one worth simply not testing.
- **Control is always released** — on normal exit, on Ctrl-C, on exception, via
  `atexit` as a backstop. If release ever fails the script says so loudly.
  Cycling the ignition drops the ECU back to its own control regardless.

## Two things a first run will settle

Both are recorded as open questions in `ROUTINE_CONTROL.md`, and running this
answers them:

1. **The reply shape.** The database says these DIDs are 4 bytes, which fits
   `6F hi lo <state>`. Strict UDS would echo the control option too, giving 5.
   The script prints which one came back.
2. **Whether `0x2F` needs SecurityAccess.** Nothing in the databases says. If
   the answer is yes you get `NRC 0x33`, and that is the end of the road — the
   `0x27` seed/key algorithm is not in these files.

If the ECU rejects the standard `0x03` shortTermAdjustment control option,
`--control-option` lets you try another.

## Rehearsing without a car

`sim_elm327.py` stands in a fake adapter and a fake VECU so you can see the
whole flow, including the failure paths, before touching the vehicle:

```bash
python sim_elm327.py
```

It runs seven scenarios — normal operation, a moving car, a charging car, an
ECU that demands security access, an ECU that rejects the DID, the 5-byte reply
shape, and a release that fails — and checks the script behaves correctly in
each. Useful after editing either file.

## Caveats

The `0x2F` frame layout has **not** been confirmed against a real car. The DIDs,
their names, ranges and platform flags come from the TDS 20.0 / 21.0 VECU
database; the control-option bytes are the UDS standard values, which the Tata
database does not record.

The `$344D` battery cooling valve rows in `tblIO_ByteType` are miscoded — both
list `ResultByte 1`, one labelled Closed and one Open. `0 = Closed, 1 = Open` is
taken by analogy with the cabin valve, which is coded correctly.
