# Documentation scripts

Deterministic helpers for authoring docs (prefer these over ad-hoc/LLM edits).

| Script | Purpose |
|--------|---------|
| `fix-plantuml-newlines.py <files>` | Joins real line breaks inside quoted PlantUML labels into escape sequences. Idempotent. |
| `build-index.py <folders>` | Adds nav lines, continuous caption numbers and the contents, figure and table lists to split document folders. |
| `build-project-catalog.py` | Regenerates `docs/patterns-discovery/07-project-catalog` from `src` (then run `build-index.py` on it). |

Validation is the MSTest project `src/Tools/OoBDev.Docs.Tests` (PlantUML structure and rendering, relative links, caption order). Run `DOCS_VALIDATE_PATHS=<folder> dotnet test src/Tools/OoBDev.Docs.Tests --filter TestCategory=Unit` before finishing a doc change; the Integration test renders every diagram through the PlantUML server that already runs on the system (container `plantuml`, `PLANTUML_URL`, default `http://localhost:8086`). VS Code: use tasks in `.vscode/tasks.json`; recommended extensions in `.vscode/extensions.json`.
