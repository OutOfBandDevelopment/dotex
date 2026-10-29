# Z-Wave core (OoBDev.ZWave) - 2026-10-28

First real device package of the device decoding migration, done before DE-5000 because the Aeotec Z-Stick S2 is the plugged-in device.

## What changed

- `OoBDev.ZWave`: `ZWaveDevice` (115200 8N1), `ZWaveSegmenter`, `ZWaveMessageDecoder`, `ZWaveMessageEncoder`, `ZWaveFrame` (checksum `0xFF` XOR len..payload), `ZWaveRequests`, `ZWaveFunctions`.
- `OoBDev.System.Abstractions`: `Segment.Using(factory)` lets a device plug in its own `ISegmenter` (Z-Wave must keep lone ACK/NAK/CAN bytes, which the builder drops).
- `OoBDev.Devices.TestUtilities`: `DevicePreflight.RequireSerialPort` and `SegmentPump<T>` (segmenter plus decoder without a port or `StreamDevice`).
- Run parameter `ZWAVE_SERIAL_PORT` in `src/.runsettings` and `TEST_VARIABLES.md`.

## Verified

- 13 Z-Wave unit tests (byte-exact dev-term frames, split reads, NAK/CAN, stray bytes, bad checksum, impossible length) and the `Segment.Using` test pass.
- Hardware test: GetVersion on the Z-Stick on COM13 returned "Z-Wave 2.78", no CAN or NAK.

## Not done

Interpreter, node table, id tables, control surface, automatic ACK responder, DI registration, network-management commands on hardware, `api-design.md` Table 2 update.

## Addendum: interpreter, node table and relay toggle test

- `ZWaveInterpreter`, `ZWaveReading`, `ZWaveNames`, `ZWaveNodeTable`/`ZWaveNodeRecord`, `ZWaveReports`, the remaining `ZWaveRequests` (include, exclude, reset, learn, neighbour update, failed-node, node info, soft reset) and `TryAddZWave()` are ported from dev-term as consumer types; no new device-layer interfaces.
- Hardware tests `Enumerate_OnTheStick_ListsNodesAndTheirProtocolInfoTest` (home ID 016A1C28, node 1 controller, node 2 Binary Switch) and `Switch_AgainstRealNode_TogglesAndRestoresTest` (`ZWAVE_SWITCH_NODE=2`) both passed on the Z-Stick on COM13; the owner watched the relay toggle and restore.
- Not ported: id tables (`Data/*.json`), ACK responder, control surface (Q3). 27 unit tests pass, 1 Hardware test Inconclusive.
