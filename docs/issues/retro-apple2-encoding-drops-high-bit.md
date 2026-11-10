# Apple II encoding drops the high bit

- **Status:** open, confirmed by code reading on 2026-11-10 (`Apple2Encoding.cs:43`)
- **Feature document:** none yet (specialized retro feature, packaged separately); the decision below goes into the Apple II project readme when fixed
- **Backlog line:** `BACKLOG.md`, section 5 (Code quality backlog)
- **Origin:** BinaryDataDecoders issue 12

## Problem

`Apple2Encoding.GetChars` masks bit 7 (`i & 0x7f`), so bytes to text and back cannot restore the original values. On the Apple II the high bit selects normal text; inverse and flashing text use the low codes, so that information is lost on decode. `Apple2Encoding` extends `ASCIIEncoding`.

## Options

1. Map inverse and flashing ranges to distinct characters (for example private-use code points) so the string carries the attribute and `GetBytes` maps them back.
2. Keep the string plain ASCII and expose the attributes through a side channel (a second span or a result type).

Option 1 keeps the `Encoding` contract (one string in, one byte array out) and round-trips with the existing APIs. Option 2 keeps strings readable but needs a new API. The owner has not chosen; take option 1 unless told otherwise, because it changes no signature.

## Done when

- Bytes with and without the high bit round-trip: bytes to string to bytes returns the same bytes (a test covers all 256 values).
- Plain ASCII text behaves as before.
- The choice is recorded in `README.System.md` (or the project readme that covers `Retro/Apple2`), then this file is deleted.
