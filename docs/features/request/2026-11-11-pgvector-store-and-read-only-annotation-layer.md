# Feature request: pgvector vector store and an owner annotation layer over read-only sources

- **Status:** triaged (pgvector store built 2026-11-16; annotation abstractions open, see the [design](../../design/EmbeddingClientAndPgVector/README.md))
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** `OoBDev.Data.Vectors` (a PostgreSQL store beside `SqlStore`), `OoBDev.Onnx.ImageEmbeddings`, and a small pattern page

## Need

Two things the catalog application does that other dotex applications will want.

**1. A pgvector store.** The catalog holds about 134,000 image vectors (DINOv2), CLIP vectors, face and pet vectors, caption and page-text vectors in PostgreSQL with pgvector and HNSW indexes. `OoBDev.Data.Vectors.SqlStore` is SQL Server; `2026-11-10-postgres-support.md` asks for Postgres support in general but names no vector store.

**2. An annotation layer over a source that must never change.** The application treats a file share as read-only and keeps everything it learns or the owner decides in its own tables: notes and page regions, brightness and crop adjustments applied only on export, links from a derived copy to its original, named saved selections, and duplicate marks. Nothing is written back to the source. The pattern (a stable content key, owner tables beside the catalog, edits applied at export time) is general.

## Proposed behavior

1. **Vector store provider:** the existing `Data.Vectors` store interface with a PostgreSQL implementation (pgvector `vector` column, cosine and L2 operators, HNSW index creation as an explicit, reviewed step because building it blocks writes for a minute or two on 134,000 rows).
2. **Multi-vector items:** a space where one item has many vectors (one per page) and matches by its closest vector, reporting which one matched.
3. **Annotation layer pattern:** a documentation page plus, if worthwhile, small abstractions: an annotation keyed by content hash, a stored adjustment record, and a derivation link. The application's tables can be the reference design.

## Acceptance criteria

- Given the vector store interface tests, when run against a PostgreSQL container with pgvector, then they pass without changes to the shared test set.
- Given 10,000 vectors and an HNSW index, when a nearest-neighbor query runs, then it returns in a bounded time recorded in the test output (host and date).
- Given an item with several vectors, when searched, then the best matching vector's index is returned.
- Given the pattern page, when read, then it states what the layer never does (write to the source) and how edits are applied (at export, not in place).

## Existing code checked

- `OoBDev.Data.Vectors` (19 files), `Data.Vectors.SqlStore`, `Data.Vectors.DB`: SQL Server storage; the store interface was not read in detail, so whether it fits pgvector without change is unknown.
- `OoBDev.Onnx.ImageEmbeddings`, `Vision.Dinov2Small`, `Vision.ClipVitB32`: produce the same embeddings the application stores today (its own container runs them); the reference bar in `docs/design/ImageEmbeddings` is cosine 0.995.
- `2026-11-10-file-inventory-and-deduplication.md`: the inventory side; the annotation layer is its natural companion.
- A search of `src` for Npgsql and pgvector found nothing.

## Source references (NAS storage catalog application)

Paths are in the catalog repository `C:epo_storage`. They are the code to merge from when this feature is taken in; the catalog will drop its copy and consume the framework version afterward (owner 2026-11-11).

- Vector spaces and multi-vector matching: `src/Storage.Query/Metadata/SpaceAttribute.cs`, `SpaceMetric.cs`, `src/Storage.Query/SpaceInfo.cs`, `QueryCompiler.cs`.
- Writers: `src/Storage.Workers/Stages/ImgVecStageRunner.cs`, `EmbedStageRunner.cs`, `PageVecStageRunner.cs`, `Duties/BothVectorsDuty.cs`, `Duties/CaptionVectorsDuty.cs`; database image `containers/pgvector`.
- Annotation layer: `src/Storage.Catalog/Owner/` (`Annotation.cs`, `ImageAdjustment.cs`, `ImageDerivation.cs`, `SavedSelection.cs`, `SelectionDecision.cs`, `ContentMark.cs`, `RemovalMark.cs`); the catalog side `src/Storage.Catalog/Domain/`.

## Security and operations

Vectors of personal photos and faces are personal data; the store provider should not log them. HNSW builds lock writes briefly; the index step is documented, not automatic.

## Priority

- **Impact:** lets the Postgres-based applications use the dotex embedding and search pieces.
- **Needed by:** no date.
- **Workaround today:** the catalog application talks to pgvector directly.

## Open questions

- One request or two (store, pattern)? Recommended: triage them separately.
- Should Postgres support be a single provider package or one package per capability?

## Triage notes

2026-11-12: designed in [EmbeddingClientAndPgVector](../../design/EmbeddingClientAndPgVector/README.md); placement and dependencies confirmed 2026-11-12.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
