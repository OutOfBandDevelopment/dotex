# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-28 · **Branch:** `dev/example-app-fixes`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] OData and GraphQL surfaces: waiting on Q2 (library choice); take the next item meanwhile
- [ ] Device decoding: all eight device packages are in `src/HardwareDevices` and `src/Extensions` with unit and Hardware tests (change documents `docs/changes/devices-*-2026-10-28.md`; feature docs [docs/design/DeviceDecoding/](docs/design/DeviceDecoding/README.md)). Open: the DE-5000 Hardware test (needs a BLE transport), the phase 0 checks in [device-parity.md](docs/migration/device-parity.md) section 4, then the dev-term merge (blocked on Q3)
- [ ] `OoBDev.MongoDB` integration tests fail (6): `MongoDispatchProxy` is sealed and `DispatchProxy` needs an unsealed base; unseal or replace it

## Next

- [ ] **Incoming code recommendation plan**: written ([docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md) and [cross-check](docs/migration/incoming-harvest-crosscheck.md)); waiting for the owner to answer D1 to D9 (Q1 in OPEN-QUESTIONS.md). No migration until then. The sections marked verify need a second look before work starts.

- [ ] `OoBDev.DacFx.Tests` SQLCLR test project
