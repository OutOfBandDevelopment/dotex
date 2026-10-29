# SCPI instrument control

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.Scpi`: profile model (`ScpiInstrumentProfile`, `ScpiCommandDefinition`, `ScpiParameterDefinition`), `ScpiCommandBuilder` (single-pass substitution, clamped fixed-width numbers), `ScpiRequest`/`ScpiReply`, `ScpiMessageEncoder`/`ScpiMessageDecoder`, `ScpiReplySegmenter`, `ScpiDevice`, `ScpiProfileCatalog` with the nine dev-term profiles embedded, and `TryAddScpi()`.
- 29 unit tests pass (build and padding rules, replies with each line ending and with no terminator, bundled profile loading, `*IDN?` matching, drop-in files and bad files, DI).
- Not done: definite-length block replies, a hardware pass on any instrument, and the control panel, reply tracker and auto-detect flow (they need the DevTerm UI framework, phase 10).
