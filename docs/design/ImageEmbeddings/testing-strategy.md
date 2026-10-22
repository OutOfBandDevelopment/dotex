# ImageEmbeddings — Testing strategy

[← API design](api-design.md) · [Overview](README.md)

## Contents

- [Comparison with Python](#comparison-with-python)
- [Unit tests (no model)](#unit-tests-no-model)
- [Conformance tests (model, Integration)](#conformance-tests-model-integration)
- [Requirement coverage](#requirement-coverage)
- [Coverage](#coverage)

### List of Tables

1. [Table 1 — Requirement coverage](#table-1)

## Comparison with Python

`scripts/embeddings/make-reference-images.py <model> <output.json>` writes vectors (and top-5 labels for classifiers) from the original models: `transformers` image processors with `CLIPModel`, `AutoModel` (DINOv2, CLS output) and `ViTForImageClassification`. The image set is generated deterministically (gradients, noise with a fixed seed, shapes, text) plus a few small public-domain photographs checked in under `TestData/`, in PNG and JPEG, with odd sizes, a grayscale image and an alpha image.

Pass bar: cosine 0.999 or better for every image; classifiers must agree on the top-1 label and the top-5 set, probabilities within 0.01. Comparison tests download the models, so they are `Integration` category and report `Inconclusive` when offline.

## Unit tests (no model)

In `OoBDev.Onnx.ImageEmbeddings.Tests`: `ImageResizerTests` (PIL-exact resize, same size, solid colour, bad arguments), `PreprocessorAndOptionsTests` (NCHW planes, shortest edge and crop, options defaults and ranges), `SkiaImageDecoderTests` (lossless PNG, pixel limit, not an image), and `ClipUnitTests` (tokenizer, text options, zero-shot ranking). `ConformanceTests`, `ReferenceTests` (Python comparison) and `AllPresets_ResolveByKeyAndWork` need the models and are Integration.

- Resize against fixed expected pixels from PIL for bicubic and bilinear, shortest-edge and direct modes, up- and down-scaling.
- Center crop, normalisation and NCHW layout.
- Options validation; `MaxImageBytes` and `MaxPixels` rejection; decoder abstraction with a fake decoder.

## Conformance tests (model, Integration)

Same checks for every preset: unit length, determinism, batch against single, order, concurrency, corrupt and empty bytes, grayscale, alpha, very large and 1 by 1 images, and similarity ordering (a picture is closer to a re-encoded copy than to an unrelated picture). CLIP: image-to-text ordering and zero-shot labels.

## Requirement coverage

<a id="table-1"></a>**Table 1 — Requirement coverage**

| Requirement | Test | Category |
|-------------|------|----------|
| REQ-001 | `SkiaImageDecoderTests.Decode_ReturnsRgbOfExpectedSize`; `ConformanceTests` unit length per preset | Unit, Integration |
| REQ-002 | `ReferenceTests` (`Dinov2Small_MatchesReference`, `ClipVitB32_MatchesReference`) | Integration |
| REQ-003 | `ConformanceTests.Generate_ParallelCallersGetTheirOwnResults`, `Generate_BatchEqualsSingleAndIsRepeatable` | Integration |
| REQ-004 | `ClipUnitTests.ZeroShot_RanksBySimilarityAndSumsToOne` | Unit |
| REQ-005 | `ReferenceTests.VitBasePatch16_MatchesReferenceLabels`, `ConformanceTests.Classifier_ProbabilitiesAreOrderedAndBounded` | Integration |
| REQ-006 | `ClipUnitTests` text options; image-to-text ordering in `ConformanceTests` | Unit, Integration |
| REQ-007 | `RegistrationTests.AllPresets_ResolveByKeyAndWork` (downloads on first use) | Integration |
| REQ-008 | `ConformanceTests.Generate_RejectsBadInput`, `Generate_EnforcesSizeLimits`; `SkiaImageDecoderTests.Decode_TooManyPixels_Throws`, `Decode_NotAnImage_Throws` | Integration, Unit |
| REQ-009 | `ConformanceTests.Generate_EmptyInputGivesEmptyResult`, `Generate_HonorsCancellation` use `IEmbeddingGenerator` | Integration |
| REQ-010 | `IImageDecoder` seam; `OoBDev.Onnx.ImageEmbeddings.Skia` is the only decoder reference | Unit |
| REQ-011 | `ConformanceTests.Generate_Batch_RecordsActivityAndMetrics` | Integration |

## Coverage

80 percent for the runner project from unit tests alone; presets are covered by the Integration suite.

[Overview](README.md)
