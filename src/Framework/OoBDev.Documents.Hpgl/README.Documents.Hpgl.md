# OoBDev.Documents.Hpgl

Converts HP-GL plotter output to SVG. `HpglToSvgConverter.ConvertToSvg(bytes)` is a small interpreter (IN, SP, PU, PD, PA, PR; other commands are ignored, malformed input never throws) that emits one static SVG. `HpglToSvgConverter.ParseScene` gives the parsed `VectorScene` (polylines with pen colour, Y pointing down, 20 unit margin) that `VectorSceneSvgWriter` writes as SVG and `OoBDev.Extensions.Drawing.VectorSceneRasterizer` draws as PNG or JPEG; `IVectorSceneParser` / `HpglSceneParser` are the parser seam for other vector languages. `HpglToSvgConversionHandler` wraps it as an `IDocumentConversionHandler` (`image/vnd.hp-hpgl` to `image/svg+xml`) so `IDocumentConversion` can chain it with other handlers.

```csharp
services.TryAddHpglConversion();
```

References `OoBDev.Documents.Abstractions` only. Moved from `OoBDev.DevTerm.Core` ([migration plan](../../../docs/design/StreamTools/migration-plan.md)).

## Audio output (oscilloscope X-Y)

`VectorSceneAudioWriter` writes a `VectorScene` as a stereo 16-bit WAV (left X, right Y, scaled to -1..1, aspect kept). `VectorSceneAudioOptions` sets `SampleRate` (48000), `Duration` (10 s), `FramesPerSecond` (25), `JumpSamples` (4) and `ZChannel` (off). By default there is no blanking track, so jumps between lines are short interpolated runs. With `ZChannel` the file is an extensible 3-channel WAV with +1 while drawing and -1 while jumping on the front-centre channel (never the LFE). An empty scene writes nothing. Design: [HpglAudioSink](../../../docs/design/HpglAudioSink/README.md).

```csharp
// A plotter capture becomes a scene, then a WAV that is played into the scope's X and Y inputs.
var scene = HpglToSvgConverter.ParseScene(File.ReadAllBytes("hpgl-capture.plt"));
using var wav = File.Create("hpgl-capture.wav");
new VectorSceneAudioWriter(new VectorSceneAudioOptions { Duration = TimeSpan.FromSeconds(30) }).Write(scene, wav);
```


Limits: the scope must be in X-Y mode and the level may need attenuating (start low). Many line outputs are AC-coupled and lose the DC offset, so the image drifts; DC-coupled or USB audio devices draw steadier. Without a Z track jumps show as faint lines unless the scope has a Z input; a faster sample rate (check with a loopback test) makes fast jumps almost invisible. An S/PDIF decoder box cannot carry a discrete Z channel.

## PostScript and PCL scenes

`PostScriptSceneParser` interprets path operators (`moveto lineto rlineto curveto arc closepath stroke fill rectstroke`), colour (`setrgbcolor setgray`) and the matrix (`translate scale rotate gsave grestore`) and skips text, images, procedures and loops. `PclSceneParser` joins the HP-GL/2 segments between `ESC % 0 B` or `ESC % 1 B` and `ESC % 0 A` and parses them with the HP-GL interpreter; text and raster PCL are not drawn. Both implement `IVectorSceneParser`, so the SVG writer and the rasterizer work unchanged. DevTerm's internal conversion mode now accepts PostScript and PCL captures.
