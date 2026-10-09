# Documentation scripts

Deterministic helpers for authoring docs (prefer these over ad-hoc/LLM edits).

| Script | Purpose |
|--------|---------|
| `validate-docs.py [paths]` | Renders every ```plantuml block through the `plantuml/plantuml-server:jetty` docker image (auto-started as `dotex-plantuml` on port 18080), checks relative links and caption order, rejects remote/C4 `!include`. Exit code 1 on problems. `--no-render` skips rendering. |
| `fix-plantuml-newlines.py <files>` | Joins real line breaks inside quoted PlantUML labels into escape sequences. Idempotent. |
| `build-index.py <folders>` | Adds nav lines, continuous caption numbers and the contents, figure and table lists to split document folders. |
| `build-project-catalog.py` | Regenerates `docs/patterns-discovery/07-project-catalog` from `src` (then run `build-index.py` on it). |

Run before finishing any doc change: `python scripts/docs/validate-docs.py docs`.
Stop the server: `docker rm -f dotex-plantuml`. VS Code: use tasks in `.vscode/tasks.json`; recommended extensions in `.vscode/extensions.json`.
