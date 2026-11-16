# OoBDev.Media.Metadata

Reads EXIF from image files without changing them: no third-party package, no logging (the files are private photos; the caller decides what to record). Design: [docs/design/MediaMetadata](../../../docs/design/MediaMetadata/README.md).

| Type | Use |
|------|-----|
| `ExifLocator.Find(file)` | the TIFF block of a TIFF file, a JPEG (APP1), a PNG (`eXIf`) or a WebP (`EXIF`); empty when there is none |
| `ExifReader.Read(file)` | `ExifFields`: make, model, orientation, taken, latitude, longitude; missing or damaged data gives nulls, never an exception |
| `ExifDump.Read(file)` | every tag of the main, Exif, `GPS.` and `Interop.` directories as `{name: text}` |
| `ExifTagNames` | tag numbers to names (`Tags`, `Gps`) |

```csharp
var fields = ExifReader.Read(File.ReadAllBytes(path));
if (fields.Taken is { } taken) { /* column */ }
var all = ExifDump.Read(bytes);   // { "Make": "Canon", "GPS.GPSLatitude": "(52.0, 30.0, 0.0)", ... }
```

## Rules

- Dump text follows Python's `str()` (shortest round-trip floats, `.0` on whole numbers) so rows stay equal to ones a Pillow-based reader wrote earlier.
- Byte values over 64 print as `<N bytes>`, sequences over 24 as `<N values>`, text is cut at 300 characters and empty values are dropped.
- `Make` and `Model` are trimmed and cut at 100 characters. The capture time is `DateTimeOriginal`, else `DateTime`, without a time zone.
- GPS south and west are negative; a result outside -90 to 90 or -180 to 180, or exactly 0, 0, is dropped.

Not built yet: a result that reports which tags were damaged, the HEIC Exif item, XMP and IPTC, audio tags and date merging.
