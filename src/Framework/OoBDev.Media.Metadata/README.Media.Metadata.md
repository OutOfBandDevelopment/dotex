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

Audio tags (`OoBDev.Media.Metadata.Audio`): `AudioTagReader.Read(bytes)` gives `AudioTags` (`Format` id3v2, id3v1 or vorbis; `Values` under `title`, `artist`, `album`, `albumartist`, `track`, `disc`, `date`, `genre`, `comment`) for MP3 (ID3v2.3 and 2.4, then ID3v1), FLAC, Ogg Vorbis and Opus (comment packet in the first 64 KiB, `Format` vorbis) and M4A/MP4 (`ilst` items, `Format` mp4); damaged frames end the read without an exception. Container facts (codec, duration, fingerprint) stay with the audio container.

Dates (`OoBDev.Media.Metadata.Dates`): `PathDateParser.Parse(path, thisYear)` reads the most specific date in any folder or file name (day 0.75, month 0.60, year 0.45, +0.05 in the file name) with a place hint; `DateCombiner.Combine(evidence)` picks the most confident `DateEvidence` (ties to the finer precision), raises confidence for agreeing sources (cap 0.99) and flags a conflict over a year apart. Confidence constants for EXIF and file times are on `DateCombiner`.

Not built yet: a result that reports which tags were damaged, the HEIC Exif item, and XMP and IPTC.

## RAW files (`OoBDev.Media.Metadata.Raw`)

- `RawDetector.IsRaw(head)` recognises RAW by content: CR2, CR3, RAF, ORF, RW2, DNG and TIFF files whose Make is a camera maker and that carry SubIFDs.
- `IRawDeveloper` (`IsRaw`, `DevelopAsync(input, output, RawDevelopOptions)`); `EmbeddedPreviewDeveloper` writes the largest embedded JPEG preview with no native code and throws `NotSupportedException` for PNG, 16 bit or white-balance changes. The true development is `OoBDev.ImageMagick`.
- `WhiteBalance.Scale` is the pure per-channel scaling over an RGB buffer.
