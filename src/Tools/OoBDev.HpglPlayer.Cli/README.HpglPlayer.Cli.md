# OoBDev.HpglPlayer.Cli

`hpgl-player` plays an HP-GL plotter file as a looping stereo X-Y signal (left X, right Y) so an oscilloscope in X-Y mode draws it. It parses the plot with `OoBDev.Documents.Hpgl` and renders it with `VectorSceneAudioWriter` ([design](../../../docs/design/HpglAudioSink/README.md)).

```
hpgl-player capture.plt                 # loops on the default sound device until Ctrl+C
hpgl-player capture.plt --wav out.wav   # writes the file instead of playing
hpgl-player capture.plt --z true --device Scope   # 3 channels (X, Y, blanking) through WASAPI on a chosen output
hpgl-player --list-devices              # numbers and names of the output devices
```

Options: `--z` (third blanking track), `--device` (number or part of the name; plays through WASAPI), `--rate` (48000), `--fps` (25), `--jump` (4 samples between lines), `--seconds` (1; the clip repeats, so one second of frames loops seamlessly).

Limits: stereo to the default device plays through `System.Media.SoundPlayer`; `--z` or `--device` use WASAPI in shared mode, so the device must already accept the channel count (set it to 5.1 or more in the Windows sound settings for the Z track, otherwise the tool says so and points to `--wav`). Windows only. Many line outputs are AC-coupled and drift; start with the volume low. Exit code 0 done, 2 error. Not yet verified on a multi-channel device.
