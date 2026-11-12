# Feature request: test results summary as markdown

- **Status:** done (2026-11-12)
- **Requested by:** owner (found while cleaning `docs/templates`, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `scripts/ci`, `src/test.sh`, `OoBDev.TemplateEngine.Cli`

## Need

`src/test.sh` merges `.trx` files into `LatestTestResults.trx`, but nothing turns them into a readable report. The old `docs/templates/TestResultsToMarkdown.md.xslt` and `docs/Tests/Summary.md` did, and are stale.

## Proposed behavior

After a test run, one markdown summary lists totals per tier (`Unit`, `Simulate`, `Integration`, `Hardware`), failures with their messages, skipped and inconclusive tests, and coverage if present. It is produced with `TemplateEngine.Cli`.

## Acceptance criteria

- Given a merged `.trx`, when the report step runs, then the totals match the `.trx` counters.
- Given a failing test, then it is listed with its error message.
- Given a Hardware test that ended inconclusive, then it appears under inconclusive, not as failed.

## Existing code checked

`scripts/ci/run-tests.ps1` and `run-linux-tests.ps1` produce results; the report step is the only missing part.

## Security and operations

None. Test output can contain machine-specific values, so mask them (rule `testing`, deterministic output).

## Priority

- **Impact:** readable results for manual CI, which is the only CI now.
- **Needed by:** no date
- **Workaround today:** read the console output

## Open questions

Commit the summary or keep it as a build artifact only?

## Triage notes

The template is kept in [docs/templates](../../templates/README.md) (`TestResultsToMarkdown.md.xslt`).

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
