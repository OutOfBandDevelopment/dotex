# OoBDev.Documents.Hpgl

Converts HP-GL plotter output to SVG. `HpglToSvgConverter.ConvertToSvg(bytes)` is a small interpreter (IN, SP, PU, PD, PA, PR; other commands are ignored, malformed input never throws) that emits one static SVG. `HpglToSvgConverter.ParseScene` gives the parsed `VectorScene` (polylines with pen colour, Y pointing down, 20 unit margin) that `VectorSceneSvgWriter` writes as SVG and `OoBDev.Extensions.Drawing.VectorSceneRasterizer` draws as PNG or JPEG; `IVectorSceneParser` / `HpglSceneParser` are the parser seam for other vector languages. `HpglToSvgConversionHandler` wraps it as an `IDocumentConversionHandler` (`image/vnd.hp-hpgl` to `image/svg+xml`) so `IDocumentConversion` can chain it with other handlers.

```csharp
services.TryAddHpglConversion();
```

References `OoBDev.Documents.Abstractions` only. Moved from `OoBDev.DevTerm.Core` ([migration plan](../../../docs/design/StreamTools/migration-plan.md)).

## Audio output (oscilloscope X-Y)

`VectorSceneAudioWriter` writes a `VectorScene` as a stereo 16-bit WAV (left X, right Y, scaled to -1..1, aspect kept). `VectorSceneAudioOptions` sets `SampleRate` (48000), `Duration` (10 s), `FramesPerSecond` (25) and `JumpSamples` (4). There is no blanking track, so jumps between lines are short interpolated runs. Design: [HpglAudioSink](../../../docs/design/HpglAudioSink/README.md).
