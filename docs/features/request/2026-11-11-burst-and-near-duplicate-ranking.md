# Feature request: Burst grouping and best-shot ranking for photo sequences

- **Status:** triaged
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** new, companion to `2026-11-10-file-inventory-and-deduplication.md`

## Need

Photos taken in a burst are grouped by time and similarity, then ranked (sharpness, eyes open, exposure) so one can be suggested and the rest marked, never deleted. The existing inventory request covers exact and near-duplicate grouping; ranking inside a group is not covered.

## Proposed behavior

A grouping step using capture time plus embedding similarity, a pluggable ranking score set, and an output of group id, rank and reasons.

## Acceptance criteria

- Given five frames within two seconds, when grouped, then they form one burst with a ranked order and reasons.
- Given a ranking, when read, then no file is modified or marked for removal automatically.

## Existing code checked

- `2026-11-10-file-inventory-and-deduplication.md` covers inventory and duplicates.

## Source references (NAS storage catalog application)

Paths are in the catalog repository `C:epo_storage`. They are the code to merge from when this feature is taken in; the catalog will drop its copy and consume the framework version afterward (owner 2026-11-11).

- Ranking and grouping: `src/Storage.Workers/BurstRanker.cs`, `Duties/BurstsDuty.cs`, `DHash.cs`, `QualityLabels.cs`, `Stages/QualityStageRunner.cs`; Python scoring `containers/tools/quality.py`, `quality_service.py`.
- Rows and API: `src/Storage.Catalog.Infrastructure/BurstBestRow.cs`, `BurstMemberRow.cs`, `src/Storage.Api/Images/BurstsController.cs`.

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see `2026-11-11-database-stage-work-queue.md`).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Which signals are default (sharpness first)?

## Triage notes

2026-11-12: superset comparison done (dotex has no grouping, ranking or perceptual hash) and the design written: [BurstRanking](../../design/BurstRanking/README.md); placement is Q-IMAGEANALYSIS-HOME.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
