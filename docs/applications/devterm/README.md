# DevTerm

Serial, network, USB and BLE terminal and device-control tool with a command line, Terminal.Gui, WPF and web front ends. Code: `src/Applications/DevTerm/OoBDev.DevTerm.*`; engine in `OoBDev.DevTerm.Core`, instrument profiles in `src/Extensions/OoBDev.Scpi`.

## Living documents

- [Requirements](requirements.md), [architecture](architecture.md), [API design](api-design.md), [testing strategy](testing-strategy.md): the current truth; change them with the code.
- [Screen specifications](specs/README.md) and the [user guide](user-guide/README.md): every user-facing screen has a spec and a guide entry with real captured screenshots.
- [Bench test reports](test/) from the `hardware-test` skill; the device list and settings are in [test hardware](../../how-tos/test-hardware.md).
- Protocol references: [docs/protocols](../../protocols/); device notes: `src/Extensions/OoBDev.Scpi/docs` and the device libraries.

## Imported material (transition)

The dev-term design documents, devices, bugs and change log were imported whole so their links hold. They are folded into the living documents above feature by feature, following the [documentation map](../../migration/devterm-docs-map.md); each folder is deleted when its map rows are done.

- [design](design/README.md): feature and transport designs written before implementation.
- [`bugs`](bugs/), [`changes`](changes/), [`source`](source/) (snapshots of dev-term's project guide, TODO, BACKLOG and open questions): history and evidence.
- [coding-standards.md](coding-standards.md): compared with the repository rule; the rule wins.
