# OoBDev.Scpi.Panels

Builds a control panel from a SCPI instrument profile, so any profile (including the generic one) gets a usable front end without a bespoke form.

## Description

`ScpiPanelBuilder.Build(profile, execute)` returns a `ScpiPanel`:

| Part | What it is |
|------|------------|
| `Definition` | A `UiDefinition` with a section per command category. Each parameter becomes a numeric, slider, text or choice control (from `ScpiParameterDefinition.Control`, else its kind), each command a button whose `ParameterFieldIds` name its fields, each query a reply text field. |
| `Model` | An object built at run time (`DynamicModelFactory`) with one property per field, an `ICommand` per button and a `string` per reply. |
| `Binding` | The `FormBinding` over the model. A renderer uses it, and replies are written through it so `Changed` fires. |

`execute` sends the request and returns the reply text for a query (for example `ScpiReplyTracker.QueryAsync`), or null. A failure is shown in the reply field as `Error: ...`. Control ids come from `ScpiPanelIds` (`{command}_{parameter}`, `{command}_reply`, letters and digits only).

## Notes

- A button packs its field values joined by `,` (the `InvokeCommand` convention), so a text value containing a comma splits into two values.
- `ParameterExpressions` are not generated (a profile has no expressions).
- Not run against a real instrument or rendered in WPF yet; the DevTerm shell wiring (`IPanelFactory`) is the next step.
