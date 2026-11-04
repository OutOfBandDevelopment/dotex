# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open

### Q1 (2026-11-04, Tools): where does the repository-wide coding-standards test project live?

`OoBDev.CodingStandards.Tests` scans every test source in the repository. **Options:** (A, recommended) `src/Tools`, beside `OoBDev.Docs.Tests`, the other repository-wide check; (B) `src/Framework` with the other test libraries, which implies the 80% coverage rule and a Framework layer entry for a project that tests no library.
**Default:** A.
**Your answer:** why are you suggesting to skip oobdev.docs.tests?

**Follow-up (2026-11-04):** Option A does not skip `OoBDev.Docs.Tests`: it puts `OoBDev.CodingStandards.Tests` in `src/Tools` next to it (both scan the whole repository) and it already lives there. Reply "A" to confirm, or "B" to move it into `src/Framework` (which brings the 80% coverage rule). If you meant that the docs tests should also check test sources, say so and it becomes a backlog item.
