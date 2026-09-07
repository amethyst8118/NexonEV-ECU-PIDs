#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
A fake ELM327 + VECU, so vecu_actuator_test.py can be exercised without a car.

The point is not to emulate a Nexon. It is to let you see exactly what the test
script sends and how it behaves when things go wrong - a refused command, a
moving car, a security-gated ECU - before any of that happens with a real
traction battery on the other end of the cable.

    python sim_elm327.py              # run every scenario
    python sim_elm327.py --scenario nrc_security

Scenarios:
    happy          stationary, not charging, ECU accepts everything
    moving         vehicle speed non-zero - interlock must refuse
    charging       charger connected - interlock must refuse
    nrc_security   ECU answers 0x2F with NRC 0x33
    nrc_range      ECU answers 0x2F with NRC 0x31
    frame5         ECU echoes the control option (5-byte reply)
    release_fails  taking control works, handing it back does not
"""

import argparse
import sys
import time

import serial

# Baseline payloads, keyed by DID. All values are "car sitting in a garage".
BASE = {
    "3421": "03E8",   # SOC 100.0 %
    "3462": "0000",   # speed 0
    "3440": "00", "34B0": "00", "34CB": "00", "34CC": "00", "34CF": "00",
    "3441": "00", "349F": "00",
    "3439": "00", "3437": "14", "3452": "00", "3453": "00",
    "344C": "00", "344D": "00", "3436": "00",
    "343A": "64", "3438": "64", "34BB": "00",
    "3423": "52", "3424": "50",          # 32 C / 30 C
    "3472": "55", "3473": "50",          # 45 C / 40 C
    "34E7": "50", "3470": "51", "3471": "51",
    "341F": "01", "3420": "01",
}


class FakeSerial(object):
    """Enough of pyserial's Serial to satisfy the test script."""

    scenario = "happy"

    def __init__(self, port, baudrate=38400, timeout=1.0, **kw):
        self.port, self.baudrate, self.timeout = port, baudrate, timeout
        self._in = bytearray()
        self._out = bytearray()
        self.state = dict(BASE)
        self.held = set()
        self.sent = []
        if self.scenario == "moving":
            self.state["3462"] = "0040"      # 2.0 m/s
        elif self.scenario == "charging":
            self.state["3440"] = "01"
            self.state["34B0"] = "02"

    # -- plumbing ---------------------------------------------------------
    def reset_input_buffer(self):
        self._out = bytearray()

    def flush(self):
        pass

    def close(self):
        pass

    def read(self, n=1):
        if not self._out:
            time.sleep(0.005)
            return b""
        take, self._out = self._out[:n], self._out[n:]
        return bytes(take)

    def write(self, data):
        self._in += data
        while b"\r" in self._in:
            line, _, rest = bytes(self._in).partition(b"\r")
            self._in = bytearray(rest)
            cmd = line.decode("ascii", "ignore").strip().upper()
            if cmd:
                self.sent.append(cmd)
                self._out += (self._reply(cmd) + "\r\r>").encode("ascii")
        return len(data)

    # -- behaviour --------------------------------------------------------
    def _reply(self, cmd):
        if cmd.startswith("AT"):
            if cmd == "ATI":
                return "ELM327 v1.5 (simulated)"
            if cmd == "ATZ":
                return "\r\rELM327 v1.5 (simulated)"
            return "OK"
        h = cmd.replace(" ", "")
        try:
            req = bytes.fromhex(h)
        except ValueError:
            return "?"
        sid = req[0]

        if sid == 0x3E:
            return "7E00"
        if sid == 0x10:
            return "50%02X003201F4" % req[1]
        if sid == 0x22:
            did = h[2:6]
            if did not in self.state:
                return "7F2231"
            return "62" + did + self.state[did]
        if sid == 0x2F:
            return self._io_control(h, req)
        return "7F%02X11" % sid

    def _io_control(self, h, req):
        did = h[2:6]
        option = req[3] if len(req) > 3 else 0x00

        if self.scenario == "nrc_security":
            return "7F2F33"
        if self.scenario == "nrc_range":
            return "7F2F31"

        if option == 0x00:                       # returnControlToECU
            if self.scenario == "release_fails":
                return "7F2F22"
            self.held.discard(did)
            self.state[did] = BASE.get(did, "00")
            self._propagate(did)
            return "6F" + did + "00"

        if option == 0x03:                       # shortTermAdjustment
            state = req[4] if len(req) > 4 else 0
            self.held.add(did)
            self.state[did] = "%02X" % state
            self._propagate(did)
            if self.scenario == "frame5":
                return "6F" + did + "03%02X" % state
            return "6F" + did + "%02X" % state

        return "7F2F12"

    def _propagate(self, did):
        """Let a commanded output move the sensors it should move."""
        v = int(self.state.get(did, "00"), 16)
        if did == "3439":
            self.state["343A"] = "64" if v else "00"
            self.state["3473"] = "%02X" % max(0x40, 0x50 - v // 20)
        elif did == "3437":
            self.state["3438"] = "64" if v else "00"
            self.state["3423"] = "%02X" % max(0x4E, 0x52 - v // 30)
        elif did in ("3452", "3453"):
            self.state["34BB"] = "50" if v else "00"
        elif did == "3436":
            self.state["3441"] = "33" if v else "00"


def install(scenario="happy"):
    FakeSerial.scenario = scenario
    serial.Serial = FakeSerial
    return FakeSerial


SCENARIOS = {
    "happy":         (["--actuator", "fan-slow", "--level", "1", "--arm"], 0),
    "moving":        (["--actuator", "fan-slow", "--level", "1", "--arm"], 3),
    "charging":      (["--actuator", "battery-pump", "--level", "80", "--arm"], 3),
    "nrc_security":  (["--actuator", "fan-fast", "--level", "1", "--arm"], 0),
    "nrc_range":     (["--actuator", "cabin-valve", "--level", "1", "--arm"], 0),
    "frame5":        (["--actuator", "traction-pump", "--level", "60", "--arm"], 0),
    "release_fails": (["--actuator", "fan-slow", "--level", "1", "--arm"], 0),
}


def run(name):
    import importlib
    install(name)
    if "vecu_actuator_test" in sys.modules:
        del sys.modules["vecu_actuator_test"]
    vat = importlib.import_module("vecu_actuator_test")
    vat.serial.Serial = FakeSerial

    args, expect = SCENARIOS[name]
    argv = ["vecu_actuator_test.py", "--port", "SIM", "--hold", "1"] + args
    old = sys.argv
    sys.argv = argv
    print("\n" + "#" * 72)
    print("#  SCENARIO: %s" % name)
    print("#  %s" % " ".join(argv[1:]))
    print("#" * 72)
    try:
        rc = vat.main()
    finally:
        sys.argv = old
    ok = (rc or 0) == expect
    print("\n[scenario %s] exit=%s expected=%s  %s"
          % (name, rc, expect, "PASS" if ok else "FAIL"))
    return ok


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--scenario", choices=sorted(SCENARIOS), help="run just one")
    a = ap.parse_args()
    names = [a.scenario] if a.scenario else list(SCENARIOS)
    results = [(n, run(n)) for n in names]
    print("\n" + "=" * 72)
    for n, ok in results:
        print("  %-14s %s" % (n, "PASS" if ok else "FAIL"))
    print("=" * 72)
    return 0 if all(ok for _, ok in results) else 1


if __name__ == "__main__":
    sys.exit(main())
