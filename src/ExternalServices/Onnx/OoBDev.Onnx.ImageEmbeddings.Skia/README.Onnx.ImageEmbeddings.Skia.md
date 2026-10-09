# OoBDev.Onnx.ImageEmbeddings.Skia

SkiaSharp `IImageDecoder` for `OoBDev.Onnx.ImageEmbeddings`. Decodes JPEG, PNG, WebP, GIF, BMP and more to RGB
(alpha dropped without compositing, first frame of animations) and rejects images over `MaxPixels` before
allocating pixels.

```csharp
services.TryAddSkiaImageDecoder();
```
