# OoBDev.UiDefinitions.Wpf

A WPF view for `OoBDev.UiDefinitions.ViewModels`: `FormView` renders a `FormViewModel` set as its `DataContext`.

## Description

Phase 7 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md). The view is pure XAML: `DataTemplate`s per view model type (check box, text box with inline error, radio group or check list, button), commands bound to the view models' `ICommand` types, and rows collapsed through `IsVisible`. The code-behind only calls `InitializeComponent`.

```csharp
var form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
window.Content = new FormView { DataContext = form };
```

Targets `net10.0-windows`. Not yet rendered: live display controls (indicator, bar graph, strip chart, vector), sliders as slider widgets (they show as text boxes), colour picker buttons.

## Verified

3 Unit tests on an STA thread (no window shown): the controls rendered, a command writing the model, a condition collapsing a row. Not looked at on screen.
