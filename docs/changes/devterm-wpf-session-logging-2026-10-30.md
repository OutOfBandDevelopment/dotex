# DevTerm WPF session logging (2026-10-30)

Each session in the WPF shell has a **Log to file** toggle (`SessionViewModel.ToggleLogging`, `ToggleLoggingCommand`). It starts a `SessionLogger` through `SessionLogging.Start` at the default path under the DevTerm home and stops it on the second press. `IsLogging` and `LogPath` (the tooltip) report the state; the log closes when the session is disposed. It is unavailable for sessions without per-connection options (the loopback session).

- Tests: `ToggleLogging_NoTab_CannotExecute`, `ToggleLogging_WithTab_StartsAndStopsALogFile` (uses `DEVTERM_HOME` in a temp folder); the WPF test project has 42 Unit tests passing.
- Left: choosing a log file path, adding a note to a running log, and the Theme and View menus.
