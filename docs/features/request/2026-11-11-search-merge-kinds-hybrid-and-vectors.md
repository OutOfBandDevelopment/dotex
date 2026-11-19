# Feature request: merge the catalog search model with dotex Search (hybrid ranking, kinds, multi-vector)

- **Status:** triaged (first slice built 2026-11-16: request model, fusion, hybrid, vector, keyword and SQL-kind providers; see the [design](../../design/SearchMerge/README.md))
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** `OoBDev.Search`, `OoBDev.Search.Abstractions`, `Data.Vectors`; companion to `2026-11-11-metadata-driven-query-engine.md`

## Need

dotex Search has providers, a hybrid provider, document summaries and a vector store factory. The catalog has a declared-kind query model, a text language, similarity spaces (including many vectors per item with a "why it matched" note) and keyset paging. Neither is a superset. The owner wants one search concept that has both.

## Proposed behavior

1. A deep comparison first (see `2026-11-11-superset-review-of-overlapping-features.md`), written down as a table of capabilities per side.
2. One search request model: free text, field filters, `like`/`near` anchors, sort and cursor, valid for any provider.
3. Providers behind it: keyword, vector (single and multi-vector), hybrid with a documented score fusion, and the catalog's SQL kinds as one provider.
4. A match explanation in every result (which field, which vector, distance).

## Acceptance criteria

- Given a text query and a vector anchor, when run through the hybrid provider, then the result carries both scores and the fusion used.
- Given an item with several vectors, when searched, then the best vector's index appears in the explanation.
- Given the same request against a keyword provider and the SQL kind provider, when the data is equal, then the filters mean the same.

## Existing code checked

- `Framework/OoBDev.Search` (`Providers/HybridProvider.cs`, `SearchProvider.cs`, `Semantic/`) and `OoBDev.Search.Abstractions`: names only; the fusion method and request shape were not read.

## Source references (NAS storage catalog application)

Paths are in `C:\repo\_storage`.

- Text language and parser: `src/Storage.Query/SearchParser.cs`, `SearchOperator.cs`, `QueryText.cs`, `ParsedSearch.cs`; page `docs/features/query-host/query-language.md`.
- Vector spaces: `src/Storage.Query/Metadata/SpaceAttribute.cs`, `SpaceMetric.cs`, `SpaceInfo.cs`, `QueryCompiler.cs`.
- Tests: `tests/Storage.Query.Tests`.

## Security and operations

Search reads only. Per-kind rights apply (see the query engine request).

## Priority

- **Impact:** one search concept for every application on the framework.
- **Needed by:** no date
- **Workaround today:** two separate implementations.

## Open questions

- Fusion method for hybrid scores (reciprocal rank fusion is the usual default).

## Triage notes

2026-11-12: compared and designed in [SearchMerge](../../design/SearchMerge/README.md). 2026-11-19: first slice built, see the status line.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
