# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-10-29, DevTerm Core): keep the plugin assemblies separate?

The owner's decision merges Plugins.* into `OoBDev.DevTerm.Core`. `Plugins.KeyValue` and `Plugins.Sample` exist to be loaded from a plugin folder (`plugin.json` names the assembly), and the loader tests use `Sample` that way, so a merged copy would no longer test the loader.

**Options**
- A (recommended): keep `Plugins.KeyValue`, `Plugins.Sample` and the KeyValue tests as separate assemblies in `src/Applications`; everything else is merged. Keeps the plugin loader honest; one more project pair.
- B: merge them into Core as built-in features and drop the external plugin loader tests' sample. Fewer projects; loses the loader's real-world test.

**Default:** A (done in step 3).

**Your answer:**

