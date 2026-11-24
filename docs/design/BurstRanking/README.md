# Burst grouping and best-shot ranking

Status: **first slice built 2026-11-16** (ranker, labels, measures, dHash in `OoBDev.Imaging.Analysis`; placement confirmed by the owner 2026-11-12, shared with [ImageAnalysis](../ImageAnalysis/README.md)). Request: [burst and near-duplicate ranking](../../features/completed/2026-11-11-burst-and-near-duplicate-ranking.md).

## Contents

1. [Need](#need)
2. [Superset comparison](#superset-comparison)
3. [Design](#design)
4. [Migration plan](#migration-plan)
5. [Testing strategy](#testing-strategy)
6. [Not in the first slice](#not-in-the-first-slice)

## Need

Frames taken in a burst are grouped by capture time and similarity, ranked by quality, and one is suggested; nothing is ever deleted or marked automatically. Output per frame: group id, rank, score and reasons.

## Superset comparison

**Table 1 — Sources compared**

| Source | What it has | Verdict |
|--------|-------------|---------|
| dotex | No grouping, ranking or perceptual hash (the only hash code is the cryptographic hash selector) | New code |
| Storage catalog `BurstRanker.cs` (68 lines) | Pairing within a time window (each frame against the next four), transitive groups (union-find), score = log2 of focus against the group's best minus label penalties, with reasons | Base for `BurstRanker` |
| Storage catalog `QualityLabels.cs` (18 lines) | Labels (shaky, defocused, soft, under, dark, over, noisy, flat) from eight stored measures with starting thresholds | Base for `QualityLabels`; thresholds become options |
| Storage catalog `DHash.cs` (113 lines) | 64-bit difference hash, bit-for-bit equal to the retired Pillow code (601 grayscale, 9x8 Lanczos in 22-bit fixed point), but takes a Skia bitmap | Base for `DHash`, reworked to take an 8-bit gray buffer so it needs no Skia |
| Storage catalog `BurstsDuty.cs`, rows and `BurstsController` | Queue, database and API wiring | Catalog-specific, not ported |
| Python `quality.py` | Computes the eight measures (tile focus, isotropy, high-frequency share, luma mean, clipping, range, noise) | Stays a container until a .NET measure set exists; the exposure part (mean, clipping, range) overlaps [ImageAnalysis](../ImageAnalysis/README.md) |

Defaults of the catalog (window 60 s, similarity 0.88, dHash 10 bits, 60 frames per group) become options; the request's open question (default signals) is answered by keeping the catalog order: subject focus when every member has it, else whole-frame focus, then penalties.

## Design

- `BurstRanker.Pairs`, `Components`, `Score` stay pure functions over times, ids and measures; the caller decides similarity (embedding or dHash) and passes the links.
- `QualityMeasures` (record of the eight numbers) and `QualityLabels` move with it; the measures come from the caller until the analysis library can compute them.
- `DHash.Compute(gray, width, height)` returns the signed 64-bit value; a `Hamming` helper compares two hashes.
- Output is a list of (id, score, basis, notes); the library never touches files and never recommends removal.
- Nothing is logged; ids and counts only if a caller logs.

## Migration plan

**Table 2 — Projects**

| Project | Current location | Target location | Namespace | Package or unit |
|---------|------------------|-----------------|-----------|-----------------|
| Ranker, labels, measures, dHash | `C:\repo\_storage\src\Storage.Workers` (read-only reference) | `src/Framework/OoBDev.Imaging.Analysis` (same project as [ImageAnalysis](../ImageAnalysis/README.md); BCL only) | `OoBDev.Imaging.Analysis.Bursts` | the same packable project |
| Tests | the copy read has none for these | `src/Framework/OoBDev.Imaging.Analysis.Tests` | `OoBDev.Imaging.Analysis.Tests` | MSTest, `Unit` |

**Table 3 — References that change**

| Project | Change |
|---------|--------|
| Storage catalog | drops `BurstRanker`, `QualityLabels`, `DHash` and references the package (outside this repository) |
| Everything else | none |

## Testing strategy

Pairs: five frames within two seconds pair up, a gap above the window breaks the chain, each frame looks at four neighbours only. Components: transitive links give one group, singletons are dropped. Score: sharpest member first with the note "sharpest", a shaky or over-exposed member loses its penalty, members without a measurement score -9 with "no measurement", subject basis only when every member has a subject focus. dHash: a known gray gradient gives the catalog's recorded value (the catalog's golden values are copied), identical images give distance 0. Labels: each threshold boundary. Target 85 to 90 percent coverage.

## Not in the first slice

- Eyes-open and face signals (need the face decision, see [face and pet matching](../../features/completed/2026-11-11-face-and-pet-matching.md)).
- Subject-region focus (needs the face decision); the eight whole-frame measures are built (below).
- Database queue and API: catalog-specific.

## Built

2026-11-18: `QualityAnalyzer.Analyze` computes the eight measures in .NET from a gray buffer (recipe `q1` port). Compared with the `storage-tools` Python container on 7 real photos, fed the same gray pixels (the container's decode and 1024 resize, dumped raw): the first run showed `focus_tile` about 10 times lower because the 4-neighbour Laplacian is not OpenCV's `ksize=3` kernel (`[[2,0,2],[0,-8,0],[2,0,2]]`), and `hf_share` off by up to 190 % because of the 512 crop. Both fixed (OpenCV kernel; whole-image spectrum with Bluestein for sides that are not powers of two). Now every measure is within about 2 % (`focus_tile` 0 to 1.7 %, `hf_share` under 0.1 %, luma, clipping, range and isotropy under 0.3 %, noise under 2 %), the remainder being reflected versus skipped borders, so the catalog thresholds carry over as they are. The comparison was a throwaway script on private photos, not committed; the tests check ordering (blur lowers focus and high-frequency share), clipping, range, isotropy and argument checks.

2026-11-16: `OoBDev.Imaging.Analysis.Bursts` has `BurstRanker` (Pairs, Components, Score), `QualityMeasures`, `SubjectFocus`, `BurstScore`, `QualityLabels` and `DHash` (gray buffer in, Pillow-equal Lanczos resize; RGB and RGBA gray helpers). Differences from the catalog copy: no Skia, a record result instead of a tuple, a dark frame is not also labelled under. The catalog's golden dHash values were not available (the catalog has no test for them), so the tests pin gradients and a step image; compare against the catalog values when the catalog switches to the package.
