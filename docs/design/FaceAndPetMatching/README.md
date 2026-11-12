# Face and pet matching

Status: **design written 2026-11-12 from the owner's answers (the framework takes this on); nothing built.** Request: [face and pet matching](../../features/request/2026-11-11-face-and-pet-matching.md). Related: [ImageEmbeddings](../ImageEmbeddings/README.md) (model cache and ONNX runners), [EmbeddingClientAndPgVector](../EmbeddingClientAndPgVector/README.md), [BurstRanking](../BurstRanking/README.md), [WorkQueue](../WorkQueue/README.md).

## Contents

1. [Need and owner decisions](#need-and-owner-decisions)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need and owner decisions

The catalog detects faces and pets, embeds them and clusters them into people and pet groups. The owner wants this in the framework to cover the storage application, with one common request model, a detailed response, and possibly several stages (owner, 2026-11-12).

**Table 1 — Owner decisions (2026-11-12)**

| Topic | Decision |
|-------|----------|
| Scope | The framework takes it on |
| Models | Run in ONNX from .NET and live in the framework when they can; use the Hugging Face cache and download pattern (`HuggingFaceHubCache`) to reuse models |
| Licences | As MIT-compatible as possible; a model that needs AGPL or LGPL (for example YOLOv10, AGPL-3.0) is maintained and deployed separately, not inside the MIT packages. InsightFace `buffalo_l` weights are restricted to non-commercial research, so they are not shipped and not a default |
| Privacy | Storage is opt-in; delete by person removes every vector, crop and cluster link; owner-confirmed assignments are never changed by automatic clustering |

## Superset comparison

**Table 2 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `Onnx.ImageEmbeddings`, `Vision.*` | ONNX runners, `HuggingFaceHubCache`, DINOv2 and CLIP embedders, cosine 0.995 bar | Base for runners, cache and the pet embedder |
| Catalog faces stage | SCRFD detection and ArcFace embedding (`buffalo_l`) on a thumbnail of at most 2048 px, minimum side 200 | Behaviour to keep; model licence blocks shipping |
| Catalog pets stage | YOLOv10 detection (AGPL) and DINOv2 embedding, species kept | Embedding side is reusable now; the detector is swapped for a permissive one or deployed separately |
| Catalog `ClusterAssigner` (96 lines) | Greedy centroid clustering; faces join the closest centroid at or above a threshold else seed a cluster; pets match within their species, a named group needs a higher threshold, a hand-assigned member weighs more | Base for the clustering service |
| Catalog `AutoClusterDuty` | Runs at most every 15 minutes and per kind only when 25 unclustered items exist; existing cluster ids are kept; named clusters tag new photos | Base for the schedule policy |

## Design

- **Stages as interfaces:** `IFaceDetector`, `IFaceEmbedder`, `IPetDetector`, `IPetEmbedder`, `ICentroidClusterer`. Stages can run alone or chained.
- **Common request and response:** `MatchRequest` (image source, kinds to find: faces, pets, optional species, thresholds, stages to run, optional candidate set to match against) and `MatchResponse` with per-item detail: kind, species, box and landmarks, detector score, embedding (only when asked), cluster id, similarity to the centroid, the nearest candidates, and a *why* note (threshold met, higher-weight confirmed member, seeded a new cluster). A response can come from one stage or all, so work can be split across hosts and resumed.
- **Models:** ONNX files fetched and verified through `HuggingFaceHubCache` with pinned revisions and SHA-256. The framework adapters list each model's licence in the readme and in the model manifest; the permissive models are used first. An AGPL or LGPL detector is a separate deployable (its own repository or container image) that implements the same HTTP form of the interface; the MIT packages never link it.
- **Clustering:** owner-fixed assignments are immutable inputs; the service only assigns unclustered items; thresholds and weights are options; the schedule policy (interval and minimum new items) is an option of a duty in the [WorkQueue](../WorkQueue/README.md) hosting.
- **Privacy:** embeddings are stored only through an opt-in store; delete by person removes vectors, crops and links; logs carry ids and counts, never vectors or crops.
- **Vector storage:** through the pgvector store or any `IVectorStore`; the catalog's face and pet vector spaces map to two spaces with their own kind.

## Migration plan

**Table 3 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Contracts, request/response, clusterer, policy | `Storage.Workers/ClusterAssigner.cs` and duty logic (read-only reference) | new `src/Framework/OoBDev.Imaging.People` | `OoBDev.Imaging.People` | packable, no model files |
| ONNX detectors and embedders | catalog stages | new `src/ExternalServices/Onnx/OoBDev.Onnx.People` | `OoBDev.Onnx.People` | packable adapter on the existing ONNX runners |
| Copyleft detector | catalog YOLOv10 stage | separate repository or container, outside the MIT packages | none | separate deployment |
| Tests | `ClusterAssigner` tests in the catalog | `OoBDev.Imaging.People.Tests` (`Unit`: clusterer, policy, immutability of owner assignments) and `OoBDev.Onnx.People.Tests` (`Integration`/`DevLocal` with downloaded models) | matching | MSTest |

**Table 4 — References that change**

| Project | Change |
|---------|--------|
| Storage catalog | drops its stages and clusterer and consumes the packages (outside this repository) |
| `OoBDev.Onnx.ImageEmbeddings` | exposes its cache and runner types to the new adapter (check visibility) |

## Testing strategy

Unit: owner-confirmed assignments are unchanged after clustering; fewer than the minimum new items runs nothing; a face and a pet are stored with their own kind and vector; a pet matches only its own species; the response carries the *why* note. Integration or DevLocal: detector and embedder output on a fixed sample photo set (no private photos in the repository). Target 85 to 90 percent coverage for the Framework project.

## Not in the first slice

- The permissive detector choice and conversion to ONNX (a spike: licence check, accuracy against `buffalo_l` on a sample set).
- Manual detections and pet claims (catalog duties tied to its screens).
- Sub-group bubbles and threshold tuning.
