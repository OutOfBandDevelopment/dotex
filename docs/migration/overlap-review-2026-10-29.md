# Migration overlap review: BinaryDataDecoders, dev-term and dotex (2026-10-29)

[Migration index](README.md) · [BinaryDataDecoders re-review](binarydatadecoders-rereview-2026-10-29.md) · [Device decoding plan](device-decoding-migration-plan.md)

Sources compared: BinaryDataDecoders (`C:\repo\mine\binary-data-decoders`, HEAD `e590b420`, last change 2026-09-26), dev-term (`C:\repo\oobdev\dev-term`, HEAD `8c66e74`, last change 2026-10-28) and this repository. Counts are lines of C# and test attributes in each repository's test project, so they show relative size, not quality.

## Contents

- [Result](#result)
- [Device libraries](#device-libraries)
- [Transports and IO](#transports-and-io)
- [What is left from each source](#what-is-left-from-each-source)
- [Not verified](#not-verified)

## Result

dev-term is newer than BinaryDataDecoders for every device both contain, and it is where the tests are. The device cores were already brought into dotex from dev-term on 2026-10-28 (`docs/changes/devices-*-core-2026-10-28.md`). So the BinaryDataDecoders copies of Busylight, K8055, Radex One, Zoom H4n and NMEA are superseded: migrate nothing from them except the small leftovers named below. dev-term's remaining pieces (control surfaces, UI definitions, panel contributions, plugin modules, the transports and the shells) are additive and are tracked as "DevTerm 100% parity".

## Device libraries

**Table 1 — Same device in the three sources**

| Device | BinaryDataDecoders (lines / tests, last change) | dev-term (lines / tests, last change) | dotex today (lines / tests) | Base |
|--------|------|------|------|------|
| Busylight | 144 / 2, 2026-09-26 | 369 / 18, 2026-10-08 | 322 / 12 | dev-term (done) |
| Radex One | 759 / 1, 2026-09-26 | 772 / 36, 2026-10-08 | 443 / 17 | dev-term (core done) |
| K8055 | 148 / 0, 2026-09-26 | 374 / 25, 2026-10-08 | 262 / 12 | dev-term (core done) |
| Zoom H4n | 154 / 0, 2026-09-26 | 418 / 16, 2026-10-08 | 249 / 12 | dev-term (core done) |
| NMEA | 6,334 (WMI import code and notes included) / 4 files of tests | 486, 2026-10-08 | 629 | dev-term (done) |
| DE-5000, SCPI, Z-Wave | not in BinaryDataDecoders | 417, 994, 1,779 | 296, 767, 1,446 | dev-term (done) |

The dotex line counts are lower than dev-term's because the UI parts of each device (below) are not carried over yet.

dev-term-only types per device, all UI or hosting: `{Device}ControlSurface`, `{Device}UiDefinition`, `{Device}PanelContribution`, `{Device}PluginModule`, and a `{Device}Decoder` for Radex One, K8055, Busylight, Zoom H4n and NMEA that dotex models through its own message types. They belong to the DevTerm parity item, not to a BinaryDataDecoders migration.

## Transports and IO

**Table 2 — Transport and IO code**

| Code | BinaryDataDecoders | dev-term | dotex | Resolution |
|------|--------------------|----------|-------|------------|
| Segmenters, checksums, `IDeviceAdapter`, `IMessageDecoder`, `Endianness` | `IO.Abstractions` | none (uses its own pipeline) | `OoBDev.System.Abstractions` | Already migrated; nothing to do |
| Serial, HID, pipelines (`SerialPortDeviceAdapter`, `UsbHidDeviceAdapter`, `PipelineBuilder`, `StreamDevice`) | `IO.Ports`, `IO.UsbHids`, `IO.Pipelines` | `Transports.Serial` (14 files), `Transports.Hid` (8) | `OoBDev.System.IO.Ports`, `.UsbHids`, `.Pipelines` | The adapters are in dotex from BinaryDataDecoders. dev-term's are a second, newer implementation behind `ITransport` (port discovery, XON/XOFF, read streams). Compare in the parity matrix and keep the better one per feature; do not carry both |
| TCP and RFC 2217 | none | `Transports.Tcp` (11, LXI discovery), `Transports.Rfc2217` (12, newest, 2026-10-14) | `OoBDev.System.IO.Tcp` (`TcpDeviceAdapter`, `Rfc2217DeviceAdapter`) | dev-term is the base for RFC 2217 and LXI discovery; merge into the existing project |
| USBTMC, VXI-11, BLE, MQTT, brokers, loopback | none | `Transports.Usbtmc`, `.Vxi11`, `.Ble`, `.Mqtt`, `.Brokers`, `.Loopback` | none | New from dev-term only; no overlap with BinaryDataDecoders |
| Text presenters, session log and playback, device manifests, UI definitions | none | present | `OoBDev.UiDefinitions*` exists; the rest is absent | New from dev-term; manifests and `.ksy` are phase 8 of the device plan |

## What is left from each source

**Table 3 — Remaining work by source**

| Source | Still to bring over |
|--------|---------------------|
| BinaryDataDecoders | The leftovers in the re-review: `GeneralHeading` and the WMI and HID import notes from NMEA, `SECURITY_ATTRIBUTES`, the LanC wiring diagram and readme, a behaviour check by running the source tests, and the Drawing port to SkiaSharp (later). The non-device projects are covered by the re-review |
| dev-term | Everything in the "DevTerm 100% parity" backlog item: the control surfaces and panels, the transports in Table 2, manifests, presenters, session log, and the Wpf, Tui and Web shells |
| Both | Nothing is migrated twice: where both have the same thing, dev-term is the base because it is newer and tested |

## Not verified

- Line counts include generated and note files in BinaryDataDecoders NMEA (6,334 lines); the real decoder is much smaller.
- Whether dotex carries the Radex One request and response types that BinaryDataDecoders names `ReadValuesRequest`, `ReadSettingsRequest` and so on, under other names was matched by file name only. The parity matrix should compare commands, not file names.
- Test counts are test attributes, not passing tests; nothing was built or run for this review.
