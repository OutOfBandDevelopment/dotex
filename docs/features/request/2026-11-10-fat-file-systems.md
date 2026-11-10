# Feature request: FAT12, FAT16 and FAT32 file system readers

- **Status:** new
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System` (`IO`, next to `Iso9660`)

## Need

The framework reads ISO 9660 images but no FAT volumes, so FAT disk, floppy and SD card images cannot be browsed.

## Proposed behavior

A read-only FAT reader: boot sector, FAT chains, directory entries with long names, file streams. Same shape as the ISO 9660 reader.

## Acceptance criteria

- Given a FAT12, FAT16 and FAT32 image, when listed, then directories and file sizes match a reference tool.
- Given a corrupt or cyclic FAT chain, then the reader stops with a clear error and does not loop.

## Existing code checked

`src/Framework/OoBDev.System/IO/Iso9660`; no FAT code anywhere in `src`.

## Security and operations

Images are untrusted input: bound every read, cluster count and chain length.

## Priority

- **Impact:** completes the file system set; useful for retro and embedded images.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Read-only first, or write support later?

## Triage notes

Origin: BinaryDataDecoders issue 18. Low priority and specialized; accept, schedule or decline.
