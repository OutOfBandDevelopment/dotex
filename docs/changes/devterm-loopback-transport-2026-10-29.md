# DevTerm loopback transport

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Transports.Loopback` (Extensions): `LoopbackTransport`, `LoopbackScript`, `LoopbackRule`, `LoopbackGenerators`, options with validator, `TryAddLoopbackTransport()`; ported from dev-term `DevTerm.Transports.Loopback`. Sample counters are per transport instance.
- 18 unit tests pass (all scripted commands, pacing, unrecognized input, reopen after close, validator, registration). Regexes carry a one second timeout and culture-invariant parsing, so the build has no analyzer warnings.
- Not done: custom scripts (the options only carry the pacing interval), and the dev-term tests that drive it through `Session` and presenters (they follow with the session engine).
