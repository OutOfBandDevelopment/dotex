# Drawing on SkiaSharp (2026-11-03)

Closes the "Drawing cross-platform" backlog item (owner, 2026-10-29: Windows only for now, SkiaSharp later) and Table 4 row 3 of the [BinaryDataDecoders re-review](../migration/binarydatadecoders-rereview-2026-10-29.md).

## Done

- `Code39`, `MultiScaleTile` and `PngPack` use `SKBitmap`, `SKCanvas` and `SKImage` instead of `System.Drawing.Common` (`Image`, `Bitmap`, `Graphics`); `[SupportedOSPlatform("windows")]` is gone and the library has no `System.Drawing.Common` reference. `SkiaSharp` and `SkiaSharp.NativeAssets.Linux.NoDependencies` come from `Directory.Packages.props`.
- `System.Drawing.Size` and `Point` stay (BCL primitives), so `SizeExtensions` is unchanged; `ZOrderCurve` and `JpegSegmentReader` were already platform-free.
- Behaviour kept: Code 39 bitmaps are 16 pixels per symbol and 16 high (now `Gray8`, bit 1 white), tiles are JPEG quality 75, `PngPack` keeps the 4-byte length header and BGRA pixel layout (unpremultiplied, so every byte round-trips).
- New behaviour: path and stream overloads throw `InvalidDataException` for data that is not an image; stream overloads leave the caller's stream open.

## Verified

`dotnet test src/Extensions/OoBDev.Extensions.Drawing.Tests`: 55 passed, 0 skipped (the Windows-only `Assert.Inconclusive` guards are removed). Run on Windows only; the Linux native asset is referenced but a Linux run was not performed here.
