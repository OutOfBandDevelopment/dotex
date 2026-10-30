# RFC 2217 adapter open-path tests (2026-10-29)

`Rfc2217DeviceAdapterTests` (3 Unit tests, loopback `TcpListener`): the handshake bytes reach the server and data bytes come back through `Input`, a refused connection ends `Faulted` with an `IOException`, and `CloseAsync` returns the adapter to `Closed`. This also exercises the open, fault and close paths of `StreamDeviceAdapter`.

Verified: the 3 tests pass. Not done: a handshake server that answers the COM-PORT negotiation (the adapter does not wait for answers).
