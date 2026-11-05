# DevTerm parity: architecture (draft)

**Status:** Draft, 2026-10-29 · [Parity matrix](../../design/DevTermParity/README.md) · [Device decoding architecture](../../design/DeviceDecoding/architecture.md)

How the dev-term engine lands in this repository. Requirements, api-design and testing-strategy follow once the owner has reviewed this document.

## Contents

- [Contracts](#contracts)
- [Landing projects](#landing-projects)
- [Order](#order)
- [Decision](#decision)

## Contracts

dev-term has two core contracts, `ITransport` (an async, full-duplex connection with a `PipeReader Input`, `OpenAsync`, `CloseAsync`, `WriteAsync` and a state with change events) and `IPresenter` (`Render(ReadOnlySequence<byte>)` returns text lines). This repository's older contract was `IDeviceAdapter` (`Type`, `Path`, `TryOpen(out Stream)`, `Stream`); since 2026-10-29 it is pipe-based like `ITransport` (see [the restructure plan](../../design/DevTermParity/restructure-plan.md#adapter-contract-step-1)), used by the serial, HID and TCP adapters and by the pipeline builder.

**Table 1 — The two transport contracts**

| Aspect | `IDeviceAdapter` (here) | `ITransport` (dev-term) |
|--------|-------------------------|--------------------------|
| Open | Synchronous `TryOpen` | `OpenAsync` with cancellation |
| Data in | `Stream` | `PipeReader` |
| Lifecycle | None | State, change event, `IAsyncDisposable` |
| Discovery | Factory by path | Per-transport discovery services |
| Users | Device libraries, pipelines, Z-Wave session | Sessions, shells, logging, routing |

Both are needed: the device libraries and the pipelines are built on streams, and the dev-term engine needs state and async. The proposal is to keep `IDeviceAdapter` unchanged and add `ITransport` beside it, with one small adapter in each direction (a stream-backed transport over any `IDeviceAdapter`, and a stream view over any transport) so that the serial, HID, TCP and RFC 2217 code is written once.

## Landing projects

**Table 2 — Proposed projects**

| dev-term group | Project here | Layer |
|----------------|--------------|-------|
| A Engine (Core, Configuration, Logging, Observability, Schemas) | `OoBDev.DevTerm.Core`, `OoBDev.DevTerm.Configuration`, `OoBDev.DevTerm.Logging` | Framework; `OoBDev.OpenTelemetry` is reused for observability |
| B Presenters | `OoBDev.DevTerm.Presenters` | Framework |
| C Manifests | `OoBDev.DevTerm.DeviceManifests` | Framework |
| E Transports | One project per transport, `OoBDev.System.IO.{Name}` where a stream adapter already exists (Ports, UsbHids, Tcp), otherwise `OoBDev.DevTerm.Transports.{Name}` | Extensions |
| F Devices | Existing device packages gain `ControlSurface`, `UiDefinition` and `PluginModule` types | HardwareDevices, Extensions |
| G Plugins | `OoBDev.DevTerm.Plugins.*` | Extensions |
| H Shells | `OoBDev.DevTerm.Wpf`, `.Tui`, `.Web` under `src/Tools` (owner decision, 2026-10-28) | Tools |

## Order

1. Contracts and the two adapters, with tests, in `OoBDev.DevTerm.Core`.
2. The session engine (session, observers, router, plugin loader), then logging and configuration.
3. Loopback transport (it lets every shell and test run without hardware), then the existing serial, TCP and HID adapters wrapped as transports.
4. Presenters and manifests.
5. The remaining transports, then the shells, each with its screen specification.

## Decision

Owner, 2026-10-29: pipes everywhere. `ITransport` (a `PipeReader` plus a lifecycle, zero-copy) is the contract; `IDeviceAdapter` and its blocking `Stream` are to be replaced, with feature parity but not interface parity with dev-term. `StreamTransport` and `StreamToPipePump` stay only as a bridge until the serial, HID, TCP and RFC 2217 adapters and the device libraries move to pipes (backlog: "Move device libraries to pipes").
