# OoBDev.Vision.VitBasePatch16

In-process ViT-base-patch16-224 image classification (1000 ImageNet classes) as an `IImageClassifier` on
[OoBDev.Onnx.ImageEmbeddings](../../Onnx/OoBDev.Onnx.ImageEmbeddings/README.Onnx.ImageEmbeddings.md).

```csharp
services.TryAddSkiaImageDecoder();
services.TryAddVitBasePatch16Services(configuration);
var classifier = serviceProvider.GetRequiredKeyedService<IImageClassifier>(VisionGlobals.VitBasePatch16Key);
var labels = await classifier.ClassifyAsync(new DataContent(bytes, "image/jpeg"), topK: 5);
```

- Options bind to the `VitBasePatch16` section as `OnnxImageEmbeddingOptions` on top of this model's defaults.
- Preprocessing follows the Hugging Face processor: bilinear resize straight to 224 by 224, mean and std 0.5.
- Labels come from `id2label` in the model's `config.json`; probabilities are a softmax over the logits.
- The ONNX export (`Xenova/vit-base-patch16-224`, about 346 MB, pinned revision, SHA-256 verified) is downloaded on first use into the Hugging Face hub cache layout shared with Python tools.
- Call `VitBasePatch16Model.EnsureAsync(options, logger)` at startup to download ahead of the first request.
