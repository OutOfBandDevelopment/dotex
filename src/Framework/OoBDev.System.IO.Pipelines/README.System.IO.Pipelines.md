# OoBDev.System.IO.Pipelines

Builds on `System.IO.Pipelines` to connect devices and streams: a fluent builder that pumps a stream through a segmenter, a stream-backed `IDeviceAdapter` base class, a message-level `StreamDevice<TMessage>`, and small stream helpers. The contracts (`IDeviceAdapter`, `ISegmenter`, `PipelineErrorHandler`, `ErrorHandling`, `IDeviceDefinition`) are in `OoBDev.System.Abstractions`.

## Contents

- [Pipeline builder](#pipeline-builder)
- [Device adapters](#device-adapters)
- [Message devices](#message-devices)
- [Stream helpers](#stream-helpers)
- [Tests](#tests)

## Pipeline builder

```csharp
await inputStream
    .Follow()                                    // pump the stream into a pipe (4096-byte minimum buffer)
    .With(segmenter)                             // reader side: split the bytes into segments
    .OnError((sender, ex) => Task.FromResult(ErrorHandling.Throw))
    .RunAsync(cancellationToken);
```

**Table 1 — Builder calls (`PipelineBuilder`)**

| Call | Meaning |
|------|---------|
| `Follow(stream, minimumBufferSize)` | Starts a definition whose writer side copies the stream into a `Pipe`. A second writer throws `NotSupportedException`. |
| `With(ISegmenter)` | Reader side: hands the pipe to the segmenter. |
| `With(Stream)` | Reader side: copies the pipe into a stream. |
| `OnError`, `OnReaderError`, `OnWriterError` | A `PipelineErrorHandler` returning `ErrorHandling` (`Ignore`, `Throw` or `Stop`). Without one the error is written to the debug output and rethrown. |
| `RunAsync(cancellationToken)` | Runs the writer and reader tasks together; throws `NotSupportedException` when either side is missing. |

The handlers and the reader must be set before `With(...)` fixes the reader; a later call throws `NotSupportedException`.

## Device adapters

**Table 2 — Adapter types**

| Type | Role |
|------|------|
| `StreamDeviceAdapter` | Abstract `IDeviceAdapter` for devices that are a `Stream`. It owns the connection state machine (`Closed`, `Opening`, `Open`, `Faulted`, `StateChanged`), pumps the stream into `Input` and sends `Output` straight to the stream. A subclass implements `Type`, `Path` and `ConnectAsync` (return `null` to fail the open, which throws `IOException`) and may override `Disconnect`. |
| `DeviceAdapterOutput.Create(write)` | A `PipeWriter` whose flushes call a delegate, for adapters whose writes need their own timeouts, flow control or framing. |
| `DeviceAdapterExtensions.AsStream()` | Wraps an open adapter as one duplex `Stream`; disposing it disposes the adapter. |
| `DeviceAdapterExtensions.WriteAsync(data)` | Writes bytes to `Output` and flushes. |

## Message devices

`StreamDevice<TMessage>` runs a device definition over an opened adapter: it initialises the device (`IDeviceDefinitionInitialize`), decodes received segments into messages (`IDeviceDefinitionReceiver<TMessage>`) and encodes queued messages for sending (`IDeviceDefinitionTransmitter<TMessage>`), pausing at least `minimumTransmissionDelay` milliseconds (the explicit argument, else the definition's `IDeviceDefinitionTransmissionDelay`, else 1000) between transmissions. It raises `MessageReceived`, `DeviceStatus` (initializing, receiving, transmitting and so on) and the two error events, `Transmit(message)` queues a message, `Runner` is the running task, and `Dispose` stops it and the adapter.

## Stream helpers

- `StreamToPipePump.RunAsync(source, writer, token)` copies a stream into a `PipeWriter` until the source ends, the token is cancelled or a read fails, then completes the writer (with the failure, if any). The source's `ReadAsync` must honour cancellation, so the serial port base stream has to be wrapped first.
- `WriteDelayStream(inner, delayMilliseconds)` is a write-only decorator that writes and flushes one byte at a time, waiting the delay between bytes (none after the last), for slow devices without a FIFO. Zero still flushes every byte; a negative value is rejected, so a caller that wants no pacing does not wrap the stream.

## Tests

Covered in `OoBDev.System.Tests/IO`: `StreamDeviceTests`, `StreamDeviceAdapterCancellationTests`, `StreamToPipePumpTests`, `DeviceAdapterOutputTests`, `WriteDelayStreamTests` and `SegmenterTests`.
