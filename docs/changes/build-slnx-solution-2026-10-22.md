# Solution converted to .slnx

**Date:** 2026-10-22 (owner request)

## Done

- `src/OoBDev.sln` replaced by `src/OoBDev.slnx` (`dotnet sln migrate`; 141 projects in both).
- `src/Directory.Build.props` finds `SolutionDir` through `OoBDev.slnx` and `*.slnx`.
- `src/test.sh`, the templates readme, the template verify script, the build-and-csproj practices page, the spike readme and the migration plans name the new file.

- `.github/workflows/dotnet.yml` path filters now list `src/**/*.slnx` (owner allowed this edit).

## Verification

`dotnet build src/OoBDev.slnx`: build succeeded, 0 errors.

# PlantUML container reused

**Date:** 2026-10-22 (owner request)

The integration stack no longer defines its own PlantUML service. `PLANTUML_URL` defaults to `http://localhost:8086`, the `plantuml` container already running on the system (rendering checked with a test diagram). Removed the compose service, the VS Code start task and the port row in `docs/example/deployment.md`; the rule, skill, readme, `TEST_VARIABLES.md` and `.vscode/settings.json` name the existing container.
