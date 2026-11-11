# OoBDev.UiDefinitions.WinForms

A Windows Forms view for `OoBDev.UiDefinitions.ViewModels`: `new FormView(formViewModel)` is a `UserControl` with a group box per section and a row per control. Targets `net10.0-windows`.

```csharp
var form = new FormViewModel(FormDefinitionGenerator.Generate(settings), new FormBinding(settings));
Controls.Add(new FormView(form) { Dock = DockStyle.Fill });
```

Editors write through the view model: toggles, choices and buttons run the view model's commands, text and sliders set its properties. Each row follows its view model for value, visibility (`IsVisible`), enabled state and error (an `ErrorProvider`); descriptions are tool tips. `Editors` exposes each editor by control id. Choices are radio buttons, or check boxes for a check list; sliders are track bars scaled by the control's step.

`LiveDisplayView` draws the indicator, bar graph, strip chart and vector displays (`PlotSurface` is the 100 by 100 plot). Not built yet: a colour picker; see the backlog. Example app: `src/Examples/OoBDev.Examples.Controls.WinForms` (`SettingsForm`, `LiveForm`).

## Verified

6 Unit tests on STA threads (the form is shown off screen so visibility is real): a conditional row hidden until its toggle is set, a slider, a radio choice and a check list writing the model, typing writing the model, and a model change reaching the editors.
