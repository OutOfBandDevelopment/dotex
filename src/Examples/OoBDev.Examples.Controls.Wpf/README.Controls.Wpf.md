# OoBDev.Examples.Controls.Wpf

A small WPF app that shows the UiDefinitions control set: one settings class (`ControlsSampleSettings`, shared with the Tui and Blazor examples from `../OoBDev.Examples.Controls.Shared`) with a toggle, text field, radio choice, numeric field, slider and a section shown only while a toggle is on. `FormDefinitionGenerator` builds the form definition from it and `FormView` (from `OoBDev.UiDefinitions.Wpf`) renders it.

Controls only: the framework demo stays in `OoBDev.Example.*`. Placement and scope: [ui-language-later.md](../../../docs/design/DevTermParity/ui-language-later.md).

```
dotnet run --project src/Examples/OoBDev.Examples.Controls.Wpf
```

## Verified

6 Unit tests in `OoBDev.Examples.Controls.Wpf.Tests` (STA thread, no window shown): the generated rows, the advanced section appearing when its toggle is set, a rendered control per row, and the live displays (one per feed control, `Step` reaching every display, the panel placed in the window). Below the form, `LiveSample` hosts an indicator, a bar graph and a strip chart (`LiveDisplayView`) fed by the shared `LiveSampleFeed`; `Program` steps it every 500 ms on a `DispatcherTimer`. The app has not been looked at on screen.
