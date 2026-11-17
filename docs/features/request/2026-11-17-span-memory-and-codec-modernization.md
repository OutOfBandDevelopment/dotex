# Feature request: Span and Memory support in codecs and segmenters

- **Status:** triaged
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

## Open questions

Keep the array overloads or remove them (breaking changes are allowed)? Recommendation: keep one span-based API.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
