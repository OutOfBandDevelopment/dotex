# OoBDev.Examples.TestUtilities

Test helpers shared by the control example journey tests (WPF, WinForms, Tui, Blazor).

| Type | Purpose |
|------|---------|
| `ExampleGuide` | Resolves `docs/applications/example/controls-guide/images/` and writes a capture through `ImageSync.RunBesideZonesFile`, so a committed screenshot changes only when it differs once the zones in `zones.json` beside it are masked. |
| `WpfCapture` | Renders a WPF window to a PNG. |
| `WinFormsCapture` | Renders a WinForms form to a PNG. |

`zones.json` (same format as the DevTerm guide) lists regions that cannot be made deterministic; it is empty because the control examples render fixed content. Rules: fixed values only, mask what cannot be fixed (testing rule).

The Tui capture lives in the example Tui test project; sharing it with `TuiScreenshot` is an open question (`OPEN-QUESTIONS.md`).
