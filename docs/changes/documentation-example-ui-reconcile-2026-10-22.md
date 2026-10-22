# example-ui design reconciled with the code

**Date:** 2026-10-22 · **Design:** [example-ui](../design/example-ui/README.md)

- Table 2 (navigation as built) matches `src/layout/nav.ts` entry for entry, and every page in `src/pages` is in the navigation. No design change was needed.
- Pages with no view-model test: Communications, Current user, Queues and Templates (the others have a `*.test.ts`). Added to BACKLOG.

Verification: read both sides only; no tests were run.
