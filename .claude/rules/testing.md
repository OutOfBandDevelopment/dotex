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
- Tests that need anything outside the process (services, other operating systems, shells, scripts) run against Docker containers, or Aspire hosting them in Docker; not against host installs such as WSL. Start the stack with `containers/testing`; add services with the `add-integration-service` skill.
- Read configuration with `TestContext.GetRequiredProperty<T>()` (URLs, credentials, connection strings) or `TestContext.GetPropertyOrDefault<T>()` (industry-standard ports). Never `Environment.GetEnvironmentVariable()`.
- Integration tests use unique resource names per run (`$"Test_{Guid.NewGuid():N}"`) and clean up in `[TestCleanup]`.
- Floating-point comparisons use `NumericAsserts.AreSimilar()`.
- New or changed test properties go in `src/.runsettings` and `TEST_VARIABLES.md`. Adding a Docker service: use the `add-integration-service` skill.
- Never test against live data or a live connection string. Tests use throwaway resources (a test database, unique names, rows created and deleted by exact key); an Integration test refuses to run when its connection string is not a test instance. Restoring a sanitized production dump into a throwaway database to test migrations is allowed (`docs/how-tos/production-operations-practices.md`).
- Performance gates for APIs: log a request over 500 ms and fail it over 750 ms where a feature's testing strategy sets a threshold.
- Report test results as they are. If a test fails or was not run, say so.
