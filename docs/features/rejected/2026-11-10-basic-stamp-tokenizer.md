# Feature request: Basic Stamp tokenizer and detokenizer

- **Status:** declined for now (owner, 2026-11-23): no consumer; was on hold since 2026-11-12. If resumed: BS2 family, format from a third-party reference such as `pbasic` or `stampy` after a licence check
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System` (`Retro` or a new microcontroller area)

## Need

There is no tool to tokenize or detokenize Basic Stamp programs for the microcontroller.

## Proposed behavior

Convert between Basic Stamp source and its tokenized EEPROM image, like the Applesoft `Detokenizer`.

## Acceptance criteria

- Given a tokenized program, then the detokenizer returns the original source.
- Given source, then the tokenizer output round-trips.

## Existing code checked

`Retro/Apple2/ApplesoftBASIC` (`Detokenizer`, `TokenMap`) is the model; nothing for Basic Stamp.

## Security and operations

Inputs are untrusted: bound lengths.

## Priority

- **Impact:** retro and hobby hardware; niche.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

On hold (owner, 2026-11-12). When resumed: BS2 family, format from a third-party reference (`pbasic` or `stampy`) chosen after a licence check.

## Triage notes

Origin: BinaryDataDecoders issue 25. Low priority and specialized; accept, schedule or decline.

2026-11-11: triaged. Accepted at low priority: every BinaryDataDecoders feature is kept (scope rule in `CLAUDE.md`), so it is scheduled behind the DevTerm and event-sourcing work. Backlog: BACKLOG.md, Migrations (specialized formats).
