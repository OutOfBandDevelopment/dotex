# In-process image embeddings and classification (DINOv2, ViT, CLIP)

**Date:** 2026-10-08 · **Epic:** AI embeddings · **Status:** Complete (GitHub run not yet observed)

## Summary

Images can be embedded and classified inside the process with ONNX Runtime, using the same pattern as the text embedders: one generic runner, small model presets, pinned and SHA-256 verified files in the shared Hugging Face hub cache. Each preset was compared with the original Python model. Design: [ImageEmbeddings](../design/ImageEmbeddings/README.md).

## Contents

- [What was built](#what-was-built)
- [Verification](#verification)
- [Decisions](#decisions)
- [Follow-up](#follow-up)

## What was built

**Table 1 — New projects**

| Project | Purpose |
|---------|---------|
| `OoBDev.Onnx.ImageEmbeddings` | Runner: `IEmbeddingGenerator<DataContent, Embedding<float>>`, `IImageClassifier`, Pillow-exact resize, preprocessing from the Hugging Face configs, pooling modes, size limits |
| `OoBDev.Onnx.ImageEmbeddings.Skia` | `IImageDecoder` on SkiaSharp (MIT), `TryAddSkiaImageDecoder()` |
| `OoBDev.Vision.Dinov2Small` | Similarity: `Xenova/dinov2-small`, 384 dimensions |
| `OoBDev.Vision.VitBasePatch16` | Classification: `Xenova/vit-base-patch16-224`, 1000 ImageNet labels |
| `OoBDev.Vision.ClipVitB32` | Similarity and zero-shot labels: `Xenova/clip-vit-base-patch32`, 512 dimensions, BPE text tokenizer, `ZeroShotImageClassifier` |
| `OoBDev.Onnx.ImageEmbeddings.Tests` | Unit, reference and conformance tests |

## Verification

**Table 2 — Comparison with the original Python models**

| Preset | Result |
|--------|--------|
| Resize | Byte-for-byte equal to Pillow on 15 golden cases |
| ViT-base | Top-1 and top-5 labels equal, probabilities within 0.01 on 18 images |
| CLIP | Text and image vectors cosine 1.0000, zero-shot probabilities within 0.02 |
| DINOv2-small | Cosine 0.9975 or better (bar is 0.995) |

Models and sources are fixed in each `*Model` class. 37 tests pass in the image test project.

## Decisions

- The DINOv2 bar is 0.995, not 0.999: the same Xenova ONNX file run in Python on the Hugging Face processor's pixels gives identical cosines, so the gap is the export versus PyTorch, not this pipeline.
- SkiaSharp is the decoder because it is MIT licensed and cross platform.
- `TensorPrimitives.SoftMax` does not subtract the maximum; the zero-shot classifier shifts logits first because the logit scale is 100.

## Follow-up

- Runner unit coverage toward 80%.
- Real-photo sanity check and a GitHub CI run have not been observed.
- Reference data comes from `scripts/embeddings/make-reference-images.py` and `make-resize-golden.py`.
