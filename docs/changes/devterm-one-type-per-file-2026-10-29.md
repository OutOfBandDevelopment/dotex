# DevTerm: one top-level type per file (2026-10-29)

The MA0048 splits for the DevTerm projects: 19 files that held more than one top-level type were split (Core: `Rfc2217PortSettings`; Web: `PendingConfirm`, `InvokeRequest`, `OpenedConnection`; Tui: the `*WindowParts` records, `TuiFormOptions`, `TuiCustomWidget`, `TuiChoice`, `TuiFormParts`, `SectionRows`, `FormRow`, `CellGlyph`, `BrailleCanvas`; Tui.Tests: `LoopbackRule`, `LoopbackGenerators`, `LoopbackScript`, `TuiLayoutOptions`; Core.Tests: `PythonProbe`, `PluginTrustProcessTests`). Moves only, no behavior change; each new file keeps the original usings and namespace.
Verified: Core 1055, Tui 420, Web 47 Unit tests pass (1 Tui and 5 Core skipped).
