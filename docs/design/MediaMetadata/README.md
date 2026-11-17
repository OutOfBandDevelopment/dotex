# Media metadata (EXIF) reader

Status: **first slice built 2026-11-16** (locator, reader, dump, tag names; placement confirmed by the owner 2026-11-12, new Framework project, option A). Request: [EXIF and media metadata extraction](../../features/completed/2026-11-11-exif-metadata-extraction.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

Read capture time, camera, orientation and GPS (and every other tag) from image files without changing them, so a catalog can store them as queryable columns. The NAS storage catalog (`C:\repo\_storage`) owns a working reader today and will consume the framework version once it exists (owner, 2026-11-11).

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex `OoBDev.Extensions.Drawing` (`JpegSegmentReader`) | Splits a JPEG into marker segments; no tag parsing | Useful for repair, not a reader; stays |
| Storage catalog `Readers/ExifReader.cs` (161 lines) | Finds the TIFF block in JPEG (APP1), PNG (`eXIf`), WebP (`EXIF`) and plain TIFF; typed fields: make, model, orientation, taken, latitude, longitude; unreadable data gives an empty result | Base for `ExifReader` |
| Storage catalog `ExifDump.cs` + `ExifTagNames.cs` (475 lines) | Every tag of IFD0, Exif, GPS and Interop as `{name: text}` with the same text a Python Pillow dump gives (so catalog rows stay equal) | Base for `ExifDump`; the Pillow-style text is kept on purpose |
| Storage catalog `HeicContainer.cs`, `AudioFormats.cs`, `DateCombiner.cs`, `FileDatesDuty.cs` | HEIC Exif item, audio tags, date merging | Later slices, see below |
| Python helpers (`exif_service.py`, `heic_service.py`) | Pillow, pillow-heif and LibRaw for camera raw | Stay a container (no first-party reader for raw); out of scope |

No third-party library is needed: the catalog reader is first-party, BCL-only and small. This answers the request's open question (wrap an existing reader or own code) with own code, recorded as the recommendation.

## Design

- Read-only: input is a `ReadOnlySpan<byte>` (or a stream wrapper later); the source is never opened for writing.
- `ExifLocator.Find(file)` returns the TIFF block of JPEG, PNG, WebP and TIFF files, or empty.
- `ExifReader.Read(file)` returns `ExifFields` (make, model, orientation, taken, latitude, longitude); damaged or missing data gives nulls, never an exception for bad input.
- `ExifDump.Read(file)` returns every tag as `{name: text}`: bytes over 64 become `<N bytes>`, sequences over 24 `<N values>`, text is cut at 300 characters, empty values are dropped.
- Nothing is logged by the library; callers decide (inputs are private photos).

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Reader, dump, tag names, locator | `C:\repo\_storage\src\Storage.Workers` (read-only reference) | new `src/Framework/OoBDev.Media.Metadata` (BCL only, no third-party package) | `OoBDev.Media.Metadata` | new packable Framework project |
| Tests | the catalog has none for these files in the copy read | new `src/Framework/OoBDev.Media.Metadata.Tests` | `OoBDev.Media.Metadata.Tests` | MSTest, `Unit` |
| Readme | none | `README.Media.Metadata.md` | - | - |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| `OoBDev.slnx` | add the two projects |
| `OoBDev.Extensions.Drawing` | none (it keeps `JpegSegmentReader`; no reference either way) |
| Storage catalog | drops its copy and references the package (outside this repository) |

Reusable means Framework (rule `layering-and-migration-plans`): no application-specific names, so the library is generic and sits in the Framework layer.

## Testing strategy

Unit tests build tiny TIFF blocks in memory (little and big endian) and wrap them as JPEG APP1, PNG `eXIf` and WebP `EXIF`, then check the fields, the dump text rules (limits, rationals, empty values), GPS sign handling and out-of-range rejection, and truncated or garbage input (empty result, no exception). Target 85 to 90 percent coverage.

## Not in the first slice

- Reporting which tags were damaged (request acceptance criterion 2): the catalog code skips them silently; add a result type with problems after the port.
- HEIC Exif item, XMP and IPTC, audio tags, date merging: separate slices in the order the catalog needs them.
- Camera raw: stays in the Pillow/LibRaw container.
- Built 2026-11-16 as `OoBDev.Media.Metadata`: `ExifLocator`, `ExifReader` (`ExifFields`), `ExifDump`, `ExifTagNames`, with the catalog's Pillow-style text kept. Differences from the catalog copy: the locator bounds-checks every chunk length instead of relying on a caught exception, one shared TIFF directory walker serves reader and dump, and tag tables are read-only.
