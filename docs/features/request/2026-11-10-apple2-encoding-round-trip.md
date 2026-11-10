# Feature request: Apple II text encoding that round-trips the high bit

- **Status:** new
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System/Retro/Apple2/Text`

## Need

`Apple2Encoding.GetChars` masks bit 7 (`& 0x7f`), so bytes to text and back cannot restore the original high-bit values (inverse and flashing text).

## Proposed behavior

Preserve or explicitly model the high bit so string to bytes to string round-trips.

## Acceptance criteria

- Given bytes with and without the high bit, then bytes to string to bytes returns the same bytes.
- Given plain ASCII, then existing behaviour is unchanged.

## Existing code checked

`Apple2Encoding` extends `ASCIIEncoding` and masks in `GetChars`.

## Security and operations

None.

## Priority

- **Impact:** correctness of a retro encoding.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Represent the high bit in the string (private-use characters) or in a side channel?

## Triage notes

Origin: BinaryDataDecoders issue 12. Low priority and specialized; accept, schedule or decline.
