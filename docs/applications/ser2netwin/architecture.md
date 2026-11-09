# ser2net-win architecture

## Contents

- [Parts](#parts)
- [Client policy](#client-policy)

## Parts

- **Library** (`OoBDev.System.IO.Ports.Bridge`): `BridgeWorker` pumps one serial port to one TCP listener; `Rfc2217Session` implements the RFC 2217 server side; `BridgeHostedService` runs all configured bridges; `ConfigStore` and `ConfigValidator` load, save and check the JSON configuration; `MdnsAdvertiser` advertises bridges over multicast DNS. The serial port sits behind `ISerialLink`, `ISerialControl` and `ISerialPortFactory`, so tests use in-memory devices.
- **Application** (`OoBDev.Ser2NetWin.Cli`): `run`, `config` (Spectre.Console editor), `list`, `ports`, `install`, `uninstall`, `start`, `stop`, `status`; `ServiceInstaller` drives `sc.exe`; the host runs as a console app or a Windows Service.

## Client policy

A COM port serves one client at a time. This came from the Z-Stick bug, where a second client saw a successful connect, an idle line and then a reset, with the reason only in the service log (reported 2026-11-04, fixed in 0.0.4, [report](bugs/single-client-silent-reject.md)):

- The rejected client gets a text line first (`ser2net: port busy, held by <address>`), then the connection closes.
- Accepted sockets use TCP keep-alive and a write timeout, so a client that vanished without a FIN frees the port.
- A short reconnect grace avoids refusing a client that reconnects at once.
- `ReplaceExistingClient` hands the port to the newest client, for bench use.

Tests: `ClientConnectionTests` and `BridgeWorkerTests` with a fake serial link and two clients.
