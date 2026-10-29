# OoBDev.DevTerm.Core

The engine contracts of the DevTerm tool, ported from dev-term (see [the parity matrix](../../../docs/design/DevTermParity/README.md) and [architecture](../../../docs/design/DevTermParity/architecture.md)).

## Description

| Type | Role |
|------|------|
| `ITransport` | Asynchronous, full-duplex connection with a lifecycle, a state event and a `PipeReader` |
| `ConnectionState`, `ConnectionStateChangedEventArgs` | Lifecycle states and the change event |
| `IPresenter` | Turns received bytes into text lines |
| `StreamToPipePump` | Copies a stream into a pipe, reading into the pipe's pooled buffers |
| `StreamTransport` | An `ITransport` over any `IDeviceAdapter`, so the serial, HID and TCP adapters serve both contracts |

`IDeviceAdapter` stays the contract of the device libraries; this package adds `ITransport` beside it (default of open question Q1). Sessions, routing, loopback, presenters and logging follow in the next slices.

## Verified

Unit tests cover the stream pump (data, end of stream, read failure, cancellation) and `StreamTransport` (open, read, write, close, reopen, failed open, write before open) with in-memory streams. Nothing was run against a device.

## Not yet verified on hardware

Serial, HID and TCP through `StreamTransport` have not been tried with real devices.
