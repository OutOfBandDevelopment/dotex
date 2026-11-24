# OoBDev.System.IO.Usb.Bridge

Shares one USBTMC instrument or HID device on one TCP port with a small framed protocol, so a client on another machine or in a container can use it as if it were local. It is not USB/IP: the frames carry the calls of `IUsbTmcDevice` and `IUsbHidDevice`, so only dotex clients talk to it.

Design and frame format: [usb-bridge.md](../../docs/design/Ser2NetWin/usb-bridge.md).

## Contents

- `UsbFrameCodec` reads and writes frames (`u32 length | u8 op | u32 id | payload`, big-endian, 16 MiB limit).
- `UsbBridgeServer` listens on `UsbBridgeOptions.TcpPort`, serves one client at a time (a second one is refused unless `ReplaceExistingClient`) and forwards requests to the device made by `IUsbTmcDeviceFactory` or `IUsbHidDeviceFactory`.
- `UsbBridgeOptions` selects the device by vendor, product and optional serial number.

## Use

```csharp
var server = new UsbBridgeServer(new UsbBridgeOptions { Kind = UsbBridgeKind.UsbTmc, UsbVendorId = "1AB1", UsbProductId = "09C4", TcpPort = 4100 }, tmcFactory, null, logger);
server.Start();
await server.RunAsync(cancellationToken);
```
