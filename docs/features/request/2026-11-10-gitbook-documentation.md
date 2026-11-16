# Feature request: evaluate GitBook for the documentation

- **Status:** declined (owner, 2026-11-16: keep reading the repository and the generated guides; nothing is published outside it)
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `docs/`

## Need

Whether the documentation should also be published through GitBook.

## Proposed behavior

A short written evaluation (cost, fit with markdown plus PlantUML, sync model) and a recommendation.

## Acceptance criteria

- Given the evaluation, then the owner accepts or declines publishing there.
- Given it is declined, then no work follows.

## Existing code checked

`docs/` is markdown with PlantUML validated by `OoBDev.Docs.Tests`; generated guides are planned (`docs/design/GeneratedGuides`).

## Security and operations

Publishing exposes the docs externally; decide what may be public first.

## Priority

- **Impact:** optional publishing channel.
- **Needed by:** no date
- **Workaround today:** read the repository

## Open questions

Is an external docs site wanted at all?

## Triage notes

Origin: BinaryDataDecoders issue 21. Accept or decline.

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
