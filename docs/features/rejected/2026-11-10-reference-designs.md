# Feature request: reference designs for common entities

- **Status:** declined (owner, 2026-11-19, Q19: closed as stale; test data uses the CSV utility project, reference designs are covered by the patterns blueprint; reopen a line by filing a new request)
- **Requested by:** owner (kept from `docs/ReferenceDesign`, removed 2026-11-10)
- **Date:** 2026-11-10
- **Area:** docs, `docs/design`, shared framework

## Need

An older idea: reference designs are common entity patterns and structures that applications reuse (users, contacts, scheduled work, state machines, workflows). Composing an application from such pieces is faster and more consistent. Only a user profile requirements page was ever written; it is covered now by `docs/design/SharedIdentity/ModularProfileManagement`.

## Proposed behavior

A short catalogue of reference entity designs, each with entities, states and the rights it needs, linked from `docs/design/README.md`. Candidates: contact, scheduled work, state machine, workflow.

## Acceptance criteria

- Given a new application, when its entities are chosen, then a reference design exists to start from or the gap is recorded.

## Existing code checked

`docs/design/SharedIdentity`, `Communications` and the `docs/patterns-discovery` blueprint; no catalogue exists.

## Security and operations

Each design lists its rights (`[ApplicationRight]`).

## Priority

- **Impact:** consistency for future products.
- **Needed by:** no date
- **Workaround today:** copy a similar project

## Open questions

Is this the patterns-discovery blueprint under another name?

## Triage notes

The deleted pages are in the parent of this commit.

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.

2026-11-19: proposed catalogue written: [ReferenceDesigns](../../design/ReferenceDesigns/README.md).
