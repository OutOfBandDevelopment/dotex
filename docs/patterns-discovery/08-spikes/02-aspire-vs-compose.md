# Spike — Aspire versus Docker Compose for the integration test stack

<!-- nav -->
[↑ 08 — Spikes](./README.md) · [← Spike — Microsoft.Extensions.AI](./01-extensions-ai.md) · [Index →](./README.md)
<!-- nav -->

## Question

The Integration tests run against 15 services started by `containers/testing/docker-compose.integration-tests.yml` through `integration-up.{sh,bat}`. Would a .NET Aspire AppHost be a better way to define and start that stack, so the Docker test documentation is not written twice?

**Status:** desk study. No AppHost was built or run; the verdict rests on the current compose file and scripts, and on the Aspire features listed below, which should be re-checked against the installed Aspire version before any migration.

## What the compose stack depends on today

**Table 1 — Compose features in use**

| Feature | Where | Needs from an alternative |
|---------|-------|---------------------------|
| 15 services plus init containers (`moto-init`) | compose file | Containers, one-shot init jobs, start ordering |
| Health checks, waited on by `wait-for-services` | scripts | Readiness gating |
| Every host port overridable via `TEST_PORT_<NAME>` | compose and `.runsettings` | Fixed, documented host ports (tests read them as properties) |
| Mounted config (Keycloak realm, Service Bus emulator, nginx, Ollama image build) | `containers/testing/*` | Bind mounts and Dockerfile builds |
| Runs the same on a developer machine and in GitHub Actions | `integration-tests.yml` | Docker only, no extra install |
| Tests read settings via `TestContext` and `.runsettings`, not the environment | test code | A way to hand connection strings to MSTest |

## Comparison

**Table 2 — Aspire versus Compose**

| Criterion | Compose (today) | Aspire AppHost |
|-----------|-----------------|----------------|
| Definition language | YAML | C# (typed, refactorable) |
| Extra tooling | Docker only | .NET SDK plus the Aspire AppHost and its orchestrator; Docker still required for containers |
| Fixed ports and `TEST_PORT_*` overrides | Native | Possible, but Aspire defaults to dynamic ports and injects connection strings; the tests would need to read those instead of `.runsettings` |
| Mounted files, Dockerfile builds, init containers | Native | Supported, with more code per service |
| CI | `docker compose up --wait`, already working | Needs the Aspire testing package and a test fixture that starts the AppHost per run |
| Dashboard and telemetry | nginx dashboard plus Grafana LGTM | Built-in dashboard with OpenTelemetry; overlaps with the LGTM container |
| Fit with MSTest and `.runsettings` | Direct | Indirect: connection strings arrive from the AppHost, not from run settings |
| Migration effort | None | Rewrite 15 services, ports, health checks, init, scripts and docs |

## Verdict

**Stay on Docker Compose for the integration stack.** The tests are configured through `.runsettings` and fixed, overridable ports; Aspire's strengths (service discovery, injected connection strings, a development dashboard) solve a different problem and would require changing how every Integration test gets its settings. The compose stack already runs in CI and locally with Docker alone.

Revisit if a distributed sample application is added to the repository: an AppHost is a good fit for running the example web API with its dependencies during development, without replacing the test stack.

**Consequence for the documentation:** the Docker test documentation under `docs/architecture/testing/` stays as written; no rewrite is needed.

---

<!-- nav -->
[↑ 08 — Spikes](./README.md) · [← Spike — Microsoft.Extensions.AI](./01-extensions-ai.md) · [Index →](./README.md)
<!-- nav -->
