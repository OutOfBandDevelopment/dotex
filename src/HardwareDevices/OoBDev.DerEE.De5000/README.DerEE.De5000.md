# OoBDev.DerEE.De5000

DER EE DE-5000 LCR meter: a 17-byte measurement packet read from a stream (no command side).

## Description

The meter (a Cyrustek ES51919 chipset) transmits unprompted, so the device only receives. Bytes arrive through the existing adapters (optical-to-USB serial, or the optical-to-BLE adapter); the package never opens a port. Source: dev-term `Devices.De5000` (new in dotex). Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

| Type | Role |
|------|------|
| `De5000Device` | `IDeviceDefinitionReceiver<De5000Frame>` |
| `De5000Segmenter` | Cuts 17-byte packets (`00 0D ... 0D 0A`); drops one byte at a time to resync, and waits when a packet is split across reads |
| `De5000MessageDecoder` | Packet to `De5000Frame` |
| `De5000Frame` | Decoded packet (flags, frequency, tolerance, primary and secondary quantity, value, unit, status); `TryParse` |
| `De5000Interpreter` / `De5000Reading` | Frame to a readable line plus named values for indicators |
| `ServiceCollectionExtensions` | `TryAddDe5000()` |

Change tracking for indicators (publish only the values that changed) and the control surface are consumer concerns, not part of this package (control surface waits on owner question Q3).

## Verified

Unit tests use the dev-term packet layout, including the sign-extended negative `%` and `deg` readings and stream resync. They are not captured from a real meter in dotex.

## Not yet verified on hardware

No Hardware test yet: dev-term reaches the meter over BLE (device id and GATT UUIDs, still unconfirmed there), and dotex has no BLE adapter in this package.
