# Local CI scripts

GitHub is no longer the primary repository and CI is manual only: nothing runs on a schedule or on push. Run these scripts when you want a build, a test run or packages.

| Script | Replaces | What it does |
|--------|----------|--------------|
| `run-linux-tests.ps1` | WSL for Linux-only tests | Copies `src/` into the `mcr.microsoft.com/dotnet/sdk:10.0` container (host build output is untouched, GitVersion off) and runs `dotnet test` there; default project is the CGI middleware tests (`.sh` fixtures). Results in `artifacts/test-results/linux` |
| `run-tests.ps1` | the former GitHub build and test workflows | Restore, build and test `src/` for the chosen categories, then writes a markdown summary per `.trx` to `artifacts/test-results/summary`; starts and always stops the Docker stack when `Integration` is included |

| `warm-up-integration.ps1` | nothing (new, 2026-11-19) | Pulls and starts the slow integration services (pgvector, SQL Server, Ollama, SBERT, Keycloak, OpenSearch, Neo4j) in the background, so they initialise while `dotnet build` runs and the first test of each project does not wait for its container; `-DryRun` checks the names and prints the commands. End with `integration-down.sh` (not needed with `run-tests.ps1 -Category Integration`, which starts the whole stack with `--wait`). |
| `sonar.ps1` | the deleted `run-sonar.bat` | Scans `src/` with `dotnet-sonarscanner` against the SonarQube service of the test stack (begin, build, tests with OpenCover coverage, end). Token from `SONAR_TOKEN` only, never a file; `-Gate` waits for the quality gate and fails the script when it fails; `-SkipTests` skips coverage |

```powershell
pwsh scripts/ci/run-tests.ps1                          # Unit and Simulate
pwsh scripts/ci/run-tests.ps1 -Category Integration    # Docker stack, then Integration
pwsh scripts/ci/run-tests.ps1 -Category Unit -Coverage
pwsh scripts/ci/run-tests.ps1 -Category Integration -KeepStack
```

Results are written to `artifacts/test-results`. `LiveIntegration` needs credentials (for example `GROQ_API_KEY`, see `docs/architecture/testing/live-integration/groq.md`).

| Script | What it does |
|--------|--------------|
| `pack.ps1` | Packs every packable project of `OoBDev.slnx` with the GitVersion version (`-Version` overrides) into `G:\nuget\`, the default `PackageOutputPath`. Packing happens only on `dotnet pack`, never on build. Test, example and AppHost projects are not packable; libraries and tools are. Publishing a package anywhere is a separate step.  and npm packages that are neither private nor examples into `G:\npm` |
| `new-runsettings.ps1` | Writes `artifacts/test.runsettings`, a copy of `src/.runsettings` whose ports follow `TEST_PORT_<NAME>` (environment or `containers/testing/.env`). `run-tests.ps1` calls it, so the stack and the tests share one source of truth. |

The versioned git hooks in `.githooks/` (install once with `pwsh .githooks/setup.ps1`) run `run-tests.ps1` before a merge commit on `main` and `pack.ps1` after it.
