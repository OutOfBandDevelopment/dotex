# Feature request: MIDI file format and serial protocol

- **Status:** done (file and stream decoding built 2026-11-12, verified 2026-11-17, see [change](../../changes/2026-11-17.md#midi-and-exif-requests-moved-to-completed))
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System` or a new `OoBDev.Midi` project

## Need

There is no support for standard MIDI files or the MIDI wire protocol on a serial or USB stream.

## Proposed behavior

Parse and write standard MIDI files (tracks, events, tempo) and decode MIDI messages from a device stream through the existing segmenter and decoder pipeline.

## Acceptance criteria

- Given a standard MIDI file, when parsed and written back, then the bytes round-trip.
- Given a MIDI byte stream that uses running status, then the decoder yields the right messages.

## Existing code checked

Segmenters and decoders in `OoBDev.System.Abstractions/IO`; no MIDI code in `src`.

## Security and operations

Files and streams are untrusted: bound track lengths and event counts.

## Priority

- **Impact:** another device family the pipeline can decode; niche.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Own project or inside `OoBDev.System`? Does DevTerm get a MIDI transport?

## Triage notes

Origin: BinaryDataDecoders issue 22. Low priority and specialized; accept, schedule or decline.

2026-11-11: triaged. Accepted at low priority: every BinaryDataDecoders feature is kept (scope rule in `CLAUDE.md`), so it is scheduled behind the DevTerm and event-sourcing work. Backlog: BACKLOG.md, Migrations (specialized formats).

2026-11-12: built as `OoBDev.Midi` (`src/Extensions`): file parse and write with byte-for-byte round trip, and `MidiStreamDecoder` with running status. Device definition and DevTerm transport remain in the backlog.

## Outcome

`OoBDev.Midi` (`src/Extensions`) parses and writes standard MIDI files with a byte-for-byte round trip and decodes streams with running status (16 Unit tests passed 2026-11-17). Not part of this request and still open: the device definition and the DevTerm transport and panel (panel in a `Panels` folder in `OoBDev.Midi`, owner 2026-11-12), tracked in `BACKLOG.md`.
