# Feature request: Applesoft BASIC language parser and syntax tree

- **Status:** triaged
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System/Retro/Apple2/ApplesoftBASIC`

## Need

Applesoft programs can be detokenized to text but not parsed into a syntax tree for analysis or re-tokenizing.

## Proposed behavior

A grammar and parser producing an AST, plus a tokenizer back to the binary form. The earlier idea was ANTLR4; that is a third-party dependency, so a hand-written parser is the alternative.

## Acceptance criteria

- Given a listing, then the parser returns an AST for every statement.
- Given an AST, then the tokenizer produces bytes that the detokenizer reads back to the same text.

## Existing code checked

`Detokenizer` and `TokenMap` exist; there is no parser or tokenizer.

## Security and operations

Bound input size and nesting.

## Priority

- **Impact:** enables analysis and conversion of retro programs.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

ANTLR4 (a dependency) or a hand-written parser?

## Triage notes

Origin: BinaryDataDecoders issue 11. Low priority and specialized; accept, schedule or decline.

2026-11-11: triaged. Accepted at low priority: every BinaryDataDecoders feature is kept (scope rule in `CLAUDE.md`), so it is scheduled behind the DevTerm and event-sourcing work. Backlog: BACKLOG.md, Migrations (specialized formats).
