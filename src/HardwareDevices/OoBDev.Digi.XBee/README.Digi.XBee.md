# OoBDev.Digi.XBee

Digi XBee API-mode frames (API 1 and the escaped API 2): segmenting a serial byte stream into frames, decoding the common frame types and building the outgoing ones.

## Frames

| Type | Byte | Direction | Class |
|------|------|-----------|-------|
| AT command | `0x08` | to radio | `XBeeFrames.AtCommand` |
| Transmit request | `0x10` | to radio | `XBeeFrames.TransmitRequest` |
| AT command response | `0x88` | from radio | `XBeeAtCommandResponse` |
| Receive packet | `0x90` | from radio | `XBeeReceivePacket` |
| I/O data sample | `0x92` | from radio | `XBeeIoSample` |
| anything else | | from radio | `XBeeUnknownFrame` (bytes kept) |

Envelope: `7E`, 16-bit big-endian length, frame data (type byte first), checksum `0xFF - (sum of frame data & 0xFF)`. In API mode 2 the bytes `7E 7D 11 13` after the delimiter are sent as `7D` plus the byte XOR `0x20`; the length and checksum describe the unescaped bytes.

## Use

- `XBeeSegmenter(onSegment, escaped)` is an `ISegmenter`: it skips noise before a `7E`, waits for a split frame, and drops a frame with a bad checksum or a length over 1600 bytes (radio input is untrusted), resynchronising on the next `7E`. It delivers the raw frame bytes.
- `XBeeFrameDecoder.TryDecode(raw, escaped, out frame)` validates and decodes one delivered frame; `DecodeData` decodes frame data only. A known type with a short body decodes to `XBeeUnknownFrame` rather than throwing.
- `XBeeFraming.Encode`, `TryRead` and `Checksum` are the envelope primitives.

## Sample: XBee to MQTT

Not shipped as a library (decision Q15). Pipe the serial stream through `XBeeSegmenter`, decode with `XBeeFrameDecoder`, and for each `XBeeReceivePacket` publish one message per configured field to `xbee/<AddressText>/<field>` with the MQTT adapter (`OoBDev.System.IO.Mqtt`); for an `XBeeIoSample` publish one message per sampled line.

## Tests

`OoBDev.Digi.XBee.Tests` (Unit): checksum and frame id, noise, bad checksum resync, split reads, oversized length, escaping round trip, and each decoder.
