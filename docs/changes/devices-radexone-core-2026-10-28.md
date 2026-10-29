# Radex One dosimeter

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.Quarta.RadexOne`: `RadexOneDevice`, segmenter, framer, decoder, `RadexOneReply` with typed accessors, request kinds and encoder, `TryAddRadexOne()`.
- 14 unit tests (captured traces, checksums, segmentation) and 3 Hardware tests ported from dev-term, run on a real Radex One on COM8: all passed.
- New run parameter `RADEX_ONE_SERIAL_PORT` in `src/.runsettings` and `TEST_VARIABLES.md`.
- Not done: reading units and the serial/version layout (M3, M4), Write Settings and Reset Accumulated on the device, control surface (Q3).
