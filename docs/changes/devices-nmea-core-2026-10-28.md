# NMEA 0183 decoding

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.Nmea`: `NmeaDevice`, `NmeaSegmenter`, `NmeaMessageDecoder`, `NmeaSentence` with `NmeaChecksum`, typed `NmeaGga`/`NmeaRmc`/`NmeaGsa`/`NmeaGsv`/`NmeaVtg`, `NmeaFixQuality`, `NmeaFixType`, `NmeaEarthmateBt20Device` (USB HID 0x1163/0x0200), `TryAddNmea()`.
- 28 unit tests pass (checksums, NUL padding, split and junk-prefixed streams, resynchronisation, each sentence type, fix-quality codes); the BT-20 Hardware test is ported and ends Inconclusive without the receiver.
- Not done: a physical pass on a BT-20 (HID report framing), per-satellite GSV blocks, other sentence types.
