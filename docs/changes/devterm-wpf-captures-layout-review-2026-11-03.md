# DevTerm WPF captures and layout review (2026-11-03)

Closes the "windows were never viewed running" item of the [DevTerm parity status](../design/DevTermParity/status.md).

## Done

- Every xaml window of `OoBDev.DevTerm.Wpf` is captured through `WpfScreenshot` for the generated user guide (theme seed picker, theme builder, instrument picker added; images under `docs/applications/devterm/user-guide/images/`).
- `WpfLayoutAssert` (bounds, overlap, clip, minimum control height, contrast, light chrome under dark themes) is migrated from dev-term, and `UiLayoutReviewTests` checks every window in the light and dark themes (`Unit`).
- Defects the review found and fixed: low-contrast warnings text in `ThemeBuilderWindow`, a light-chrome `ToggleButton` under the dark theme (new template in `DarkControls.xaml`), and a 16 px `ExpressionBox` in `ExpressionPickerWindow`.
- Control panels are built in code (`SurfacePanelBuilder`, `FormView`) and render inside the main window, so the shell review covers them through the demo panel.

## Verified

`dotnet test src/Applications/OoBDev.DevTerm.Wpf.Tests`: 194 passed, 0 failed.
