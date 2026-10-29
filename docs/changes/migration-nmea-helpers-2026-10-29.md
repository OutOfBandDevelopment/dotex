# BinaryDataDecoders NMEA helpers

**Date:** 2026-10-29

The source `BinaryDataDecoders.Nmea/Notes` holds `Degree`, `Bearing` and `GeneralHeading`. They are excluded from that project's build (`Compile Remove`, kept as `None`), in a `Test1` namespace. They became `NmeaCoordinate`, `NmeaBearing` and `NmeaCompassPoint` in `OoBDev.Nmea`, documented in [README.Nmea.md](../../src/Extensions/OoBDev.Nmea/README.Nmea.md).

**Changes from the source (breaking changes are accepted):**
- Value types (`readonly record struct`) with the invariant culture, instead of mutable-looking classes that threw `NullReferenceException` for bad input; parsing is `TryParse`.
- `double` instead of `float`, and the seconds and minutes are rounded once as whole thousandths, so 59.9996 seconds no longer prints as 60.000.
- The compass table is the same 16 points and the same boundaries (a point covers 22.5 degrees centred on it); a bearing is wrapped into 0 up to 360, and NaN or infinity throw.
- The signed (`±`) text forms are dropped: the hemisphere letter or `Degrees` already carries the sign. `GeneralHeadingShort` is the `Abbreviation` property. The stray `Xor` extension in the source is already `NmeaChecksum`.
- The WMI and HID import notes (`Gprmc`, `HIDImports`, `Program`, `ROOT.CIMV2.*`) are scratch code for a Windows-only discovery and were not migrated (the USB HID adapters and `NmeaEarthmateBt20Device` cover the device).

**Verified:** 36 Unit tests in `OoBDev.Nmea.Tests` pass (the 12 existing plus the new bearing and coordinate tests, including the compass boundaries and the carry case). Hardware tests were not run.
