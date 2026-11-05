---
paths:
  - "src/**/*Tests*/**"
  - "src/**/*Test*.cs"
  - "src/.runsettings"
  - "containers/testing/**"
---

# Testing rules

- MSTest. Framework layer needs 80%+ coverage. Method names `Method_Scenario_Expected`, with Stage / Mock / Test / Assert / Verify comments.
- Every test carries exactly one tier: `Unit` (isolated, mocked), `Simulate` (full stack, in-memory persistence), `Integration` (Docker services), `DevLocal` (manual only), `LiveIntegration` (real cloud, manual only), `Hardware` (a physical device; runs in the normal run and ends `Assert.Inconclusive` when the device is not detected). Every library has its own test project; device libraries share helpers from `OoBDev.Devices.TestUtilities`. A test that touches an external service is never `Unit`. Secondary categories (owner, 2026-11-04, from DevTerm) sit beside the tier and are optional: transports (`Serial`, `Tcp`, `Hid`, `Usbtmc`, `Ble`, `Loopback`, `Rfc2217`, `Vxi11`, `Mqtt`, `Brokers`, `Web`), `Scpi`, features (`Logging`, `BugRegression` on the method of a test written from a bug report) and device models (`Kuando_Busylight`, `Velleman_K8055`, `Radex_One`, `Zoom_H4n`, `DerEe_De5000`, `Delorme_EarthmateBt20`, `Aeotec_ZStickS2`, `Rigol_*`, `Korad_*`, `Tektronix_*`, `Hp_34401a`), so `--filter TestCategory=Serial` selects across tiers. They live in `OoBDev.TestUtilities.TestCategories` and `OoBDev.CodingStandards.Tests` rejects an unknown one.
- Tests that need anything outside the process (services, other operating systems, shells, scripts) run against Docker containers, or Aspire hosting them in Docker; not against host installs such as WSL. Start the stack with `containers/testing`; add services with the `add-integration-service` skill.
- Read configuration with `TestContext.GetRequiredProperty<T>()` (URLs, credentials, connection strings) or `TestContext.GetPropertyOrDefault<T>()` (industry-standard ports). Never `Environment.GetEnvironmentVariable()`.
- Integration tests use unique resource names per run (`$"Test_{Guid.NewGuid():N}"`) and clean up in `[TestCleanup]`.
- Tests are deterministic (owner, 2026-10-30), above all when they persist screenshots, logs or other files in the repository: fixed clocks (`TimeProvider`), no random or machine-specific values in output, and masked zones for what cannot be fixed (`image-mask`).
- Floating-point comparisons use `NumericAsserts.AreSimilar()`.
- New or changed test properties go in `src/.runsettings` and `TEST_VARIABLES.md`. Adding a Docker service: use the `add-integration-service` skill.
- Never test against live data or a live connection string. Tests use throwaway resources (a test database, unique names, rows created and deleted by exact key); an Integration test refuses to run when its connection string is not a test instance. Restoring a sanitized production dump into a throwaway database to test migrations is allowed (`docs/how-tos/production-operations-practices.md`).
- Performance gates for APIs: log a request over 500 ms and fail it over 750 ms where a feature's testing strategy sets a threshold.
- Repository-wide check projects (`OoBDev.Docs.Tests`, `OoBDev.CodingStandards.Tests`) live in `src/Tools`, beside each other, not in a layer folder (owner, 2026-11-04).
- Report test results as they are. If a test fails or was not run, say so.
