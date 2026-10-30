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

## Step 5b: USBTMC moves into `System.IO.UsbTmc`

`DevTerm.Transports.Usbtmc` is now `UsbTmcDeviceAdapter` (`UsbTmcDeviceAdapterOptions`, `AddUsbTmcDeviceAdapter()`) in the new `OoBDev.System.IO.UsbTmc`: an `IDeviceAdapter` (type `UsbTmc`) with `Output` built by `DeviceAdapterOutput`; the codec, device location, quirks, discovery and the LibUsbDotNet device are ported with one type per file. `LibUsbDotNet` 3.0.224 and `libusb-dynamic` 1.0.26 were added to `Directory.Packages.props`. 43 Unit tests moved to `OoBDev.System.IO.UsbTmc.Tests` and pass; no instrument was attached.

## Step 5c: BLE moves into `System.IO.Bluetooth`

`DevTerm.Transports.Ble` is now `BleDeviceAdapter` (`BleDeviceAdapterOptions`, `AddBleDeviceAdapter()`) in `OoBDev.System.IO.Bluetooth`, an `IDeviceAdapter` of type `Ble`. The per-OS seam `IBleAdapter` is renamed `IBleConnection` (and its factory `IBleConnectionFactory`) so it is not confused with `IDeviceAdapter`. `DevTerm.Transports.Ble.Windows` is `OoBDev.System.IO.Bluetooth.Windows` (`WindowsBleConnection`, `AddWindowsBleConnection()`). The multi-type files are split, one type per file. 19 Unit tests moved to `OoBDev.System.IO.Bluetooth.Tests` and pass; the Windows backend only builds, and no peripheral was attached.

## Step 5d: MQTT moves into `System.IO.Mqtt`

`DevTerm.Transports.Mqtt` is now `MqttDeviceAdapter` (`MqttDeviceAdapterOptions`, `AddMqttDeviceAdapter()`) in the new `OoBDev.System.IO.Mqtt`: an `IDeviceAdapter` of type `Mqtt` (path `Host:Port`) over MQTTnet 5.2.0.1603 (added to `Directory.Packages.props`). The `IMqttConnection` seam and the MQTTnet connection and factory are ported with one type per file. `MqttRouterBridge` depends on the DevTerm `MessageRouter`, so it lives in `OoBDev.DevTerm.Core` (`Brokers/`, namespace `OoBDev.DevTerm.Brokers`) and Core references the new library. 10 Unit tests in `OoBDev.System.IO.Mqtt.Tests` and the bridge tests in `Core.Tests/Brokers` pass; the Mosquitto test is `DevLocal` and was not run (no broker).
