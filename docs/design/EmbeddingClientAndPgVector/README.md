# Embedding HTTP client, pgvector store and annotation layer

Status: **first slice built 2026-11-16 (HTTP embedding client and pgvector store, two ExternalServices projects per owner placement 2026-11-12; Npgsql and Pgvector in the store only); the items under "Not in the first slice" are open.** Requests: [embedding container and client](../../features/request/2026-11-11-embedding-container-and-client.md), [pgvector store and read-only annotation layer](../../features/completed/2026-11-11-pgvector-store-and-read-only-annotation-layer.md). Related: [ImageEmbeddings](../ImageEmbeddings/README.md), [VectorSearch](../VectorSearch/README.md), [WorkQueue](../WorkQueue/README.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

Applications want vectors from a configurable HTTP embedding server (a container, or any OpenAI-compatible server), stored in PostgreSQL with pgvector, with items that own several vectors (one per page), and a documented way to keep owner decisions about a read-only source in separate tables.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `OoBDev.AI.Abstractions` `IEmbeddingProvider` | Text embedding (`GenerateEmbeddingAsync`, `Length`) | Text contract to implement for HTTP; there is no image variant |
| dotex `Onnx.ImageEmbeddings`, `Onnx.SentenceEmbeddings`, `Vision.*` | In-process ONNX providers, cosine 0.995 bar | Already the preferred path; the catalog's imgvec stage already calls an in-process `IImageEmbedder` |
| dotex `Ollama`, `SBert` | Embedding over HTTP to a specific server | Reference for options and timeouts; no generic OpenAI-compatible client found (`v1/embeddings` has no hits in `src`) |
| dotex `OoBDev.Search.Abstractions` `IVectorStore` | Store, list, nearest neighbours, group-by | Fits single-vector spaces; a match explanation (which vector, distance) and multi-vector items are not expressible |
| dotex `Data.Vectors.SqlStore`, `Qdrant` | SQL Server and Qdrant stores | Siblings for a PostgreSQL store; no Npgsql or pgvector in `src` |
| Catalog tools container (`containers/tools`) | Python HTTP server for exif, heic, media and quality (no embeddings) | Not an embedding server: the embedding stages use ONNX in process, so the request's container premise is partly out of date |
| Catalog `SpaceAttribute` and halfvec SQL | Metric, multi-vector `PageColumn`, halfvec cast | Base for the pgvector store and match note |
| Catalog annotation tables | Notes, regions, adjustments applied at export, derivation links, saved selections | Pattern page; the tables are the reference design |

## Design

- **Embedding client:** `OpenAiCompatibleEmbeddingProvider : IEmbeddingProvider` with options (base URL, model, dimensions, timeout, batch size, API key from configuration), registered with `TryAdd*`; an image variant `IImageEmbeddingProvider` posting bytes with the same response shape. ONNX providers implement the same interface so configuration chooses.
- **pgvector store:** `PgVectorStore : IVectorStore` (cosine and L2 operators); HNSW index creation is an explicit, separate method that is never run implicitly; a halfvec option.
- **Multi-vector items:** an extension of the store contract (`FindNeighborsWithExplanationAsync`) returning the matched vector index and distance; the explanation type is shared with [HybridSearch](../HybridSearch/README.md).
- **Annotation layer:** a documentation page under `docs/design` stating that the source is never written, edits apply at export, and listing the reference tables (annotation by content hash, adjustment record, derivation link). Small abstractions only if a second consumer appears.
- Inputs are private files: log identifiers and counts only.

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| HTTP embedding client | none (new) | `src/ExternalServices/OoBDev.Embeddings.Http` (built) | `OoBDev.Embeddings.Http` | packable |
| pgvector store | catalog SQL (reference only) | `src/ExternalServices/OoBDev.Data.Vectors.Postgres` (built) | `OoBDev.Data.Vectors.Postgres` | packable, Npgsql and pgvector here only |
| Annotation pattern page | catalog tables | [`docs/design/AnnotationLayer/`](../AnnotationLayer/README.md) (written 2026-11-18) | none | docs |
| Tests | none | `Unit` (fake HTTP handler, request/response shape), `Integration` (PostgreSQL with pgvector service in the stack; Ollama for a real server) | matching | MSTest |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| Test stack | gains a pgvector PostgreSQL service (shared with the work queue tests) |
| Storage catalog | consumes the packages (outside this repository) |

## Testing strategy

Unit: batch vectors come back in request order with the configured dimension; options validation; a non-200 response maps to a clear exception without logging content. Integration: the shared vector store tests run against pgvector unchanged; 10,000 vectors with HNSW return in a bounded time recorded with host and date; multi-vector search returns the best vector's index; ONNX versus HTTP cosine is at least 0.995 on a fixed sample against Ollama or another server. Target 85 to 90 percent coverage.

## Not in the first slice

- The container image and compose file (nothing in the catalog is an embedding server; revisit if the owner wants one published).
- Annotation abstractions in code.
- The image variant (`IImageEmbeddingProvider` posting bytes) and the ONNX providers implementing the shared interface. Deferred 2026-11-16: no server in the stack or the catalog speaks an image wire format (OpenAI has no image embeddings route), so a contract written now could not be tested against a real server; build it with the first server that needs it.
- A shared explanation type with HybridSearch: `VectorMatch` (vector index and distance) lives in the pgvector project until HybridSearch needs it.
- ONNX versus HTTP cosine comparison: built 2026-11-19 (`OnnxVersusHttpTests`, Integration, against the stack's Ollama `all-minilm`): five sentences, lowest cosine 1.00000, bar 0.995. Whether a dedicated embedding image is still wanted is Q12.
