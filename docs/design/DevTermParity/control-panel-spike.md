# Control panel renderer spike (2026-10-29)

Status: Draft, written for open question Q4 (`OPEN-QUESTIONS.md`). Read-only analysis; no code changed.

## Question

Can `FormView` render a device panel (a `UiDefinition` plus an `IControlSurface`) so that the WPF `ControlPanelWindow` needs no second renderer?

## Findings

| Area | Today | Gap for a surface-driven panel |
|---|---|---|
| Value lookup | `FormBinding.Property(id)` reflects `Source.GetType().GetProperty(id)` and caches the `PropertyInfo` | A surface has no properties; ids come from the definition at run time |
| Value typing | Conversion and validation read `PropertyInfo.PropertyType` (`TryConvert`, `IsBlankOptional`, `SetText`, `SetSelection`) | The type must come from the control kind (number, text, choice, toggle) |
| Commands | `Invoke(id)` reads an `ICommand` property; `InvokeCommand` builds parameters from sibling field text and `ParameterExpressions` | `IControlSurface.InvokeAsync(commandId, value)` takes one string; the same parameter text can be passed |
| Change notification | `INotifyPropertyChanged` on the source, or `Changed` after a write | A dictionary source raises `Changed(id)` itself |
| Visibility | `IsVisible(UiCondition)` uses `GetValue(condition.Id)` | Works unchanged over any source |
| Indicators | Read through `GetValue(id)` | Values come from the structured presenter (`Merge` in the Web host) and must be written into the source |
| Previews | not in `FormBinding` | `ICommandPreview` tooltip text, per control, recomputed from current values |

## Shape that fits

1. Extract the five `PropertyInfo` uses in `FormBinding` behind a small internal accessor (`Has`, `Type`, `CanWrite`, `Read`, `Write`) with the reflection version as the default, so existing models and tests are unchanged.
2. Add a `FormBinding` constructor over a dictionary-backed source (`SurfaceFormModel`) that builds one accessor per control of the definition, typed from the control kind, raises `Changed(id)` on a write and exposes `Apply(IReadOnlyDictionary<string,string>)` for indicator values.
3. A `SurfaceCommand` per button control implements `ICommand` by calling `surface.InvokeAsync(commandId, parameter)`, so `InvokeCommand` works as it does for models.
4. `FormViewModel` is then built from the definition and that binding; `FormView` and the WPF shell panel need no change, and the Web front end can reuse the same source.

## Not covered yet

Colour swatches for custom colour choices, `EnabledWhen` interlocks with `DisabledReason` tooltips, the Notes section, the preview tooltip and live displays (strip charts, bar graphs) must be checked against `FormView` and `LiveDisplayView` before the item closes; dev-term's `ControlPanelWindowTests` and `ControlPanelWindowLayoutTests` list the expected behaviour and are the test source.

## Next step

Owner decision 2026-10-30: option C (`DynamicModelFactory` / `SurfacePanelBuilder`, already built); the B steps below are not planned. If B is chosen later: implement steps 1 to 3 in `OoBDev.UiDefinitions` (80% coverage, tests first from `FormBindingTests`), then step 4 in the WPF shell with a `DevicePanelContribution` command in the shell, then port the dev-term control panel tests.
