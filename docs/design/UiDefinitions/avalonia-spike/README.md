# Avalonia spike screenshots

Headless renders of the Avalonia spike (`src/Spikes/OoBDev.Spike.Avalonia`), one folder per platform. They are copies of the files the tests write to `TestContext.TestResultsDirectory` and attach with `AddResultFile` (`src/Spikes/OoBDev.Spike.Avalonia.Tests`), taken on 2026-11-10: `windows` from a local run, `linux` from the `mcr.microsoft.com/dotnet/sdk:10.0` container with `libfontconfig1` and `fonts-dejavu-core`.

- The file name is `{test}-{step}`, so `StripChart_AfterSamples_...-30-samples.png` is the strip chart after 30 samples of the shared `LiveSampleFeed`.
- The feed is deterministic, so the chart geometry is identical on both platforms; the text differs because the fonts differ (Segoe UI on Windows, DejaVu on Linux). Compare the shapes, not the pixels.
- These are evidence, not golden files: no test compares them. Rerun the tests to refresh them; see [the findings](../avalonia-spike-findings.md).
