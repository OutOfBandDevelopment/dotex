# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q31 — 2026-11-23 — DevTerm plugins: sandbox the `process` plugins?

**Question:** DevTerm `process` plugins (Python, Java, Go over JSON lines) run with the user's full rights after approval. The sandbox library can limit them, but its launchers start only their own plugin host and speak a different protocol ([design](docs/design/Plugins/sandboxed-launch.md)). Should DevTerm plugins run through the sandbox?

**Options:**
- A (recommended): add a command-line mode to the sandbox launchers and keep the JSON-lines protocol; `plugin.json` gets an optional `permissions` block, and a plugin with it is refused where no launcher exists. Existing examples keep working and the isolation is real.
- B: move presenter plugins to the sandbox frame protocol. One protocol, but every language needs a frame codec and presenters gain nothing from the broker.
- C: stay unsandboxed with approval only. No work, no isolation.

**Default:** A, built only after your answer (it changes the sandbox library API); until then the design is the deliverable and nothing is built.

**Your answer:**
