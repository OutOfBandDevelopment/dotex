# OoBDev.Imaging.People

Clustering of detected faces and pets into people and pet groups. Pure logic: it takes embeddings and returns assignments, so it needs no model files, store or host. Detectors and embedders are separate adapters ([design](../../../docs/design/FaceAndPetMatching/README.md)).

| Type | Purpose |
|------|---------|
| `ClusterItem` | A face or pet: id, embedding, current cluster (null when unclustered), species (pets), confirmed flag |
| `CentroidClusterer.Faces` | An unclustered face joins the closest centroid at or above `FaceThreshold`, else seeds a new cluster |
| `CentroidClusterer.Pets` | Same inside the pet's own species; a named cluster needs `PetNamedThreshold`; a member the owner assigned by hand weighs `ConfirmedWeight` |
| `ClusterAssignment`, `ClusterResult` | One entry per newly assigned item with target cluster (`-k` for the k-th new cluster), similarity and a *why* note |
| `ClusterSchedulePolicy.ShouldRun` | A run is due after `MinInterval` (15 minutes) once `MinNewItems` (25) items wait |
| `PeopleClusteringOptions` | Thresholds, weight and schedule values (defaults taken from the catalog application) |

Items that already have a cluster are never reassigned, so an assignment the owner confirmed does not change. Ported from the storage catalog's `ClusterAssigner` (behaviour kept, with similarity and notes added).

## Stages and the match pipeline

The detect, embed and cluster stages are interfaces an adapter implements; the framework ships no model (owner Q22: the application supplies detectors and embedders).

| Type | Purpose |
|------|---------|
| `IFaceDetector`, `IPetDetector` | Find faces or pets in an encoded image (a pet detection names its species) |
| `IFaceEmbedder`, `IPetEmbedder` | Embed one detection |
| `DetectedItem`, `ImageBox`, `ImagePoint` | A detection: kind, box, score, species, landmarks |
| `MatchRequest` | Image, kinds, species filter, stages, items of an earlier run, candidate set, named pet clusters, thresholds, `IncludeEmbeddings`, `NearestCount` |
| `MatchPipeline` | Runs the requested stages in order; any adapter can be left out when its stage is not asked for, and a missing one fails with a message naming it |
| `MatchResponse`, `MatchedItem`, `NearCandidate` | Per item: detection, embedding (only when asked), cluster, similarity, *why* note, nearest candidates; counts of new clusters |

A run may do one stage (for example detect on one host) and a later run continues from the returned items (`MatchRequest.Items`), so the work can be split and resumed. Embeddings are left out of a response unless `IncludeEmbeddings` is set; they are private data.
