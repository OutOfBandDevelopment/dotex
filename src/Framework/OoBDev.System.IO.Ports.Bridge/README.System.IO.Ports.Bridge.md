# OoBDev.System.IO.Ports.Bridge

Bridges serial (COM) ports to TCP: a raw socket per port or an RFC 2217 server, with optional mDNS advertisement.

## What it holds

- `BridgeWorker`: pumps one serial port to one TCP listener. One client at a time; a second client is told why it was rejected (`ser2net: port busy, held by ...`), connections use keep-alive and a write timeout, a short reconnect grace avoids refusing an immediate reconnect, and `ReplaceExistingClient` hands the port to the newest client.
- `Rfc2217Session`, `Rfc2217Codes`: RFC 2217 (Telnet COM port control) server side: baud, data bits, parity, stop bits, DTR/RTS and modem state.
- `BridgeHostedService`: runs every configured bridge as a hosted service (console or Windows Service).
- `ConfigStore`, `ConfigValidator`, `Ser2NetConfig`, `BridgeOptions`, `MdnsOptions`: JSON configuration model, load and save, validation.
- `MdnsAdvertiser`, `IServiceAdvertiser`: advertises each bridge as `_ser2net._tcp` (and extra service types) over multicast DNS.
- `UsbPortResolver`, `IUsbPortLister`, `RegistryUsbPortLister`, `UsbPortInfo`: a bridge can set `UsbVendorId`, `UsbProductId` (and `UsbSerialNumber`) instead of `ComPort`; the port is resolved on every open, so a replug on another COM number reconnects. Two matches without a serial number refuse and name both ports. Ids come from the Windows registry.
- `ISerialLink`, `ISerialControl`, `ISerialPortFactory`, `SystemSerialLink`: the serial seam; tests use in-memory fakes.

## Used by

The `ser2net-win` application ([README.Ser2NetWin.Cli.md](../../Applications/Ser2NetWin/OoBDev.Ser2NetWin.Cli/README.Ser2NetWin.Cli.md)); documentation in [docs/applications/ser2netwin](../../../docs/applications/ser2netwin/README.md).

## Tests

`OoBDev.System.IO.Ports.Bridge.Tests`: `Unit` (config, validator, RFC 2217 session, mDNS plan), `Simulate` (bridge over loopback TCP with fake serial devices) and `Hardware` (a real COM port; ends inconclusive when the port is absent or has no TX-RX loopback).
