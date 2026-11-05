# Migration plan: guide renderers to a Framework library

[GeneratedGuides](README.md) | Status: **awaiting owner confirmation** (answer Q1, 2026-11-04: option A, a Framework library with a generic name)

Rule `layering-and-migration-plans`: reusable means Framework. `ApiGuideRenderer` and `KsyGuideRenderer` have no DevTerm names, and the Example WebApi guide and the `src/Tools` command-line guides need the same helpers.

**Table 1 — Moves**

| Item | Current location | Target location | Namespace | Package or unit |
|------|------------------|-----------------|-----------|-----------------|
| `ApiGuideRenderer` (OpenAPI and AsyncAPI to markdown) | `src/Applications/DevTerm/OoBDev.DevTerm.TestUtilities` | `src/Framework/OoBDev.Documents.Guides` | `OoBDev.Documents.Guides` | New Framework project, packable, `OoBDev.Documents.Guides` |
| `KsyGuideRenderer` (Kaitai struct to markdown) | same | same | same | same |
| `GuideImage` (screenshot helper, stays only if it has no DevTerm types) | same | stays in DevTerm.TestUtilities unless review shows it generic | - | - |
| New `CliToolGuide` (run a tool, capture command lines and output, write a guide) | does not exist | `OoBDev.Documents.Guides` | `OoBDev.Documents.Guides` | same |
| Tests | `OoBDev.DevTerm.Core.Tests/DeviceManifests/KsyGuideTests.cs`, `OoBDev.DevTerm.Web.Tests/ApiGuideTests.cs` | renderer unit tests in new `src/Framework/OoBDev.Documents.Guides.Tests`; DevTerm tests keep the end-to-end guide generation | - | MSTest, `Unit` |
| Readme | none | `README.Documents.Guides.md` | - | - |

**Table 2 — References that change**

| Project | Change |
|---------|--------|
| `OoBDev.DevTerm.TestUtilities` | add ProjectReference to `OoBDev.Documents.Guides`; remove the moved files |
| `OoBDev.DevTerm.Core.Tests`, `OoBDev.DevTerm.Web.Tests` | `using OoBDev.Documents.Guides;` |
| `OoBDev.Documents.Guides` | references only BCL (`System.Text.Json`); no MSTest, no ASP.NET |
| Example tests and `src/Tools` tests | reference `OoBDev.Documents.Guides` when their guides are added |

Order: create the project and tests, move the files with `git mv`, fix usings, build, run the guide tests, check the generated guides are byte-identical, then add the Example WebApi guide and the CLI helper.
