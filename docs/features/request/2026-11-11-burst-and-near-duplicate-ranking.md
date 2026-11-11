# Feature request: Burst grouping and best-shot ranking for photo sequences

- **Status:** new
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

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see `2026-11-11-database-stage-work-queue.md`).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Which signals are default (sharpness first)?

## Triage notes
