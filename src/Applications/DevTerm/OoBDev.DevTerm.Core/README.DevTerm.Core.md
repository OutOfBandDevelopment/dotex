# OoBDev.DevTerm.Core

The engine contracts of the DevTerm tool, ported from dev-term (see [the parity matrix](../../../docs/design/DevTermParity/README.md) and [architecture](../../../docs/applications/devterm/architecture.md)).

## Description

| Type | Role |
|------|------|
| `ITransport` | Asynchronous, full-duplex connection with a lifecycle, a state event and a `PipeReader` |
| `ConnectionState`, `ConnectionStateChangedEventArgs` | Lifecycle states and the change event |
| `IPresenter`, `IPresenterInput`, `IResettablePresenter`, `IStructuredMessageSource`, `IAsyncOutputSource`, `IOriginatingPresenter`, `IContentCapturePresenter` | Presenter contracts: render bytes to text lines, parse typed input, reset on reopen, emit structured or asynchronous output |
| `Pipeline`, `PresenterOutput`, `StructuredMessage` | Fans received bytes out to the presenters; content-capturing presenters take a chunk first |
| `MessageRouter`, `RoutingRule`, `RoutingRuleSet`, `IMessageSink` | Regex routing between a device session and a broker, JSON rule sets, confirm-before-send |
| `StreamContentWatcher`, `StreamContentSniffer`, `StreamContentEndFinder`, `HpglToSvgConverter`, `StreamCapture` | Detects and captures images, PostScript, PCL and HP-GL in the raw byte stream (declared hints or signature sniffing), converts HP-GL to SVG |
| `LineReplyPresenter`, `IReplyTracker`, `ReplyMatch` | Base for line presenters that correlate reply lines with pending queries (CR, LF, CRLF and LFCR terminators, bounded buffer) |
| `PresenterCatalog`, `PluginLoader`, `PluginTrust`, `LivePlugins`, `ExternalProcessPresenter` | Presenter lookup; in-process plugins (`IPluginModule`, `plugin.json`) and approval-gated out-of-process plugins (JSON over stdin/stdout) |
| `IControlSurface`, `IDevicePanelContribution`, `IInstrumentPanelProvider`, `IDeviceConfigEditor`, `DeviceConfigSession`, `ICommandPreview`, `ParameterValueList` | Device control contracts: command surface, panel contribution, config editing with read-back and preview |
| `IComPortControl`, `WriteDelayStream`, `ConnectionStateChangedEventArgs` | Serial-port control surface and a per-byte write delay |
| `SessionPipeServer`, `SessionPipeClient`, `SessionControlPipeServer`, `SessionControlClient`, `SessionTcpShareServer`, `SessionHttpControlServer`, `SessionCommands` | Cross-process access to a live session: read-only pipe tail, control pipe, TCP share and HTTP control |
| `AddDevTermCore` | Registers the catalog and session factory (`TryAdd*`) |
| `DevTermHome` | The per-user DevTerm folder (override by environment) |
| `Session`, `ISessionFactory`, `SessionLimits`, `ISessionObserver`, `DevTermTelemetry` | Binds a transport to a pipeline: read loop, send throttle, connect retry and timeout, query with timeout, self-reported disconnect, observers, `DevTerm` activity source and meter |
| `StreamToPipePump` | Copies a stream into a pipe, reading into the pipe's pooled buffers |
| `StreamTransport` | An `ITransport` over any `IDeviceAdapter`, so the serial, HID and TCP adapters serve both contracts |

`IDeviceAdapter` stays the contract of the device libraries; this package adds `ITransport` beside it (default of open question Q1). Plugins, logging and the control servers follow in the next slices.

## Verified

Unit tests cover the stream pump, `StreamTransport`, `Session` (lifecycle, faults and reconnect, limits, observers, telemetry) and `Pipeline`, with in-memory pipes and mocks. Nothing was run against a device.

## Not yet verified on hardware

Serial, HID and TCP through `StreamTransport` have not been tried with real devices.



## OoBDev.DevTerm.DeviceManifests

