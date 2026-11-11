# Feature request: publish target for report projects

- **Status:** triaged
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** build, `src/`

## Need

The old repository had a custom SDK-style project and publish profile for report definitions; nothing equivalent exists here.

## Proposed behavior

Decide whether report projects are wanted. If so, add a project type or publish profile that builds and packages report definitions.

## Acceptance criteria

- Given a report project, then `dotnet publish` produces the report artifact.
- Given reports are not wanted, then the request is declined with the reason.

## Existing code checked

The migration plan rejected the old report project and report list as old build glue ("revisit on request").

## Security and operations

None.

## Priority

- **Impact:** only if reports return.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Are report projects wanted at all? Recommended default: decline.

## Triage notes

Origin: BinaryDataDecoders issue 19. Accept or decline.

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
