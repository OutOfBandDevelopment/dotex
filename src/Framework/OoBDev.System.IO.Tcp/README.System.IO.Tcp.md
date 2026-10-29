# OoBDev.System.IO.Tcp

A TCP socket as an `IDeviceAdapter`, so a networked instrument or bridge plugs into `StreamDevice<T>` like a serial port or a USB HID device.

## Description

`TcpDeviceAdapter(host, port, connectTimeout)` connects in `TryOpen` and exposes the `NetworkStream`; disposing the stream closes the socket. A refused or timed-out connection returns `false` rather than throwing.

```csharp
var adapter = new TcpDeviceAdapter("192.168.1.50", 5025);
if (adapter.TryOpen(out var stream)) { /* hand the adapter to StreamDevice<ScpiReply> */ }
```

`Rfc2217DeviceAdapter(host, port, baudRate, dataBits, parity, stopBits)` reaches a serial port on a terminal server with RFC 2217: it connects, announces the COM-PORT option and sends the four line-setting subnegotiations (`Handshake()` shows the bytes). `TelnetStream` does the Telnet part: doubles `IAC` on write, drops commands and subnegotiations on read, and answers option requests (only COM-PORT is accepted).

## Notes

- Phase 9 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md); USBTMC, VXI-11 and BLE are not done. RFC 2217 modem-control and flow-control commands and the server's subnegotiation replies are ignored.
- Verified against an in-memory Telnet peer and a loopback listener only, not a real instrument.
