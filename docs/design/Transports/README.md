# Transports

The `ITransport` contract and the transports built on it (serial, TCP, HID, USBTMC, BLE, RFC 2217, VXI-11, MQTT, AMQP, STOMP, loopback). Moved here from the DevTerm design folder on 2026-11-04 as a reusable capability design; the implementation is in `OoBDev.System.IO.*` and `OoBDev.DevTerm.Transports.*`. Application use: [DevTerm](../../applications/devterm/README.md).

| Document | Covers |
|----------|--------|
| [transports.md](transports.md) | The contract, lifecycle, discovery and the transport list |
| [rfc2217.md](rfc2217.md) | RFC 2217 client and server modes |
| [vxi11-transport.md](vxi11-transport.md) | VXI-11 core channel client for LXI instruments |
| [usbtmc-transport.md](usbtmc-transport.md) | Raw USB Test and Measurement Class transport; protocol reference in [docs/protocols](../../protocols/usbtmc-protocol-conformance.md) |

Test peers (ser2net, Mosquitto, RabbitMQ, loopback device) run in the shared stack: [integration services](../../architecture/testing/integration/services.md#devterm-peers-mosquitto-ser2net-loopback-device).