Declarative device description for DevTerm: a YAML/JSON manifest (transport hints, outbound commands with parameters, inbound response patterns and frame layouts) is loaded, validated and turned into a control surface, a reply presenter, a frame presenter and a generated `UiDefinition`.

| Area | Types |
|------|-------|
| Model and I/O | `DeviceManifest`, `DeviceManifestLoader`, `DeviceManifestSerializer`, `DeviceManifestWriter`, `DeviceManifestValidator`, `ManifestCatalog` |
| Runtime | `ManifestControlSurface`, `ManifestReplyPresenter`, `ManifestFramePresenter`, `ManifestUiBuilder`, `ManifestPanel` |
| Frames | `FrameSchema`, `FrameField`, `FrameLayout`, `FrameDecoder`, `KsyImporter` (Kaitai `.ksy` subset) |
| Preview data | `ValuePathCatalog`, `SampleDataGenerator`, `RecordedSamples` |

Pre-packaged manifests under `Manifests/` are copied to `manifests\{device}\` next to the app. The manifest editor (`Editing`: `ManifestEditorViewModel`, the form classes, `ExpressionPickerViewModel`) is in; WPF and web forms rendering is separate.

Tests: `OoBDev.DevTerm.DeviceManifests.Tests`.


## OoBDev.DevTerm.Configuration

The DevTerm composition root: CLI/environment/JSON options (`CliOptions`, `CliOptionsValidator`), `.devterm` project files, connection profiles, user data paths (under `DEVTERM_HOME`), themes and preferences, converter tools, capture export and stream monitoring, routing, network discovery (mDNS, SSDP, USR bridge) and the `ServiceCollectionExtensions` that build a session (`DevTermSessionBuilder`, `SessionTab`).

`AddDevTermFrontEnd` currently wires the loopback transport; other transports throw `NotSupportedException` until they are ported as pipe transports.

Not yet ported: the connection editor view model, the BLE/HID/USBTMC/LXI option classes and scanners, and `RoutingLinkFactory`.


## OoBDev.DevTerm.Schemas

Exports a JSON Schema for each hand-written DevTerm format (device manifest, UI definition, connection profile) from the C# model, so a schema cannot drift from the code.

```
SchemaGenerator.Generate() returns the files (a command-line hook is in BACKLOG.md)
```

The committed copies live in `schemas/devterm/`; `SchemaDriftTests` fails when they differ from the generated text.


## OoBDev.DevTerm.Presenters.Text

Presenters that turn received bytes into viewable text, ported from dev-term `DevTerm.Presenters.Text`. Includes `keyvalue` (formerly the KeyValue example plugin): it decodes `name=value [unit]` lines into a `StructuredMessage`.

| Name | Behaviour |
|------|-----------|
| `ascii` | Line-buffered: emits a line on CR, LF or CRLF (one terminator) or at `MaxLineLength` (default 4096, `0` = unbounded). Stateful; implements `IResettablePresenter`. |
| `utf8` | Stateful decoder, so a multi-byte character split across reads decodes once complete. |
| `hex` | Upper-case hex, unspaced. `Parse` accepts spaces and `0x` prefixes. |
| `decimal`, `octal`, `binary` | Space-separated bytes, padded to 1, 3 and 8 digits. |

All implement `IPresenterInput` to turn typed text back into bytes.

```csharp
services.TryAddTextPresenters();
services.Configure<AsciiPresenterOptions>(o => o.MaxLineLength = 1024);
```

Presenters are transient: resolve one per session, since `ascii` and `utf8` hold partial input.


## OoBDev.DevTerm.Transports.Loopback

An in-process `ITransport` that answers scripted commands, so the DevTerm shells and tests run with no hardware. Ported from dev-term's `DevTerm.Transports.Loopback`.

### Description

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

Matching is case-insensitive. The sample counters belong to the transport instance, so each one starts at sample 0. `LoopbackTransportOptions.SampleIntervalMs` (default 0) paces the lines of `Send Events` and `Samples`.

### Usage

```csharp
services.TryAddLoopbackTransport();
```

`ITransport` is registered transient; a closed transport can be reopened.

### Verified

Unit tests cover the scripted commands, pacing, unrecognized input, reopen after close, the options validator and the registration. No hardware is involved.
