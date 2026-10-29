# OoBDev.DevTerm.Core

The engine contracts of the DevTerm tool, ported from dev-term (see [the parity matrix](../../../docs/design/DevTermParity/README.md) and [architecture](../../../docs/design/DevTermParity/architecture.md)).

## Description

| Type | Role |
|------|------|
| `ITransport` | Asynchronous, full-duplex connection with a lifecycle, a state event and a `PipeReader` |
| `ConnectionState`, `ConnectionStateChangedEventArgs` | Lifecycle states and the change event |
| `IPresenter`, `IPresenterInput`, `IResettablePresenter`, `IStructuredMessageSource`, `IAsyncOutputSource`, `IOriginatingPresenter`, `IContentCapturePresenter` | Presenter contracts: render bytes to text lines, parse typed input, reset on reopen, emit structured or asynchronous output |
| `Pipeline`, `PresenterOutput`, `StructuredMessage` | Fans received bytes out to the presenters; content-capturing presenters take a chunk first |
| `Session`, `ISessionFactory`, `SessionLimits`, `ISessionObserver`, `DevTermTelemetry` | Binds a transport to a pipeline: read loop, send throttle, connect retry and timeout, query with timeout, self-reported disconnect, observers, `DevTerm` activity source and meter |
| `StreamToPipePump` | Copies a stream into a pipe, reading into the pipe's pooled buffers |
| `StreamTransport` | An `ITransport` over any `IDeviceAdapter`, so the serial, HID and TCP adapters serve both contracts |

`IDeviceAdapter` stays the contract of the device libraries; this package adds `ITransport` beside it (default of open question Q1). Routing, plugins, logging, the control servers and stream content follow in the next slices.

## Verified

Unit tests cover the stream pump, `StreamTransport`, `Session` (lifecycle, faults and reconnect, limits, observers, telemetry) and `Pipeline`, with in-memory pipes and mocks. Nothing was run against a device.

## Not yet verified on hardware

Serial, HID and TCP through `StreamTransport` have not been tried with real devices.
