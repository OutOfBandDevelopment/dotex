# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q1 (2026-10-29, DevTerm plugins) Where does the Sample plugin live?

`OoBDev.DevTerm.Plugins.Sample` must stay a separate assembly, because the plugin-loader tests load it from `plugins/sample` the way a real plugin loads. Today it sits beside the applications and reads like a shipped plugin, although it is only a loader-test fixture.

**Options**
- A. Rename it to a test fixture project beside the tests, `OoBDev.DevTerm.Core.TestPlugin` (recommended): the name says what it is, and nothing can mistake it for a product plugin. Costs one rename (project, namespace, loader-test paths).
- B. Keep the name and add a readme line that says it is a fixture. No churn, but the name still suggests a product.
- C. Compile a plugin assembly at test time (Roslyn). No fixture project at all, but slower and more fragile tests.

**Default:** A when answered; until then it stays as is.

**Your answer:**
