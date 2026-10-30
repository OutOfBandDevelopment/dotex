# Guide screenshots through image-mask sync (2026-10-30)

## What changed

- `GuideImage` in `OoBDev.DevTerm.TestUtilities` renders a capture to a temporary file and runs `image-mask sync` against the committed image, using the `zones.json` next to the images.
- `TuiScreenshot.Save` and the Web `SaveAsync` (and the configure-device locator capture) use it, so a run that only moves a clock, a port or a temp path leaves the committed image alone.
- `docs/applications/devterm/user-guide/images/zones.json` masks the temp-path guid in `tui-main-logging.png` and the message timestamps in `tui-routing.png`.
- Tui screenshot tests: 26 pass with no image churn after the zones were added. Web screenshot tests: 37 pass; the 19 Web images that changed differ for real (the Plugins nav entry, the panel layout) and are recommitted.

## Not done

- WPF: the capture code (`WpfScreenshot`) has not been migrated, so there is nothing to route yet (backlog).

## Shared test fakes

- `ManualTimeProvider` (three copies) and `FakeDeviceConfigEditor` (two copies) moved into `OoBDev.DevTerm.TestUtilities`, which now references Core; `Core.Tests` and `Tui.Tests` reference it with a global using. Core Unit 1063 pass (5 skipped), Tui Unit 271 pass. `FakeTransport` (five differing copies) is still to merge.
