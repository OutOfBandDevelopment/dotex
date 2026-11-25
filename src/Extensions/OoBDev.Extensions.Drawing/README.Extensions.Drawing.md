# OoBDev.Extensions.Drawing

Bitmap and image helpers on SkiaSharp (cross-platform), migrated from BinaryDataDecoders.Drawing. Bitmaps are `SKBitmap`; sizes and points are the BCL `System.Drawing.Size` and `Point`. The pure helpers (`ZOrderCurve`, `SizeExtensions`, `JpegSegmentReader`, `Code39.ToPattern`, `Code39.ToStandard`) run anywhere.

| Type | Purpose |
|------|---------|
| `Code39` | Code 39 barcodes. `EncodeStandard("HELLO")` and `EncodeFullAscii("Hello World!")` draw a grayscale `SKBitmap` (16 pixels per symbol, 16 high); `ToStandard` applies the full-ASCII escapes; `ToPattern` gives the bit pattern. |
| `JpegSegmentReader` | `GetSegments(path or stream)` lists the marker segments (type, payload) of a JPEG up to the end-of-image marker, for inspecting Exif and other metadata. A scan segment includes its entropy-coded data. |
| `PngPack` | `Pack(bytes)` stores arbitrary bytes in the pixels of a lossless PNG (length header, then data); `Unpack` reads them back. |
| `ZOrderCurve` | `GetValue(Point)` and `GetPoint(uint)` interleave and split the X and Y bits (Morton order), 16 bits per axis. |
| `MultiScaleTile` | Deep-zoom tile pyramid: `GetMaxLevel`, `GetTileCount`, `GetTile`, `GetTileAsBytes` (JPEG) and `GetTileAsStream` on an `SKBitmap`, with path and stream overloads. |
| `VectorSceneRasterizer` | `Rasterize(scene, widthPixels, format)` draws a `VectorScene` (from `OoBDev.Documents.Hpgl`, for example `HpglToSvgConverter.ParseScene`) to PNG or JPEG bytes: white background, anti-aliased 1 px strokes, aspect ratio kept, width 1 to 8192. |
| `SizeExtensions` | `MaxLevel`, `Scale` and `OffsetBy` used by the pyramid. |

```csharp
byte[] png = PngPack.Pack(File.ReadAllBytes("report.pdf"));
byte[] back = PngPack.Unpack(png);

foreach (var segment in JpegSegmentReader.GetSegments("photo.jpg"))
    Console.WriteLine($"{segment.Type:x2} {segment.Length}");
```

Tests: `OoBDev.Extensions.Drawing.Tests` (Unit; they run on any platform).
