# OoBDev.UiDefinitions.Tui

A Terminal.Gui renderer for the UiDefinitions form language, extracted from DevTerm.Tui (step 1 of the [control-example migration plan](../../../docs/design/example-ui/migration-plan.md)).

## Description

`FormRenderer.Build` turns a `FormDefinition` and its binding into Terminal.Gui views (check boxes, text fields with inline errors, option selectors wrapped in `TuiChoice`, lists, buttons, sections with remembered expand state). `FormRow` and `SectionRows` track row positions for scrolling and focus reveal, `TuiCustomWidget` hosts host-supplied widgets, `TuiTextWrap.WordWrap` wraps notes to a column count, and `PickFromList` shows a filterable list dialog.

```csharp
var parts = FormRenderer.Build(app, definition, binding, new TuiFormOptions());
```

References `OoBDev.UiDefinitions` (and `Terminal.Gui`) only; no DevTerm names. Tests: `OoBDev.UiDefinitions.Tui.Tests` (headless `Application`). `SectionExpansionState` now lives in `OoBDev.UiDefinitions`.

## Live display renderers

`CellCharts` renders the bar graph, strip chart and vector plot of a `LiveDisplayState` into a `CellGrid` of `CellGlyph`s (braille dots through `BrailleCanvas`), and `CellCanvasView` paints a grid cell by cell, each in its own color. They moved here from DevTerm.Tui (owner, 2026-11-06, Q8 option A). The two theme inputs (muted ink and the dark chart palette) come from `CellChartStyle.Source`; without a source the charts use grey on the light palette, and DevTerm points it at its active theme. Tests: `CellChartsTests` and `CellChartStyleTests`.
