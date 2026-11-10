# Avalonia spike findings

**Status:** Done 2026-11-09 (spike code in `src/Spikes/OoBDev.Spike.Avalonia` and `.Tests`, not in the solution). Plan: [avalonia-spike.md](avalonia-spike-plan.md). The owner decides the toolkit.

## Result

The shared `OoBDev.UiDefinitions.ViewModels` render in an Avalonia 11.3.23 window with no change to them and no code-behind: `FormView` only binds to `FormViewModel`, `FormSectionViewModel` and the control view models. Four headless tests pass: toggle, slider, choice and a conditional row (`VisibleWhen`) hidden and then revealed through the real `ICommand` types.

| Check | Finding |
|-------|---------|
| .NET 10 and licence | `Avalonia`, `Avalonia.Headless`, `Avalonia.Themes.Fluent` 11.3.23 target net8+ and run on net10.0; MIT. |
| Binding fit | Close: `ICommand`, `INotifyPropertyChanged` and `IReadOnlyList` map directly. Gaps: a `RadioButton` or `CheckBox` needs `CommandParameter` bound (the option text, or the negated `IsChecked`); an invisible row is absent from the visual tree, so tests assert absence, not `IsVisible`. |
| Headless | `HeadlessUnitTestSession` works under MSTest on Windows. `Avalonia.Headless.MSTest` does not exist on NuGet, so the session is started by hand (`Dispatch`). Frames are captured with `Avalonia.Skia` and `UseHeadless(UseHeadlessDrawing = false)`: `window.CaptureRenderedFrame().Save(path)` then `TestContext.AddResultFile(path)`, so every test attaches its PNG to the result: the files are written to `TestContext.TestResultsDirectory` and listed with `AddResultFile`, so they sit under `TestResults/<run>/In/<id>/<machine>/` and in the TRX (2026-11-10). The live displays (indicator, bar graph, strip chart) are drawn too: `LiveDisplayView` draws a `LiveDisplayViewModel` (strip chart and vector in a fixed 100 by 100 box in a `Viewbox`, which honours the configured axis where the WPF `Stretch=Fill` polyline rescales to the data), fed by the shared `LiveSampleFeed`; 5 more tests, 3 more screenshots. The vector display is drawn but not covered by a test. The first capture showed a slider with no track (no width in an auto column); fixed with a fixed width. The spike is also a runnable app: `dotnet run --project src/Spikes/OoBDev.Spike.Avalonia`. |
| Platforms | Windows and Linux. Linux ran in the `mcr.microsoft.com/dotnet/sdk:10.0` container (2026-11-10): all 4 tests pass and the Skia screenshots render the same form (DejaVu instead of Segoe UI). The image needs `libfontconfig1` and a font (`fonts-dejavu-core`), otherwise Skia fails with `DllNotFoundException: libSkiaSharp` / `libfontconfig.so.1`. The container build needs `-p:DisableGitVersionTask=true` when `.git` is not copied in. A real window under WSLg and macOS were not run. |
| Package size | Not measured. |
| Eto.Forms, GTK | Not compared; not needed for the finding above. |

## Recommendation

Avalonia is a workable cross-platform renderer for the UiDefinition view models: the view-model layer needs no change. Before adopting it: tie the captured frames to the guide screenshots, run the tests in a Linux container, and measure published size. Views would be written in AXAML rather than code (this spike builds them in C# to avoid a XAML toolchain).
