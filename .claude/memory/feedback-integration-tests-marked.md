---
name: feedback-integration-tests-marked
description: Integration tests may be added freely but must carry TestCategories.Integration (or DevLocal/LiveIntegration) so they stay out of the Unit/Simulate pipeline
metadata:
  type: feedback
---

Adding tests that hit Docker services or other external systems is fine, but they must be marked `[TestCategory(TestCategories.Integration)]` (or DevLocal / LiveIntegration as appropriate), never Unit or Simulate.

**Why:** the CI unit-test pipeline runs only Unit and Simulate; unmarked external-service tests would break it.

**How to apply:** tag every new test that needs a container, network or cloud service; use `TestContext.GetPropertyOrDefault` for settings. Qdrant tests are the reference example. See [[feedback-docs-plantuml]] for the other standing test/doc rules.
