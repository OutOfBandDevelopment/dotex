# Feature request: file globbing through Microsoft.Extensions.FileSystemGlobbing

- **Status:** new
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System` (`IO/PathEx`)

## Need

`PathEx` carries its own wildcard parsing while `Microsoft.Extensions.FileSystemGlobbing` is already used by other projects in the repo.

## Proposed behavior

Replace the custom wildcard logic with the platform matcher and keep the same public helpers.

## Acceptance criteria

- Given the existing `PathEx` tests, then they pass unchanged.
- Given `**` and negation patterns, then they work.

## Existing code checked

`PathEx.cs` (several `wildCards` code paths); FileSystemGlobbing is used in `DacPacMergeDefinitionFactory`, `PackMan.Cli` and `FixSourceLinks.Cli`.

## Security and operations

Resolve patterns against a root so they cannot escape it.

## Priority

- **Impact:** less custom code and standard semantics; fits the backlog theme of replacing hand-built code with platform primitives.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Does `OoBDev.System` accept the package reference (layering question for the owner)?

## Triage notes

Origin: BinaryDataDecoders issue 17. Accept, schedule or decline.
