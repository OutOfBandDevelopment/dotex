# WPF screenshot capture (2026-10-30)

## What changed

- `WpfScreenshot` (Wpf.Tests) shows a window far off-screen and renders it with `RenderTargetBitmap`, writing through `GuideImage`, so a committed guide image is replaced only when the masked capture differs.
- `ConnectionEditorScreenshotTests` (DevLocal) captures the connection editor for tcp, serial and loopback into `docs/applications/devterm/user-guide/images/wpf-device-profiles-*.png`; a second run leaves the images byte-identical.
- `ToolWindowScreenshotTests` (DevLocal) captures the converter tools and the empty stream monitor (`wpf-converter-tools.png`, `wpf-stream-monitor.png`); `WpfScreenshot.Capture` builds a window on an STA thread, shows it off-screen, saves it through `GuideImage` and closes it.
- `ExpressionPickerScreenshotTests` (DevLocal) captures the expression picker in its seven states (`wpf-expression-picker*.png`: plain, error, warning, text, parameters, find, channels), identical across repeated runs.
- `RoutingScreenshotTests` (DevLocal) captures the routing window with a fake broker link, two rules and traffic (`wpf-routing.png`); the history timestamps are a masked zone in `zones.json`.
- `PlaybackScreenshotTests` (DevLocal) captures the playback window part way through a Tektronix 2230 session (`wpf-playback.png`, 1000 wide so the toolbar fits); the local-time part of the description is a masked zone.
- `OoBDev.DevTerm.Wpf.Tests` references `OoBDev.DevTerm.TestUtilities`.

## Not done

- The other windows (main shell, playback, control panels, manifest editor, themes, routing, color and expression pickers) are not captured yet, nor dev-term's layout review tests.
- The test profile folder still comes from `Path.GetTempPath()` (`ITempFileFactory` makes files, not folders).
