# ImageEmbeddings — API design

[← Architecture](architecture.md) · [Testing strategy →](testing-strategy.md)

## Contracts

```csharp
public interface IImageDecoder
{
    /// <summary>Decodes encoded bytes to 8-bit RGB (alpha dropped as Python's convert("RGB") does).</summary>
    DecodedImage Decode(ReadOnlySpan<byte> encoded);
}

public readonly record struct DecodedImage(int Width, int Height, byte[] Rgb);

// Embeddings: Microsoft.Extensions.AI
IEmbeddingGenerator<DataContent, Embedding<float>>

public interface IImageClassifier
{
    Task<IReadOnlyList<ImageLabel>> ClassifyAsync(DataContent image, int topK = 5, CancellationToken ct = default);
}

public sealed record ImageLabel(string Label, float Probability);
```

CLIP additionally exposes `IEmbeddingGenerator<string, Embedding<float>>` for the text tower and `ZeroShotImageClassifier` that takes labels per call (`ClassifyAsync(image, labels, topK)`), using the CLIP logit scale of 100 and softmax.

## Options (`OnnxImageEmbeddingOptions`)

**Table 1 — Options**

| Option | Meaning |
|--------|---------|
| `ModelPath` (default `models`), `ModelFiles` | Same meaning as the text runner (`ModelFileSource` list); sub-folder names allowed |
| `ModelFileName` (default `model.onnx`) | File inside the folder to load (`onnx/vision_model.onnx`) |
| `InputName` (default `pixel_values`), `OutputName` | Session names; `OutputName` null takes the first output |
| `ImageSize`, `ResizeShortestEdge`, `Resample`, `CenterCrop`, `Mean`, `Std` | Preprocessing, set by each preset |
| `Pooling` | `Output` (model embedding), `Cls` (first token of the hidden states) or `MeanPatch` |
| `Normalize` | Scale to unit length |
| `MaxConcurrentInferences` (1), `MaxBatchSize` (8), `MaxImageBytes` (64 MiB), `MaxPixels` (100 million) | Limits; oversized input is rejected |
| `IntraOpThreads` | ONNX runtime threads per operator; 0 lets the runtime choose |

## Registration

Keyed singletons per model key (`clip-vit-base-patch32`, `dinov2-small`, `vit-base-patch16-224`), like the text presets; unkeyed defaults via `TryAdd`. `TryAddClipVitB32Services(configuration)` and so on, in each preset project (`OoBDev.Vision.ClipVitB32`, `Dinov2Small`, `VitBasePatch16`) as `ServiceCollectionExtensions`, keys from `VisionGlobals`. `OnnxImageEmbeddingOptions.Validate()` checks the settings. The decoder registers once with `TryAddSkiaImageDecoder()`.

[Testing strategy →](testing-strategy.md)
