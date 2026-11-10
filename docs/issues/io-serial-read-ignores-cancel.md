# Serial read ignores cancellation and can leak the COM port

- **Status:** open, found by code reading on 2026-11-10; not reproduced in this repository
- **Feature document:** [DeviceDecoding](../design/DeviceDecoding/README.md)
- **Backlog line:** `BACKLOG.md`, section 5 (Code quality backlog)
- **Origin:** imported BinaryDataDecoders bug BUG-002 (reproduced there live against a Radex One on COM8, September 2026)

## Problem

`StreamPipelineFactory` reads with `owner.ReadAsync(memory, token)` on `SerialPort.BaseStream`. That read does not observe the token while no bytes arrive, and `SerialPortAttribute.ReadTimeout` defaults to `-1` (infinite). After cancel, the receive loop in `StreamDevice` never unwinds, `Runner` never completes, a caller that awaits it never returns, and the COM port stays open. A second run then fails with `UnauthorizedAccessException` on the port (that part is handled now: `SerialPortDeviceAdapter.ConnectAsync` returns `null`).

`StreamDevice.Dispose()` cancels and then disposes the adapter, which closes the port and should end the read, so the hang needs a caller that waits on `Runner` before disposing. DevTerm uses `ReadTimeoutMs = 1000` and probably does not hit it.

## Options

1. Register the cancellation token to close the `SerialPort` or stream, so the pending read faults and the existing error path handles it.
2. Give the port a finite default `ReadTimeout` so the loop re-checks the token.
3. Wait on `Runner` with a grace timeout in callers.

Option 1 is the most direct. Whichever is chosen needs a test on a virtual serial pair or a `Hardware` test, and a check that `SerialStream.ReadAsync` behaviour matches on the target OS.

## Done when

A cancelled device session completes within a bounded time, the port is closed, and a test proves it. Then move the detail into the DeviceDecoding design and delete this file.
