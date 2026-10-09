# OoBDev.Vision.ClipVitB32

In-process CLIP ViT-B/32 (OpenAI weights, Xenova ONNX export): image embeddings, text embeddings in the same 512 dimension space, and zero-shot image classification with labels chosen at call time. No service, no Python.

| Service (key `clip-vit-base-patch32`) | Type |
|---|---|
| Image vectors | `IEmbeddingGenerator<DataContent, Embedding<float>>` |
| Text vectors | `IEmbeddingGenerator<string, Embedding<float>>` |
| Zero-shot labels | `ZeroShotImageClassifier` |

## Usage

```csharp
services.TryAddSkiaImageDecoder();
services.TryAddClipVitB32Services(configuration); // sections "ClipVitB32" and "ClipVitB32Text"

var classifier = provider.GetRequiredKeyedService<ZeroShotImageClassifier>("clip-vit-base-patch32");
var top = await classifier.ClassifyAsync(new DataContent(bytes, "image/jpeg"), ["a photo of a cat", "a photo of a dog"]);
```

The image tower reuses `OnnxImageEmbeddingOptions` (see `OoBDev.Onnx.ImageEmbeddings`); the text tower uses `ClipTextOptions`.

## Models and cache

Files are pinned to a Hugging Face revision, verified with SHA-256, and stored in the Hugging Face hub cache (`HF_HOME` / `HF_HUB_CACHE` honored), so Python tools on the same machine share them. The first resolve downloads about 350 MB.

## Conformance

`ReferenceTests.ClipVitB32_MatchesReference` compares with vectors and zero-shot probabilities produced by the original PyTorch model (`scripts/embeddings/make-reference-images.py`): text and image cosine 1.0000, probabilities within 0.02.

## Notes

- `ClipTokenizer` is a BPE tokenizer for `vocab.json` and `merges.txt`; text is cut at 77 ids and padded with the end-of-text id (the model pools at the end token).
- Logit scale is 100 as in the original model.
