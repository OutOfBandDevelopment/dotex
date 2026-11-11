# Feature request: Image color analysis: histograms, HSV facets and exposure metrics

- **Status:** new
- **Requested by:** owner (filed from the notes repository, `projects/applications/second-shooter-photo-library.md`; design concepts only, its code is not reused)
- **Date:** 2026-11-11
- **Area:** new, near `OoBDev.Onnx.ImageEmbeddings.Skia`, `OoBDev.System.Graphics.ColorConversion` and `OoBDev.Extensions.Drawing`

## Need

The photo library design (never built) listed per-channel RGB histograms, RGB to HSV, conversion to black and white, and resizing as basic tools. The NAS storage catalog
(`C:\repo\_storage`) has a proposed faceted explorer whose design calls for a histogram, and its burst ranking scores exposure. dotex has RGB/HSL/HSV conversion for
single colors but nothing that analyses a whole image, and no histogram code was found in `src`.

## Proposed behavior

1. **Histograms:** per-channel (R, G, B) and luminance histograms with 256 bins for 8-bit data (a bin count option), computed from a decoded image without modifying it.
2. **Color facets:** dominant hue, mean saturation and value, and a small palette, built on `ColorConversion`, so a catalog can filter "similar color" or "mostly dark".
3. **Exposure metrics:** mean luminance, percent of pixels clipped at black and white, and a simple contrast figure, as inputs to burst ranking and quality scoring.
4. **Transforms:** black and white conversion and brightness/contrast adjustment as pure functions over pixel buffers, plus resizing where Skia does not already cover it.
5. **Rendering (optional):** draw a histogram to an image through `OoBDev.Extensions.Drawing`, for a web or terminal view.

## Acceptance criteria

- Given a solid mid-gray image, when analysed, then every channel histogram has all counts in one bin.
- Given a 16-bit or non-8-bit source, when analysed, then bins span the real range (not a fixed 0 to 63).
- Given an image with 10 percent pure-white pixels, when exposure metrics are read, then the clipped-white figure is about 10 percent.
- Given the same image analysed twice, when compared, then the results are identical and the source file is unchanged.

## Existing code checked

- `OoBDev.System/Graphics/ColorConversion.cs` converts single RGB values to HSL and HSV; it has no image-level analysis.
- The old sample (`proving-grounds/examples/SecondShooter/src/SortEm/OpenCvExample`) was read and not reused: it is adapted from a public OpenCvSharp sample, needs desktop windows,
  and, reading the code only, histograms channel 0 (blue) over 64 bins covering 0 to 63, which looks wrong for 8-bit data. Not run.
- Search of `src` project files found no OpenCV or ImageMagick dependency; Skia is the imaging dependency in `Onnx.ImageEmbeddings.Skia`.
- `2026-11-11-burst-and-near-duplicate-ranking.md` and `2026-11-11-exif-metadata-extraction.md` are neighbours; this request supplies pixel-level inputs for the first.

## Security and operations

Reads decoded pixels only. Cost is one pass over the image, so it should reuse an already decoded bitmap from the embedding step where it can.

## Priority

- **Impact:** gives the catalog's quality scoring and faceted explorer a shared, tested source instead of Python helpers.
- **Needed by:** no date
- **Workaround today:** the catalog's quality scoring runs in Python containers (`containers/tools/quality.py`).

## Open questions

- Skia pixel access or a small purpose-built buffer type, to avoid a heavy dependency?
- Should histograms be stored as arrays in the catalog, or recomputed on demand?
- Is a rendered histogram in scope, or only the data?

## Triage notes

Origin: notes repository `projects/applications/second-shooter-photo-library.md`. Maintainers: accept, schedule or decline.
