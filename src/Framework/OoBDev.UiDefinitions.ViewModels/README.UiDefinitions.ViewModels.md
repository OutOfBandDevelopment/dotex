# OoBDev.UiDefinitions.ViewModels

Toolkit-neutral view models over a `UiDefinition` and a `FormBinding`. A renderer (WPF, Avalonia, terminal, Blazor) binds to these and contains no logic of its own.

## Description

Phase 7 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md), first part. The dev-term WPF renderer was code-behind; here the behaviour lives in testable types.

| Type | Role |
|------|------|
| `FormViewModel` | Sections of rows for a definition; refreshes every row when the model changes |
| `ControlViewModel` | Base row: label, description, `IsVisible`, `IsEnabled`, `DisabledReason`, `Error` |
| `ToggleViewModel`, `TextViewModel`, `ChoiceViewModel`, `ButtonViewModel` | One per form control kind (`TextViewModel` serves text field, numeric and slider) |
| `ToggleCommand`, `SelectOptionCommand`, `ToggleOptionCommand`, `InvokeCommand` | Real `ICommand` types with the binding and control injected |
| `ControlViewModelFactory` | Picks the view model for a control; live display controls are not form fields and throw |

| `LiveDisplayViewModel` | Wraps a live display state (indicator, bar graph, strip chart, vector); `Revision` changes per published update |

No code-behind, no lambdas as commands, no UI toolkit reference.

## Verified

19 Unit tests: visibility conditions, validation errors, model-to-view refresh, the commands, disposal.
