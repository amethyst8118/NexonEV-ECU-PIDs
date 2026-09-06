# CarScanner custom PID profiles

Import via **Settings → Custom PIDs → Import** (or Profile → Custom PIDs, depending
on version). Files are plain JSON, so you can edit them in any text editor.

| File | Sensors | Header | Response | ECU |
|------|---------|--------|----------|-----|
| `NexonEV_BMS.csp` | 65 | **`785`** | `78D` | Battery Management |
| `NexonEV_VECU.csp` | 462 | `7E3` | `7EB` | Vehicle Control |
| `NexonEV_MCU.csp` | 15 | `783` | `78B` | Motor Control |
| `NexonEV_DCDC.csp` | 20 | `784` | `78C` | DC-DC Converter |
| `NexonEV_OBC.csp` | 47 | `786` | `78E` | On Board Charger |
| `NexonEV_VECU_battery.csp` | 23 | `7E3` | `7EB` | VECU, battery subset |

All are generated from the **TDS 20.0** databases (`BMS_DB.sdf`,
`DB_VECU_DiagnosticsDB.mdb`, `MCU_DiagnosticsDB.sdf`, `DCDC_EV.sdf`, `OBC_EV.sdf`),
which is what the current factory tool ships. Earlier profiles were built from the
older TDS 8.9S files and had fewer signals.

> `NexonEV_VECU.csp` carries 462 sensors. CarScanner polls every visible sensor in
> a round-robin, so adding all of them at once makes each one update slowly — put
> the handful you care about on a dashboard page and leave the rest hidden.

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

`$3479` packs several flags into one byte, and the profile shows it as a **decimal
number**, not as separate switches. Decode it by ANDing the masks.

> ⚠️ **Two database generations define this DID differently, and the balancing
> bit moves between them.** Since this profile talks to `785`, the TDS 20.0 table
> is the one that applies.

**TDS 20.0 (`0x785`) — what this profile uses:**

| Mask | Bit | Signal |
|------|-----|--------|
| `0x01` | 0 | VCU HVIL Detect Signal |
| `0x02` | 1 | VCU Insulation Control Command |
| `0x04` | 2 | **BMS Cell Balance Status — balancing running now** |

Values you would see in decimal:

| Shows | Meaning |
|-------|---------|
| `0` | idle |
| `1` | HVIL detection on |
| `4` | **balancing** |
| `5` | HVIL + balancing |
| `7` | HVIL + insulation control + balancing |

**TDS 8.9S (29-bit `0x1BDA96F1`) — the older pack**, where balancing is `0x02`
and the byte also carries derate, charging and the VCU equalization trigger at
`0x20`. Full table in [`../Nexon_EV_All_PIDs.md`](../Nexon_EV_All_PIDs.md).

Reading the wrong table does not fail visibly — on TDS 20.0, `0x02` is
*insulation control*, not balancing. A quick check: if `22 340E` returns data,
you are on the TDS 20.0 database.

On TDS 20.0 the older packed bits also got their own DIDs — `$340E` operating
mode and `$340F` derate flag — which are easier to read than bit-masking.

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
