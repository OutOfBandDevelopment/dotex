# OoBDev.Imaging.Analysis

Pure image-analysis functions over decoded pixels. BCL only: no image library, no logging (private photos), no file access.

## Bursts (`OoBDev.Imaging.Analysis.Bursts`)

| Type | Purpose |
|------|---------|
| `BurstRanker.Pairs` | Frame pairs within a time window (each frame against the next four) |
| `BurstRanker.Components` | Transitive groups of two or more from similarity links |
| `BurstRanker.Score` | Best-first ranking with reasons; advice only, nothing is removed or marked |
| `QualityAnalyzer.Analyze(gray, w, h)` | Computes the eight measures from an 8-bit gray buffer (tile focus, isotropy, high-frequency share, luma mean, clipping, range, noise); BCL only. The Laplacian is OpenCV's 3x3 kernel and the spectrum covers the whole image, so on identical pixels every measure is within about 2 % of the Python recipe `q1` (borders are skipped instead of reflected, which is the remaining difference) |
| `QualityMeasures`, `QualityLabels` | Eight stored measures and the labels derived from them (shaky, defocused, soft, under, dark, over, noisy, flat) |
| `DHash` | 64-bit difference hash of a gray buffer, equal to the retired Pillow values; `Hamming` compares |

The caller decides similarity (embedding or dHash) and passes the links and the measures.

Design: [docs/design/BurstRanking](../../../docs/design/BurstRanking/README.md).

## Whole-image analysis (`OoBDev.Imaging.Analysis`)

| Type | Purpose |
|------|---------|
| `RgbImage` | Immutable 8-bit RGB view (the caller decodes); `ToGrayscale`, `AdjustBrightnessContrast` return new images |
| `ImageHistograms.Compute` | Red, green, blue and Rec. 709 luminance counts over 1 to 256 bins |
| `ExposureMetrics.Compute` | Mean luminance, percent clipped black and white (thresholds optional), contrast as standard deviation |
| `ColorFacets.Compute` | Saturation-weighted dominant hue, mean saturation and value, deterministic median-cut palette |

Everything is deterministic. `ColorFacets` converts each pixel through `OoBDev.System` `ColorConversion`, so pass a thumbnail.

Design: [docs/design/ImageAnalysis](../../../docs/design/ImageAnalysis/README.md).
