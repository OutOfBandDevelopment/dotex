# DevTerm restructure plan

Owner decisions, 2026-10-29 (planning session). They supersede the project layout in [architecture](architecture.md); the Q1 answer (pipes everywhere) still holds.

## Decisions

1. **One core assembly.** `OoBDev.DevTerm.Schemas`, `Logging`, `DeviceManifests` and `Configuration` merge into `OoBDev.DevTerm.Core`. Presenters, plugins (KeyValue, Sample), Observability, the Loopback transport and the session servers merge into it as well ("merge everything into Core").
2. **Location.** `OoBDev.DevTerm.Core` moves from `src/Framework` to `src/Applications/`. `OoBDev.DevTerm.Wpf`, `.Tui` and `.Web` also live in `src/Applications/`. One merged `OoBDev.DevTerm.Core.Tests`.
3. **Transports follow `OoBDev.System.IO.*`.** They are not `OoBDev.DevTerm.Transports.*` projects. `IDeviceAdapter` (in `OoBDev.System.Abstractions`) evolves from a blocking `Stream` to a pipe shape (`PipeReader`/`PipeWriter`) and takes over `ITransport`'s connection state, events and options. Serial goes to `System.IO.Ports`, TCP, RFC 2217 and VXI-11 to `System.IO.Tcp`, HID to `System.IO.UsbHids`; USBTMC, BLE and MQTT/brokers become new `System.IO.*` libraries.
4. **Layering follows from it.** Framework code never references `src/Applications`, so every contract a transport needs lives in Framework (`OoBDev.System.Abstractions`, `OoBDev.System.IO.Pipelines`), and `DevTerm.Core` consumes them.
5. **Order.** Restructure first, then port the remaining transports straight into the new layout. Q2 (schema `$id` host) stays on its default.

## Intent (owner, 2026-10-29, later the same session)

- Transports must be reusable and independent of the decoders, and zero-copy where possible (pipe readers and writers, no intermediate copies). Nothing in `System.IO.*` may reference DevTerm.
- Most shared DevTerm logic consolidates into the one core library used by the three front ends. Little reuse is expected across the former libraries, so they are not kept separate.
- A `DevTerm.Abstractions` (for example the plugin model) may be extracted later if extension needs it; not now.
- Schema identifiers use the `oobdev://{application}/{namespace}/...` scheme, not a hosted URL (Q2 answered): `oobdev://devterm/schemas/<file>`. They are names, not fetch locations.

## Telemetry and the user accessor (owner, 2026-10-29)

- `ICurrentUserAccessor` (`OoBDev.System.Abstractions`) gains `ClaimsPrincipal? User`; `UserName` derives from it. Implementations: `HttpContextUserAccessor` (`OoBDev.AspNetCore.Mvc`, wraps `IHttpContextAccessor`), `EnvironmentUserAccessor` (`OoBDev.System`, builds a principal from the active Windows identity for CLI and WPF), and any application-supplied one (token, SSO). Correlation already uses `IAccessor<CorrelationInfo>`.
- `UserLogProcessor`, `UserTelemetryProcessor` and `UserClaims` read the accessor, not `IHttpContextAccessor`.
- The processors, options, DI extensions and the standalone OTLP exporter all live in one host-free Framework library, `OoBDev.Telemetry` (no ASP.NET reference). `ExternalServices/OpenTelemetry/OoBDev.OpenTelemetry` is retired, its tests move, and the Example web, API and AppHost references are updated.

## Adapter contract (step 1)

- `OoBDev.System.Abstractions/IO`: `ConnectionState`, `ConnectionStateChangedEventArgs` and `IDeviceAdapter : IAsyncDisposable` with `Type`, `Path`, `State`, `StateChanged`, `PipeReader Input`, `PipeWriter Output`, `OpenAsync`, `CloseAsync`. `TryOpen` and `Stream` are gone; `IBufferedDeviceAdapter` keeps `BytesToRead`.
- `OoBDev.System.IO.Pipelines`: `StreamToPipePump`, the abstract `StreamDeviceAdapter` (state machine, pump, `PipeWriter.Create`) and `AsStream(this IDeviceAdapter)` for stream-based callers. Serial, TCP, RFC 2217 and HID adapters derive from it.
- `StreamDevice<T>` reads `Input` and writes `Output`; callers open the adapter first. `StreamTransport` is deleted; `ITransport` was deleted in step 2a ([change](../../changes/transports-as-device-adapters-2026-10-29.md)).

## Migration table (confirmed 2026-10-29)

| Project today | Target | Namespace | Notes |
|---|---|---|---|
| `OoBDev.DevTerm.Core`, `.Logging`, `.DeviceManifests`, `.Configuration`, `.Schemas`, `.Presenters.Text`, `.Plugins.*`, `.Loopback` | `src/Applications/OoBDev.DevTerm.Core` | `OoBDev.DevTerm.*` per folder | one assembly, one `Core.Tests` |
| `OoBDev.DevTerm.Observability` and `ExternalServices/OpenTelemetry/OoBDev.OpenTelemetry` (+ Tests) | `src/Framework/OoBDev.Telemetry` (+ Tests) | `OoBDev.Telemetry` (was `OoBDev.OpenTelemetry`) | no ASP.NET reference; Example projects re-referenced |
| `OoBDev.DevTerm.Transports.Serial` | `OoBDev.System.IO.Ports` | `OoBDev.System.IO.Ports` | pipe `IDeviceAdapter` |
| `OoBDev.DevTerm.Transports.Tcp`, `.Rfc2217`, `.Vxi11` | `OoBDev.System.IO.Tcp` | `OoBDev.System.IO.Tcp` | |
| HID, USBTMC, BLE, MQTT, brokers (not yet ported) | `OoBDev.System.IO.UsbHids` and new `OoBDev.System.IO.*` | `OoBDev.System.IO.*` | |
| Wpf, Tui, Web | `src/Applications/OoBDev.DevTerm.Wpf`, `.Tui`, `.Web` | `OoBDev.DevTerm.*` | |

