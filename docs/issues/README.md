# Issues

Working detail for items in [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md) that needs more room than a feature design document should carry: a bug with a reproduction, a finding with evidence, a step-by-step work plan.

- One file per item, named `{area}-{short-name}.md`; the first lines hold `Status`, the linked feature document and the linked backlog line.
- The backlog and the TODO stay one line each and link the issue or the feature document.
- When the work is done, the final detail goes into the feature design specification and is covered by tests (rule `work-tracking`), the change document is written, and the issue file is deleted. History stays in `git log`.
- Imported dev-term bug reports remain under [docs/applications/devterm/bugs](../applications/devterm/bugs/README.md) until the documentation cleanup.
