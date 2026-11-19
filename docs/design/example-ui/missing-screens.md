# Example app: missing screens review (2026-10-29)

[Screen designs](screens.md) · [Plan](plan.md) · [Shell design](README.md)

Review of the example web app (`src/Examples/OoBDev.Example.Web`) and API against the framework's features. Built means a routed page with a controller behind it; the router has 21 pages plus two links (API reference, event reference).

## Contents

- [Planned but not built](#planned-but-not-built)
- [Framework areas the plan does not mention](#framework-areas-the-plan-does-not-mention)
- [Built screens with gaps](#built-screens-with-gaps)
- [Recommended order](#recommended-order)

## Planned but not built

**Table 1 — Planned screens still missing**

| Screen | Framework projects | What the screen would show | Needs |
| --- | --- | --- | --- |
| Template management | `OoBDev.TextTemplating`, Handlebars | List, edit, save and preview templates | List, open and preview built 2026-11-09; save built (writes over the repository file, owner 2026-11-09) |
| Identity management | `OoBDev.Identity`, `OoBDev.Identity.Jws`, Keycloak | User lookup, rights (read-only page `/identity/directory` built 2026-11-11; sessions are not covered) | Keycloak admin scope; the host runs the API anonymously today |
| Data access and DacPac builder | `OoBDev.Data.Common`, MongoDB, `OoBDev.DacFx` | Read-only query runner, dacpac build and diff | Read-only guard design |
| Semantic Kernel plug-in tester | `OoBDev.SemanticKernel` | Built 2026-11-17 as `/utilities/kernel`: plug-in function list and a direct invoke with text arguments (no model) | Controller (built) |
| Vector store browser | Qdrant, OpenSearch, SQL store | Collection list, upsert, query | `IVectorStoreCollectionCatalog` (opt-in) exists with the Qdrant, SQL store and Azure AI Search implementations (2026-11-19); the screen is built 2026-11-19 (collection picker on `/utilities/vectors`, `GET api/VectorStore/collections`) |
| Mail receive | MailKit IMAP | Inbox view | Decision in [the plan](plan.md) section 6 |
| Formats and pipelines | `OoBDev.System.IO.Pipelines`, Tar, BCD, bit tools | Pipeline builder (the file inspector is on the Toolbox page since 2026-11-19, as are the Morse, Roman, Base32 and packed BCD codecs are on the Toolbox page since 2026-11-19) | Controller |
| CLI tool catalog | DacPacCompiler, DocumentConverter, FileRagEngine, and the other CLI tools | Built 2026-11-16 as `/utilities/tools` (static list with a filter; no execution from the browser) | Static page |

## Framework areas the plan does not mention

These have no row in [the plan](plan.md) or `screens.md`; whether each deserves a screen is an owner decision.

**Table 2 — Framework features with no screen and no plan**

| Area | Projects | Possible screen |
| --- | --- | --- |
| Query over HTTP | `OoBDev.AspNetCore.OData` (`[OoBDevEnableQuery]`, `QUERY` verb) | Query builder against an `IQueryable` endpoint; needs one in the example API |
| Complex events | `OoBDev.ComplexEvents` (+ DatabaseExtensions, EntityFrameworkCore) | Event stream and rule viewer |
| Spatial and geocoding | `OoBDev.SpatialServices`, `OoBDev.Census.Geocoding` | Address to coordinates, distance between points |
| Data loading | `OoBDev.DataLoader` | Load a file into a table with a preview of the mapping |
| Code generation | `OoBDev.Generations` | Generate from a model and show the output |
| Server-driven UI | `OoBDev.UiDefinitions` (+ ViewModels, Wpf) | A page rendered from a `UiDefinition` returned by the API (the framework's own UI contract is not shown anywhere in the SPA) |
| Token tools | `OoBDev.Identity.Jws` | Built 2026-11-16 as a section of the Toolbox page: sign HS256, read a token, check the signature |
| Instrument protocols | `OoBDev.Nmea`, `OoBDev.Scpi`, `OoBDev.ZWave` | Deferred with the hardware UI |

## Built screens with gaps

**Table 3 — Built screens that fall short**

| Screen | Gap |
| --- | --- |
| Telemetry | Traces from the host-run API do not reach the Aspire dashboard (see `BACKLOG.md`) |
| Current user, Sign in | Fail against the Aspire host, which runs anonymously |
| Developer | API and event references are links only |
| Overview | Health tiles per provider are planned (`screens.md`, Table 2) and not built |

## Recommended order

1. ~~Search~~ (built 2026-11-09 as `/ai/search`).
2. ~~Template management~~ (list, open and preview built 2026-11-09; save built, owner 2026-11-09: writes over the repository file).
3. ~~Queue adapters and blob browser~~ (channel list and received monitor on the Queues screen, stored-documents list on the Documents screen, built 2026-11-09).
4. ~~Rights and roles~~ (built 2026-11-10 as `/identity/rights`), then Identity management (built 2026-11-11 as `/identity/directory`, read-only), because they show the authorization model that every other API surface relies on.
5. ~~Server-driven UI~~ (built 2026-11-10 as `/utilities/server-ui`: bench-supply hand-built definition and a generated shipping form served by `/api/UiDefinitions`).
6. The rest as the owner prioritizes; instrument protocols stay deferred.
