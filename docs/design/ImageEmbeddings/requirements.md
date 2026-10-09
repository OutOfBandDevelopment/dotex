# ImageEmbeddings — Requirements

[← Overview](README.md) · [Architecture →](architecture.md)

## Problem

The repository has text embeddings only. Image search, duplicate detection and labelling need image vectors and classes, without a Python sidecar and without bundling model files.

## Requirements

**Table 1 — Requirements**

| Id | Requirement |
|----|-------------|
| REQ-001 | Turn encoded images (JPEG, PNG, WebP, GIF, BMP) into unit-length vectors |
| REQ-002 | Match the original Python model: cosine 0.999 or better per image |
| REQ-003 | Thread-safe; one shared session; bounded concurrency |
| REQ-004 | Zero-shot classification with caller-supplied labels (CLIP) |
| REQ-005 | Fixed-label classification with the model's own labels (ViT), top-K with probabilities |
| REQ-006 | Text vectors in the same space as image vectors (CLIP text tower) so text can search images |
| REQ-007 | Model files downloaded on first use into the Hugging Face hub cache, pinned and hash verified |
| REQ-008 | Bad input (corrupt, empty, huge, animated) fails with a clear exception for that item or returns a defined result, never a process crash |
| REQ-009 | Public API is `Microsoft.Extensions.AI` types where one exists |
| REQ-010 | Decoder is replaceable; no new dependency in the runner |

## Non-goals

- Training or fine-tuning, object detection, segmentation, video.
- GPU execution providers (an option later).

## Open questions

- Alpha handling: composite on white or black (Python `convert("RGB")` drops alpha, which keeps the stored colour). Follow Python.
- EXIF orientation: Python image processors ignore it; follow Python and expose an option.

[Architecture →](architecture.md)
