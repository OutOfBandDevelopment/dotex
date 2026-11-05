# OoBDev.Examples.Controls.Wpf

A small WPF app that shows the UiDefinitions control set: one settings class (`ControlsSampleSettings`) with a toggle, text field, radio choice, numeric field, slider and a section shown only while a toggle is on. `FormDefinitionGenerator` builds the form definition from it and `FormView` (from `OoBDev.UiDefinitions.Wpf`) renders it.

Controls only: the framework demo stays in `OoBDev.Example.*`. Placement and scope: [ui-language-later.md](../../../docs/design/DevTermParity/ui-language-later.md).

```
dotnet run --project src/Examples/OoBDev.Examples.Controls.Wpf
```

## Verified

3 Unit tests in `OoBDev.Examples.Controls.Wpf.Tests` (STA thread, no window shown): the generated rows, the advanced section appearing when its toggle is set, and a rendered control per row. The app has not been looked at on screen.
