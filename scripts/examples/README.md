# Example scripts

| Script | Purpose |
| --- | --- |
| `verify-example-web.ps1` | Unit tests, build, Playwright journeys, and a check that the generated guides and specs under `docs/example` match what is committed (screenshots are refreshed but not compared). Run it before committing UI changes. `-SkipInstall` reuses `node_modules`; `-Accept` keeps regenerated docs. |

The check is local on purpose; there is no CI workflow for it.
