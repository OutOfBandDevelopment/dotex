# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-22 · **Branch:** `dev/claude-config`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] **CGI middleware tests and docs** ([design](docs/design/CgiMiddleware/README.md), `src/Extensions/OoBDev.AspNetCore.Cgi`)
  - [ ] Shell-script fixtures (`.sh`), run in a Linux container (Docker, or Aspire into Docker) instead of WSL
  - [ ] Tests for timeout (process tree kill), 503 at the concurrency cap, response size limit, stderr flood, client abort
  - [ ] DevLocal benchmark (REQ-015)
  - [ ] Run `validate-docs.py docs/design/CgiMiddleware` where Python exists

## Next

- [ ] HTTP querying and the rights middleware (waits for the answer to Q6 in [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md))
- [ ] `OoBDev.DacFx.Tests` SQLCLR test project
- [ ] AsyncAPI: send-only queues by type
- [ ] Analyzer `suggestion` rules to `warning` and the remaining build warnings (waits for Q12)
