# ser2net-win

Windows-native stand-in for Linux `ser2net`: bridges COM ports to raw TCP or RFC 2217, runs as a console app or a Windows Service, and can advertise bridges over mDNS. It hosts the Aeotec Z-Stick for the Z-Wave Hardware tests.

Commands: `run`, `config` (interactive editor), `list`, `ports`, `install`, `uninstall`, `start`, `stop`, `status`.

`ports` shows each COM port with its USB vendor, product and serial number; a bridge may select its device by those ids ([USB devices and containers](../../../../docs/applications/ser2netwin/usb-devices-and-containers.md)).

Publish a single-file exe with `scripts/ser2netwin/publish.ps1`. The bridge code is the Framework library [OoBDev.System.IO.Ports.Bridge](../../../Framework/OoBDev.System.IO.Ports.Bridge/README.System.IO.Ports.Bridge.md). Usage, settings and mDNS are in [docs/applications/ser2netwin](../../../../docs/applications/ser2netwin/README.md).

## USB bridges

An optional `UsbBridges` array in the config file exposes USBTMC and HID devices as framed TCP streams (one port per device, see the [design](../../../../docs/design/Ser2NetWin/usb-bridge.md)). The `ports` command lists USBTMC and HID devices next to the COM ports.

```json
"UsbBridges": [ { "Kind": "UsbTmc", "UsbVendorId": "1AB1", "UsbProductId": "0588", "TcpPort": 4100 } ]
```
