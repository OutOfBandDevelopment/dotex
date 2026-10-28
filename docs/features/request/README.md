# Feature requests

## Summary

Anyone can ask for a feature by adding a document to this folder. The maintainers triage each request into the design documents and the backlog, so nothing here needs to be perfect: say what you need and why.

## How to request a feature

1. Copy [TEMPLATE.md](TEMPLATE.md) to `docs/features/request/YYYY-MM-DD-short-name.md` (for example `2026-10-28-cgi-openapi-listing.md`).
2. Fill in every section. Write "unknown" instead of leaving one empty.
3. Commit the file on a `dev/<topic>` branch and tell the maintainers, or hand it over directly.

Before you write one, look at what already exists so the request does not duplicate it:

- [BACKLOG.md](../../../BACKLOG.md) and [TODO.md](../../../TODO.md) for open and active work.
- [docs/design](../../design) for features that already have requirements.
- [docs/harvest-candidates.md](../../harvest-candidates.md) for code in other repositories that could be reused.

## What happens next

A maintainer sets the **Status** line at the top of your file:

- `new` — waiting for triage.
- `triaged` — merged into a design document (`docs/design/<Feature>/`) and [BACKLOG.md](../../../BACKLOG.md); the file names both.
- `declined` — not planned, with the reason.
- `done` — built and tested; the file links the change document in [docs/changes](../../changes/README.md).

Questions back to the requester go into the file under **Triage notes**, so the history stays with the request.
