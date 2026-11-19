# OoBDev.Midi

Standard MIDI files (parse and write) and the MIDI wire protocol decoded from a byte stream.

## Description

Pure and host-free: no I/O, no references beyond the base library. Bytes arrive from the existing serial or USB adapters or from a file the caller read.

| Type | Role |
|------|------|
| `MidiFileReader.Parse` | Reads `MThd` and `MTrk` chunks into a `MidiFile`; other chunks are skipped |
| `MidiFileWriter.Write` | Writes a `MidiFile` back to bytes |
| `MidiFile`, `MidiTrack`, `MidiEvent`, `MidiEventKind` | The parsed model: channel, SysEx, escape and meta events with delta ticks |
| `MidiStreamDecoder` | `Feed(bytes)` yields `MidiMessage` values from a live stream |
| `MidiMessage` | Status, two data bytes, and the payload of a system exclusive message |

- **Round trip.** A channel event records `UsedRunningStatus`, and the writer omits the status byte again, so a parsed file writes back byte for byte (variable-length values are written in their shortest form, as files normally are; skipped unknown chunks are not preserved).
- **Untrusted input.** Chunk lengths are checked against the data, a variable-length value is limited to 4 bytes, a channel data byte with the top bit set is rejected, and a track may hold at most `MidiFileReader.DefaultMaxEventsPerTrack` events (a parameter). Failures throw `InvalidDataException`.
- **Stream decoding.** Running status, real-time bytes (`F8` and up) inside another message, system common messages clearing running status, and system exclusive up to `MidiStreamDecoder.MaxSysExLength` bytes (longer is abandoned). A data byte with no status is dropped.

## Not built

A device definition that plugs the decoder into the segmenter pipeline, and a MIDI transport in DevTerm, wait for a consumer (request `docs/features/completed/2026-11-10-midi-support.md`). Verified by unit tests on hand-built bytes; no real `.mid` files or MIDI hardware were used.
