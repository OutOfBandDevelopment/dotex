# Feature request: XBee API-mode frames and an XBee-to-MQTT gateway

- **Status:** new
- **Requested by:** owner (notes repository project mapping, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/HardwareDevices` (new `OoBDev.Digi.XBee`) and `OoBDev.System.IO.Mqtt`

## Need

The notes backlog has an XBee mesh sensor network, an XBee-to-MQTT gateway and battery-powered remote nodes. The host side is .NET: read XBee API-mode
frames from a serial port, decode them into sensor readings and publish them to MQTT. dotex has the serial and MQTT transports but no XBee framing, so
every host program would write its own frame parser.

## Proposed behavior

A device package that segments and decodes XBee API frames (start delimiter, length, frame data, checksum) from a serial stream, and builds the common
outgoing frames (AT command, transmit request). Decoded receive packets (64-bit source address, payload) can be routed to an MQTT topic such as
`xbee/<address>/<field>`, using the existing MQTT adapter and the segmenter and decoder pipeline.

## Acceptance criteria

- Given a byte stream with a valid API frame preceded by noise, when segmented, then the frame is returned and the noise is skipped.
- Given a frame with a bad checksum, when decoded, then it is rejected and the stream resynchronises on the next start delimiter.
- Given an escaped frame (API mode 2), when decoded, then escaped bytes are restored.
- Given a receive packet, when routed, then one MQTT message is published per configured field.
- Given an AT command request, when built, then the checksum and frame id are correct.

## Existing code checked

- `OoBDev.System.IO.Ports` and `OoBDev.System.IO.Mqtt`: transports only, no XBee knowledge. The MQTT adapter publishes `topic<TAB>payload` lines, which a
  gateway could write to.
- `src/HardwareDevices`: no XBee package. A text search of `src` finds XBee only in one drawing test, which is unrelated.
- `docs/design`, `BACKLOG.md`, `docs/harvest-candidates.md` and the other requests: nothing on XBee. Not compared: any XBee library on NuGet, which could be
  used instead of a new parser (see open questions).

## Security and operations

Radio payloads are untrusted: bound frame length and reject short frames. The gateway holds MQTT credentials in options like the existing adapter. It runs
as a small host process or inside DevTerm.

## Priority

- **Impact:** unblocks the XBee-to-MQTT gateway and sensor-network ideas in the notes backlog; not needed by anything in dotex today.
- **Needed by:** no date
- **Workaround today:** a third-party XBee library, or write the parser in the gateway program.

## Open questions

- Own package (`OoBDev.Digi.XBee`) or inside `OoBDev.System.IO.Ports`?
- Adopt an existing NuGet XBee library behind a thin adapter, or write frames from the Digi documentation?
- Is a gateway a DevTerm plugin, a small application, or only a documented sample?
- Which API frame types are in scope for a first pass (AT, transmit, receive, I/O sample)?

## Triage notes

Origin: notes repository backlog (`projects/project-ideas.md`, Wireless & IoT). Maintainers: accept, schedule or decline.
