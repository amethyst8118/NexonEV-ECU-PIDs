#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Tata Nexon EV - VECU actuator test over a Bluetooth ELM327.

Drives the seven InputOutputControlByIdentifier (UDS 0x2F) outputs the VECU
exposes, and - unlike TDS itself - reads the same DID back with 0x22 so you can
see whether the command actually landed.

    python vecu_actuator_test.py --list-ports
    python vecu_actuator_test.py --port COM5                     # dry run, reads only
    python vecu_actuator_test.py --port COM5 --scan              # full sensor snapshot
    python vecu_actuator_test.py --port COM5 --actuator fan-slow --arm
    python vecu_actuator_test.py --port COM5 --actuator traction-pump --level 60 --arm

Nothing is actuated unless you pass --arm. Without it the script connects,
checks preconditions, reads the baseline and prints the exact frames it would
have sent.

SAFETY - read this once.

  These are real outputs on a live traction-battery system. The pumps and fans
  are the thermal management of a pack that can be sitting at 350 V.

  * Never run with the car moving or on a charger. The script checks and
    refuses, but the check is only as good as the DIDs it reads.
  * Forcing a cooling pump to 0 % while the pack is hot or charging can damage
    it. Prefer raising a pump over lowering one.
  * $3436 is the charging gun lock. Releasing it mid-session unlatches a
    connector carrying hundreds of volts. It is refused unless you pass
    --allow-gun-lock, and it is the one actuator worth not testing at all.
  * Control is released on exit, on Ctrl-C, and on crash. If release ever
    fails the script says so loudly - cycle the ignition, which drops the
    ECU back to its own control.

Definitions come from the TDS 20.0 / 21.0 VECU database; scaling formulas are
the ones already validated in carscanner/nexon-ev/VECU.csp. The 0x2F frame
layout itself is NOT confirmed against a car - see FRAME NOTES below.

FRAME NOTES
  Request   2F <did hi> <did lo> <control option> [state]
  Response  6F <did hi> <did lo> ...

  Control option 0x03 is shortTermAdjustment (take control, apply state) and
  0x00 is returnControlToECU (hand it back). Those are the UDS standard values;
  the Tata database does not record them, so --control-option lets you override.

  The database says these DIDs are 4 bytes total, which fits a reply of
  '6F hi lo <state>' with no echo of the control option. Strict UDS would echo
  it, giving 5. The script prints the raw reply either way and tells you which
  shape came back - running it once settles the question.
