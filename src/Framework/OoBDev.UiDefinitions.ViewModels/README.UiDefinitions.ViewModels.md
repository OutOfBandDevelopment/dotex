# OoBDev.UiDefinitions.ViewModels

Toolkit-neutral view models over a `UiDefinition` and a `FormBinding`. A renderer (WPF, Avalonia, terminal, Blazor) binds to these and contains no logic of its own.

## Description

Phase 7 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md), first part. The dev-term WPF renderer was code-behind; here the behaviour lives in testable types.

| Type | Role |
|------|------|
| `FormViewModel` | Sections of rows for a definition; refreshes every row when the model changes |
| `ControlViewModel` | Base row: label, description, `IsVisible`, `IsEnabled`, `DisabledReason`, `Error` |
| `ToggleViewModel`, `TextViewModel`, `ChoiceViewModel`, `ButtonViewModel` | One per form control kind (`TextViewModel` serves text field, numeric and slider) |
| `ToggleCommand`, `SelectOptionCommand`, `ToggleOptionCommand`, `InvokeCommand` | Real `ICommand` types with the binding and control injected; `InvokeCommand` also takes the sibling controls and, for a button with `ParameterFieldIds`, passes their current texts joined by `,` as the command parameter (`ParameterExpressions` are not evaluated yet) |
| `ControlViewModelFactory` | Picks the view model for a control; live display controls are not form fields and throw |
| `LiveDisplayViewModel`, `BarViewModel`, `ChartLineViewModel` | Wrap a live display state (indicator, bar graph, strip chart, vector); `Revision` changes per published update; an indicator has `Text`, a bar graph `Bars`, a strip chart `Lines` (points in a 100 by 100 box), a vector `Readout` |

No code-behind, no lambdas as commands, no UI toolkit reference.

## Verified

47 Unit tests: visibility conditions, validation errors, model-to-view refresh, the commands, disposal, the vector plot projection (XY, polar, XYZ, colour), the colour picker command and dialog state, the Custom option link (`SelectCustomColorCommand`). Line coverage with the own test project (measured 2026-10-29): 96%.

## Colour picker

`IColorPicker` is the dialog a view supplies. Pass it to `new FormViewModel(definition, binding, colorPicker)`; a `ButtonControl` with `ColorPickerTargetCommandId` then gets a `PickColorCommand` that writes the picked `r,g,b` text to that control (the dialog starts at the last pick or the current text). `ColorPickerViewModel` holds the dialog state (three channels, preview, accept and cancel commands).
