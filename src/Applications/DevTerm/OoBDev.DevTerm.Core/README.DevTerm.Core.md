# OoBDev.DevTerm.Core

The engine of the DevTerm tool, shared by the Tui, Web and WPF front ends. It binds an `IDeviceAdapter` to a pipeline of presenters in a `Session` and holds everything around it: configuration and profiles, device manifests, text presenters, plugins, session logs and playback, stream capture, routing to brokers, the loopback test device and the JSON schemas. The device contracts (`IDeviceAdapter`, `ConnectionState`, `IComPortControl`, `IControlSurface`, `IDevicePanel`, `ICommandPreview`, `IDeviceConfigEditor`) are in `OoBDev.System.Abstractions`; the adapters are in the `OoBDev.System.IO.*` libraries. See [the parity matrix](../../../../docs/design/DevTermParity/README.md) and [architecture](../../../../docs/applications/devterm/architecture.md).

## Contents

- [Session and presenters](#session-and-presenters)
- [Configuration](#configuration)
- [Device manifests](#device-manifests)
- [Schemas](#schemas)
- [Text presenters](#text-presenters)
- [Loopback device](#loopback-device)
- [Tests](#tests)

## Session and presenters

**Table 1 — Core types**

| Type | Role |
|------|------|
| `Session`, `ISessionFactory`, `SessionFactory`, `SessionLimits`, `ISessionObserver`, `DevTermTelemetry` | Binds one `IDeviceAdapter` to a `Pipeline`: read loop, send throttle, connect retry and timeout, query with timeout, self-reported disconnect, observers, the `DevTerm` activity source and meter |
| `IPresenter`, `IPresenterInput`, `IResettablePresenter`, `IStructuredMessageSource`, `IAsyncOutputSource`, `IOriginatingPresenter`, `IContentCapturePresenter` | Presenter contracts: render bytes to text lines, parse typed input, reset on reopen, emit structured or asynchronous output |
| `Pipeline`, `PresenterOutput`, `StructuredMessage`, `PresenterCatalog` | Fans received bytes out to the presenters; content-capturing presenters take a chunk first. The catalog is transient: resolve it once per session |
| `LineReplyPresenter`, `IReplyTracker`, `ReplyMatch` | Base for line presenters that correlate reply lines with pending queries (CR, LF, CRLF and LFCR terminators, bounded buffer) |
| `PluginLoader`, `PluginTrust`, `PluginInstaller`, `LivePlugins`, `ExternalProcessPresenter` (`Plugins/`) | In-process plugins (`IPluginModule`, `plugin.json`) and approval-gated out-of-process plugins (JSON over stdin/stdout) |
| `StreamContentWatcher`, `StreamContentSniffer`, `StreamContentEndFinder`, `StreamCapture` (`StreamContent/`) | Detects and captures images, PostScript, PCL and HP-GL in the raw byte stream (declared hints or signature sniffing); `HpglToSvgConverter` turns HP-GL into SVG |
| `SessionLogWriter`, `SessionLogger`, `PlaybackEngine`, `PlaybackController` (`Logging/`) | Session log files (with periodic `checkpoint` records) and their replay; `ISnapshotPresenter` lets a backward seek restart from a checkpoint |
| `MessageRouter`, `RoutingRule`, `RoutingRuleSet`, `IMessageSink`, `BrokerRouterBridge`, `MqttRouterBridge` | Regex routing between a device session and a broker, JSON rule sets, confirm-before-send |
| `DevicePanelContribution`, `SurfacePanelBuilder`, `DeviceDecoderPresenter` (`Panels/`), `IInstrumentPanelProvider`, `DeviceConfigSession`, `ParameterValueList` | Turn the device control contracts into panels; config editing with read-back and preview |
| `SessionPipeServer`, `SessionPipeClient`, `SessionControlPipeServer`, `SessionControlClient`, `SessionTcpShareServer`, `SessionHttpControlServer`, `SessionCommands` | Cross-process access to a live session: read-only pipe tail, control pipe, TCP share and HTTP control |
| `Rfc2217ServerBridge`, `Rfc2217PortSettings` | Shares a session's serial port over RFC 2217 |
| `ServiceHostBridge`, `ServiceKind` | Hosts the echo, discard, daytime, time or chargen TCP server as a device (`--transport service --service <kind>`); clients show as received data, replies on `Replied` |
| `DevTermHome` | The per-user DevTerm folder (`~/.dev-term`; `DEVTERM_HOME` overrides it) |

```csharp
services.AddDevTermCore();   // PresenterCatalog (transient), ISessionFactory, IRoutingLinkFactory (TryAdd*)
```

## Configuration

`Configuration/` is the composition root of the DevTerm hosts: CLI, environment and JSON options (`CliOptions`, `CliOptionsValidator`), `.devterm` project files, connection profiles and the connection editor view model, user data paths, themes and preferences, converter tools, capture export, stream monitoring, routing, send history, and network discovery (mDNS, SSDP, USR bridge).

`AddDevTermFrontEnd(cliOptions)` registers the core, the text presenters and the adapter the `--transport` option names, configured from the options: `loopback`, `service`, `tcp`, `hid`, `usbtmc`, `ble`, `rfc2217`, `vxi11`, `mqtt`, `amqp`, `stomp` and the serial default. `AddPlugins(cliOptions)` adds the plugin loader. `DevTermSessionBuilder` and `SessionTab` build a session from the same options. The BLE platform adapter is loaded by reflection (`BlePlatformAdapterLoader`) so plain net10.0 hosts need no Windows target.

## Device manifests

`DeviceManifests/`: a YAML or JSON manifest (transport hints, outbound commands with parameters, inbound response patterns and frame layouts) is loaded, validated and turned into a control surface, a reply presenter, a frame presenter and a generated `UiDefinition`.

**Table 2 — Manifest types**

| Area | Types |
|------|-------|
| Model and I/O | `DeviceManifest`, `DeviceManifestLoader`, `DeviceManifestSerializer`, `DeviceManifestWriter`, `DeviceManifestValidator`, `ManifestCatalog` |
| Runtime | `ManifestControlSurface`, `ManifestReplyPresenter`, `ManifestFramePresenter`, `ManifestUiBuilder`, `ManifestPanel` |
| Frames | `FrameSchema`, `FrameField`, `FrameLayout`, `FrameDecoder`, `KsyImporter` (Kaitai `.ksy` subset) |
| Preview data | `ValuePathCatalog`, `SampleDataGenerator`, `RecordedSamples` |
| Editing | `ManifestEditorViewModel`, the form classes and `ExpressionPickerViewModel` (`Editing/`); WPF and web form rendering are in the front ends |

Pre-packaged manifests under `DeviceManifests/Manifests/` are copied to `manifests\{device}\` next to every app that references this project.

## Schemas

`SchemaGenerator.Generate()` exports a JSON Schema for each hand-written DevTerm format (device manifest, UI definition, connection profile) from the C# model, so a schema cannot drift from the code. The committed copies are in `schemas/devterm/`, and `SchemaDriftTests` fails when they differ from the generated text. A command-line hook is in `BACKLOG.md`.

## Text presenters

`Presenters/Text/` holds the presenters that turn received bytes into viewable text. `keyvalue` (formerly the KeyValue example plugin) decodes `name=value [unit]` lines into a `StructuredMessage`.

**Table 3 — Presenters**

| Name | Behaviour |
|------|-----------|
| `ascii` | Line-buffered: emits a line on CR, LF or CRLF (one terminator) or at `MaxLineLength` (default 4096, `0` = unbounded). Stateful; implements `IResettablePresenter` |
| `utf8` | Stateful decoder, so a multi-byte character split across reads decodes once complete |
| `hex` | Upper-case hex, unspaced. `Parse` accepts spaces and `0x` prefixes |
| `decimal`, `octal`, `binary` | Space-separated bytes, padded to 1, 3 and 8 digits |

All implement `IPresenterInput` to turn typed text back into bytes.

```csharp
services.TryAddTextPresenters();
services.Configure<AsciiPresenterOptions>(o => o.MaxLineLength = 1024);
```

Presenters are transient: resolve one per session, since `ascii` and `utf8` hold partial input.

## Loopback device

`Loopback/` is an in-process `IDeviceAdapter` that answers scripted commands, so the DevTerm shells and tests run with no hardware.

**Table 4 — Loopback commands**

| Command | Response |
|---------|----------|
| `hello` | `From Loopback test` |
| `Send Stream: N, ascii` | one line of N repeating A-Z characters |
| `Send Events: N` | `Event 1` .. `Event N`, one per line |
| `MEAS?` | the next simulated sensor sample |
| `Samples: N` | the next N samples (N is capped at 1000) |
| `STATUS?` | a labelled reading with units |
| `help`, `?` | the command list |
| anything else | `? Unrecognized: <line>` |

Matching is case-insensitive. The sample counters belong to the transport instance, so each one starts at sample 0. `LoopbackTransportOptions.SampleIntervalMs` (default 0) paces the lines of `Send Events` and `Samples`. `services.TryAddLoopbackTransport()` registers it transient; a closed transport can be reopened.

## Tests

`OoBDev.DevTerm.Core.Tests` is organised by the same folders (Sessions, Configuration, DeviceManifests, Logging, Loopback, Plugins, Presenters, Routing, Brokers, Control, Panels, StreamContent, Schemas), with in-memory pipes, mocks and the loopback device. Nothing here talks to a real device; hardware checks are in the transport and device test projects.