"""

import argparse
import atexit
import re
import signal
import sys
import time

MIN_PY = (3, 7)
if sys.version_info < MIN_PY:
    sys.exit("Python 3.7+ required")

try:
    import serial
    from serial.tools import list_ports
except ImportError:
    sys.exit("pyserial is required:  pip install pyserial")

VECU_TX = "7E3"
VECU_RX = "7EB"

# ---------------------------------------------------------------------------
# Sensor table. (formula, label, byte length) keyed by DID.
# Formulas are lifted verbatim from carscanner/nexon-ev/VECU.csp, where they
# have already been checked against a real car.
# ---------------------------------------------------------------------------
SENSORS = {
    # the seven actuator DIDs, all also readable with 0x22
    "3439": ("A", "Traction cooling pump PWM dutycycle (%)", 1),
    "3437": ("A", "Battery cooling pump PWM dutycycle (%)", 1),
    "3452": ("A", "Traction cooling fan FAST enable cmd", 1),
    "3453": ("A", "Traction cooling fan SLOW enable cmd", 1),
    "344C": ("A", "Cabin cooling solenoid valve on cmd", 1),
    "344D": ("A", "Battery cooling solenoid valve on cmd", 1),
    "3436": ("A", "Charging gun lock/unlock cmd [code]", 1),
    # correlated effects
    "343A": ("A", "Traction cooling pump PWM freq (Hz)", 1),
    "3438": ("A", "Battery cooling pump PWM freq (Hz)", 1),
    "34BB": ("A", "Cooling power request from FATC (%)", 1),
    "3441": ("A/51", "Vehicle inlet gun lock feedback", 1),
    "349F": ("A", "Gun unlock fascia switch status", 1),
    # thermal context
    "3423": ("A-50", "Battery cell temperature MAX (C)", 1),
    "3424": ("A-50", "Battery cell temperature MIN (C)", 1),
    "3472": ("A-40", "Motor winding temperature (C)", 1),
    "3473": ("A-40", "Inverter temperature (C)", 1),
    "34E7": ("A-48", "3-in-1 unit DC-DC temperature (C)", 1),
    "3470": ("A-40", "Vehicle inlet temp +ve (C)", 1),
    "3471": ("A-40", "Vehicle inlet temp -ve (C)", 1),
    # preconditions / state
    "3462": ("(A*256+B)/32", "Vehicle speed (m/s)", 2),
    "3421": ("(A*256+B)/10", "HV battery SOC (%)", 2),
    "3440": ("A", "Charger ON sense status", 1),
    "34B0": ("A", "OBC charging status", 1),
    "34CB": ("A", "CCS fast charging gun status", 1),
    "34CC": ("A", "Fast charging diagnostic state", 1),
    "34CF": ("A", "Slow charging sequence on vehicle [code]", 1),
    "341F": ("A", "HV battery negative contactor state", 1),
    "3420": ("A", "HV battery positive contactor state", 1),
}

# DIDs that must all read zero before anything is actuated.
INTERLOCKS = ["3462", "3440", "34B0", "34CB", "34CC"]

# Extra interlocks that only apply to the gun lock.
GUN_INTERLOCKS = ["34CF", "3441"]

CONTEXT = ["3421", "3423", "3424", "3472", "3473", "34E7", "341F", "3420"]


class Actuator(object):
    def __init__(self, key, did, label, kind, watch, note="", gated=False):
        self.key, self.did, self.label = key, did, label
        self.kind = kind            # "percent" or "switch"
        self.watch = watch
        self.note = note
        self.gated = gated


ACTUATORS = [
    Actuator("traction-pump", "3439", "Traction Cooling Pump", "percent",
             ["3439", "343A", "3472", "3473"],
             "0-100 %. Cools motor and inverter."),
    Actuator("battery-pump", "3437", "Battery Cooling Pump", "percent",
             ["3437", "3438", "3423", "3424"],
             "0-100 %. Cools the traction pack - do not force to 0 when warm."),
    Actuator("fan-fast", "3452", "Traction Cooling Fan Fast", "switch",
             ["3452", "34BB", "3473"],
             "Audible. Database mask 0x01."),
    Actuator("fan-slow", "3453", "Traction Cooling Fan Slow", "switch",
             ["3453", "34BB", "3473"],
             "Audible. Database mask 0x02."),
    Actuator("cabin-valve", "344C", "Cabin Cooling Solenoid Valve", "switch",
             ["344C", "34BB"],
             "0 = Closed, 1 = Open."),
    Actuator("battery-valve", "344D", "Battery Cooling Solenoid Valve", "switch",
             ["344D", "3423", "3424"],
             "0 = Closed, 1 = Open (database rows for this DID are miscoded - "
             "both list ResultByte 1; taken by analogy with the cabin valve)."),
    Actuator("gun-lock", "3436", "Charging Gun Lock/Unlock", "switch",
             ["3436", "3441", "349F"],
             "HIGH VOLTAGE. No decode rows exist for this DID in the database. "
             "Requires --allow-gun-lock.", gated=True),
]
BY_KEY = dict((a.key, a) for a in ACTUATORS)


# ---------------------------------------------------------------------------
# formula evaluation
# ---------------------------------------------------------------------------
_SAFE = re.compile(r"^[AB0-9+\-*/(). ]+$")


def apply_formula(fr, data):
    """Evaluate a CarScanner FR string over the payload bytes."""
    if not _SAFE.match(fr):
        raise ValueError("unsafe formula: %r" % fr)
    env = {"__builtins__": {}}
    env["A"] = data[0] if len(data) > 0 else 0
    env["B"] = data[1] if len(data) > 1 else 0
    return eval(fr, env)  # noqa: S307 - input is our own table, regex-gated


def fmt(did, data):
    if did not in SENSORS:
        return "raw " + data.hex()
    fr, label, _ = SENSORS[did]
    try:
        v = apply_formula(fr, data)
    except Exception as exc:
        return "raw %s (formula failed: %s)" % (data.hex(), exc)
    if isinstance(v, float) and v != int(v):
        return "%.2f" % v
    return str(int(v))


# ---------------------------------------------------------------------------
# ELM327
# ---------------------------------------------------------------------------
NRC = {
    0x11: "service not supported",
    0x12: "sub-function not supported",
    0x13: "incorrect message length",
    0x22: "conditions not correct",
    0x31: "request out of range - DID not supported here",
    0x33: "security access denied (needs 0x27 first)",
    0x78: "response pending",
    0x7E: "sub-function not supported in active session",
    0x7F: "service not supported in active session",
}

JUNK = ("SEARCHING", "BUS INIT", "NO DATA", "CAN ERROR", "UNABLE TO CONNECT",
        "STOPPED", "BUFFER FULL", "?", "ERROR", "FB ERROR", "DATA ERROR")


class Elm327(object):
    def __init__(self, port, baud, timeout, verbose=False, log=None):
        self.verbose = verbose
        self.log = log
        self.ser = serial.Serial(port, baudrate=baud, timeout=timeout)
        time.sleep(0.4)
        self.ser.reset_input_buffer()

    def close(self):
        try:
            self.ser.close()
        except Exception:
            pass

    def _write(self, cmd):
        self.ser.reset_input_buffer()
        self.ser.write((cmd + "\r").encode("ascii"))
        self.ser.flush()

    def raw(self, cmd, wait=5.0):
        """Send a command, return the lines before the '>' prompt."""
        self._write(cmd)
        buf, deadline = b"", time.time() + wait
        while time.time() < deadline:
            chunk = self.ser.read(256)
            if chunk:
                buf += chunk
                if b">" in buf:
                    break
            else:
                time.sleep(0.01)
        text = buf.decode("ascii", "ignore").replace("\r", "\n")
        lines = [l.strip() for l in text.split("\n")]
        lines = [l for l in lines if l and l != ">" and l != cmd]
        if self.verbose:
            print("    >> %-14s  << %s" % (cmd, " | ".join(lines) or "(nothing)"))
        if self.log:
            self.log.write(">> %s\n<< %s\n" % (cmd, " | ".join(lines)))
            self.log.flush()
        return lines

    def at(self, cmd, expect_ok=True):
        lines = self.raw(cmd)
        ok = any("OK" in l or "ELM" in l or "v" in l.lower() for l in lines)
        if expect_ok and not ok:
            print("    ! %s did not acknowledge: %s" % (cmd, lines))
        return lines

    # -- UDS -------------------------------------------------------------
    def uds(self, payload_hex, wait=5.0):
        """Send a UDS request, return (bytes, error_string)."""
        lines = self.raw(payload_hex.replace(" ", ""), wait=wait)
        if not lines:
            return None, "no reply"
        for l in lines:
            up = l.upper()
            for j in JUNK:
                if up.startswith(j):
                    return None, l
        blob = ""
        for l in lines:
            l = re.sub(r"^[0-9A-Fa-f]{3}:", "", l)      # ISO-TP line index
            l = re.sub(r"^[0-9A-Fa-f]{3}\s", "", l)      # leftover header
            blob += re.sub(r"[^0-9A-Fa-f]", "", l)
        if len(blob) < 2:
            return None, "unparsable: %s" % lines
        if len(blob) % 2:
            blob = blob[:-1]
        try:
            data = bytes.fromhex(blob)
        except ValueError:
            return None, "bad hex: %s" % blob
        if len(data) >= 3 and data[0] == 0x7F:
            code = data[2]
            return None, "NRC 0x%02X - %s" % (code, NRC.get(code, "unknown"))
        return data, None

    def read_did(self, did):
        data, err = self.uds("22" + did)
        if err:
            return None, err
        want = bytes.fromhex("62" + did)
        idx = data.find(want)
        if idx < 0:
            return None, "no 62%s echo in %s" % (did, data.hex())
        return data[idx + 3:], None

    def tester_present(self):
        self.uds("3E00", wait=1.5)


# ---------------------------------------------------------------------------
# session state
# ---------------------------------------------------------------------------
class Session(object):
    def __init__(self, elm, control_option):
        self.elm = elm
        self.control_option = control_option
        self.held = {}          # did -> label
        self.release_failed = []

    def take(self, did, state_byte):
        req = "2F%s%02X%02X" % (did, self.control_option, state_byte)
        data, err = self.elm.uds(req)
        if err is None:
            self.held[did] = req
        return req, data, err

    def release(self, did):
        req = "2F%s00" % did
        data, err = self.elm.uds(req)
        self.held.pop(did, None)
        if err:
            self.release_failed.append((did, err))
        return data, err

    def release_all(self):
        for did in list(self.held):
            print("    releasing $%s ..." % did, end=" ")
            _, err = self.release(did)
            print("failed: %s" % err if err else "ok")
        if self.release_failed:
            print("\n" + "!" * 72)
            print("! CONTROL MAY STILL BE HELD ON: %s"
                  % ", ".join("$" + d for d, _ in self.release_failed))
            print("! Cycle the ignition. That drops the ECU back to its own control.")
            print("!" * 72)


# ---------------------------------------------------------------------------
# helpers
# ---------------------------------------------------------------------------
def list_serial_ports():
    ports = list(list_ports.comports())
    if not ports:
        print("No serial ports found.")
        print("Pair the ELM327 in Windows Bluetooth settings first; it then")
        print("appears as an outgoing COM port. Note that BLE-only adapters")
        print("(ELM327 v2.1 'BLE') never get a COM port and will not work here.")
        return
    print("%-8s %-38s %s" % ("PORT", "DESCRIPTION", "HWID"))
    for p in ports:
        print("%-8s %-38s %s" % (p.device, (p.description or "")[:38], p.hwid or ""))
    print("\nThe Bluetooth ELM327 is usually the 'Standard Serial over Bluetooth'")
    print("entry. If two appear, the lower-numbered one is normally outgoing.")


def autodetect():
    best = None
    for p in list_ports.comports():
        d = ((p.description or "") + " " + (p.hwid or "")).lower()
        if any(k in d for k in ("obd", "elm", "vgate", "obdii")):
            return p.device
        if "bluetooth" in d and best is None:
            best = p.device
    return best


def init_elm(elm, args):
    print("[*] Initialising adapter")
    elm.raw("ATZ", wait=6.0)
    time.sleep(0.5)
    for cmd in ("ATE0", "ATL0", "ATS0", "ATH0"):
        elm.at(cmd)
    ident = elm.raw("ATI")
    print("    adapter: %s" % (" ".join(ident) or "unknown"))
    elm.at("ATSP6")                        # 11-bit CAN, 500 kbps
    elm.at("ATSH" + VECU_TX)
    elm.at("ATCRA" + VECU_RX)
    elm.at("ATFCSH" + VECU_TX)
    elm.at("ATSTFF")                       # generous per-request timeout
    elm.at("ATAT" + str(args.adaptive))


def enter_session(elm, preferred):
    """Try the requested session, then the other one. Returns the code used."""
    order = [preferred] + [s for s in ("03", "01") if s != preferred]
    for s in order:
        data, err = elm.uds("10" + s)
        if err is None:
            print("    session 10 %s accepted" % s)
            return s
        print("    session 10 %s rejected (%s)" % (s, err))
    return None


def snapshot(elm, dids, indent="      "):
    out = {}
    for did in dids:
        data, err = elm.read_did(did)
        label = SENSORS.get(did, ("", "unknown", 0))[1]
        if err:
            print("%s$%-5s %-42s  -- %s" % (indent, did, label, err))
            out[did] = None
        else:
            val = fmt(did, data)
            print("%s$%-5s %-42s  =  %-10s [%s]"
                  % (indent, did, label, val, data.hex()))
            out[did] = (val, data)
    return out


def check_preconditions(elm, actuator, force):
    print("\n[*] Preconditions")
    readings = snapshot(elm, INTERLOCKS + (GUN_INTERLOCKS if actuator and
                                           actuator.gated else []))
    problems, unknown = [], []
    for did in INTERLOCKS + (GUN_INTERLOCKS if actuator and actuator.gated else []):
        r = readings.get(did)
        label = SENSORS[did][1]
        if r is None:
            unknown.append(label)
            continue
        val, _ = r
        try:
            if abs(float(val)) > 0.001:
                problems.append("%s = %s (expected 0)" % (label, val))
        except ValueError:
            pass
    if unknown:
        print("\n    ! Could not read: %s" % ", ".join(unknown))
        print("    ! An interlock that cannot be read is not an interlock.")
    if problems:
        print("\n    ! INTERLOCK FAILED:")
        for p in problems:
            print("    !   %s" % p)
        if not force:
            return False
        print("    ! --force given, continuing anyway")
    elif not unknown:
        print("\n    all interlocks clear: stationary, not charging")
    return True


# ---------------------------------------------------------------------------
# the test itself
# ---------------------------------------------------------------------------
def run_actuator(elm, sess, act, state, hold, armed):
    print("\n" + "=" * 72)
    print("  %s   $%s" % (act.label.upper(), act.did))
    print("  %s" % act.note)
    print("=" * 72)

    if state is None:                       # dry run with no --level given
        state = 50 if act.kind == "percent" else 1
        print("  (no --level given; previewing with %d)" % state)

    print("\n[*] Baseline")
    before = snapshot(elm, act.watch)

    req = "2F%s%02X%02X" % (act.did, sess.control_option, state)
    rel = "2F%s00" % act.did
    print("\n[*] Command")
    print("      take control : %s" % " ".join(req[i:i + 2] for i in range(0, len(req), 2)))
    print("      release      : %s" % " ".join(rel[i:i + 2] for i in range(0, len(rel), 2)))

    if not armed:
        print("\n    DRY RUN - nothing sent. Re-run with --arm to actuate.")
        return

    print("\n[*] Taking control")
    _, data, err = sess.take(act.did, state)
    if err:
        print("    ! rejected: %s" % err)
        if "0x33" in err:
            print("    ! This ECU wants SecurityAccess (0x27) first. The seed/key")
            print("    ! algorithm is not in the Tata databases, so this is a dead end.")
        if "0x31" in err:
            print("    ! The VECU does not accept 0x2F on this DID. Check the platform:")
            print("    ! these seven are flagged BaseVariant/MidVariant/NanoVariant.")
        return
    print("    accepted: %s" % data.hex())
    # resolve the frame-shape question while we are here
    body = data[3:] if len(data) >= 3 else b""
    if len(data) == 4:
        print("    reply is 4 bytes -> '6F hi lo <state>', state = 0x%02X" % data[3])
        print("    (matches the database's ByteLength 4; no control-option echo)")
    elif len(data) >= 5:
        print("    reply is %d bytes -> '6F hi lo <option> <state>', "
              "option = 0x%02X state = 0x%02X" % (len(data), data[3], data[4]))
        print("    (strict UDS shape, not what the database's ByteLength 4 implied)")
    elif body:
        print("    unexpected reply shape, body = %s" % body.hex())

    try:
        print("\n[*] Holding %d s - reading back and keeping tester-present alive" % hold)
        end = time.time() + hold
        tick = 0
        while time.time() < end:
            elm.tester_present()
            tick += 1
            print("\n    t+%ds" % tick)
            snapshot(elm, act.watch, indent="        ")
            spd, _ = elm.read_did("3462")
            if spd is not None:
                try:
                    if float(fmt("3462", spd)) > 0.05:
                        print("\n    ! VEHICLE IS MOVING - aborting")
                        break
                except ValueError:
                    pass
            time.sleep(0.4)
    finally:
        print("\n[*] Releasing control")
        _, err = sess.release(act.did)
        print("    %s" % ("failed: %s" % err if err else "returned to ECU"))

    print("\n[*] After release")
    after = snapshot(elm, act.watch)

    print("\n[*] Result")
    changed = False
    for did in act.watch:
        b, a = before.get(did), after.get(did)
        if b and a:
            mark = "same" if b[0] == a[0] else "CHANGED %s -> %s" % (b[0], a[0])
            if b[0] != a[0]:
                changed = True
            print("      $%-5s %-42s %s" % (did, SENSORS[did][1], mark))
    if not changed:
        print("\n      Nothing moved between baseline and post-release, which is")
        print("      what you want - the ECU took its output back. Whether the")
        print("      actuator responded during the hold is in the t+N readings.")


def main():
    ap = argparse.ArgumentParser(
        description="Test the Nexon EV VECU's 0x2F actuators over a Bluetooth ELM327.",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="Actuators: " + ", ".join(a.key for a in ACTUATORS))
    ap.add_argument("--list-ports", action="store_true", help="list serial ports and exit")
    ap.add_argument("--port", help="COM port of the paired ELM327, or 'auto'")
    ap.add_argument("--baud", type=int, default=38400, help="default 38400")
    ap.add_argument("--timeout", type=float, default=1.0, help="serial read timeout")
    ap.add_argument("--actuator", help="which actuator, or 'all'")
    ap.add_argument("--level", type=int, help="percent 0-100 for pumps; 0/1 for switches")
    ap.add_argument("--hold", type=int, default=5, help="seconds to hold control (default 5)")
    ap.add_argument("--session", default="03", choices=["01", "03"],
                    help="diagnostic session to try first (default 03)")
    ap.add_argument("--control-option", type=lambda s: int(s, 16), default=0x03,
                    help="0x2F control option byte, hex (default 03 shortTermAdjustment)")
    ap.add_argument("--adaptive", type=int, default=1, choices=[0, 1, 2],
                    help="ATAT adaptive timing mode (default 1)")
    ap.add_argument("--arm", action="store_true", help="actually send 0x2F (default is dry run)")
    ap.add_argument("--allow-gun-lock", action="store_true",
                    help="permit $3436, the HV charging gun lock")
    ap.add_argument("--force", action="store_true", help="proceed despite failed interlocks")
    ap.add_argument("--scan", action="store_true", help="read every known sensor and exit")
    ap.add_argument("--verbose", action="store_true", help="show every AT exchange")
    ap.add_argument("--log", help="write the full exchange to this file")
    args = ap.parse_args()

    if args.list_ports:
        list_serial_ports()
        return 0
    if not args.port:
        ap.error("--port is required (try --list-ports)")

    port = autodetect() if args.port == "auto" else args.port
    if not port:
        print("Could not autodetect an adapter. Try --list-ports.")
        return 1

    targets = []
    if args.actuator:
        if args.actuator == "all":
            targets = [a for a in ACTUATORS if not a.gated]
            if args.allow_gun_lock:
                targets = list(ACTUATORS)
        elif args.actuator in BY_KEY:
            targets = [BY_KEY[args.actuator]]
        else:
            ap.error("unknown actuator %r; choose from %s"
                     % (args.actuator, ", ".join(BY_KEY)))

    for a in targets:
        if a.gated and not args.allow_gun_lock:
            print("Refusing %s ($%s) without --allow-gun-lock." % (a.key, a.did))
            print("This unlatches a connector that may be carrying HV. If the car")
            print("is on a charger, do not pass that flag.")
            return 2

    state = args.level
    if targets and args.arm:
        if len(targets) > 1:
            ap.error("--actuator all cannot be combined with --arm. One --level "
                     "cannot be right for both a 0-100 %% pump and a 0/1 valve, "
                     "and these carry different risks. Actuate one at a time.")
        if state is None:
            ap.error("--level is required with --arm "
                     "(0-100 for pumps, 0 or 1 for switches)")
        for a in targets:
            if a.kind == "percent" and not 0 <= state <= 100:
                ap.error("%s takes 0-100" % a.key)
            if a.kind == "switch" and state not in (0, 1):
                ap.error("%s takes 0 or 1" % a.key)

    logf = open(args.log, "w", encoding="utf-8") if args.log else None
    print("[*] Opening %s at %d baud" % (port, args.baud))
    try:
        elm = Elm327(port, args.baud, args.timeout, args.verbose, logf)
    except serial.SerialException as exc:
        print("    ! could not open %s: %s" % (port, exc))
        print("    ! Is the adapter paired and powered? Is another app holding it?")
        return 1

    sess = Session(elm, args.control_option)

    def panic(*_):
        print("\n\n[!] Interrupted - releasing control before exit")
        sess.release_all()
        elm.close()
        if logf:
            logf.close()
        sys.exit(130)

    signal.signal(signal.SIGINT, panic)
    atexit.register(sess.release_all)

    rc = 0
    try:
        init_elm(elm, args)

        print("\n[*] Contacting VECU at %s (replies on %s)" % (VECU_TX, VECU_RX))
        data, err = elm.read_did("3421")
        if err:
            print("    ! no answer: %s" % err)
            print("    ! Ignition on? Correct protocol? Try --verbose.")
            return 1
        print("    VECU alive - HV battery SOC = %s %%" % fmt("3421", data))

        used = enter_session(elm, args.session)
        if used is None:
            print("    ! no diagnostic session accepted; continuing in default")

        if args.scan or not targets:
            print("\n[*] Actuator DIDs as currently reported by the ECU")
            snapshot(elm, [a.did for a in ACTUATORS])
            print("\n[*] Context")
            snapshot(elm, CONTEXT)

        if args.scan:
            return 0

        if not targets:
            print("\n[*] No --actuator given. Nothing to do.")
            print("    Available: %s" % ", ".join(a.key for a in ACTUATORS))
            return 0

        if not check_preconditions(elm, targets[0], args.force):
            print("\n    Refusing to actuate. Use --force only if you are certain.")
            return 3

        for a in targets:
            run_actuator(elm, sess, a, state, args.hold, args.arm)

    finally:
        sess.release_all()
        atexit.unregister(sess.release_all)
        elm.close()
        if logf:
            logf.close()
        print("\n[*] Port closed.")
    return rc


if __name__ == "__main__":
    sys.exit(main())