## Steps

| # | Step | Verify |
|---|------|--------|
| 1 | Rework `IDeviceAdapter` to pipes in `OoBDev.System.Abstractions`; adapt `System.IO.Ports/Tcp/UsbHids` and the eight device libraries and tests ("Move device libraries to pipes") | all affected test projects green |
| 2 | Move the already-ported Serial, TCP, RFC 2217 and VXI-11 code and tests into `System.IO.*`; delete the `OoBDev.DevTerm.Transports.*` projects | transport tests green |
| 3 (done 2026-10-29) | Create `src/Applications/OoBDev.DevTerm.Core`; merge the listed projects and their tests under namespaces per folder (`Logging`, `Manifests`, `Configuration`, `Schemas`, `Presenters`, `Plugins`, `Observability`); the Schemas generator becomes a `dotnet run` mode or tool inside Core | `OoBDev.DevTerm.Core.Tests` equals the sum of the old counts (381 + 168 + the rest) |
| 3b (done 2026-10-29) | Create Framework `OoBDev.Telemetry` with the standalone OTLP exporter (and its tests); delete `OoBDev.DevTerm.Observability` | exporter tests green in the new home |
| 4 | Update `OoBDev.slnx`, readmes (one `README.DevTerm.Core.md`), the change documents and this design folder | docs tests pass |
| 5 | Port the remaining transports (USBTMC, HID, BLE, MQTT, brokers) as `System.IO.*` libraries | their tests green |
| 6 | Shells (`Wpf`, `Tui`, `Web`) in `src/Applications/` | per shell |

Observability (owner, 2026-10-29: if it is meant to be reusable it belongs in the framework). `TelemetryExporter` is generic apart from one line, `AddSource(DevTermTelemetry.Name)`: it starts a standalone (no host, gRPC) OTLP trace and metric exporter and parses an endpoint. It moves to a new host-free Framework library `OoBDev.Telemetry` (owner, 2026-10-29: telemetry is generic, so that is the target), holding the standalone OTLP exporter with source and meter names as parameters, and its tests. `OoBDev.OpenTelemetry` (ExternalServices) is not the home: it references `Microsoft.AspNetCore.App` and the Hosting extensions because of its user and correlation processors, which a WPF or TUI front end must not pull in. `OoBDev.DevTerm.Observability` is deleted and DevTerm Core only supplies `DevTermTelemetry.Name`. Merging the two libraries later is a separate decision.

Step 3 result (2026-10-29): `OoBDev.DevTerm.Core` and `OoBDev.DevTerm.Core.Tests` live in `src/Applications`. Folders: `Logging`, `DeviceManifests` (with `Manifests/`), `Configuration`, `Schemas`, `Presenters/Text`, `Loopback` (namespace `OoBDev.DevTerm.Loopback`, was `Transports.Loopback`). `Plugins.KeyValue`, `.KeyValue.Tests` and `Plugins.Sample` were kept separate at this step; the owner answered 2026-10-29 that they merge into Core (see "Plugins and device panels"). The Schemas `Program.cs` entry point was dropped; `SchemaGenerator` is a library class and a generator hook is in BACKLOG.md. Tests: 892 passed, 5 skipped.

## Plugins and device panels (owner answers, 2026-10-29)

- **Plugins merge into Core.** `Plugins.KeyValue`, `Plugins.Sample` and the KeyValue tests become built-in features of `OoBDev.DevTerm.Core` and `OoBDev.DevTerm.Core.Tests` (answer to the earlier Q1: "Merge into Core"). The loader tests then need another way to load a real external assembly, for example a small test-only plugin built by the test project (see BACKLOG.md).
- **Device panels live with their protocols.** The owner asked whether the K8055 and Busylight panels can use the generic `UiDefinition` and live with their protocols. They can: `UiDefinitions` is Framework and the device libraries already reference Framework, so `K8055UiDefinition`, the command encoding and the decoder values belong in `OoBDev.Velleman.K8055` and `OoBDev.Kuando.Busylight`. DevTerm Core keeps only a generic adapter (panel id, menu title, USB vendor and product match) that resolves them. What blocks it today: `IControlSurface`, `ICommandPreview` and `IStructuredPresenter` live in `OoBDev.DevTerm.Core`, and the surfaces send through `Session`. They have to move to a host-free Framework home and send through a delegate or `IDeviceAdapter.Output`. The migration table and the home are Q1 in OPEN-QUESTIONS.md, to be confirmed before work starts (rule `layering-and-migration-plans`).
- **Device-dependent configuration and options follow the same rule** (owner, 2026-10-29: "Same with device dependent configuration/options"). Whatever is specific to one device, such as its settings model, config editor, option lists and per-device manifest, lives in that device's library next to its protocol. `OoBDev.DevTerm.Core` keeps only the generic contracts (`IDeviceConfigEditor`, `DeviceConfigTarget`, the manifest engine) and finds device pieces through DI. The contracts these need in Framework are part of OPEN-QUESTIONS Q1.
