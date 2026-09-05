# CarScanner custom PID profiles

Import via **Settings → Custom PIDs → Import** (or Profile → Custom PIDs, depending
on version). Files are plain JSON, so you can edit them in any text editor.

| File | Sensors | Header | Reachable from |
|------|---------|--------|----------------|
| `NexonEV_VECU.csp` | 183 | `7E3` (11-bit) | **standard OBD port, pins 6/14** |
| `NexonEV_VECU_battery.csp` | 23 | `7E3` (11-bit) | standard OBD port — battery-related subset of the above |
| `NexonEV_BMS_KPD.csp` | 39 | `785` (11-bit) | see the warning below |

Start with `NexonEV_VECU.csp`. It is the only one confirmed to answer from an
ordinary ELM327 plugged into the OBD socket.

## Which one do I want?

- **Just want data that works** → `NexonEV_VECU.csp`. Verified live: this address
  self-identifies as `VECU_R15.2` via `22 F197`.
- **Only battery figures, less clutter** → `NexonEV_VECU_battery.csp`. Pack
  temperatures, busbar voltage, cumulative charge/discharge and the HV fault flags.
  No SOC and no per-cell millivolts — the VECU does not expose those.
- **Real BMS data** (SOC, cell min/max mV, pack current) → `NexonEV_BMS_KPD.csp`,
  but read the warning first.

## ⚠️ About the BMS profile

On a Nexon EV the BMS sits on the **Powertrain CAN — OBD pins 3 (CAN-H) and
11 (CAN-L)** — at 29-bit address `0x1BDA96F1`. The gateway does not route
diagnostic requests to it, so a normal adapter wired to pins 6/14 **will not reach
it**. This was checked directly: an 11-bit sweep of `780`–`7E7` and a 29-bit sweep
including `1BDA96F1` were both silent, while `7E3` answered on the same run.

The profile ships with header `785` because that is the address the Tata Punch EV
and the community `tata-ev-bms` app use for the same DID set. If your car answers
there, everything in the file works as-is. If it does not, the DIDs and scalings are
still correct — you need an adapter tapped to pins 3/11, and CarScanner cannot send
29-bit headers, so an ESP32 on the powertrain bus is the practical route.

## Known limitations

- **`$3479` and `$3404` are bit-packed**, and these profiles expose them as a raw
  byte (named `[bits]` / `[code]`) rather than as separate switches. CarScanner's
  bit-extraction fields were not confirmed, so guessing them would have produced
  silently wrong readings. Decode tables are in
  [`Nexon_EV_All_PIDs.md`](../Nexon_EV_All_PIDs.md) — `$3479` carries six flags
  including cell-balancing status.
- **Enumerated PIDs show a number, not text.** `[code]` in a sensor name means look
  the value up in the reference.
- **Identification PIDs are omitted.** They return ASCII strings, which CarScanner
  renders as meaningless numbers.
- **Units live in the sensor name**, e.g. `Batt Cur Voltage (V)`. CarScanner's unit
  field is an internal enum whose mapping is not documented, so it is left at 0.
- **Min/max are display bounds**, chosen to make gauges sensible — not limits read
  from the vehicle.

## How the values are computed

Every sensor uses `value = raw × MUL ÷ DIV + OFS`, with `FR` set to `A` for
one-byte PIDs and `A*256+B` for two-byte ones. `BCM` is `1003`, which enters the
extended diagnostic session before each read — several DIDs return nothing without
it. `ACT` is `false` on every sensor: setting it true marks the entry as an *action*
PID and it will never poll for data.

Pack current is signed via the offset, not the sign flag: `raw × 0.1 − 3200`, so
`SIG` stays `false`. Setting it true double-counts the sign.

Scalings come from the Tata TDS ECU databases and are documented, with confidence
markers and sources, in [`Nexon_EV_All_PIDs.md`](../Nexon_EV_All_PIDs.md).
