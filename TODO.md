# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-29 · **Branch:** `dev/example-app-fixes`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] **DevTerm restructure**: step 0 (telemetry, user accessor) is done ([change](docs/changes/telemetry-user-accessor-2026-10-29.md)); step 1 (pipe-based `IDeviceAdapter`) is done ([change](docs/changes/device-adapter-pipes-2026-10-29.md)); step 2a is done ([change](docs/changes/transports-as-device-adapters-2026-10-29.md): `ITransport` deleted, transports implement `IDeviceAdapter`); step 2b is done (Serial in `System.IO.Ports`, TCP/RFC 2217/VXI-11 in `System.IO.Tcp`); step 3 is done (`OoBDev.DevTerm.Core` and its tests merged in `src/Applications`); step 5 is in progress (HID and USBTMC done; next BLE and Windows BLE, MQTT and brokers as `System.IO.*` libraries, see [the plan](docs/design/DevTermParity/restructure-plan.md)), then step 6 (Wpf, Tui, Web shells), order in [BACKLOG.md](BACKLOG.md).

- [ ] Device decoding: all eight device packages are in `src/HardwareDevices` and `src/Extensions` with unit and Hardware tests (change documents `docs/changes/devices-*-2026-10-28.md`; feature docs [docs/design/DeviceDecoding/](docs/design/DeviceDecoding/README.md)). Open: the DE-5000 Hardware test (needs a BLE transport), the phase 0 checks in [device-parity.md](docs/migration/device-parity.md) section 4, then the dev-term merge (`OoBDev.UiDefinitions` is in; `OoBDev.UiDefinitions.ViewModels` form view models are in; `OoBDev.UiDefinitions.Wpf` `FormView` is in; the colour picker link option is in too, and the `OoBDev.DevTerm.Wpf` shell follows; unblocked 2026-10-29, owner answer to Q3: option A before the Avalonia spike, see the device decoding line in [BACKLOG.md](BACKLOG.md))

## Next

- [ ] **BinaryDataDecoders items** (re-review done 2026-10-29: [binarydatadecoders-rereview-2026-10-29.md](docs/migration/binarydatadecoders-rereview-2026-10-29.md)): Numerics, NMEA helpers, Drawing, Code analysis, Fencing, LanC, the Xslt CLI and the PackMan CLI are done; next is the order in Table 4 of that document; backlog lines are in [BACKLOG.md](BACKLOG.md).

