---
paths:
  - "src/**/*Tests*/**"
  - "src/**/*Test*.cs"
  - "src/.runsettings"
  - "containers/testing/**"
---

# Testing rules

- MSTest. Framework layer needs 80%+ coverage. Method names `Method_Scenario_Expected`, with Stage / Mock / Test / Assert / Verify comments.
- Every test carries one category: `Unit` (isolated, mocked), `Simulate` (full stack, in-memory persistence), `Integration` (Docker services), `DevLocal` (manual only), `LiveIntegration` (real cloud, manual only). A test that touches an external service is never `Unit`.
- Read configuration with `TestContext.GetRequiredProperty<T>()` (URLs, credentials, connection strings) or `TestContext.GetPropertyOrDefault<T>()` (industry-standard ports). Never `Environment.GetEnvironmentVariable()`.
- Integration tests use unique resource names per run (`$"Test_{Guid.NewGuid():N}"`) and clean up in `[TestCleanup]`.
- Floating-point comparisons use `NumericAsserts.AreSimilar()`.
- New or changed test properties go in `src/.runsettings` and `TEST_VARIABLES.md`. Adding a Docker service: use the `add-integration-service` skill.
- Report test results as they are. If a test fails or was not run, say so.
