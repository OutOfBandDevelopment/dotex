# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 · 2026-10-29 · DevTerm parity · Transport contract

dev-term's engine uses `ITransport` (async, state, PipeReader); the device libraries here use `IDeviceAdapter` (stream). See [architecture](docs/design/DevTermParity/architecture.md#contracts). Which contract do we build on?

**Options**

- A. Keep `IDeviceAdapter`, add `ITransport` beside it with an adapter in each direction (recommended: no change to the device libraries, one implementation of each transport, and it fits the owner's rule of building on the existing device model).
- B. Replace `IDeviceAdapter` with `ITransport` everywhere (cleaner end state, rewrites every device library and test at once; allowed because the framework is unreleased).

**Default:** A.

**Your answer:**
