# OoBDev.Tui.TestUtilities

Test helpers shared by the projects that test Terminal.Gui front ends (the DevTerm Tui tests and the Examples Controls Tui tests).

| Type | Purpose |
|------|---------|
| `TuiScreenRenderer.Save(app, path)` | Renders the headless screen buffer to a PNG, cell by cell. Windows only (`System.Drawing.Common`). A cell whose foreground equals its background (the headless driver's unthemed default) is drawn black on white. |

Test projects only; not packaged. The namespace is `OoBDev.Tui...` rather than `OoBDev.Terminal...` because a namespace named `OoBDev.Terminal` would shadow `Terminal.Gui` inside every `OoBDev.*` namespace.
