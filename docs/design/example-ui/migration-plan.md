# Control examples: renderer migration plan

Status: confirmed by the owner on 2026-11-05 (Q1 and Q2, option A). Steps 1 to 4 done 2026-11-06.

Why: the Tui and Blazor control examples (`src/Examples`) need a renderer for the UiDefinitions form language. An example cannot reference an Applications project, and the Terminal.Gui renderer has no DevTerm-specific names, so it is reusable and belongs in a library (rule `layering-and-migration-plans`).

**Table 1 — Projects**

| Project | Current location | Target location | Namespace | Package or deployment unit | References that change |
|---------|------------------|-----------------|-----------|----------------------------|------------------------|
| `FormRenderer` (class and `FormRendererTests`) | `src/Applications/DevTerm/OoBDev.DevTerm.Tui` and `.Tui.Tests` | new `src/Framework/OoBDev.UiDefinitions.Tui` and `.Tests` | `OoBDev.UiDefinitions.Tui` | `OoBDev.UiDefinitions.Tui` (library, Terminal.Gui reference) | `DevTerm.Tui` references the new library; `Directory.Packages.props` unchanged (Terminal.Gui already central) |
| Blazor renderer | none | new `src/Extensions/OoBDev.UiDefinitions.Blazor` (Razor class library) and `.Tests` (bUnit) | `OoBDev.UiDefinitions.Blazor` | `OoBDev.UiDefinitions.Blazor`, optional and host-bound | references `OoBDev.UiDefinitions.ViewModels`; adds `Microsoft.AspNetCore.App` framework reference and bUnit to the central packages |
| Tui control example | none | new `src/Examples/OoBDev.Examples.Controls.Tui` and `.Tests` | `OoBDev.Examples.Controls.Tui` | sample app | references `OoBDev.UiDefinitions.Tui` |
| Blazor control example | none | new `src/Examples/OoBDev.Examples.Controls.Blazor` and `.Tests` | `OoBDev.Examples.Controls.Blazor` | sample app | references `OoBDev.UiDefinitions.Blazor` |

Order: (1) move `FormRenderer` with its tests, DevTerm.Tui tests stay green; (2) Tui example mirroring `OoBDev.Examples.Controls.Wpf`; (3) Blazor renderer with bUnit tests; (4) Blazor example. Each step ends with a readme (`README.{Project}.md`), the solution entries, a build and the tests, and a change document.
