# BinaryDataDecoders Fencing score machines (2026-10-29)

Ported `ElectronicScoringMachines.Fencing` (Favero and Saint George) as `src/HardwareDevices/OoBDev.Fencing.ScoreMachines` with `OoBDev.Fencing.ScoreMachines.Tests` (40 Unit tests, all passing).

## Decisions

- Placed in `HardwareDevices` as `OoBDev.Fencing.ScoreMachines`, following the existing `OoBDev.<Vendor>.<Device>` convention, not the `OoBDev.Extensions.Hardware.Fencing` name in the re-review document.
- Dropped `System.Composition` `[Export]` attributes; registration is `TryAddFencingScoreMachines()` with keyed decoders.
- `Fencer` is a `readonly record struct` and `ScoreMachineState` a record, so equality is by value.
- Favero parser: added a frame-length guard (the source threw `IndexOutOfRangeException`); the decoder throws `FormatException` for an empty frame.
- Saint George parser ported faithfully, including the priority frame that never sets priority (tracked in `BACKLOG.md`).
- The source tests were one real test plus comments of raw frames; the new tests cover each bit and frame layout, but none comes from a live machine.

## Not run

No Hardware test (no machine attached).
