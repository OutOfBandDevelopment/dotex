# Feature request: project readme index and a markdown endpoint listing

- **Status:** done (2026-11-12; the index is the project catalog, the listing is the API guide with access lines)
- **Requested by:** owner (found while removing `docs/code`, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Directory.Build.props`, `docs/`, `OoBDev.TemplateEngine.Cli`

## Need

Two generated views existed and are gone:

1. `docs/code/{Project}/README.{Project}.md` copied every project's markdown and PlantUML files into one browsable place and packed them under `\docs` ("self-documenting packages", see [build and packaging](../../patterns-discovery/01-architecture/09-build-and-packaging.md)). The copy step was removed. The 49 copies were stale or orphaned and were deleted (commit `936b2ed7`).
2. `docs/Service-Endpoints.md` with `docs/swagger.json` was a markdown listing of endpoints, rights and request types, generated from OpenAPI with `docs/templates/Service-Endpoints.md.hbs`. Scalar and the OpenAPI documents replaced the viewer, but there is no committed, reviewable listing.

## Proposed behavior

- A generated index (links, not copies) lists every project with its readme, layer and package.
- A generated endpoint listing from the OpenAPI document includes `x-permissions` (anonymous flag and rights), written by an MSTest entry point (guides are generated, rule `work-tracking`).

## Acceptance criteria

- Given the solution, when the index is generated, then every project with a `README.{Project}.md` is listed once.
- Given a project without a readme, then the index flags it (as warning OBDPK001 does).
- Given the example API, when the listing is generated, then each endpoint shows its method, rights and anonymous flag.

## Existing code checked

[GeneratedGuides](../../design/GeneratedGuides/README.md) already covers API guides from OpenAPI and AsyncAPI, so this request may fold into it. The `CodeDocumentation` item in `src/Directory.Build.props` still packs docs into the packages.

## Security and operations

None.

## Priority

- **Impact:** discoverability of the 100+ projects; reviewing rights per endpoint.
- **Needed by:** no date
- **Workaround today:** browse `src`; use Scalar

## Open questions

Fold into GeneratedGuides or keep separate?

## Triage notes

The templates are kept in [docs/templates](../../templates/README.md).

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
