---
name: feedback-migrate-whole-project
description: When migrating a project, review everything (project guide, agent config, docs, questions, TODO/backlog, UIs, tests, future plans), not just code
metadata:
  type: feedback
---

Owner, 2026-10-30: when migrating projects, review all docs including the project guide, skills, rules, hooks and other agent configuration, plus documentation, open questions, TODO and backlog, as well as code, UIs and tests, so everything including future plans is migrated.

**Why:** parity means nothing is lost: workflows and plans are part of the project.
**How to apply:** each item is migrated, merged or rejected with a recorded reason (rule `owner-decisions`, skill `compare-incoming-codebase`). Pending for dev-term: the BACKLOG item "dev-term surroundings review". Related: [[reference-test-hardware]].
