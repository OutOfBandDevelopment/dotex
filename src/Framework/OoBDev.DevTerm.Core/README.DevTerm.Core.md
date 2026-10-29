# OoBDev.DevTerm.Core

The engine contracts of the DevTerm tool, ported from dev-term (see [the parity matrix](../../../docs/design/DevTermParity/README.md) and [architecture](../../../docs/design/DevTermParity/architecture.md)).

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
