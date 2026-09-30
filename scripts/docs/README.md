# Documentation scripts

Deterministic helpers for authoring docs (prefer these over ad-hoc/LLM edits).

| Script | Purpose |
|--------|---------|
| `validate-docs.py [paths]` | Renders every ```plantuml block through the `plantuml/plantuml-server:jetty` docker image (auto-started as `dotex-plantuml` on port 18080), checks relative links and caption order, rejects remote/C4 `!include`. Exit code 1 on problems. `--no-render` skips rendering. |
| `fix-plantuml-newlines.py <files>` | Joins real line breaks inside quoted PlantUML labels into escape sequences. Idempotent. |

Run before finishing any doc change: `python scripts/docs/validate-docs.py docs`.
Stop the server: `docker rm -f dotex-plantuml`. VS Code: use tasks in `.vscode/tasks.json`; recommended extensions in `.vscode/extensions.json`.
