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
| Data access and DacPac builder | `OoBDev.Data.Common`, MongoDB, `OoBDev.DacFx` | Dacpac half dropped (owner default, Q31 option A: builds and diffs stay in the CLI). Read-only query runner built 2026-11-20 as `/data/access` (`POST api/DataAccess/query`, `SelectOnlyGuard`, `ReadOnlyQueryRunner`); dacpac build and diff still open | Read-only guard decided 2026-11-20 (owner Q22): a dedicated read-only login on a test or reporting instance (`OoBDev:DataAccess:ConnectionString`), `ApplicationIntent=ReadOnly`, one statement that must parse as SELECT (ScriptDom), a row cap and timeout, right `data.query` on admin only; the login permissions are the real guard |
| Semantic Kernel plug-in tester | `OoBDev.SemanticKernel` | Built 2026-11-17 as `/utilities/kernel`: plug-in function list and a direct invoke with text arguments (no model) | Controller (built) |
| Vector store browser | Qdrant, OpenSearch, SQL store | Collection list, upsert, query | `IVectorStoreCollectionCatalog` (opt-in) exists with the Qdrant, SQL store and Azure AI Search implementations (2026-11-19); the screen is built 2026-11-19 (collection picker on `/utilities/vectors`, `GET api/VectorStore/collections`) |
| Mail receive | MailKit IMAP | Inbox view | Built 2026-11-20 as the Inbox section of `/messaging/communications` (`POST api/Communications/inbox/poll`, right `communications.receive`) |
| Formats and pipelines | `OoBDev.System.IO.Pipelines`, Tar, BCD, bit tools | Built 2026-11-19 on the Toolbox page: the file inspector, the Morse, Roman, Base32 and packed BCD codecs and a pipeline builder (hex, base64, base32, gzip, SHA-256 and SHA-512, reverse) | Controller |
| CLI tool catalog | DacPacCompiler, DocumentConverter, FileRagEngine, and the other CLI tools | Built 2026-11-16 as `/utilities/tools` (static list with a filter; no execution from the browser) | Static page |

## Framework areas the plan does not mention

These have no row in [the plan](plan.md) or `screens.md`; whether each deserves a screen is an owner decision.

**Table 2 — Framework features with no screen and no plan**

| Area | Projects | Possible screen |
| --- | --- | --- |
| Query over HTTP | `OoBDev.AspNetCore.OData` (`[OoBDevEnableQuery]`, `QUERY` verb) | Query builder against an `IQueryable` endpoint; `GET api/SampleCatalog` built 2026-11-23 (right `catalog.read`); the screen `/data/query` is built 2026-11-23 with a journey and guide |
| Complex events | `OoBDev.ComplexEvents` (schedule store and runner built; the screen is `/messaging/schedule`, built 2026-11-20) | A schedule of due items with a hosted runner; the rule viewer stays open |
| Spatial and geocoding | `OoBDev.Census.Geocoding`, `OoBDev.SpatialServices` (`GeoMath`) | Address to coordinates built 2026-11-20 as `/data/geocode` (`POST api/Geocode`, right `geocode.use`); distance and bearing built 2026-11-23 as `/data/distance` (`POST api/Distance`, same right) |
| Data loading | `OoBDev.DataLoader` (built; `/data/load`, 2026-11-23) | Load a CSV or JSON text into a throwaway in-memory table and read the report; the mapping preview waits for `docs/design/MasterData` |
| Test data generation | `OoBDev.Generations` (core built 2026-11-19, test-support library; provider hooks and DI registration not built) | Pick a sample type and a seed, show the generated object graph; the same seed gives the same output |
| Server-driven UI | `OoBDev.UiDefinitions` (+ ViewModels, Wpf) | A page rendered from a `UiDefinition` returned by the API (the framework's own UI contract is not shown anywhere in the SPA) |
| Token tools | `OoBDev.Identity.Jws` | Built 2026-11-16 as a section of the Toolbox page: sign HS256, read a token, check the signature |
| Instrument protocols | `OoBDev.Nmea`, `OoBDev.Scpi`, `OoBDev.ZWave` | Deferred with the hardware UI |

## Built screens with gaps

**Table 3 — Built screens that fall short**

| Screen | Gap |
| --- | --- |
| Telemetry | Fixed 2026-11-20: the API now lists the `Microsoft.AspNetCore` and `System.Net.Http` sources, and request traces reach the Aspire dashboard (checked live); the page itself lists the recent traces |
| Current user, Sign in | Sign in shows only when an identity provider is configured; the Current user page says so when the host runs anonymously (2026-11-24) |
| Developer | API and event references are links only |
| Overview | Probes cover eight endpoints (vector store, operations and rights added 2026-11-20); a providers table from `GET api/Operations/health` was built 2026-11-25 (Q40 A); its journey step and screenshot wait for a Playwright run (`screens.md`, Table 2) |

## Recommended order

1. ~~Search~~ (built 2026-11-09 as `/ai/search`).
2. ~~Template management~~ (list, open and preview built 2026-11-09; save built, owner 2026-11-09: writes over the repository file).
3. ~~Queue adapters and blob browser~~ (channel list and received monitor on the Queues screen, stored-documents list on the Documents screen, built 2026-11-09).
4. ~~Rights and roles~~ (built 2026-11-10 as `/identity/rights`), then Identity management (built 2026-11-11 as `/identity/directory`, read-only), because they show the authorization model that every other API surface relies on.
5. ~~Server-driven UI~~ (built 2026-11-10 as `/utilities/server-ui`: bench-supply hand-built definition and a generated shipping form served by `/api/UiDefinitions`).
6. The rest as the owner prioritizes; instrument protocols stay deferred.
