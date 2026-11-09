# Example app: missing screens review (2026-10-29)

[Screen designs](screens.md) · [Plan](../../../future-example.md) · [Shell design](README.md)

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
| Template management | `OoBDev.TextTemplating`, Handlebars | List, edit, save and preview templates | List, open and preview built 2026-11-09; save waits for OPEN-QUESTIONS Q2 |
| Identity management | `OoBDev.Identity`, `OoBDev.Identity.Jws`, Keycloak | User lookup, rights, sessions | Keycloak admin scope; the host runs the API anonymously today |
| Queue adapters and blob browser | RabbitMQ, SQS, Service Bus, Storage queues, Azure Storage containers | Provider status, receive monitor, container browser | Queue receive endpoint, blob list endpoint (Azurite and Moto exist in the test stack) |
| Data access and DacPac builder | `OoBDev.Data.Common`, MongoDB, `OoBDev.DacFx` | Read-only query runner, dacpac build and diff | Read-only guard design |
| Semantic Kernel plug-in tester | `OoBDev.SemanticKernel` | Plug-in list and invoke | Controller |
| Vector store browser | Qdrant, OpenSearch, SQL store | Collection list, upsert, query | Collection listing on the vector store interface |
| Mail receive | MailKit IMAP | Inbox view | Decision in `future-example.md` section 6 |
| Formats and pipelines | `OoBDev.System.IO.Pipelines`, Tar, BCD, bit tools | File inspector, pipeline builder | Controller |
| CLI tool catalog | DacPacCompiler, DocumentConverter, FileRagEngine, and the other CLI tools | Catalog with run instructions (no execution from the browser) | Static page |

## Framework areas the plan does not mention

These have no row in `future-example.md` or `screens.md`; whether each deserves a screen is an owner decision.

**Table 2 — Framework features with no screen and no plan**

| Area | Projects | Possible screen |
| --- | --- | --- |
| Query over HTTP | `OoBDev.AspNetCore.OData` (`[OoBDevEnableQuery]`, `QUERY` verb) | Query builder against an `IQueryable` endpoint; needs one in the example API |
| CGI hosting | `OoBDev.AspNetCore.Cgi` (+ OpenApi; referenced by the example API) | Script list, run a script, see the OpenAPI it produced |
| Complex events | `OoBDev.ComplexEvents` (+ DatabaseExtensions, EntityFrameworkCore) | Event stream and rule viewer |
| Spatial and geocoding | `OoBDev.SpatialServices`, `OoBDev.Census.Geocoding` | Address to coordinates, distance between points |
| Data loading | `OoBDev.DataLoader` | Load a file into a table with a preview of the mapping |
| Code generation | `OoBDev.Generations` | Generate from a model and show the output |
| Server-driven UI | `OoBDev.UiDefinitions` (+ ViewModels, Wpf) | A page rendered from a `UiDefinition` returned by the API (the framework's own UI contract is not shown anywhere in the SPA) |
| Rights and roles | `[ApplicationRight]`, claims transformation | Who can call what: rights per endpoint and per role |
| Token tools | `OoBDev.Identity.Jws` | Decode and verify a JWS |
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
2. ~~Template management~~ (list, open and preview built 2026-11-09; save waits for Q2).
3. Queue adapters and blob browser, reusing the Azurite and Moto stack.
4. Rights and roles, then Identity management, because they show the authorization model that every other API surface relies on.
5. Server-driven UI, since it is the framework's own UI contract.
6. The rest as the owner prioritizes; instrument protocols stay deferred.
