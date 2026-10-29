# DevTerm stream content detection

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains `StreamContentWatcher` (presenter that captures renderable binary content without emitting text), `StreamContentSniffer`, `StreamContentEndFinder`, `HpglToSvgConverter`, `StreamCapture`/`StreamCaptureEnd`, `StreamContentKind`/`Format`, `IStreamContentHintSink` and options.
- 101 ported tests (sniffer, end finder, watcher, HP-GL converter, pipeline withholding); `OoBDev.DevTerm.Core.Tests` 158 passing. Four dense single-pass parsers carry a justified `MA0051` suppression instead of being split.
- Not done: plugin loader, logging/configuration, the control servers.
