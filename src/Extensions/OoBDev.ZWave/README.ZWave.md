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
| `ZWaveRequests` | Builders: GetVersion, MemoryGetId, GetInitData, GetNodeProtocolInfo, SendData, BasicGet, SwitchSet/Get, LevelSet, AddNode, RemoveNode, SetDefault, SetLearnMode, NeighborUpdate, IsFailedNode, RemoveFailedNode, RequestNodeInfo, SoftReset |
| `ZWaveInterpreter` / `ZWaveReading` | Frame to readable text plus named values; hex, node bitmap and command helpers |
| `ZWaveNodeTable` / `ZWaveNodeRecord` | Per-node facts built from frames; `Expect(function, node)` attributes replies that do not name their node |
| `ZWaveNames` | Function, command class, device class, library type, manufacturer and product names; the manufacturer, product, command class and device class tables are embedded JSON (`Data/`), with hex as the fallback |
| `ZWaveConfigParameter` | One manufacturer-documented configuration parameter (command class 0x70: index, label, size, range, default, help, named values); `ZWaveNames.ConfigParameters(manufacturer, type, id)` reads them from the embedded `product-configs.json` (1,237 product ids over 830 configuration files; empty for an unknown product) |
| `ZWaveReports` | Typed readers for reports (Binary Switch) |
| `ServiceCollectionExtensions` | `TryAddZWave()` registers the decoder and encoder |
| `ZWaveFunctions` | Serial API function ids |

The consumer must answer every received data frame, or the stick retransmits and answers CAN: `ZWaveAckResponder.Respond(frame)` returns `ZWaveFrame.Ack` for a checksum-valid data frame, `ZWaveFrame.Nak` for a frame with a bad checksum, and `null` for everything else.

## Not yet ported

The control surface (waits on owner question Q3), and a session-level responder that writes the answers itself (the framework has no session observer; `ZWaveAckResponder.Respond` picks the answer and the consumer writes it, as `ZWaveStick` in the tests does). Network-management commands (include, exclude, reset) are not exercised on hardware.

## Verified

- Unit tests use byte-exact frames captured from an Aeotec Z-Stick S2 by dev-term.
- Hardware test `ZWaveHardwareTests` (run parameter `ZWAVE_SERIAL_PORT`) sent GetVersion to a Z-Stick S2 on COM13 and got "Z-Wave 2.78" with no CAN or NAK. It ends Inconclusive when the port is absent or busy.

Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

## Device documentation

[docs/aeotec-zstick-s2/known-configuration.md](docs/aeotec-zstick-s2/known-configuration.md) records the Aeotec Z-Stick S2 bench setup (COM port, framing, what was verified). Paths inside it such as `docs/test/...` name files in the dev-term repository.
