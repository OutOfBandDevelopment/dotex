# BinaryDataDecoders Drawing

**Date:** 2026-10-29

`BinaryDataDecoders.Drawing` (850 lines with tests) became `OoBDev.Extensions.Drawing` ([readme](../../src/Extensions/OoBDev.Extensions.Drawing/README.Extensions.Drawing.md)). Breaking changes are accepted, so names changed freely.

**Changes from the source:**
- `Code39`: static; the 128-entry full-ASCII table is generated from the escape rules instead of typed out (spot checks cover each range); `ToStandard` and `ToPattern` are separate, testable steps; the bitmap is locked in a `try/finally`.
- `JpegSegmentReader`: rewritten. The source read the length little-endian (JPEG is big-endian), kept the two length bytes in the count, never stopped (`CanRead` is always true, so it threw at the end of every file) and had no marker-without-length handling. The new reader handles SOI/EOI/RSTn/TEM, stops at EOI, reads scan data until the next real marker, and throws `InvalidDataException` or `EndOfStreamException` on bad or truncated input. The console and file-writing `Entry` method is dropped.
- `PngPack`: static, validates the stored length against the image capacity.
- `ZOrderCurveEx` became `ZOrderCurve` using the standard bit-spreading steps (same results for 16-bit axes).
- `MultiScaleTileEx` (three copies of each method) became `MultiScaleTile` with one implementation on `Image` and thin path and stream overloads; the tile count is computed once. The `Indexed` pixel-format check was removed because it tested a new 32-bit bitmap and could never fire. `MaxLevel` is an `int`. `GraphicsEx` became `SizeExtensions` (`OffSetBy` is `OffsetBy`).
- The source tests were DevLocal image dumps with no assertions; they are replaced by 54 asserting Unit tests.

**Verified:** 54 tests pass on Windows. They were not run on Linux (the bitmap tests end Inconclusive there; `System.Drawing.Common` needs Windows).
