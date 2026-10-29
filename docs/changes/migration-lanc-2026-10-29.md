# BinaryDataDecoders LanC command tables (2026-10-29)

Ported the four LanC enums as `src/HardwareDevices/OoBDev.LanC` with `OoBDev.LanC.Tests` (23 Unit tests, all passing).

## Decisions

- Placed in `HardwareDevices`, like the Fencing score machines.
- Renamed typo and underscore members (CA1707): `SillNormal` to `StillNormal`, `SlowMinux`, `ShuttleEditMinu`, `TimerDIsplay`, `AutoPLay`, `X1_10`, `*_NoDigital` and similar. Types were renamed `LanCCameraVtrCommand` and `LanCCameraSpecialCommand`. Byte values are unchanged (breaking rename, allowed).
- Dropped the `System.Composition` package reference; the source never used it.

## Not run

No Hardware test (no LANC interface or camera).
