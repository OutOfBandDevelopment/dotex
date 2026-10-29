# OoBDev.System.IO.Tcp

A TCP socket as an `IDeviceAdapter`, so a networked instrument or bridge plugs into `StreamDevice<T>` like a serial port or a USB HID device.

## Description

`TcpDeviceAdapter(host, port, connectTimeout)` derives from `StreamDeviceAdapter` (`OoBDev.System.IO.Pipelines`): `OpenAsync` connects and starts the pump, `Input` and `Output` are pipes, and `CloseAsync` closes the socket. A refused or timed-out connection moves the adapter to `Faulted` and throws `IOException`.

```csharp
var adapter = new TcpDeviceAdapter("192.168.1.50", 5025);
await adapter.OpenAsync(); // then hand the adapter to StreamDevice<ScpiReply>, or use adapter.Input and adapter.Output directly
```

`Rfc2217DeviceAdapter(host, port, baudRate, dataBits, parity, stopBits)` reaches a serial port on a terminal server with RFC 2217: it connects, announces the COM-PORT option and sends the four line-setting subnegotiations (`Handshake()` shows the bytes). `TelnetStream` does the Telnet part: doubles `IAC` on write, drops commands and subnegotiations on read, and answers option requests (only COM-PORT is accepted).

## Notes

- Phase 9 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md); USBTMC, VXI-11 and BLE are not done. RFC 2217 modem-control and flow-control commands and the server's subnegotiation replies are ignored.
- Verified against an in-memory Telnet peer and a loopback listener only, not a real instrument.
