# Manual CI and packaging on demand (2026-10-29)

Owner decisions of 2026-10-29: CI is manual only, the GitHub workflows do not apply, and packages are built only when `dotnet pack` runs.

## What changed

- `GeneratePackageOnBuild` is false for every project (`src/Directory.Build.props`, `src/Directory.Build.targets`). A build no longer writes packages.
- `IsPackable` is false for names ending `.Test`, `.Tests`, `.Example`, `.Examples` or `AppHost`, and for `.Example.` projects (web apps and the Aspire hosts). Libraries and tools pack.
- Default `PackageOutputPath` is `G:\nuget\`.
- `OoBDev.Common` no longer references the Windows-only `OoBDev.UiDefinitions.Wpf`: a `Remove` glob `Framework\**\OoBDev.*.Wpf.csproj` sits beside the other exclusions. `dotnet pack OoBDev.slnx` restores again (it failed with NU1201).
- New `scripts/ci/pack.ps1` packs the solution with the GitVersion version (`-Version` overrides) into `G:\nuget`; it deletes nothing there.
- Removed `.github/workflows/*` and `scripts/ci/register-daily-task.ps1`. No scheduled builds.

## Verified

- MSBuild evaluation: `IsPackable` false for `OoBDev.Example.WebApi`, `OoBDev.Example.AppHost`, `OoBDev.Testing.AppHost` and `OoBDev.System.Tests`; true for `OoBDev.System` and `OoBDev.PackMan.Cli`; `GeneratePackageOnBuild` false and `PackageOutputPath` `G:\nuget\` everywhere.
- `pack.ps1 -Version 0.0.1-localtest` over the solution produced 115 packages (into a temporary folder before the output default moved to `G:\nuget`). That run predates the name filter; the filter was checked by property evaluation only, and `pack.ps1` was not re-run against `G:\nuget`.
