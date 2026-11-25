# ser2net-win

Windows-native stand-in for Linux `ser2net`: bridges COM ports to raw TCP or RFC 2217, runs as a console app or a Windows Service, and can advertise bridges over mDNS. It hosts the Aeotec Z-Stick for the Z-Wave Hardware tests.

Commands: `run`, `config` (interactive editor), `list`, `ports`, `install`, `uninstall`, `start`, `stop`, `status`.

`ports` shows each COM port with its USB vendor, product and serial number; a bridge may select its device by those ids ([USB devices and containers](../../../../docs/applications/ser2netwin/usb-devices-and-containers.md)).

Publish a single-file exe with `scripts/ser2netwin/publish.ps1`. The bridge code is the Framework library [OoBDev.System.IO.Ports.Bridge](../../../Framework/OoBDev.System.IO.Ports.Bridge/README.System.IO.Ports.Bridge.md). Usage, settings and mDNS are in [docs/applications/ser2netwin](../../../../docs/applications/ser2netwin/README.md).

## USB devices

The `ports` command lists USBTMC and HID devices next to the COM ports. The raw USB bridge (`UsbBridges`) was removed 2026-11-25 because it had no authentication; a config that still has the section fails at startup. USB devices on another machine are reached through the [remote DevTerm host](../../../../docs/design/RemoteDevTerm/README.md).

