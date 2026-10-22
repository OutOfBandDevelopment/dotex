# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-22 · **Branch:** `dev/claude-config`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] **CGI middleware tests and docs** ([design](docs/design/CgiMiddleware/README.md), `src/Extensions/OoBDev.AspNetCore.Cgi`)
  - [ ] Shell-script fixtures (`.sh`), run in a Linux container (Docker, or Aspire into Docker) instead of WSL
  - [ ] Tests for timeout (process tree kill), 503 at the concurrency cap, response size limit, stderr flood, client abort
  - [ ] DevLocal benchmark (REQ-015)

## Next

- [ ] **BinaryDataDecoders comparison and recommendation plan** (`compare-incoming-codebase` skill): compare `C:epomineinary-data-decoders` (newest) and `Incoming/BinaryDecoders` with what exists in `src/`, and write a plan the owner reviews (feature mapping, what exists, gaps, proposed phases and placement, each decision as options with a recommendation). No migration until the owner approves the plan.

- [ ] Rights middleware, then `QUERY`/OData, then GraphQL (order decided; roles map to rights in configuration, `OoBDev:Authorization:Roles:<role>`, applied by a claims transformation; write the feature design first in `docs/design/`)
- [ ] `OoBDev.DacFx.Tests` SQLCLR test project
- [ ] AsyncAPI: send-only queues by type
- [ ] Promote the clean analyzer `suggestion` rules to `warning` and fix or remove the `#warning` markers (decided)
- [ ] Docs validation as a .NET test (decided: `src/Tools/OoBDev.Docs.Tests`, Integration category with the `plantuml/plantuml-server` container; link and caption checks also as Unit tests; remove `validate-docs.py` once it covers the same checks)
