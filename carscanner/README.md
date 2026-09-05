# CarScanner custom PID profiles

Import via **Settings → Custom PIDs → Import** (or Profile → Custom PIDs, depending
on version). Files are plain JSON, so you can edit them in any text editor.

| File | Sensors | Header | Response |
|------|---------|--------|----------|
| `NexonEV_BMS_KPD.csp` | 40 | **`785`** (11-bit) — plus one `7E3` sensor | `78D` / `7EB` |
| `NexonEV_VECU.csp` | 183 | `7E3` (11-bit) | `7EB` |
| `NexonEV_VECU_battery.csp` | 23 | `7E3` (11-bit) | `7EB` |

All three use **11-bit (standard) addressing** — `HDR` is a three-digit ID with
`FHID` false. CarScanner derives the response header automatically (request + 8).

## Which one do I want?

- **Battery data** — SOC, cell min/max millivolts, pack current, temperatures →
  `NexonEV_BMS_KPD.csp` on header **`785`**.
- **Everything else** — motor, charging, thermal, vehicle state →
  `NexonEV_VECU.csp` on `7E3`. Verified live: this address self-identifies as
  `VECU_R15.2` via `22 F197`.
- **Battery figures from the VECU instead** → `NexonEV_VECU_battery.csp`. Pack
  temperatures, busbar voltage, cumulative charge/discharge and HV fault flags. No
  SOC and no per-cell millivolts — the VECU does not expose those.

## If the BMS profile returns nothing

`785` is the right header for this DID set — it is what the Tata Punch EV answers on
and what the community `tata-ev-bms` app uses on a Nexon EV Max (via `ATSP6`, i.e.
11-bit). If your car does not respond there, the DIDs and scalings in the file are
still correct; the problem is the bus your adapter is on.

The KPD BMS database also defines a **29-bit** identity for the same ECU —
`0x1BDA96F1` request / `0x1BDAF196` response — on the **Powertrain CAN, OBD pins
3 (CAN-H) and 11 (CAN-L)**. On the car this was tested against, a sweep from pins
6/14 found `785` silent while `7E3` answered on the same run, which means that
gateway was not forwarding diagnostics to the BMS.

So, in order:

1. Try `785` as shipped. On many cars this is all you need.
2. If silent, the BMS is on pins 3/11. CarScanner cannot send 29-bit headers, so an
   ESP32 tapped to pins 3/11 is the practical route — see
   [`Nexon_EV_All_PIDs.md`](../Nexon_EV_All_PIDs.md) for the 29-bit addressing.

Both transports carry the same `34xx` DID block, so nothing else in the file changes.

## Reading `Cell Balance Status [bits]`

`$3479` packs six flags into one byte, and the profile shows it as a **decimal
number**, not as separate switches. Decode it by ANDing the masks:

| Mask | Bit | Signal | Set means |
|------|-----|--------|-----------|
| `0x01` | 0 | BMS_DerateFlag | power derating active |
| `0x02` | 1 | **BMS_CellBalanceStatus** | **balancing running now** |
| `0x04` | 2 | HSC_BCM_LEAKAGE_ENA | leakage detection enabled |
| `0x08` | 3 | VCU_Charging_Flag | charging |
| `0x10` | 4 | VCU_HVILDetect | HVIL detection enabled |
| `0x20` | 5 | **VCU_EqualizationTrigger** | VCU requested balancing |

Common values, in decimal as CarScanner shows them:

| Shows | Meaning |
|-------|---------|
| `0` | parked, idle |
| `8` | charging, not balancing |
| `40` | charging, VCU asked — BMS declined, conditions not met |
| `42` | charging **and balancing** |
| `16` | driving |
| `17` | driving, power derating active |

**On the range.** The six documented masks OR together to `0x3F`, so any combination
of *known* flags lands between 0 and 63. The DID is a full byte, though, and bits 6
and 7 (`0x40`, `0x80`) are **not defined in any Tata database** — the ECU may still
use them. The sensor is therefore left at 0–255 rather than clamped to 63, so that
an undocumented bit shows up as a value above 63 instead of being silently hidden.
If you ever see one, that is a real finding worth reporting.

### The VECU side of the same signal

`VECU Equalization Cmd [code]` is `$34BC` on the VECU, header **`7E3`** — the only
sensor in this file that is not on `785`. It is the *request* half of the pair:
`$3479` bit 5 is the BMS confirming it received a trigger, and `$34BC` is the VECU
issuing one.

It is included here deliberately, because the VECU is reachable from OBD pins 6/14
even where the BMS is not. If the rest of this profile is blank but this sensor
reads, you can still see when the car asks for balancing — just not whether the
battery acted on it.

Reading the two together:

| `$34BC` | `$3479` bit 5 | Meaning |
|---------|---------------|---------|
| active | set | VECU asked, BMS heard it |
| active | clear | request not reaching the BMS |
| idle | set | trigger latched from an earlier request |

No database defines the value set for `$34BC`, so it is shown as a raw code rather
than a decoded on/off. Watch it across a charge to learn its states.

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
