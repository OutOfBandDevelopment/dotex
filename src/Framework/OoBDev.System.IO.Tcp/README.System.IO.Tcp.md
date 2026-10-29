# OoBDev.System.IO.Tcp

A TCP socket as an `IDeviceAdapter`, so a networked instrument or bridge plugs into `StreamDevice<T>` like a serial port or a USB HID device.

## Description

`TcpDeviceAdapter(host, port, connectTimeout)` connects in `TryOpen` and exposes the `NetworkStream`; disposing the stream closes the socket. A refused or timed-out connection returns `false` rather than throwing.

```csharp
var adapter = new TcpDeviceAdapter("192.168.1.50", 5025);
if (adapter.TryOpen(out var stream)) { /* hand the adapter to StreamDevice<ScpiReply> */ }
```

## Notes

- Phase 9 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md); USBTMC, VXI-11, RFC 2217 and BLE are not done.
- Verified against an in-process loopback listener only, not a real instrument.
