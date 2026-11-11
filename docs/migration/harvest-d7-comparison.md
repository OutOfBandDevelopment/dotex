# D7 harvest candidates — comparison

[↑ Migration plan](README.md#harvest-candidates)

**Status:** Proposal for owner decision · **Date:** 2026-11-10 · Sources under `C:\repo\mine\proving-grounds\examples`. Nothing is copied by this document.

## Contents

- [Summary](#summary)
- [Findings](#findings)
- [Pattern references](#pattern-references)

### List of Tables

1. [Table 1 — Recommendation per candidate](#table-1)
2. [Table 2 — What to extract from the pattern references](#table-2)

## Summary

Compared by file listing and reading the key files against `src`. The covered candidates (markdownplantuml, TotpExample, HandyClasses (Code39, ConsoleEx, XFragment, SMTP, and since 2026-11-11 CsvWriter and IniFile)) are dropped from this list; only open items remain. Oobtainium is already decided (not migrated; see the plan) and `postsql-playground` is not on this disk, so it was not compared.

<a id="table-1"></a>**Table 1 — Recommendation per candidate**

| Candidate | Already in dotex | Recommendation |
|-----------|------------------|----------------|
| BuildFirstOnce | Covered | `DeepClean` and `CleanPaths` already exist in `src/Directory.Build.targets` (same logic, documented in the build and packaging doc); the "build a tool first, once" trick stays a technique only (owner, 2026-11-11) |
| hybridsearch | `OoBDev.Search`, `Qdrant`, `OpenSearch`, `Ollama`, `SBert`, `Documents` via Tika, `Markdig`, `WkHtmlToPdf` | Mostly covered (see [HybridSearch docs](../design/HybridSearch/README.md)). **Decided (owner, 2026-11-11):** Azure AI Search and Neo4j graph adapters are built (backlog); the Go variant stays a reference; the lexical-plus-vector merge is already tracked in `BACKLOG.md` |
| postsql-playground | Not on this disk | Keep as a design input for the PostgreSQL support feature request; compare when the repository is available |

## Findings

- **Small feature to decide (1):** the `DeepClean` build targets. `CsvWriter` and `IniFile` were built in `OoBDev.System.IO` on 2026-11-11.
- **Large and already tracked (1):** the hybrid merge, with the Azure and Neo4j providers as optional later adapters.
- Hybridsearch's Go version is a language port of the same design; no action, except to keep it as a read-only reference.

## Pattern references

`event-sourcing`, `fohjin-cqrs` and `sync-mesh` (`C:epolearning`, read-only) are examples of CQRS, event sourcing and distributed sync, not code to port. Extract knowledge, not source: one reference design in `docs/design/`, and a library only when a consumer appears.

<a id="table-2"></a>**Table 2 — What to extract from the pattern references**

| Source | Extract | Into | Not taken |
|--------|---------|------|-----------|
| `fohjin-cqrs` | The first-principles pattern explanations (`docs/patterns/`), the feature docs as worked domain examples (accounts, transfers, cards), the command/aggregate/read-model split | A CQRS and event sourcing reference design (`docs/design/CqrsEventSourcing/`) as the teaching baseline, linked not copied | The bank application, the three UI clients, the identity provider |
| `event-sourcing` | The write-side and read-side design (append-only store, schema registry, follow API, projections, full-state versus delta events), the governing principle (never lose or corrupt data) and the ADRs on tamper detection and late arrival | The same reference design as the infrastructure target; a Framework library from it only when a consumer appears, design first (`06-design-document-standard.md`). Sanitized copying is allowed (owner, 2026-11-11), with each source recorded in the readme | The GraphQL gateway, DPoP, erasure and other product features |
| `sync-mesh` | The hybrid logical clock (`HlcGenerator`, `HybridLogicalClock`) and ADR-0003; ADR-0006 on event lineage as descriptive provenance | A small `OoBDev.System` feature request (`TimeProvider`, deterministic tests, 80% coverage); the ADR reasoning cited from the reference design | The NATS transport, reverse tunnel, mesh monitor, order-book demo, EF migrations, `FeatureDocExtractor` (generated guides cover it) |

Order: the reference design first (it also settles how the SharedFramework "Complex Events" design relates to `event-sourcing`), then the HLC feature request, then any infrastructure library.
