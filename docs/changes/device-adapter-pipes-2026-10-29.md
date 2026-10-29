# Pipe-based device adapter (2026-10-29)

Step 1 of the [DevTerm restructure](../design/DevTermParity/restructure-plan.md#adapter-contract-step-1).

## What changed

- `IDeviceAdapter` now exposes `PipeReader Input`, `PipeWriter Output`, `State`, `StateChanged`, `OpenAsync`, `CloseAsync` and `IAsyncDisposable`. `TryOpen` and `Stream` are removed (breaking, unreleased).
- `ConnectionState` and `ConnectionStateChangedEventArgs` moved from `OoBDev.DevTerm.Core` to `OoBDev.System.Abstractions` (`OoBDev.System.IO`).
- `OoBDev.System.IO.Pipelines` gains `StreamToPipePump` (moved from DevTerm Core), the abstract `StreamDeviceAdapter` and `AsStream(IDeviceAdapter)`.
- `SerialPortDeviceAdapter`, `TcpDeviceAdapter`, `Rfc2217DeviceAdapter` and `UsbHidDeviceAdapter` derive from `StreamDeviceAdapter`. A failed open now throws `IOException` and leaves the adapter `Faulted`.
- `StreamDevice<T>` reads `Input` and writes `Output`; the adapter must be opened first. `DevicePreflight.RequireUsbHid` and `ScpiTcpExecutor` use the new API.
- `StreamTransport` and its test are deleted; `ITransport` stays until step 2.

## Verified

Unit tests: `OoBDev.System.Tests` 1872 passed (165 skipped), `System.IO.Tcp.Tests` 9, `DevTerm.Core.Tests` 211, Serial 61, Tcp 31, Rfc2217 44, Configuration 381; whole solution builds. Hardware tests were not run; the Rfc2217 adapter has no open-path test yet (backlog).
