# Feature request: EXIF and media metadata extraction

- **Status:** done (first slice built 2026-11-16, verified 2026-11-17, see [change](../../changes/2026-11-17.md#midi-and-exif-requests-moved-to-completed))
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

- Superset comparison done 2026-11-12: dotex has only `JpegSegmentReader`; the catalog reader is the base. See [MediaMetadata](../../design/MediaMetadata/README.md). Library choice: own code, no package.

## Source references (NAS storage catalog application)

Paths are in the catalog repository `C:epo_storage`. They are the code to merge from when this feature is taken in; the catalog will drop its copy and consume the framework version afterward (owner 2026-11-11).

- Stages: `src/Storage.Workers/Stages/ExifStageRunner.cs`, `ImgMetaStageRunner.cs`, `AudioMetaStageRunner.cs`; readers `ExifDump.cs`, `ExifTagNames.cs`, `HeicContainer.cs`, `ImageFormats.cs`, `AudioFormats.cs`, `DateCombiner.cs`; duty `Duties/FileDatesDuty.cs`.
- Python helpers: `containers/tools/exif_service.py`, `heic_service.py`.
- Model: `src/Storage.Catalog/Domain/Content.cs`.

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see `2026-11-11-database-stage-work-queue.md`).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Library choice (an existing .NET reader or own code)? Recommended: wrap an existing reader.

## Triage notes

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.

## Outcome

`OoBDev.Media.Metadata` (`src/Framework`) locates the EXIF block in JPEG, PNG and WebP, reads capture time, camera model and orientation without touching the source, and returns what is valid when tags or blocks are damaged (73 Unit tests passed 2026-11-17). Later slices (HEIC Exif item, audio tags, date merging) stay in the [design](../../design/MediaMetadata/README.md) and `BACKLOG.md`.
