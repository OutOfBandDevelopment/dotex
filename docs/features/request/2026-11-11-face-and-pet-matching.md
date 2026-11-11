# Feature request: Face and pet detection, embedding and clustering

- **Status:** triaged
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** new, near `Onnx.ImageEmbeddings` and `Data.Vectors`

## Need

The catalog detects faces and pets in photos, stores their vectors, and clusters them into people, with owner decisions never overridden by automatic clustering (at most every 15 minutes, only with 25 or more new items). No dotex piece does this.

## Proposed behavior

Detection and embedding providers, a vector store for the results, and an incremental clustering service that respects owner-fixed assignments and reports why an item joined a cluster.

## Acceptance criteria

- Given new face vectors, when clustering runs, then owner-confirmed assignments are unchanged.
- Given fewer new items than the threshold, when the schedule fires, then nothing runs.
- Given a face and a pet, when processed, then each is stored with its own kind and vector.

## Existing code checked

- No project or request found for face or pet recognition. Personal data: see Security.

## Source references (NAS storage catalog application)

Paths are in the catalog repository `C:epo_storage`. They are the code to merge from when this feature is taken in; the catalog will drop its copy and consume the framework version afterward (owner 2026-11-11).

- Stages: `src/Storage.Workers/Stages/FacesStageRunner.cs`, `PetsStageRunner.cs`; clustering `src/Storage.Workers/ClusterAssigner.cs`, `Duties/AutoClusterDuty.cs`, `Duties/PetClaimsDuty.cs`, `Duties/ManualDetectionsDuty.cs`.
- Owner model: `src/Storage.Catalog/Owner/` (`Person.cs`, `ContentPerson.cs`, `ManualDetection.cs`, `PersonUnion.cs`, `PersonParent.cs`).
- API: `src/Storage.Api/People/`.

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see `2026-11-11-database-stage-work-queue.md`).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Which detection model and licence? The catalog's choice would be the starting point.

## Triage notes

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
