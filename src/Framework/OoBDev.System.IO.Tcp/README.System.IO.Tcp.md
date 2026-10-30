# OoBDev.System.IO.Tcp

A TCP socket as an `IDeviceAdapter`, so a networked instrument or bridge plugs into `StreamDevice<T>` like a serial port or a USB HID device.

## Description

`TcpDeviceAdapter(host, port, connectTimeout)` derives from `StreamDeviceAdapter` (`OoBDev.System.IO.Pipelines`): `OpenAsync` connects and starts the pump, `Input` and `Output` are pipes, and `CloseAsync` closes the socket. A refused or timed-out connection moves the adapter to `Faulted` and throws `IOException`.

```csharp
var adapter = new TcpDeviceAdapter("192.168.1.50", 5025);
await adapter.OpenAsync(); // then hand the adapter to StreamDevice<ScpiReply>, or use adapter.Input and adapter.Output directly
```

`Rfc2217DeviceAdapter(host, port, baudRate, dataBits, parity, stopBits)` reaches a serial port on a terminal server with RFC 2217: it connects, announces the COM-PORT option and sends the four line-setting subnegotiations (`Handshake()` shows the bytes). `TelnetStream` does the Telnet part: doubles `IAC` on write, drops commands and subnegotiations on read, and answers option requests (only COM-PORT is accepted).


## Configured adapters

These take options and register with DI (`ServiceCollectionExtensions`); each registers `IDeviceAdapter` and reuses `ITcpConnectionSource` (`TryAdd`).

- `TcpEndpointDeviceAdapter` (`AddTcpEndpointDeviceAdapter()`, `TcpEndpointOptions`, validated by `TcpEndpointOptionsValidator`): raw or Telnet TCP for instruments and serial-to-Ethernet bridges, with optional XON/XOFF flow control. `LxiDiscovery` finds LXI devices.
- `Rfc2217ClientDeviceAdapter` (`AddRfc2217ClientDeviceAdapter()`, `Rfc2217ClientOptions`): a serial port on a terminal server such as ser2net, with the full RFC 2217 codec (modem and line state, flow control, purge). Implements `IComPortControl` and `ILineStatusSource`.
- `Vxi11DeviceAdapter` (`AddVxi11DeviceAdapter()`, `Vxi11DeviceAdapterOptions`): VXI-11 (ONC-RPC over TCP) instruments, tested against a loopback RPC server.

The RFC 2217 server bridge (`Rfc2217ServerBridge`) shares a session and lives in `OoBDev.DevTerm.Core` until the Applications move; this assembly grants it `InternalsVisibleTo` for the codec.

Verified against in-memory peers and loopback listeners; the real-hardware VXI-11 and ser2net tests end `Assert.Inconclusive` when the device is absent.
