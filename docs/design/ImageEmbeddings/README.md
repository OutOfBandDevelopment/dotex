# ImageEmbeddings — In-Process Image Embeddings and Classification

**Status:** Implemented 2026-10-08 ([change](../../changes/migration-image-embeddings-2026-10-08.md)); DINOv2 reference bar is cosine 0.995 because the Xenova ONNX export differs from PyTorch · Design 2026-10-08 · Epic: AI embeddings · Follows [AllMiniLmL6V2](../AllMiniLmL6V2/README.md)

First-party, thread-safe image embedding and classification on ONNX Runtime, using the same pattern as the text embedders: one generic runner, small model presets, model files downloaded on first use into the shared Hugging Face hub cache, and every preset compared with the original Python model.

## Documents

**Table 1 — Document set**

| Document | Purpose |
|----------|---------|
| [requirements.md](requirements.md) | Problem, requirements, non-goals |
| [architecture.md](architecture.md) | Projects, preprocessing, flows, decisions |
| [api-design.md](api-design.md) | Public contracts, options, registration |
| [testing-strategy.md](testing-strategy.md) | Comparison with Python, conformance tests |

## Decisions in one view

**Table 2 — Summary of the plan**

| Topic | Decision |
|-------|----------|
| Models | `Xenova/clip-vit-base-patch32` (image and text towers, 512 dimensions, zero-shot classification), `Xenova/dinov2-small` (image similarity, 384 dimensions), `Xenova/vit-base-patch16-224` (1000 ImageNet classes) |
| Runtime | `Microsoft.ML.OnnxRuntime` only |
| Contract | `IEmbeddingGenerator<DataContent, Embedding<float>>` for images; `IImageClassifier` for labels |
| Decoding | Behind `IImageDecoder` (bytes to 8-bit RGB). Default implementation uses SkiaSharp (MIT). The runner has no imaging dependency |
| Preprocessing | First-party resize, crop and normalise that follow each model's `preprocessor_config.json`. A PIL-style antialiased bicubic and bilinear resize is written by hand because decoder libraries resize differently and the result must match Python |
| Cache | Hugging Face hub layout through `HuggingFaceHubCache`, pinned revisions, SHA-256 verified |
| Verification | Cosine 0.999 or better against `transformers` (and `sentence-transformers` for CLIP) over a fixed image set; top-5 labels for classification |

[Requirements →](requirements.md)
