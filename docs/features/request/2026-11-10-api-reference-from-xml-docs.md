# Feature request: API reference generated from the XML docs

- **Status:** triaged
- **Requested by:** owner (found while removing `docs/Libraries`, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** build, `OoBDev.TemplateEngine.Cli`, `docs/`

## Need

`docs/Libraries` (47 files, February 2025) was a per-project API reference generated from the XML doc comments with `docs/templates/Documentation.md.xslt`. It went stale, listed removed projects, and was deleted (commit `e73474e2`). Public API docs now exist only inside the code.

## Proposed behavior

A script generates one API reference page per packable project from its XML doc file, on demand, into a build output folder (or `docs/api` if the owner wants it committed). It runs through `TemplateEngine.Cli` and leaves no stale pages for projects that no longer exist.

## Acceptance criteria

- Given a project with XML docs, when the generator runs, then a page lists its public types, members and summaries.
- Given a project removed from the solution, then no page remains for it after a run.
- Given two runs on the same commit, then the output is identical.

## Existing code checked

The XSLT is `docs/templates/Documentation.md.xslt`. DocFX and similar tools are an alternative; the owner prefers deterministic MSTest or script entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)).

## Security and operations

None.

## Priority

- **Impact:** nice to have; the readmes carry the usage docs.
- **Needed by:** no date
- **Workaround today:** IDE tooltips and the XML files in the packages

## Open questions

Committed or generated only? One page per project or per namespace?

## Triage notes

The template is kept in [docs/templates](../../templates/README.md) (`Documentation.md.xslt`); the deleted pages are in the parent of commit `e73474e2`.

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
