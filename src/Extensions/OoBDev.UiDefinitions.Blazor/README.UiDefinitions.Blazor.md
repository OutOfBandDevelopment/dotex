# OoBDev.UiDefinitions.Blazor

A Blazor renderer for `OoBDev.UiDefinitions.ViewModels` (step 3 of the [control-example migration plan](../../../docs/design/example-ui/migration-plan.md)). Razor class library; lives in Extensions because it carries the `Microsoft.AspNetCore.App` framework reference.

## Description

`FormView` takes a `FormViewModel` and renders its visible sections; `ControlRow` renders one row by view model type: check box (toggle), text input, range input (slider), radio group, check list, select (drop-down choice) and button, plus the row's error. Commands on the view models do the work (`Toggle`, `Select`, `ToggleOption`, `Invoke`); the component re-renders on the form's and rows' `PropertyChanged`. Rows carry `data-id` for tests and styling; there is no bundled CSS.

```razor
<FormView Form="form" />
```

Live displays (bar, chart, vector) are not rendered yet.

## Verified

3 Unit tests with bUnit (`OoBDev.UiDefinitions.Blazor.Tests`): each control kind renders and the conditional section stays hidden, input changes write the model and reveal the section, and a change made in code re-renders. Not looked at in a browser.
