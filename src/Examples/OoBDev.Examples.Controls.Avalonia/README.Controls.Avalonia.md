# OoBDev.Examples.Controls.Avalonia

An Avalonia app that renders the shared `ControlsSampleSettings` through `OoBDev.UiDefinitions.Avalonia` (`ControlsWindow`), and opens a second window with the live displays fed by `LiveSampleFeed` (`LiveSample`).

```
dotnet run --project src/Examples/OoBDev.Examples.Controls.Avalonia
```

## Verified

7 Unit tests in `OoBDev.Examples.Controls.Avalonia.Tests`, headless with the Skia renderer, each attaching PNG screenshots: the live displays drawn from the feed, and the journey (click Show advanced and the Notes row appears; click Enabled and the settings change). The app has not been looked at on a real desktop.
