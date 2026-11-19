# Feature request: superset review of features that exist in both dotex and the catalog application

- **Status:** triaged (comparisons written; the built features are listed in the [SearchMerge design](../../design/SearchMerge/README.md))
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** cross-cutting: query layer, vectors, embeddings, search, queue, caching, telemetry, identity and rights, documents

## Need

Where both sides already have a feature, the owner wants a deep comparison, then a version with the union of the capabilities, before the catalog is reintegrated onto the framework (owner 2026-11-11). Without it, the catalog would either keep a private copy or lose a capability on the way.

## Proposed behavior

One comparison page per area in `docs/design` listing capabilities side by side, the gaps each way, the chosen superset and the migration step. Starting list, with the catalog's code to compare:

| Area | dotex side | Catalog side (in `C:\repo\_storage`) |
|---|---|---|
| Query layer and cost limits | `Extensions/OoBDev.AspNetCore.OData` (`ODataQueryLimits`: page size, max top, filter size, expand depth) | `src/Storage.Query/QueryEngine.cs`, `QueryCompiler.cs`, `FilterCompiler.cs`; cost guard to be added from dotex |
| Vectors | `Data.Vectors` (SQL Server CLR), `Qdrant` | pgvector spaces in `src/Storage.Query/Metadata/SpaceAttribute.cs` |
| Embeddings | `Onnx.ImageEmbeddings`, `Onnx.SentenceEmbeddings`, `Vision.*` | `src/Storage.Workers/Stages/ImgVecStageRunner.cs`, `EmbedStageRunner.cs`, `containers/tools/` |
| Search | `Framework/OoBDev.Search` | `src/Storage.Query/SearchParser.cs` |
| Queue | `MessageQueueing` (broker) | `src/Storage.Work/` (pipeline rows) |
| Caching | `Framework/OoBDev.Caching` | `src/Storage.Query.Web/ProbeCache.cs` |
| Telemetry | `Framework/OoBDev.Telemetry` | `src/Storage.ServiceDefaults/ServiceDefaultsExtensions.cs` |
| Rights and command relay | `Identity`, ApplicationRights design | `src/Storage.Query.Web/CommandRelay.cs`, `src/Storage.Catalog/Owner/Access*.cs` |
| Documents and Markdown | `Documents`, `System.Text.Markdown` | `src/Storage.Workers/ExtractConverter.cs`, `HtmlMarkdown.cs` |

## Acceptance criteria

- Given an area, when its page is read, then every capability on either side is listed with where it lives.
- Given a capability missing on one side, when the superset is chosen, then it has a backlog entry on the owning side.
- Given the catalog reintegration for an area, when it starts, then the page names the exact tests that must pass.

## Existing code checked

- dotex side: project names, file counts, `ODataQueryLimits.cs`, and the `Data.Vectors` and `MessageQueueing.Abstractions` file lists. Implementations were not read.

## Security and operations

None; this is documentation work that decides what gets built.

## Priority

- **Impact:** prevents lost capabilities and double implementations.
- **Needed by:** before any catalog reintegration starts.
- **Workaround today:** none.

## Open questions

- One page per area, or one page for all (recommended: one per area, as they are triaged separately)?

## Triage notes

2026-11-12: compared and designed in [SearchMerge](../../design/SearchMerge/README.md). 2026-11-19: first slice built, see the status line.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
