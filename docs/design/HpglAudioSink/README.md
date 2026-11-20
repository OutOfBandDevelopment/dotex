# HP-GL stereo audio sink

Status: **proposed design, written 2026-11-19 on the assumed answer A of Q20 (an output format of the Hpgl library, 48 kHz, fast jumps without a Z track); built 2026-11-19 (`VectorSceneAudioWriter`, `VectorSceneAudioOptions`, 5 unit tests); the optional Z channel is not built.** Request: [HP-GL vector display sink](../../features/request/2026-11-10-hpgl-vector-display-sink.md).

## Contents

1. [Behavior](#behavior)
2. [Architecture](#architecture)
3. [API sketch](#api-sketch)
4. [Testing strategy](#testing-strategy)
5. [Limits](#limits)

### List of Tables

1. Table 1 — Options
2. Table 2 — Test plan

## Behavior

`VectorSceneAudioWriter` turns a `VectorScene` (from `HpglSceneParser`) into a PCM WAV: the left channel is X and the right is Y, both scaled to -1..1 keeping the aspect ratio, centred. Points are resampled along each polyline at constant speed so the beam draws evenly. Between polylines the writer moves along a straight jump of a few samples (fast, so it is nearly invisible without blanking). The scene repeats until the configured duration is reached. An empty scene produces no output and no error.

## Architecture

The writer sits beside `VectorSceneSvgWriter` in `OoBDev.Documents.Hpgl` (host-free Framework). It has no sound-device dependency: output is 16-bit PCM WAV bytes, so tests compare bytes. Live playback is a later, separate adapter. A conversion handler `HpglToWavConversionHandler` mirrors `HpglToSvgConversionHandler` so the document converter reaches it by content type (`audio/wav`).

## API sketch

```csharp
public sealed class VectorSceneAudioOptions
{
    public int SampleRate { get; set; } = 48000;
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(10);
    public int JumpSamples { get; set; } = 4;
    public bool ZChannel { get; set; }
}

public sealed class VectorSceneAudioWriter(IOptions<VectorSceneAudioOptions> options)
{
    public void Write(VectorScene scene, Stream output);
}
```

**Table 1 — Options**

| Option | Default | Meaning |
|--------|---------|---------|
| `SampleRate` | 48000 | baseline; 96 kHz and up allowed for faster converters (verify with a loopback test) |
| `Duration` | 10 s | the scene repeats to fill it |
| `JumpSamples` | 4 | samples spent moving between polylines |
| `ZChannel` | off | third blanking track in a multi-channel WAV, never on an LFE channel |

*Table 1 lists the options, validated with `AddValidatedOptions<T>()` (sample rate and duration above zero).*

## Testing strategy

**Table 2 — Test plan**

| Case | Tier | Check |
|------|------|-------|
| one polyline | Unit | left and right samples follow its points in order |
| coordinates outside -1..1 | Unit | scaled to fit, aspect ratio kept |
| empty scene | Unit | no bytes written, no exception |
| WAV header | Unit | read back: PCM, configured rate, two channels, data length matches the duration |
| determinism | Unit | two runs give identical bytes |
| scope capture | DevLocal | manual: draw a captured plot on a scope in X-Y mode |

*Table 2 maps each case to its tier; the scope check is manual because it needs a scope and an audio device.*

## Limits

No Z (blanking) channel by default, so jumps show faintly. Many line outputs are AC-coupled and lose the DC offset, so the image drifts; DC-coupled or USB audio devices draw steadier. The scope must be in X-Y mode and the level may need attenuating. An S/PDIF decoder box cannot carry a discrete Z channel (a lossy bitstream smears it).
