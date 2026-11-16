# OoBDev.Imaging.Analysis

Pure image-analysis functions over decoded pixels. BCL only: no image library, no logging (private photos), no file access.

## Bursts (`OoBDev.Imaging.Analysis.Bursts`)

| Type | Purpose |
|------|---------|
| `BurstRanker.Pairs` | Frame pairs within a time window (each frame against the next four) |
| `BurstRanker.Components` | Transitive groups of two or more from similarity links |
| `BurstRanker.Score` | Best-first ranking with reasons; advice only, nothing is removed or marked |
| `QualityMeasures`, `QualityLabels` | Eight stored measures and the labels derived from them (shaky, defocused, soft, under, dark, over, noisy, flat) |
| `DHash` | 64-bit difference hash of a gray buffer, equal to the retired Pillow values; `Hamming` compares |

The caller decides similarity (embedding or dHash) and passes the links and the measures.

Design: [docs/design/BurstRanking](../../../docs/design/BurstRanking/README.md).
