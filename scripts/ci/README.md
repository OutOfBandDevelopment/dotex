# Local CI scripts

GitHub is no longer the primary repository and CI is manual only: nothing runs on a schedule or on push. Run these scripts when you want a build, a test run or packages.

| Script | Replaces | What it does |
|--------|----------|--------------|
| `run-linux-tests.ps1` | WSL for Linux-only tests | Copies `src/` into the `mcr.microsoft.com/dotnet/sdk:10.0` container (host build output is untouched, GitVersion off) and runs `dotnet test` there; default project is the CGI middleware tests (`.sh` fixtures). Results in `artifacts/test-results/linux` |
| `run-tests.ps1` | the former GitHub build and test workflows | Restore, build and test `src/` for the chosen categories; starts and always stops the Docker stack when `Integration` is included |

```powershell
pwsh scripts/ci/run-tests.ps1                          # Unit and Simulate
pwsh scripts/ci/run-tests.ps1 -Category Integration    # Docker stack, then Integration
pwsh scripts/ci/run-tests.ps1 -Category Unit -Coverage
pwsh scripts/ci/run-tests.ps1 -Category Integration -KeepStack
```

Results are written to `artifacts/test-results`. `LiveIntegration` needs credentials (for example `GROQ_API_KEY`, see `docs/architecture/testing/live-integration/groq.md`).

| Script | What it does |
|--------|--------------|
| `pack.ps1` | Packs every packable project of `OoBDev.slnx` with the GitVersion version (`-Version` overrides) into `G:\nuget\`, the default `PackageOutputPath`. Packing happens only on `dotnet pack`, never on build. Test, example and AppHost projects are not packable; libraries and tools are. Publishing a package anywhere is a separate step. |
| `new-runsettings.ps1` | Writes `artifacts/test.runsettings`, a copy of `src/.runsettings` whose ports follow `TEST_PORT_<NAME>` (environment or `containers/testing/.env`). `run-tests.ps1` calls it, so the stack and the tests share one source of truth. |
