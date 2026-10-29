# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-29 · **Branch:** `dev/example-app-fixes`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] Device decoding: all eight device packages are in `src/HardwareDevices` and `src/Extensions` with unit and Hardware tests (change documents `docs/changes/devices-*-2026-10-28.md`; feature docs [docs/design/DeviceDecoding/](docs/design/DeviceDecoding/README.md)). Open: the DE-5000 Hardware test (needs a BLE transport), the phase 0 checks in [device-parity.md](docs/migration/device-parity.md) section 4, then the dev-term merge (unblocked 2026-10-29, owner answer to Q3: option A before the Avalonia spike, see the device decoding line in [BACKLOG.md](BACKLOG.md))

## Next

- [ ] **BinaryDataDecoders items** (re-review done 2026-10-29: [binarydatadecoders-rereview-2026-10-29.md](docs/migration/binarydatadecoders-rereview-2026-10-29.md)): Numerics, NMEA helpers, Drawing, Code analysis, Fencing and LanC are done; next is the order in Table 4 of that document; backlog lines are in [BACKLOG.md](BACKLOG.md).

