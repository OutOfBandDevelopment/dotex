# OoBDev.ZWave

Z-Wave Serial API framing, decoding and request building (the protocol, not one stick).

## Description

`ZWaveDevice` implements `IDeviceDefinitionReceiver<ZWaveFrame>` and `IDeviceDefinitionTransmitter<ZWaveFrame>` on the existing `OoBDev.System.Abstractions` IO model, at 115200 baud 8N1. Bytes arrive through the existing adapters, so the package never opens a port.

| Type | Role |
|------|------|
| `ZWaveSegmenter` | Cuts the stream into units: a lone ACK, NAK or CAN, a whole `01 len ...` frame, or one stray byte (via `Segment.Using`) |
| `ZWaveMessageDecoder` | Classifies a unit as `Ack`, `Nak`, `Can`, `Data` (checksum valid) or `Invalid` |
| `ZWaveMessageEncoder` | Writes a frame's raw bytes |
| `ZWaveFrame` | Wire bytes plus `Type`, `Function`, `Payload`; `Create` and `Checksum` (`0xFF` XOR len..payload) |
| `ZWaveRequests` | Builders: GetVersion, MemoryGetId, GetInitData, GetNodeProtocolInfo, SendData, BasicGet, SwitchSet/Get, LevelSet |
| `ZWaveFunctions` | Serial API function ids |

The consumer must send `ZWaveFrame.Ack` for every received data frame, or the stick retransmits and answers CAN.

## Not yet ported

Interpreter, node table, manufacturer/command-class id tables, control surface, and an automatic ACK responder. Network-management commands (include, exclude, reset) are not exercised on hardware.

## Verified

- Unit tests use byte-exact frames captured from an Aeotec Z-Stick S2 by dev-term.
- Hardware test `ZWaveHardwareTests` (run parameter `ZWAVE_SERIAL_PORT`) sent GetVersion to a Z-Stick S2 on COM13 and got "Z-Wave 2.78" with no CAN or NAK. It ends Inconclusive when the port is absent or busy.

Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).
