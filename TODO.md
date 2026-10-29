# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-22 · **Branch:** `dev/claude-config`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] OData and GraphQL surfaces: waiting on Q2 (library choice); take the next item meanwhile
- [ ] Device decoding phase 1: feature docs written ([docs/design/DeviceDecoding/](docs/design/DeviceDecoding/README.md), Draft); stub projects for all 8 devices, their test projects and `OoBDev.Devices.TestUtilities` exist (build only, 2026-10-28; Q2 answered: `Hardware` category); IO gaps closed (Xor8/word-sum checksums, fixed-length segmenter, busy-port fix, `Segment.Using` custom segmenter seam); Z-Wave core is done end to end and verified on the Z-Stick on COM13 ([change](docs/changes/devices-zwave-core-2026-10-28.md)); Z-Wave interpreter, node table, names and the remaining requests are in (27 unit tests); id tables are in ([change](docs/changes/devices-zwave-id-tables-2026-10-28.md)) and the ACK responder follows, DE-5000 packet decoding is in (13 unit tests, no hardware test yet); Busylight is done and verified on the light ([change](docs/changes/devices-busylight-core-2026-10-28.md)), K8055 is in (11 unit tests, 3 Hardware tests passed on a real board), Radex One is in and verified on the device ([change](docs/changes/devices-radexone-core-2026-10-28.md)), the Zoom H4n is in (22 unit tests, Hardware test ported, [change](docs/changes/devices-zoomh4n-core-2026-10-28.md)), NMEA is in (28 unit tests, BT-20 Hardware test ported, [change](docs/changes/devices-nmea-core-2026-10-28.md)), SCPI profiles, builder and codecs are in (29 unit tests, [change](docs/changes/devices-scpi-core-2026-10-28.md)), and the device packages now live in `src/HardwareDevices`; phase 0 open checks in [device-parity.md](docs/migration/device-parity.md) section 4

## Next

- [ ] **Incoming code recommendation plan**: written ([docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md) and [cross-check](docs/migration/incoming-harvest-crosscheck.md)); waiting for the owner to answer D1 to D9 (Q1 in OPEN-QUESTIONS.md). No migration until then. The sections marked verify need a second look before work starts.

- [ ] `OoBDev.DacFx.Tests` SQLCLR test project
