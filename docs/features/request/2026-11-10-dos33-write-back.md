# Feature request: DOS 3.3 disk image write support

- **Status:** triaged
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System/Retro/Apple2/Dos33`

## Need

The DOS 3.3 library lists and reads files from a disk image but cannot store data back.

## Proposed behavior

Add, replace and delete files in a DOS 3.3 image: catalog entry, track and sector list, bitmap update.

## Acceptance criteria

- Given an image and a new file, when added and the image reopened, then the file reads back identically.
- Given a full disk, then the write fails and the image is unchanged.

## Existing code checked

`DiskImageCommands`, `CatalogEntry`, `FileEntry` and `AppleFileType` are read-only.

## Security and operations

Write to a copy and replace, so a failure cannot corrupt the source image.

## Priority

- **Impact:** completes the retro file system.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Which operations first (add only, or delete and rename too)?

## Triage notes

Origin: BinaryDataDecoders issue 10. Low priority and specialized; accept, schedule or decline.

2026-11-11: triaged. Accepted at low priority: every BinaryDataDecoders feature is kept (scope rule in `CLAUDE.md`), so it is scheduled behind the DevTerm and event-sourcing work. Backlog: BACKLOG.md, Migrations (specialized formats).
