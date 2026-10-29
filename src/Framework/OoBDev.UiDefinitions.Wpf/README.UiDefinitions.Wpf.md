# OoBDev.UiDefinitions.Wpf

A WPF view for `OoBDev.UiDefinitions.ViewModels`: `FormView` renders a `FormViewModel` set as its `DataContext`.

## Description

Phase 7 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md). The view is pure XAML: `DataTemplate`s per view model type (check box, text box with inline error, radio group or check list, button), commands bound to the view models' `ICommand` types, and rows collapsed through `IsVisible`. The code-behind only calls `InitializeComponent`.

```csharp
var form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
window.Content = new FormView { DataContext = form };
```

`LiveDisplayView` renders an indicator (text), bar graph (progress bars), strip chart (polylines) or vector (readout text) from a `LiveDisplayViewModel`. Targets `net10.0-windows`. Sliders show a slider beside the text box. Not yet rendered: a vector plot, colour picker buttons.

## Verified

8 Unit tests on STA threads (no window shown; tests run serially because parallel STA threads deadlocked): controls rendered, a command writing the model, a condition collapsing a row, bar and indicator updates. Not looked at on screen.
