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

## Step 2b-i: shared contracts move out of DevTerm Core

`IComPortControl`, `ILineStatusSource`, `ComParity` and `ComStopBits` moved to `OoBDev.System.Abstractions` (`OoBDev.System.IO`); `WriteDelayStream` and its tests moved to `OoBDev.System.IO.Pipelines` / `OoBDev.System.Tests`. The serial and TCP libraries can now take the transport code without referencing DevTerm. Tests after the move: Core 207, System 1879, Configuration 381, Serial 61, Rfc2217 44 (Unit, all passing).

## Step 2b-ii: serial moves into `System.IO.Ports`

`OoBDev.DevTerm.Transports.Serial` and its tests are gone. `SerialTransport` is now `SerialDeviceAdapter` (options `SerialDeviceAdapterOptions`, DI `AddSerialDeviceAdapter()`) in `OoBDev.System.IO.Ports`, beside the thin `SerialPortDeviceAdapter(SerialPort)`; the port abstraction, discovery, per-OS descriptions and read/write streams moved with it. The tests live in the new `OoBDev.System.IO.Ports.Tests` (61 Unit tests pass, hardware tests not run). `Parity`, `StopBits` and `Handshake` are written as `global::System.IO.Ports.*` because `OoBDev.System.Abstractions` defines same-named enums in the same namespace.

## Step 2b-iii: TCP, RFC 2217 and VXI-11 move into `System.IO.Tcp`

The six `OoBDev.DevTerm.Transports.{Tcp,Rfc2217,Vxi11}` projects are gone. `TcpTransport` is `TcpEndpointDeviceAdapter` (`TcpEndpointOptions`, `AddTcpEndpointDeviceAdapter()`), `Rfc2217Transport` is `Rfc2217ClientDeviceAdapter` (`Rfc2217ClientOptions`, `AddRfc2217ClientDeviceAdapter()`) and `Vxi11Transport` is `Vxi11DeviceAdapter` (`Vxi11DeviceAdapterOptions`, `AddVxi11DeviceAdapter()`), all in `OoBDev.System.IO.Tcp` with one `ServiceCollectionExtensions`. The internal `ComPortParity` merged into the public one (gained `Request = 0`). `Rfc2217ServerBridge` depends on `Session`, so it moved to `OoBDev.DevTerm.Core` (with its test) and reads the codec through `InternalsVisibleTo\

## Step 3: DevTerm Core moves to `src/Applications`

`OoBDev.DevTerm.Core` now holds Logging, DeviceManifests, Configuration, Schemas (`SchemaGenerator`), Presenters.Text and Loopback as folders (`OoBDev.DevTerm.Loopback` replaces `OoBDev.DevTerm.Transports.Loopback`); the old libraries and test projects are deleted and one `OoBDev.DevTerm.Core.Tests` carries all their tests (892 Unit tests passed, 5 skipped; hardware tests not run). The plugin assemblies (`Plugins.KeyValue`, `.KeyValue.Tests`, `Plugins.Sample`) moved to `src/Applications` as separate projects because they are loaded from a plugin folder. The libraries' readmes are appended to `README.DevTerm.Core.md`. `OoBDev.slnx` is updated; the whole solution builds with 0 errors.

## Step 5a: HID moves into `System.IO.UsbHids`

`DevTerm.Transports.Hid` is now `UsbHidEndpointDeviceAdapter` (`UsbHidEndpointOptions`, `AddUsbHidEndpointDeviceAdapter()`) in `OoBDev.System.IO.UsbHids`, with `IUsbHidDevice`, `IUsbHidDeviceFactory`, `SystemUsbHidDevice`, `UsbHidReadStream` and `IUsbHidDiscovery` (one type per file). The 11 adapter tests moved to the new `OoBDev.System.IO.UsbHids.Tests` (11 Unit tests passing; real-hardware behavior not re-verified). It sits beside the thin `UsbHidDeviceAdapter` and `UsbHidFactory`; merging them is in BACKLOG.md.
