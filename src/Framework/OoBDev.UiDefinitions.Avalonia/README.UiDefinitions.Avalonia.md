# OoBDev.UiDefinitions.Avalonia

An Avalonia view for `OoBDev.UiDefinitions.ViewModels`, built in code with bindings only (no XAML, no code-behind logic). Promoted from the Avalonia spike on 2026-11-11; findings: [avalonia-spike-findings.md](../../../docs/design/UiDefinitions/avalonia-spike-findings.md).

```csharp
var form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
window.Content = FormView.Create(form);
```

- `FormView.Create(FormViewModel)`: a section per group with a header; a check box and toggle command for toggles, a radio list for choices, a button for buttons, a slider for sliders, a text box otherwise. Row visibility binds to `IsVisible` and enablement to `IsEnabled`.
- `LiveDisplayView.Create(LiveDisplayViewModel)`: indicator, bar graph, strip chart and vector. The strip chart and vector are drawn in a fixed 100 by 100 box in a `Viewbox`, so the configured axis is honoured.

Example app: `src/Examples/OoBDev.Examples.Controls.Avalonia`.

## Verified

4 Unit tests in `OoBDev.UiDefinitions.Avalonia.Tests`, headless with the Skia renderer: controls rendered, a toggle command revealing a row, a slider and a choice writing the model. Each test attaches a PNG screenshot to its result. On Linux install `libfontconfig1` and `fonts-dejavu-core`. The vector display is drawn but not covered by a test.
