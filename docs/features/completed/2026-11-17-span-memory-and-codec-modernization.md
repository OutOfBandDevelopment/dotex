# Feature request: Span and Memory support in codecs and segmenters

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#span-support-for-bcd-and-base32-and-a-copy-free-segmenter))
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.System` (`BcdEx`, `Base32Codec`), `OoBDev.System.Abstractions` (segmenters)

## Need

Several low-level helpers allocate arrays where `ReadOnlySpan<byte>` or `Memory<byte>` would avoid copies, and one segmenter handles both endiannesses when only little endian needs native support.

## Proposed behavior

Add `ReadOnlySpan`/`Memory` overloads (or replace the array forms, the framework is unreleased) for BCD and Base32; `StartAndFixLengthSegmenter` supports little endian only and converts big endian input.

## Acceptance criteria

- Given a span input, when BCD or Base32 encodes or decodes, then results equal the array forms and no extra array is allocated for the input.
- Given big endian length bytes, when segmented, then the result equals the little endian result after conversion.

## Existing code checked

`docs/design/DeviceDecoding/api-design.md`, `Jws/api-design.md` use the same codecs. No request covers the modernization.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.System/BcdEx.cs`: "extend to support ReadOnlySpan".
- `src/Framework/OoBDev.System/Codecs/Base32Codec.cs`: "refactor this to use ReadOnlySpan/Memory".
- `src/Framework/OoBDev.System.Abstractions/IO/Segmenters/StartAndFixLengthSegmenter.cs`: "drop the endian check... only support little and convert".

## Security and operations

None.

## Priority

- **Impact:** performance only.
- **Needed by:** no date.
- **Workaround today:** none needed.

## Outcome

- `Base32Codec.Encode(ReadOnlySpan<byte>)` and `Decode(ReadOnlySpan<char>)` replace the array and string forms (arrays and strings convert implicitly, so callers are unchanged); the LINQ and array copies are gone and unpadded input decodes.
- `BcdEx` gained `AsBCD(ReadOnlySpan<byte>)` (packed, big endian, up to 18 digits) and `ToBCD(long, Span<byte>)`; the single-byte forms stay.
- `StartAndFixLengthSegmenter` reads the length field into a stack span and reverses it for big endian: no array, and the endianness option stays (declined to drop it: DevTerm profiles use both).
