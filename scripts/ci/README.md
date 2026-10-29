# Local CI scripts

GitHub is no longer the primary repository, so the build and test pipelines run locally.

| Script | Replaces | What it does |
|--------|----------|--------------|
| `run-linux-tests.ps1` | WSL for Linux-only tests | Copies `src/` into the `mcr.microsoft.com/dotnet/sdk:10.0` container (host build output is untouched, GitVersion off) and runs `dotnet test` there; default project is the CGI middleware tests (`.sh` fixtures). Results in `artifacts/test-results/linux` |
| `run-tests.ps1` | `.github/workflows/dotnet.yml` (build and test) and `integration-tests.yml` | Restore, build and test `src/` for the chosen categories; starts and always stops the Docker stack when `Integration` is included |

```powershell
pwsh scripts/ci/run-tests.ps1                          # Unit and Simulate
pwsh scripts/ci/run-tests.ps1 -Category Integration    # Docker stack, then Integration
pwsh scripts/ci/run-tests.ps1 -Category Unit -Coverage
pwsh scripts/ci/run-tests.ps1 -Category Integration -KeepStack
```

Results are written to `artifacts/test-results`. `LiveIntegration` needs credentials (for example `GROQ_API_KEY`, see `docs/architecture/testing/live-integration/groq.md`).

Not covered yet: the release workflows (`release.yml`, `deploy-release.yml`, `scheduled-release.yml`), the `validated-v{version}` tag.

| Script | What it does |
|--------|--------------|
| `register-daily-task.ps1` | the daily cron of `integration-tests.yml` | Registers a Windows Task Scheduler entry that runs `run-tests.ps1 -Category Integration` daily (`-At 16:00`, `-Unregister` removes it). Not registered by default |
| `new-runsettings.ps1` | Writes `artifacts/test.runsettings`, a copy of `src/.runsettings` whose ports follow `TEST_PORT_<NAME>` (environment or `containers/testing/.env`). `run-tests.ps1` calls it, so the stack and the tests share one source of truth. |
