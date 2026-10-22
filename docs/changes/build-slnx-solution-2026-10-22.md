# Solution converted to .slnx

**Date:** 2026-10-22 (owner request)

## Done

- `src/OoBDev.sln` replaced by `src/OoBDev.slnx` (`dotnet sln migrate`; 141 projects in both).
- `src/Directory.Build.props` finds `SolutionDir` through `OoBDev.slnx` and `*.slnx`.
- `src/test.sh`, the templates readme, the template verify script, the build-and-csproj practices page, the spike readme and the migration plans name the new file.

## Not changed

`.github/workflows/dotnet.yml` still lists `src/**/*.sln` path filters. The workflows are left alone until the local runner is verified (BACKLOG).

## Verification

`dotnet build src/OoBDev.slnx`: build succeeded, 0 errors.
