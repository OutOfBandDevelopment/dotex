# OoBDev.UiDefinitions.Blazor

A Blazor renderer for `OoBDev.UiDefinitions.ViewModels` (step 3 of the [control-example migration plan](../../../docs/design/example-ui/migration-plan.md)). Razor class library; lives in Extensions because it carries the `Microsoft.AspNetCore.App` framework reference.

## Description

`FormView` takes a `FormViewModel` and renders its visible sections; `ControlRow` renders one row by view model type: check box (toggle), text input, range input (slider), radio group, check list, select (drop-down choice) and button, plus the row's error. Commands on the view models do the work (`Toggle`, `Select`, `ToggleOption`, `Invoke`); the component re-renders on the form's and rows' `PropertyChanged`. Rows carry `data-id` for tests and styling; there is no bundled CSS.

```razor
<FormView Form="form" />
```

`LiveDisplayView` takes a `LiveDisplayState` (from `LiveDisplayState.For`) and draws an expression indicator (text), a bar graph (SVG bars per channel) and a strip chart (SVG polylines per channel, palette from `ChartPalette`, `Dark` switches it). The host applies values to the state and calls `Refresh()`. Vector displays are not rendered yet (shows a note).

```razor
<LiveDisplayView State="state" Dark="false" />
```

## Verified

7 Unit tests with bUnit (`OoBDev.UiDefinitions.Blazor.Tests`): each control kind renders and the conditional section stays hidden, input changes write the model and reveal the section, a change made in code re-renders, and `LiveDisplayView` draws the indicator, bars and chart and redraws on `Refresh`. Not looked at in a browser.
