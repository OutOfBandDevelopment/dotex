# Network devices
**Status:** Partly built: the probes and the connection editor code live in `OoBDev.DevTerm.Core` (`BleDeviceScanner`, `ConnectionEditorViewModel`); the move into a Framework library is still a proposal and the bridge config editors are open requests (reconciled with `src` 2026-11-25)

Finding network devices and configuring serial-to-Ethernet bridges. Moved from the DevTerm design folder on 2026-11-04. Application: [DevTerm](../../applications/devterm/README.md).

- [network-device-discovery.md](network-device-discovery.md) - probes (LXI, mDNS, SSDP) and the detect list
- [migration-plan.md](migration-plan.md) - move of the probes into a Framework library (proposed)
- [network-device-config-editors.md](network-device-config-editors.md) - in-app configuration editors for bridges
