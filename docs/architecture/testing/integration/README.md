# Integration Testing

[Testing home](../README.md) · [Docker infrastructure →](docker-infrastructure.md)

Integration tests run against real services in Docker containers (databases, brokers, emulators, identity, AI). They are marked `[TestCategory(TestCategories.Integration)]` and run in CI on the daily schedule, never in the per-PR run. This folder documents the stack; the operational guide (scripts, troubleshooting, nginx dashboard) stays in [`containers/testing`](../../../../containers/testing/README.md).

## Contents

- [Documents](#documents)
- [Dependency matrix](#dependency-matrix)
- [Decision: Aspire](#decision-aspire)

## Documents

**Table 1 — This folder**

| Document | Contents |
|----------|----------|
| [Docker infrastructure](docker-infrastructure.md) | Topology, networks, ports and overrides, volumes, health checks, startup order, CI |
| [Services](services.md) | One section per stack: image, ports, credentials, initialization, which tests use it |
| [Writing tests](writing-tests.md) | Test properties, `.runsettings`, cleanup, a worked example |

## Dependency matrix

Which test projects need which service. A test project that appears here must mark those tests `Integration` (or `DevLocal`), never `Unit`.

**Table 2 — Test project to service**

| Test project | Services |
|--------------|----------|
| `OoBDev.MongoDB.Tests` | MongoDB |
| `OoBDev.Redis.Caching.Tests` | Redis |
| `OoBDev.OpenSearch.Tests` | OpenSearch |
| `OoBDev.Qdrant.Tests` | Qdrant |
| `OoBDev.Data.Vectors.DB.Tests` | SQL Server |
| `OoBDev.Amazon.Sqs.Tests` | Moto |
| `OoBDev.Microsoft.Azure.ServiceBus.Tests` | Service Bus emulator (and its SQL Server) |
| `OoBDev.RabbitMQ.Tests` | RabbitMQ |
| `OoBDev.MailKit.Tests` | smtp4dev |
| `OoBDev.Apache.Tika.Tests`, `OoBDev.Documents.Tests` (conversion), `OoBDev.Example.Tests` (document conversion) | Apache Tika |
| `OoBDev.Keycloak.Tests` | Keycloak |
| `OoBDev.SBert.Tests`, `OoBDev.SBert.AllMiniLmL6V2.Tests` | SBert |
| `OoBDev.Ollama.Tests` | Ollama |
| `OoBDev.OpenTelemetry.Tests` | Grafana LGTM (Tempo, Loki) |
| `OoBDev.Onnx.*.Tests` | None (models download into the shared hub cache) |

Azurite has a stack entry and test variables, but no test project uses it yet.

## Decision: Aspire

The owner decision in [testing alternatives](../../../patterns-discovery/05-industry-alternatives/06-testing-and-quality.md) allows .NET Aspire if no functionality is lost, and asks for a spike first. Until that spike exists, the Docker Compose stack is the supported infrastructure and these pages describe it. Anything written here about ports, health and credentials is also the checklist the spike has to reproduce.

[Testing home](../README.md) · [Docker infrastructure →](docker-infrastructure.md)
