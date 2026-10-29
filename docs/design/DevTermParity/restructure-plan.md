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

## Migration table (to confirm before step 1)

| Project today | Target | Namespace | Notes |
|---|---|---|---|
| `OoBDev.DevTerm.Core`, `.Logging`, `.DeviceManifests`, `.Configuration`, `.Schemas`, `.Presenters.Text`, `.Plugins.*`, `.Loopback` | `src/Applications/OoBDev.DevTerm.Core` | `OoBDev.DevTerm.*` per folder | one assembly, one `Core.Tests` |
| `OoBDev.DevTerm.Observability` | `src/Framework/OoBDev.Telemetry` | `OoBDev.Telemetry` | no ASP.NET reference |
| `OoBDev.DevTerm.Transports.Serial` | `OoBDev.System.IO.Ports` | `OoBDev.System.IO.Ports` | pipe `IDeviceAdapter` |
| `OoBDev.DevTerm.Transports.Tcp`, `.Rfc2217`, `.Vxi11` | `OoBDev.System.IO.Tcp` | `OoBDev.System.IO.Tcp` | |
| HID, USBTMC, BLE, MQTT, brokers (not yet ported) | `OoBDev.System.IO.UsbHids` and new `OoBDev.System.IO.*` | `OoBDev.System.IO.*` | |
| Wpf, Tui, Web | `src/Applications/OoBDev.DevTerm.Wpf`, `.Tui`, `.Web` | `OoBDev.DevTerm.*` | |

## Steps

| # | Step | Verify |
|---|------|--------|
| 1 | Rework `IDeviceAdapter` to pipes in `OoBDev.System.Abstractions`; adapt `System.IO.Ports/Tcp/UsbHids` and the eight device libraries and tests ("Move device libraries to pipes") | all affected test projects green |
| 2 | Move the already-ported Serial, TCP, RFC 2217 and VXI-11 code and tests into `System.IO.*`; delete the `OoBDev.DevTerm.Transports.*` projects | transport tests green |
| 3 | Create `src/Applications/OoBDev.DevTerm.Core`; merge the listed projects and their tests under namespaces per folder (`Logging`, `Manifests`, `Configuration`, `Schemas`, `Presenters`, `Plugins`, `Observability`); the Schemas generator becomes a `dotnet run` mode or tool inside Core | `OoBDev.DevTerm.Core.Tests` equals the sum of the old counts (381 + 168 + the rest) |
| 3b | Create Framework `OoBDev.Telemetry` with the standalone OTLP exporter (and its tests); delete `OoBDev.DevTerm.Observability` | exporter tests green in the new home |
| 4 | Update `OoBDev.slnx`, readmes (one `README.DevTerm.Core.md`), the change documents and this design folder | docs tests pass |
| 5 | Port the remaining transports (USBTMC, HID, BLE, MQTT, brokers) as `System.IO.*` libraries | their tests green |
| 6 | Shells (`Wpf`, `Tui`, `Web`) in `src/Applications/` | per shell |

Observability (owner, 2026-10-29: if it is meant to be reusable it belongs in the framework). `TelemetryExporter` is generic apart from one line, `AddSource(DevTermTelemetry.Name)`: it starts a standalone (no host, gRPC) OTLP trace and metric exporter and parses an endpoint. It moves to a new host-free Framework library `OoBDev.Telemetry` (owner, 2026-10-29: telemetry is generic, so that is the target), holding the standalone OTLP exporter with source and meter names as parameters, and its tests. `OoBDev.OpenTelemetry` (ExternalServices) is not the home: it references `Microsoft.AspNetCore.App` and the Hosting extensions because of its user and correlation processors, which a WPF or TUI front end must not pull in. `OoBDev.DevTerm.Observability` is deleted and DevTerm Core only supplies `DevTermTelemetry.Name`. Merging the two libraries later is a separate decision.
