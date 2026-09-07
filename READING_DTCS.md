# Reading DTCs over OBD

Every diagnosable ECU on this car answers standard UDS `ReadDTCInformation` on
**11-bit CAN at 500 kbps**, from the ordinary OBD-II socket. A plain ELM327 is
enough. There are **1,158 defined fault codes** across the eight ECUs, and the
`DTCMaster` tables in [`data/`](data/) decode them to text.

## The commands

| Purpose | Request | Notes |
|---------|---------|-------|
| Read stored DTCs | `19 02 FF` | status mask `FF` = report every status bit |
| Read stored DTCs (BCM) | `19 02 09` | BCM's database specifies mask `09` |
| Read freeze frame | `19 04 <hi> <mid> <lo> 01` | snapshot captured with the fault |
| Clear DTCs | `14 FF FF FF` | ⚠️ erases all groups |

## Which ECU

| ECU | Request | Response | Session | DTCs defined |
|-----|---------|----------|---------|-------------:|
| BCM — Body Control | `0x701` | `0x709` | `10 03` | 352 |
| IMMO — Immobiliser | `0x704` | `0x70C` | `10 03` | 25 |
| PEPS — Entry / Start | `0x710` | `0x718` | `10 03` | 103 |
| MCU — Motor Control | `0x783` | `0x78B` | `10 03` | 89 |
| DCDC — Converter | `0x784` | `0x78C` | `10 03` | 17 |
| BMS — Battery | `0x785` | `0x78D` | `10 03` | 256 |
| OBC — Charger | `0x786` | `0x78E` | `10 01` | 31 |
| VECU — Vehicle Control | `0x7E3` | `0x7EB` | `10 01` | 285 |

Response id is always request + 8.

## Worked example — reading the BMS

```
ATZ              reset
ATE0             echo off
ATSP6            protocol 6: 11-bit CAN, 500 kbps
ATSH785          request header = BMS
ATCRA78D         only accept its replies
ATFCSH785        flow-control header, for multi-frame replies
1003             enter extended session
1902FF           read DTCs
```

`10 03` first matters — several ECUs reject `19` in the default session. Use
`10 01` for OBC and VECU, which is what their databases specify.

## Reading the reply

```
7F 19 78                     busy, response pending - wait, do not retry
59 02 FF                     no faults stored
59 02 FF 12 01 17 2F ...     one or more faults
```

`59` is the positive reply to `19`, `02` echoes the sub-function, then the status
mask, then **five bytes per fault**: three DTC bytes and a status byte.

Anything past ~7 bytes arrives as multi-frame ISO-TP. Send flow control
(`30 00 00`) or let the adapter handle it — `ATFCSH` plus `ATCAF1` covers it on an
ELM327. A car with several stored faults will always be multi-frame.

## Decoding a code

Take `12 01 17`:

1. **Top two bits of the first byte** give the letter — `0x12` is `00010010`, so
   `00` → **P** (powertrain). `01` → C (chassis), `10` → B (body), `11` → U (network).
2. **The remaining 14 bits** are the number: `0x1201` → `1201`.
3. **The third byte** is the failure type from ISO 14229: `17` = above threshold,
   `16` = below threshold, `1C` = out of range, `13` = circuit open,
   `96` = component internal failure, `73` = hardware fault.

So `12 01 17` is **`P1201-17`** — which the BMS `DTCMaster` table gives as
*"BMS cell voltage high first level alarm - circuit voltage above threshold"*.

The `DTCBytes` column in each `DTCMaster` CSV stores exactly this triple, so you
can look codes up directly rather than reconstructing them.

## The status byte

Bit flags per ISO 14229, and the reason a code being *present* does not mean it is
*active*:

| Bit | Mask | Meaning |
|-----|------|---------|
| 0 | `0x01` | test failed now |
| 1 | `0x02` | test failed this operating cycle |
| 2 | `0x04` | pending |
| 3 | `0x08` | confirmed (stored) |
| 4 | `0x10` | test not completed since last clear |
| 5 | `0x20` | test failed since last clear |
| 6 | `0x40` | test not completed this operating cycle |
| 7 | `0x80` | warning indicator requested |

A status of `0x08` with `0x01` clear is a **historical** fault: stored, not
currently failing. Chasing those wastes time — check `0x01` first.

## Freeze frames

`19 04 <hi> <mid> <lo> 01` returns the snapshot stored when the fault set. For
`P1201-17` that is `19 04 12 01 17 01`. The BMS also advertises this as
`ReportDTCSnapshotRecordByDTCNumber` in its `DTCFormatIdentifiers` table.

Snapshot layout is per-ECU and is described by the `DTCSnapshot_ValueType` and
`DTCSnapshot_ByteType` tables in the VECU database. The other ECUs do not ship
snapshot definitions, so their freeze-frame payloads have to be interpreted by
hand.

## Variant-specific tables

The BMS ships three DTC sets and the right one depends on the pack:

| Table | Rows | Applies to |
|-------|-----:|------------|
| `DTCMaster` | 256 | the general set |
| `DTCMaster_K1AIO` | 72 | K1AIO pack |
| `DTCMaster_K2AIO` | 72 | K2AIO pack |
| `DTCMaster_Limber` | 68 | Limber platform |

The K1AIO and K2AIO tables match the `BMS_K1AIO_DTC.pdf` / `BMS_K2AIO_DTC.pdf`
service documents that ship inside TDS. The VECU similarly carries per-platform
tables (`_K2`, `_K3`, `_Tamor`, `_Nova`, `_Limber`, and others).

For a Nexon EV use **`tblDTCMaster_K2`** (195 codes) or **`tblDTCMaster_K3`** (201).
The 285-code general table above is the union across platforms and includes codes
this car does not implement. Both Nexon tables are unchanged between TDS 20.0 and
21.0; the 33 codes 21.0 added all landed in the general table.

## Before you clear anything

`14 FF FF FF` erases stored faults **and** their freeze frames, across every group.
The snapshot is usually the only record of the conditions when the fault set, so
read it first if you intend to diagnose rather than just reset.

Clearing an active fault hides it until the next drive cycle; it does not fix it.
And on a high-voltage system a stored fault may be the only sign of a real
hazard — the BMS set includes contactor welding, insulation failure and thermal
runaway. Read the description before deciding a code is noise.
