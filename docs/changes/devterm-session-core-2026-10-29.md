# DevTerm session engine core

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains `Session` (read loop over the transport `PipeReader`, send throttle, connect timeout and retry, `QueryAsync` with timeout, self-reported `Disconnected`, observers, telemetry), `Pipeline`, `SessionLimits`, `ISessionObserver`, `ISessionFactory`, `DevTermTelemetry` and the optional presenter interfaces (structured, async, originating, content-capturing).
- 52 unit tests pass in `OoBDev.DevTerm.Core.Tests` (40 ported from dev-term: session lifecycle, faults, limits, observer, telemetry, pipeline). The build has no warnings; `StopAsync` takes its token last and timeouts keep their inner exception.
- Not done: the session control servers (pipe, TCP share, HTTP), stream-content watcher and its pipeline test, message router, plugin loader, logging and configuration; each is a later slice of the DevTerm parity item.
