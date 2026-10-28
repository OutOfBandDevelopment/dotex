# Device Decoding — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft · **Date:** 2026-10-28 · Everything marked *Proposed* is not implemented.

## Contents

- [Existing contracts used](#existing-contracts-used)
- [Gaps to close](#gaps-to-close)
- [Per device shape](#per-device-shape)
- [Naming](#naming)
- [To confirm in the template device](#to-confirm-in-the-template-device)

### List of Tables

1. Table 1 — Contracts used as they are
2. Table 2 — Gaps and proposed handling
3. Table 3 — Per device shape

## Existing contracts used

**Table 1 — Contracts used as they are**

| Contract | Location | Use |
|----------|----------|-----|
| `IDeviceDefinitionReceiver<T>` (`SegmentDefintion`, `Decoder`) | `OoBDev.System.Abstractions/IO` | A device's receive definition |
| `IDeviceDefinitionTransmitter<T>` (`Encoder`) | same | A device's transmit definition |
| `ISegmenter`, `ISegmentBuildDefinition`, `StartAndFixLengthSegmenter`, `BetweenSegmenter` | `.../IO/Segmenters` | Framing |
| `IMessageDecoder<T>.Decode(ReadOnlySequence<byte>)`, `IMessageEncoder<T>.Encode(ref T)` | `.../IO/Messages` | Decoding and encoding |
| `IChecksumCalculator.Simple16(ReadOnlySpan<ushort>)` | `.../IO/Functions` | Word-sum style checksums |
| `SerialPortAttribute`, `UsbHid` attribute, `StreamDevice`, `PipelineBuilder` | `.../IO/Ports`, `.UsbHids`, `OoBDev.System.IO.Pipelines` | Transport settings and plumbing |

## Gaps to close

Read from the code on 2026-10-28; each is closed in the template device or the device that first needs it.

**Table 2 — Gaps and proposed handling**

| Gap | Evidence | Proposed |
|-----|----------|----------|
| The segment definition can describe a start marker, an end byte, a fixed or maximum length and one length field (`SegmentExtensionDefinition`: type, length, position, endianness), but a start-less fixed length (K8055 reports have no marker) is not shown | `SegmentBuildDefinition` takes a `StartsWith` byte array | Confirm an empty start marker works; otherwise add a fixed-length segmenter |
| A length field that counts the remainder of the packet (Radex One: a 6-byte header, then an extension whose length is in the header) needs a defined meaning for the field position and base | `SegmentExtensionDefinition` has no base or adjustment | Confirm against the Radex One frame in phase 3; add an adjustment value if needed |
| Checksum algorithms: only `Simple16` over 16-bit values exists. Radex One needs `0xFFFF - (sum of little-endian words % 0xFFFF)` over bytes, NMEA needs XOR of the characters between `$` and `*` | dev-term `RadexOneFramer.ComputeChecksum`, `NmeaSentence` | Add the byte-oriented methods to `IChecksumCalculator`, tested with dev-term's byte-exact traces (R4) |
| `IMessageDecoder<T>` is stateless, dev-term decoders are not | `K8055Decoder` buffers 9-byte frames and keeps `_lastValues` | Framing to the segmenter; change tracking to a consumer type (architecture, Table 1) |
| `IMessageEncoder<T>.Encode(ref T)` returns bytes from one message, but Busylight commands come from accumulated UI state | `BusylightControlSurface.BuildFrame` | A record holding the full state is the message; the consumer owns the state |
| The serial adapter catches only `IOException` when opening, so a busy port throws `UnauthorizedAccessException` | `OoBDev.System.IO.Ports` `SerialPortDeviceAdapter.TryOpen` | Fix with a test in the template device (R9); check the three sibling defects in the same pass |

## Per device shape

**Table 3 — Per device shape**

| Device | Framing | Message types | Notes |
|--------|---------|---------------|-------|
| DE-5000 | Fixed 17 bytes | One reading record | Template device |
| Busylight | Fixed 9-byte report (leading report id) | Command record (step, colour, on, off, audio); ASCII identification reply | M1 adds the 16-step sequence |
| K8055 | Fixed 9 bytes | Input record, output command record | M2 adds the named input flags; raw value kept until the bit order is confirmed on hardware |
| NMEA | Start `$`, end line feed, XOR checksum | One record per sentence type (GGA, RMC, GSA, GSV, VTG) | Generic over talker prefix |
| Radex One | Prefix `0x7A 0xFF` inbound, `0x7B 0xFF` outbound, length field, outer and inner checksums | Read data, serial and version, settings, write and reset acknowledgements | M3 and M4 as documented hypotheses |
| Zoom H4n | Single status byte inbound; two-byte button codes outbound | Status flags record, button command | Handshake lives in the consumer |

## Naming

Names follow [Table 5 and Table 6 of the plan](../../migration/device-decoding-migration-plan.md): `{Device}MessageDecoder`, `{Device}MessageEncoder`, `{Device}Message` records, `{Vendor}Globals.ProviderKey`, `{Device}Options`, `TryAdd{Device}Services`. Implementations take no `Impl` suffix.

## To confirm in the template device

1. An empty start marker, or the need for a fixed-length segmenter.
2. How a record exposes its text rendering (an `ISpanFormattable` or a separate formatter type).
3. Whether `QuartaGlobals` style classes sit per vendor package or are shared; the plan assumes per package.
