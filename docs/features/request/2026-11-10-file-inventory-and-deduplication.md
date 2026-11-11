# Feature request: File inventory and duplicate detection (exact and near-duplicate images)

- **Status:** new
- **Requested by:** owner (filed from the notes repository, `projects/applications/nas-storage-catalog` and `second-shooter-photo-library.md`)
- **Date:** 2026-11-10
- **Area:** new, near `OoBDev.Onnx.ImageEmbeddings`, `OoBDev.Documents` and the blob container request

## Need

Two notes projects both hash and deduplicate files: the NAS storage catalog (documents and programs, Postgres, a work queue) and the second shooter photo library
(photos, a `HashEm` tool in its code). Each plans its own scanner. dotex has image embeddings and document packaging, but a search of `src` and the requests found no
file inventory or duplicate detection.

## Proposed behavior

1. **Inventory:** walk a root without modifying it, record path, size, times and a content hash (streamed, with a chosen algorithm), and report only changes on a later run.
2. **Exact duplicates:** group by size, then hash, and report groups without deleting anything.
3. **Near-duplicate images:** a perceptual hash and, optionally, the existing image embeddings, with a similarity threshold.
4. **Store-agnostic output:** results go through an interface so the NAS catalog can write to Postgres and the photo library to its own store.

## Acceptance criteria

- Given a tree with two identical files in different folders, when scanned, then they are reported as one group and neither file is touched.
- Given an unchanged tree, when scanned a second time, then no file is re-hashed (size and time unchanged).
- Given an image and a resized copy, when scanned with near-duplicate detection on, then they are grouped; unrelated images are not.
- Given a root that is read-only, when scanned, then the scan completes.

## Existing code checked

- Search of `docs/features/request` and `src` for dedup and duplicate: no matches.
- `OoBDev.Onnx.ImageEmbeddings` (embeddings and vector stores) is the nearest piece for near-duplicates. Not read.
- The two notes projects were checked from their pages only; their code was not compared with each other.

## Source references (NAS storage catalog application, added 2026-11-11)

Paths are in the catalog repository `C:epo_storage`; merge from them when this is taken in.

- Hashing and recheck: `src/Storage.Workers/FileHasher.cs`, `Stages/HashStageRunner.cs`, `Duties/HashRecheckDuty.cs`; archive walking `ArchiveWalker.cs`, `Stages/ArchiveStageRunner.cs`; perceptual hash `DHash.cs`.
- Duplicate notes and removal marks (never performed): `src/Storage.Catalog/Owner/RemovalMark.cs`, `ContentMark.cs`; archive overlap `src/Storage.Catalog.Infrastructure/ArchiveOverlapRow.cs`; page `docs/features/pipeline/purge-and-cleanup.md`.

## Security and operations

Reads whole files, so IO cost is large on a NAS; the scan should be throttled and resumable. Paths are listed in output, so reports may contain private names.

## Priority

- **Impact:** one implementation for two projects, and a base for the blob container and packaging work.
- **Needed by:** no date
- **Workaround today:** each project writes its own scanner.

## Open questions

- Is this a library only, or also a command line in `src/Tools`?
- Which hash by default (a fast non-cryptographic one for the first pass, then a strong one to confirm)?
- Does the NAS catalog want this as a package, or keep its own scanner and only share the interfaces?

## Triage notes

Origin: notes repository `projects/dotex-consolidation/README.md` (mapping section). Maintainers: accept, schedule or decline.
