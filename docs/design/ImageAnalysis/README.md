# Image analysis (histograms, colour facets, exposure)

Status: **design written 2026-11-12; placement confirmed by the owner 2026-11-12 (new Framework project, option A), nothing built.** Request: [image colour analysis and histograms](../../features/request/2026-11-11-image-color-analysis-and-histograms.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Later slices](#later-slices)

## Need

Whole-image analysis for a catalog and for burst ranking: per-channel and luminance histograms, colour facets (dominant hue, mean saturation and value, small palette) and exposure metrics (mean luminance, clipped black and white, contrast). Pure functions over decoded pixels, no image library in the analysis code.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| `OoBDev.System/Graphics/ColorConversion` | RGB to HSL and HSV for one colour | Reused per pixel (or via a lookup) for the facets |
| `OoBDev.Onnx.ImageEmbeddings` (`DecodedImage(Width, Height, Rgb)`, `IImageDecoder`) and `.Skia` (`SkiaImageDecoder`) | A decoded 8-bit RGB buffer and a Skia decoder | Decoding stays there; analysis takes a plain RGB buffer so it does not depend on ONNX or Skia |
| `OoBDev.Extensions.Drawing` | GDI-flavoured drawing helpers | Optional histogram rendering only, later |
| Storage catalog `containers/tools/quality.py` (Python) | Quality scoring in a container | Behaviour reference for exposure metrics; stays a container until the .NET version replaces it |
| Old SecondShooter OpenCV sample | Histograms channel 0 over 64 bins (0 to 63), which is wrong for 8-bit data | Rejected (not reused), as recorded in the request |

No histogram or whole-image colour code exists in `src`, so the new code is the base.

## Design

- `RgbImage(int width, int height, ReadOnlyMemory<byte> rgb)`: a small immutable view; `DecodedImage` and Skia bitmaps convert to it without copying where possible.
- `ImageHistograms.Compute(image, bins = 256)`: counts for R, G, B and luminance (Rec. 709 weights); bins span the real range of the sample depth.
- `ColorFacets.Compute(image)`: dominant hue (saturation-weighted circular mean), mean saturation and value, palette of N colours by a deterministic median-cut.
- `ExposureMetrics.Compute(image)`: mean luminance, percent clipped at 0 and 255 (a threshold option), contrast as luminance standard deviation.
- Transforms (`ToGrayscale`, `AdjustBrightnessContrast`) return new buffers; the input is never changed.
- Everything is deterministic (no randomness, fixed iteration order) so a catalog can store the results.

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Analysis library | does not exist | new `src/Framework/OoBDev.Imaging.Analysis` (BCL only; references `OoBDev.System` for `ColorConversion`) | `OoBDev.Imaging.Analysis` | new packable Framework project |
| Tests | does not exist | new `src/Framework/OoBDev.Imaging.Analysis.Tests` | `OoBDev.Imaging.Analysis.Tests` | MSTest, `Unit` |
| Readme | none | `README.Imaging.Analysis.md` | - | - |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| `OoBDev.slnx` | add the two projects |
| `OoBDev.Onnx.ImageEmbeddings` | none now; a conversion `DecodedImage` to `RgbImage` is an extension in the analysis library only if the owner wants an ONNX reference (not planned: the adapter rule keeps adapters on Abstractions) |

## Testing strategy

Synthetic buffers only: solid mid-grey (one bin per channel), 10 percent white pixels (clipped-white about 10 percent), a half black / half white image (contrast, two bins), pure red/green/blue hues, a 16-bit style input scaled to the real range, and identical results on a second run. Target 85 to 90 percent coverage.

## Later slices

- Rendering a histogram to an image (Extensions.Drawing or a web view), if wanted.
- Burst and near-duplicate ranking builds on these metrics ([request](../../features/request/2026-11-11-burst-and-near-duplicate-ranking.md)).
- Resizing stays with the imaging library that decodes (Skia).
