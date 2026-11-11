# Feature request: EXIF and media metadata extraction

- **Status:** new
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** new, near `Onnx.ImageEmbeddings.Skia` and file inventory

## Need

The catalog reads capture time, camera, lens, exposure, GPS and orientation from images into queryable columns without modifying the files.

## Proposed behavior

A read-only metadata reader interface with an image implementation (EXIF, XMP, IPTC), a normalized model, and tolerance for damaged tags.

## Acceptance criteria

- Given a JPEG with EXIF, when read, then capture time, camera and orientation are returned and the file is unchanged.
- Given damaged tags, when read, then the valid ones are returned and the rest reported.

## Existing code checked

- No metadata reader found by project names; not all packages were read.

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see `2026-11-11-database-stage-work-queue.md`).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Library choice (an existing .NET reader or own code)? Recommended: wrap an existing reader.

## Triage notes
