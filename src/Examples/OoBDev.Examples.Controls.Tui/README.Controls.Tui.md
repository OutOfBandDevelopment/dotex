# OoBDev.Examples.Controls.Tui

A small Terminal.Gui app that shows the UiDefinitions control set: the same settings class as the WPF example (`ControlsSampleSettings`, shared source `../OoBDev.Examples.Controls.Shared`, linked: toggle, text field, radio choice, numeric field, slider and a section shown only while a toggle is on). `FormDefinitionGenerator` builds the form definition and `FormRenderer` (from `OoBDev.UiDefinitions.Tui`) renders it.

Controls only: the framework demo stays in `OoBDev.Example.*`. Plan: [example-ui migration plan](../../../docs/design/example-ui/migration-plan.md), step 2.

```
dotnet run --project src/Examples/OoBDev.Examples.Controls.Tui
```

## Verified

2 Unit tests in `OoBDev.Examples.Controls.Tui.Tests` (headless Terminal.Gui, nothing shown): a widget per visible setting with the advanced section hidden, and the advanced section appearing when its toggle is set. The app has not been looked at in a real terminal.

## Live displays

`LiveSample` shows the indicator (a label), the bar graph and the strip chart (`CellCanvasView` over `CellCharts`) below the form, fed by the shared `LiveSampleFeed` on a 500 ms timeout. 2 more Unit tests (`LiveSampleTests`) cover the displays and a published sample; the timer in `Program` has not been watched in a real terminal.
