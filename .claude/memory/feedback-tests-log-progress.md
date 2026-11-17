---
name: feedback-tests-log-progress
description: New tests log what they do (TestContext.WriteLine per phase) so a run shows how far it got
metadata:
  type: feedback
---

Write new tests with extra logging so the owner can tell how far a test got and what it actually did.

**Why:** Owner, 2026-11-17: output of skipped, Inconclusive or failing runs (especially Integration, Hardware and LiveIntegration) did not show progress. Existing tests are not to be rewritten for this.

**How to apply:** `TestContext.WriteLine` per Stage / Test / Assert step with inputs, endpoint or container, key values and the reason before any `Assert.Inconclusive`; never secrets. Recorded in rule `testing` and skill `write-tests`. Related: [[feedback-integration-tests-marked]].
