# DE-5000 LCR meter decoding

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.DerEE.De5000`: `De5000Device` (receive only), `De5000Segmenter` (17-byte packets with one-byte resync), `De5000MessageDecoder`, `De5000Frame` with `TryParse`, `De5000Interpreter`/`De5000Reading`, `TryAddDe5000()`. Ported from dev-term `Devices.De5000`; the chipset quirks (footer `0D 0A`, `MSB * 0x100 + LSB`, secondary status masked to 3 bits, sign extension only for `%` and `deg`) are kept.
- 13 unit tests pass (frame, stream split and resync, interpreter).
- Not done: Hardware test (needs a BLE or optical serial path to a real meter), control surface (Q3).
