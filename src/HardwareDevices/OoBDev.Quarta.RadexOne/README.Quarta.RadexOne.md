# OoBDev.Quarta.RadexOne

Quarta Radex One dosimeter on a serial port (9600 baud, 8-N-1, no handshake): prefixed packets with a length field and outer and inner checksums.

## Description

Hardware driver, so it lives in `src/HardwareDevices`. Source: dev-term `Devices.RadexOne`, with merges M3 and M4 (BinaryDataDecoders) kept as documented hypotheses. Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

Bytes arrive through `OoBDev.System.IO.Ports`, so the package never opens the port.

| Type | Role |
|------|------|
| `RadexOneDevice` | Receiver of `RadexOneReply`, transmitter of `RadexOneRequest` |
| `RadexOneSegmenter` | Finds `7A FF` replies, validates the header checksum, resyncs byte by byte |
| `RadexOneFramer` | Builds and parses the outer packet; the `0xFFFF - sum % 0xFFFF` checksum |
| `RadexOneMessageDecoder` | Packet to `RadexOneReply` (throws on an invalid packet) |
| `RadexOneReply` | `TryGetReadData`, `TryGetSettings`, `IsAcknowledgement`, `SerialVersionText`; inner checksums are verified |
| `RadexOneRequest` / `RadexOneRequestKind` / `RadexOneMessageEncoder` | Read data, serial and version, settings; write settings; reset accumulated |
| `RadexOneReadData`, `RadexOneSettings`, `RadexOneAlarmMode`, `RadexOneCommandCode` | Values and codes |
| `ServiceCollectionExtensions` | `TryAddRadexOne()` |

The device only applies Write Settings when it is sent three times in a row; repeating the request is the caller's job.

## Verified

- Unit tests (14): requests and a reply matched to the captured traces, inner checksum rejection, noise and split-packet segmentation, DI.
- Hardware tests (3, `RADEX_ONE_SERIAL_PORT`, default `COM8`) ran on a real device on 2026-10-28: Read Data gave `Ambient=14 Accumulated=3049 Cpm=18`, Read Settings gave `Alarm=Off Threshold=20`, and Read Serial Version answered.

## Not yet verified

- The unit of each reading (BinaryDataDecoders scales 32-bit fields as hundredths of a microsievert per hour; here they are 16-bit fields with a reserved word between them).
- The serial and version layout: the payload is shown as ASCII with dots, and it does not reconcile with the documentation (merge M3).
- Write Settings and Reset Accumulated against the device (no Hardware test sends them, because they change the device).
