# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q45 (2026-11-25, Test infrastructure): how should a cold Integration run start the Docker stack?

A whole-solution Integration run on a cold stack (about 15 containers starting together) fails 5 to 15 tests that each pass alone ([issue](docs/issues/cold-stack-integration-run.md)). `-m:2` and a settle delay did not help, and a mutex around compose up only staggers starts by seconds. Each attempt to prove a cure costs a 15-minute cold run.

**Options**

- **A. Ordered start in one pre-step (recommended).** A single `scripts/ci` step (and the test harness when it finds the stack down) starts the services in dependency waves and waits for each wave to be healthy before any test project runs. Trade-off: slower first start, but stable; the cost is one new script and a harness change. Matches your earlier warm-up script.
- **B. Keep the warm-up script as the only supported path.** Document that a cold whole-solution run is unsupported; run `warm-up-integration.ps1` first. Trade-off: no code, but the failure stays for anyone who forgets.
- **C. Retry flaky Integration tests once.** Trade-off: hides real failures and lengthens the run.

**Default:** A, built as the script first (reversible, no live data).

**Your answer:**
