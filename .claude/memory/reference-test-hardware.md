---
name: reference-test-hardware
description: Which physical devices are attached to which COM/USB/LXI ports on the owner's machine, for Hardware and DevLocal protocol tests
metadata:
  type: reference
---

Owner's bench (stated 2026-10-30), for protocol and Hardware tests:

- COM6: Korad KA3005P (power supply)
- COM7: Korad KA6003P (power supply; owner corrected the model 2026-10-30)
- COM13: Z-Stick (Z-Wave)
- COM8: Radex One
- COM3: Zoom H4n
- USB: Rigol DS1102E, Siglent/Rigol-class DM3058, DG1022 and DG1062Z (function generators)
- DG1062Z is also reachable over LXI (network)
- K8055 and Busylight are also online (2026-10-30)

Port numbers can change when devices are replugged; confirm before relying on them. Hardware tests end `Assert.Inconclusive` when the device is not detected.

**How to apply:** the owner authorised testing these as needed (2026-10-30; devices are set up for testing). Use them for the Hardware/DevLocal checks in `BACKLOG.md` (Devices) and the WPF detected-device pickers. Ask the owner to watch (step by step) when a physical effect must be observed.

Network instruments (raw TCP port 23, stated 2026-10-30):
- 192.168.0.109:23 HP 34401A multimeter
- 192.168.0.110:23 Tektronix TDS2024 scope
- 192.168.0.108:23 Tektronix 2230 (online)
- 192.168.0.107:23 second Tektronix 2230 (offline at that time)
- [Bench device list](docs/how-tos/test-hardware.md) is the in-repo version of [[reference-test-hardware]].
