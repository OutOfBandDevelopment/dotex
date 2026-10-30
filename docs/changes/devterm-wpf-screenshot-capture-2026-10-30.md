# WPF screenshot capture (2026-10-30)

## What changed

- `WpfScreenshot` (Wpf.Tests) shows a window far off-screen and renders it with `RenderTargetBitmap`, writing through `GuideImage`, so a committed guide image is replaced only when the masked capture differs.
- `ConnectionEditorScreenshotTests` (DevLocal) captures the connection editor for tcp, serial and loopback into `docs/applications/devterm/user-guide/images/wpf-device-profiles-*.png`; a second run leaves the images byte-identical.
- `OoBDev.DevTerm.Wpf.Tests` references `OoBDev.DevTerm.TestUtilities`.

## Not done

- The other windows (main shell, playback, converter tools, control panels, manifest editor, themes, routing, stream monitor) are not captured yet, nor dev-term's layout review tests.
- The test profile folder still comes from `Path.GetTempPath()` (`ITempFileFactory` makes files, not folders).
