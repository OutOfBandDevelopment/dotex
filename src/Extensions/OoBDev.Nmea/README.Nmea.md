# OoBDev.Nmea

NMEA 0183 sentences (GGA, RMC, GSA, GSV, VTG): `$` start, line feed end, XOR checksum.

## Description

Receive-only. `NmeaDevice` is an `IDeviceDefinitionReceiver<NmeaSentence>` from `OoBDev.System.Abstractions`; bytes arrive through the existing adapters (serial, TCP, USB HID), so the package never opens a port.

- `NmeaSegmenter` cuts lines that start with `$` and end with a line feed, dropping bytes before the `$` and abandoning a line longer than 256 bytes.
- `NmeaMessageDecoder` drops NUL bytes (HID report padding), splits talker (first two characters), type and fields, and checks the `*hh` checksum (`NmeaChecksum`: Absent, Valid, Mismatch). A mismatch is reported, not thrown.
- Typed views: `NmeaGga`, `NmeaRmc`, `NmeaGsa`, `NmeaGsv` (header only), `NmeaVtg`, each with `TryParse(NmeaSentence, out ...)`. Positions are signed decimal degrees; a blank field is `null`.
- `NmeaEarthmateBt20Device` adds `[UsbHid(0x1163, 0x0200)]` for the DeLorme Earthmate BT-20.
- `services.TryAddNmea()` registers the decoder.

Source: dev-term `Devices.Nmea` (base); nothing to merge from BinaryDataDecoders except the fix-quality names (M5), which match. Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md).

## Not yet verified on hardware

The Hardware test reads from a BT-20 for up to 35 seconds and ends Inconclusive without one. How the BT-20 frames its HID reports is not confirmed on a device.
