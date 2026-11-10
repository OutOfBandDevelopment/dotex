# OoBDev.Spike.Avalonia

Throwaway spike: renders the shared `FormViewModel` in an Avalonia window by bindings only (`FormView`). Findings: [avalonia-spike-findings.md](../../../docs/migration/avalonia-spike-findings.md). Not packaged. It is in `src/OoBDev.Spikes.slnx`. Run the window with `dotnet run --project src/Spikes/OoBDev.Spike.Avalonia`. Tests in `OoBDev.Spike.Avalonia.Tests` run headless with the Skia renderer and write a PNG screenshot of each rendered state to `TestContext.TestResultsDirectory` and attach it with `TestContext.AddResultFile`, so the files sit under `TestResults/<run>/In/<id>/<machine>/` next to the result. The live displays (indicator, bar graph, strip chart) use the shared `LiveSampleFeed` and open in a second window when the app runs.

## Linux

Run the tests in a `mcr.microsoft.com/dotnet/sdk:10.0` container after `apt-get install -y libfontconfig1 fonts-dejavu-core`, with a copy of `src` (no `bin` or `obj`) and `dotnet test -p:DisableGitVersionTask=true` when `.git` is absent.
