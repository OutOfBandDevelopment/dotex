# LXI discovery scan test

**Date:** 2026-11-04

`LxiDiscovery.ScanAsync` takes optional `target`, `localAddresses` and `scpiPorts` (defaults unchanged: UDP broadcast to port 111, every up IPv4 interface, ports 5025 and 5555). `ScanAsync_AgainstALoopbackResponder_FindsTheDeviceAndItsIdentity` runs the whole scan against a fake portmapper responder and a fake raw-SCPI listener on loopback, so it needs no broadcast and no hardware. Closes the "LxiDeviceScanner socket scan" test gap in [status](../design/DevTermParity/status.md).

Verified: `OoBDev.System.IO.Tcp.Tests` Unit run, 94 passed.
