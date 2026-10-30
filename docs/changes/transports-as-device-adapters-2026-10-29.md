# Transports become device adapters (2026-10-29)

Step 2a of the [DevTerm restructure](../design/DevTermParity/restructure-plan.md). Step 2b (moving the code into `System.IO.Ports` and `System.IO.Tcp`) follows.

## What changed

- `ITransport` is deleted. `Session`, `ISessionFactory`, `SessionFactory`, `DevTermSessionBuilder` and the plugin docs use `IDeviceAdapter` (breaking, unreleased).
- `SerialTransport`, `TcpTransport`, `Rfc2217Transport`, `Vxi11Transport` and `LoopbackTransport` implement `IDeviceAdapter`: `Type` (`Serial`, `Tcp`, `Rfc2217`, `Vxi11`, `Loopback`), `Path` (port name, `host:port`, `host:port/device`, `loopback`) and `Output`.
- `Output` is built with `DeviceAdapterOutput.Create(write)` (new, `OoBDev.System.IO.Pipelines`): a `PipeWriter` whose flush calls the transport's own `WriteAsync`, so write timeouts, XON/XOFF gating and per-byte delay are unchanged.
- `WriteAsync(this IDeviceAdapter, ...)` writes to `Output` and flushes; `Session.SendAsync` uses it.
- DI registers each transport as `IDeviceAdapter`.
- Tests: `Mock<IDeviceAdapter>` sites set `Output` through `DeviceAdapterOutput.Create`; the three `FakeTransport` classes implement the new members; `DeviceAdapterOutputTests` covers the new helper.

## Verified

Whole `OoBDev.slnx` builds. Unit tests pass: DevTerm Core 211 (5 skipped), Logging 78, Configuration 381, DeviceManifests 168, Serial 61, Tcp 31, Rfc2217 44, Vxi11 6, Loopback 18, `DeviceAdapterOutputTests` 3. Hardware tests were not run.
