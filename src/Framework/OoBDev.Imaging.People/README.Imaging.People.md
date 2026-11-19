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
