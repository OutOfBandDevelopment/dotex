# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-22 · **Branch:** `dev/claude-config`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] OData and GraphQL surfaces: waiting on Q2 (library choice); take the next item meanwhile
- [ ] Device decoding phase 1: feature docs written ([docs/design/DeviceDecoding/](docs/design/DeviceDecoding/README.md), Draft); stub projects for all 8 devices, their test projects and `OoBDev.Devices.TestUtilities` exist (build only, 2026-10-28; Q2 answered: `Hardware` category); next the template device DE-5000 with the IO gaps from its api-design (checksums, segmenter, serial adapter fix); phase 0 open checks in [device-parity.md](docs/migration/device-parity.md) section 4

## Next

- [ ] **Incoming code recommendation plan**: written ([docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md) and [cross-check](docs/migration/incoming-harvest-crosscheck.md)); waiting for the owner to answer D1 to D9 (Q1 in OPEN-QUESTIONS.md). No migration until then. The sections marked verify need a second look before work starts.

- [ ] `OoBDev.DacFx.Tests` SQLCLR test project
