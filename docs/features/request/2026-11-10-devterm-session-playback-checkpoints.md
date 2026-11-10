# Feature request: DevTerm session playback checkpoints and note revisions

- **Status:** new
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm` (`Logging`, presenters)

## Need

Decided 2026-10-13: playback gets periodic checkpoint markers (so seeking restores the screen), a skip-silence gap cap (default 1 s), and note edit and delete recorded as revisions.

## Proposed behavior

- Checkpoint markers need an `IPresenter` snapshot contract.
- Notes are append-only: an edit or delete is a new revision.

## Source (read-only, to be retired)

dev-term `docs/design/session-logging.md`; dotex: [session logging](../../design/SessionLogging/session-logging.md).

## Acceptance criteria

- Given a long log, when seeking, then the screen matches the live session at that time.
- Given a gap longer than the cap, then playback skips it.
- Given an edited note, then both revisions are kept.

## Existing code checked

`PlaybackEngine` and `PlaybackController` already have a skip-silence gap cap, with tests. Not found: snapshot contract, checkpoint markers, note revisions.

## Security and operations

Logs can hold secrets: revisions keep old text, so redaction must cover them.

## Priority

- **Impact:** usable playback of long sessions.
- **Needed by:** no date
- **Workaround today:** replay from the start

## Open questions

None.

## Triage notes

Origin: dev-term BACKLOG (UX decisions awaiting implementation). Skip-silence part appears done: confirm and drop it from the scope.
