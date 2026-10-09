# OoBDev.Onnx.ImageEmbeddings

In-process image embeddings and classification with ONNX Runtime, behind `Microsoft.Extensions.AI`
(`IEmbeddingGenerator<DataContent, Embedding<float>>`) and `IImageClassifier`.

- `OnnxImageEmbeddingGenerator` - one vector per image (class token, mean of patch tokens, or model output).
- `OnnxImageClassifier` - softmax over logits, top-K labels.
- `ImageResizer` - Pillow-exact bicubic/bilinear resize so results match the Python reference pipelines.
- `IImageDecoder` - bytes to RGB; use `OoBDev.Onnx.ImageEmbeddings.Skia` for the default implementation.

Model presets live in `OoBDev.Vision.*` projects. Design: `docs/design/ImageEmbeddings/`.
