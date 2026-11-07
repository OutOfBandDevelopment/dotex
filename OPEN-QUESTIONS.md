# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-11-06, SharedFramework): which unbuilt design is implemented first?

The reconciliation of all five SharedFramework epics against the code (2026-11-06) shows the built parts and the unbuilt ones: background tasks and cache warming (Caching); profiles, roles and claims, sessions and tokens (Identity); extraction, OCR, rendering, splitting, packing, unpacking and composition (Documents); database template source, `IDataContainer` and compiled templates (Text Templating); channel routing and message composition (Communications, partly).

**Options**

- **A. Text Templating database source** (recommended): smallest (about 400 LOC per the design), builds on the working engine and Handlebars provider, and the Example API already has a template controller to show it.
- **B. Documents packing and unpacking**: reuses the Archives (Zip, Tar) code already in `OoBDev.System`; medium size.
- **C. Identity roles and claims**: ties into the rights middleware decision, so it is the most valuable but needs the layering answers first.
- **D. None for now**: keep them as designs until a consumer appears.

**Default:** D until answered: option A needs a layering answer first (which project hosts a database-backed source without a data-access reference in Framework: a new adapter under ExternalServices, recommended, or `OoBDev.Data.Common`), and the rule `layering-and-migration-plans` forbids deciding that mid-task. Reply with the letter and the host project.

**Your answer:**
