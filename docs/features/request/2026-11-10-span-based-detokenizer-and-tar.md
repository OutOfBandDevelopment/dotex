# Feature request: span and sequence based Applesoft detokenizer and Tar archive

- **Status:** new
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System` (`Archives/Tar`, `Retro/Apple2/ApplesoftBASIC`)

## Need

`Detokenizer.GetLines` takes `IEnumerable<byte>`, and the Tar code is a legacy port with `byte[]` and P/Invoke helper types. Both allocate more than needed.

## Proposed behavior

Move both to `ReadOnlySequence<byte>`, `ReadOnlySpan<byte>` and `ReadOnlyMemory<byte>`.

## Acceptance criteria

- Given the existing test inputs, then outputs are identical before and after.
- Given a large archive, then allocations drop (measured).

## Existing code checked

`Detokenizer.cs` and `Archives/Tar/*` (for example `TarHeader`, `IOUtilities`).

## Security and operations

Bound lengths when reading headers.

## Priority

- **Impact:** performance and modern style; no new behaviour.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Keep the old overloads or remove them? The framework is unreleased, so breaking changes are fine.

## Triage notes

Origin: BinaryDataDecoders issues 9 and 14. Low priority; accept, schedule or decline.
