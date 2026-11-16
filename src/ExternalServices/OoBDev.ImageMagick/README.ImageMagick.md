# OoBDev.ImageMagick

Adapter that develops camera RAW files with ImageMagick (Magick.NET Q16, native assets included in the package).

```csharp
services.TryAddMagickRawDeveloper();   // IRawDeveloper, also keyed "image-magick"
await developer.DevelopAsync(rawStream, jpegStream, new RawDevelopOptions(RedScale: 1.1), ct);
```

- Options: JPEG or PNG, JPEG quality, 8 or 16 bit, red, green and blue scales (applied before encoding).
- A non-RAW input throws `InvalidDataException`; a bit depth other than 8 or 16 throws `NotSupportedException`.
- Detection reuses `RawDetector`; without this adapter `EmbeddedPreviewDeveloper` gives the camera's own JPEG.
- Tests: Unit tests cover rendering and registration; a DevLocal test develops a real file named by `RAW_TEST_FILE`.
- Design: [RawDevelopment](../../../docs/design/RawDevelopment/README.md).
