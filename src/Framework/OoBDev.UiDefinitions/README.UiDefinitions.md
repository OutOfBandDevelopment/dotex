# OoBDev.UiDefinitions

A serializable, toolkit-neutral description of a control panel. A renderer (WPF, terminal, web) turns a `UiDefinition` into controls and a view model drives it; the definition itself references no device, transport or UI toolkit.

## Description

Ported from dev-term `UiDefinitions` (phase 6 of [the device decoding plan](../../../docs/migration/device-decoding-migration-plan.md)).

| Area | Types |
|------|-------|
| Document | `UiDefinition`, `UiSection`, `UiDefinitionSerializer` (JSON and XML) |
| Controls | `ToggleControl`, `SliderControl`, `ChoiceControl`, `NumericControl`, `TextFieldControl`, `ButtonControl`, `IndicatorControl`, `BarGraphControl`, `StripChartControl`, `VectorControl` |
| Values | `ValueConstraint`, `ValueKind`, `ValueValidator`, `ValueValidationResult` |
| Rules | `Expression` (a small expression language for computed values and conditions), `UiCondition` |
| Live display | `LiveDisplayState` with the per-control states (`BarGraphState`, `StripChartState`, `VectorState`, `IndicatorState`), `StripChartHistory`, `ChartPalette`, `CustomColorChoices` |
| Forms | `Forms.FormDefinitionGenerator` builds a definition from a type with `FormField` and `FormSection` attributes; `Forms.FormBinding` reads and writes the values back | `Forms.DynamicModelFactory` builds a model with public properties from a `DynamicProperty` list at run time, for forms generated from data. |

## Notes

- No package dependency beyond the framework; the project is net10.0 and has no UI reference.
- `MA0016` (List properties) stays: the serializers need settable concrete lists. `MA0051` stays on the expression parser and the form generator (long switch methods ported as they were).
- The view-model layer with `ICommand` types is the next phase and lives in a separate project.
