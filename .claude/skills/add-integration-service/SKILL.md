---
name: add-integration-service
description: Add, change or remove a Docker service in the integration test stack (containers/testing), or add integration test parameters. Use when asked to "add new container", "add integration test service", "setup {service} for integration testing", or when a new Integration test needs a service or settings.
---

# Add or change an integration test service

The stack lives in `containers/testing/` (compose file `docker-compose.integration-tests.yml`). Current services: Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch (+ Dashboards), Qdrant, Azurite, Moto (AWS), Service Bus emulator, Keycloak, SBert, Ollama, Aspire dashboard (`otel-dashboard`: OTLP receiver, traces, logs, metrics, telemetry API), PlantUML, and nginx (dashboard/proxy). Read `docs/architecture/testing/integration/` first; it is the source of truth for the layout.

## Checklist (adding a service)

1. **Compose entry** (`docker-compose.integration-tests.yml`): container name `oobd-test-{name}`, network `integration-test-net`, health check, ephemeral named volume `{name}-test-data` if stateful. Every host port is `"${TEST_PORT_<NAME>:-<default>}:<container>"`; pick a default that does not clash with other services or common local installs (see RabbitMQ 5673 and Ollama 11435 for how clashes were handled).
2. **Scripts** (`scripts/`): add to the startup output in `integration-up.sh` and `.bat`, and to the `SERVICES` list in `wait-for-services.sh` and `.bat`. A service with no health check (Service Bus emulator) must be noted as such.
3. **nginx** (`nginx/nginx.conf`, `nginx/html/index.html`): proxy location and dashboard card when the service has a UI or API; update the counts on the dashboard; document the path in `NGINX-DASHBOARD.md`.
4. **Test settings**: parameters in `src/.runsettings` (URL, HOST, PORT, credentials, connection string), matching names in `containers/testing/.env.integration`, and (while the legacy workflow stays) the env block of the "Run Integration Tests" step in `.github/workflows/integration-tests.yml`. Names are `{SERVICE}_{PROPERTY}`.
5. **Docs**: `TEST_VARIABLES.md` (root), `containers/testing/README.md` (tables, port-conflicts list), and under `docs/architecture/testing/integration/` the `services.md` section and the dependency matrix row in `README.md`. Diagrams are PlantUML.
6. **Tests**: category `Integration`; read settings with `TestContext.GetRequiredProperty<T>()` / `GetPropertyOrDefault<T>()` (never `Environment.GetEnvironmentVariable`); unique resource names; clean up in `[TestCleanup]`.
7. **Verify**: `cd containers/testing && ./scripts/integration-up.sh --wait` (or `.bat`), then `cd ../../src && dotnet test --filter "TestCategory=Integration"` for the affected tests, then `./scripts/integration-down.sh --clean`. Say which services you actually started and which tests ran. If Docker is unavailable, say the change is unverified.
8. **Track**: update `TODO.md`, and `CLAUDE.md`'s service list only if the count or names changed.

## Changing or removing a service

Touch the same files as above in reverse: compose (service and volume), both script pairs, nginx conf and dashboard counts, `.runsettings`, `.env.integration`, the CI env block, docs, and any tests or hard-coded values (search for the old port and name).

## Reference

`reference.md` is the original January 2026 protocol with full YAML, nginx, HTML and script snippets. The snippets are still useful as templates; where it disagrees with this file or with the compose file, this file and the compose file win (it predates Moto, Aspire and the `TEST_PORT_*` overrides).
