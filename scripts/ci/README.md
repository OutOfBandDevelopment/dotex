# Local CI scripts

GitHub is no longer the primary repository, so the build and test pipelines run locally.

| Script | Replaces | What it does |
|--------|----------|--------------|
| `run-tests.ps1` | `.github/workflows/dotnet.yml` (build and test) and `integration-tests.yml` | Restore, build and test `src/` for the chosen categories; starts and always stops the Docker stack when `Integration` is included |

```powershell
pwsh scripts/ci/run-tests.ps1                          # Unit and Simulate
pwsh scripts/ci/run-tests.ps1 -Category Integration    # Docker stack, then Integration
pwsh scripts/ci/run-tests.ps1 -Category Unit -Coverage
pwsh scripts/ci/run-tests.ps1 -Category Integration -KeepStack
```

Results are written to `artifacts/test-results`. `LiveIntegration` needs credentials (for example `GROQ_API_KEY`, see `docs/architecture/testing/live-integration/groq.md`).

Not covered yet: the release workflows (`release.yml`, `deploy-release.yml`, `scheduled-release.yml`), the `validated-v{version}` tag, and a schedule (use Windows Task Scheduler to run the script daily).
