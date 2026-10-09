# OoBDev.Vision.Dinov2Small

In-process DINOv2-small image embeddings (384 dimensions, class token) for image similarity, as a
`Microsoft.Extensions.AI` `IEmbeddingGenerator<DataContent, Embedding<float>>` on
[OoBDev.Onnx.ImageEmbeddings](../../Onnx/OoBDev.Onnx.ImageEmbeddings/README.Onnx.ImageEmbeddings.md).

```csharp
services.TryAddSkiaImageDecoder();
services.TryAddDinov2SmallServices(configuration);
var generator = serviceProvider.GetRequiredKeyedService<IEmbeddingGenerator<DataContent, Embedding<float>>>(VisionGlobals.Dinov2SmallKey);
var vectors = await generator.GenerateAsync([new DataContent(bytes, "image/jpeg")]);
```

- Options bind to the `Dinov2Small` section as `OnnxImageEmbeddingOptions` on top of this model's defaults.
- Preprocessing follows the Hugging Face processor: bicubic resize to a shorter edge of 256, center crop 224, ImageNet mean and std.
- The ONNX export (`Xenova/dinov2-small`, about 88 MB, pinned revision, SHA-256 verified) is downloaded on first use into the Hugging Face hub cache layout shared with Python tools.
- Call `Dinov2SmallModel.EnsureAsync(options, logger)` at startup to download ahead of the first request.
