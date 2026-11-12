# RAW camera file development

Status: **design written 2026-11-12; the native provider is a third-party choice that waits for the owner (Q-RAW-PROVIDER in `OPEN-QUESTIONS.md`), nothing built.** Request: [RAW camera file development](../../features/request/2026-11-11-raw-camera-file-development.md).

## Contents

1. [Need](#need)
2. [Comparison](#comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

Embeddings, histograms and thumbnails need a decoded RGB image; RAW files (Nikon `.nef`, Canon `.cr2`, `.arw`, `.dng`) cannot be fed to them. A host needs to detect RAW by content and develop it to RGB or JPEG without touching the original.

## Comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex | No RAW decoder, no LibRaw, rawpy or ImageMagick dependency; `ImageMagick` is absent from the package set; Skia is not known to decode NEF | New |
| Storage catalog | No RAW code named in the request (the earlier attempts were a hard-coded ImageMagick loop and a Python `rawpy` notebook, neither a library) | Nothing to merge |
| TIFF container reader in the [MediaMetadata](../MediaMetadata/README.md) design | NEF, CR2, ARW and DNG are TIFF-based, so the container header and the embedded full-size JPEG preview are readable with no native code | Basis for detection and for the preview fast path |

## Design

- `IRawDeveloper` with `IsRaw(stream)` (content based: TIFF header plus maker and RAW markers, or the Fuji and Canon CR3 box headers) and `DevelopAsync(input, output, RawDevelopOptions)`; options hold output format (JPEG or PNG), JPEG quality, 8 or 16 bit, and white-balance scales for the three channels.
- Two providers behind it: an always-available `EmbeddedPreviewDeveloper` (extracts the camera's full-size JPEG preview, no native code, not a true development), and a native provider (LibRaw through a wrapper) that does the real demosaic. A host without the native provider gets a clear `NotSupportedException` for a true development.
- The source is read only; output goes to a separate stream or path.
- The native provider is a separate adapter project so Framework never carries native code.

## Migration plan

**Table 2 — Projects**

| Project | Target location | Namespace | Package or unit |
|---------|-----------------|-----------|-----------------|
| Interface, options, detection, preview developer | the project chosen in Q-MEDIAMETADATA-HOME (TIFF reader shared) | `OoBDev.Media.Metadata.Raw` | with that project |
| Native LibRaw adapter | new `src/ExternalServices/OoBDev.Raw.LibRaw` (after approval) | `OoBDev.Raw.LibRaw` | separate package, native assets |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| Native adapter | references the interface project and the approved wrapper package |
| Everything else | none |

## Testing strategy

Detection: a TIFF-based RAW header is RAW, a plain JPEG and a plain TIFF are not. Preview developer: a synthetic TIFF container with an embedded JPEG yields that JPEG and the source bytes are unchanged. Unsupported: true development without the native provider throws the clear message. White-balance scale 1, 1, 1 leaves the pixels equal and one changed scale changes only that channel (tested on the pure scaling function over an RGB buffer). The native provider is a `DevLocal` test on a real `.nef` that records host and date. Target 85 to 90 percent coverage for the managed code.

## Not in the first slice

- The native provider itself (needs the owner's package choice).
- Colour profiles and lens corrections.
- Batch conversion and the work queue (see [work queue](../../features/request/2026-11-11-database-stage-work-queue.md)).
