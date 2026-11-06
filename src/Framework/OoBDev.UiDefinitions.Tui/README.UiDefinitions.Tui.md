# OoBDev.UiDefinitions.Tui

A Terminal.Gui renderer for the UiDefinitions form language, extracted from DevTerm.Tui (step 1 of the [control-example migration plan](../../../docs/design/example-ui/migration-plan.md)).

## Description

`FormRenderer.Build` turns a `FormDefinition` and its binding into Terminal.Gui views (check boxes, text fields with inline errors, option selectors wrapped in `TuiChoice`, lists, buttons, sections with remembered expand state). `FormRow` and `SectionRows` track row positions for scrolling and focus reveal, `TuiCustomWidget` hosts host-supplied widgets, `TuiTextWrap.WordWrap` wraps notes to a column count, and `PickFromList` shows a filterable list dialog.

```csharp
var parts = FormRenderer.Build(app, definition, binding, new TuiFormOptions());
```

References `OoBDev.UiDefinitions` (and `Terminal.Gui`) only; no DevTerm names. Tests: `OoBDev.UiDefinitions.Tui.Tests` (headless `Application`). `SectionExpansionState` now lives in `OoBDev.UiDefinitions`.
