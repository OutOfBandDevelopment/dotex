# TODO - Local Integration Testing (Docker) Epic

**Last Updated:** 2026-01-24

Docker-based integration testing infrastructure for OoBDev framework.

> **Parent Document:** [TODO.md](./TODO.md)
> **Related:**
> - [TODO-testing-live-integration.md](./TODO-testing-live-integration.md) - Cloud-based testing
> - [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Test property reference (14 Docker services)
> - [docs/architecture/testing-guidelines.md](./docs/architecture/testing-guidelines.md) - Testing best practices

---

## Overview

Complete Docker-based integration testing infrastructure enabling automated testing of external service integrations in CI/CD pipelines.

**Goal:** Run Integration tests against real Docker-based services (MongoDB, SQL Server, RabbitMQ, etc.) in CI/CD with clean state management and health checks.

**Test Category:** `Integration` - Docker services that can run anywhere Docker is available

---

## Completed Work ✓

Moved to the change history: [testing-local-integration-completed-2026-10-09.md](docs/changes/testing-local-integration-completed-2026-10-09.md) (Weeks 1 and 2, validation, scripts and health checks, plus the former architecture, risk, success criteria and environment variable reference sections).

---

## Pending Work

### Week 3: CI/CD Pipeline Enablement (IN PROGRESS)

The workflow `.github/workflows/integration-tests.yml` is enabled (daily 16:00 UTC schedule and `workflow_dispatch`, `ubuntu-latest`, 30-minute limit). It has not run on GitHub yet.

- [ ] Merge to `main`, then trigger it once by hand (`gh workflow run integration-tests.yml`); the manual trigger only exists once the file is on the default branch
- [ ] Fix whatever the first run shows (the solution has net48 and SQL CLR projects that were built on Windows before)
- [ ] Verify the Docker services start and report healthy, the Integration tests pass, results upload and cleanup runs even on failure
- [ ] Verify the `validated-v{version}` tag is created on `main`
- [ ] Watch the first scheduled run; document any CI-specific adjustments

**Rollback:** comment the triggers out again; local Docker testing stays available.

---

### Week 4 (Part 1): Docker Documentation (PENDING)

#### Integration Category Documentation

- [ ] Create `docs/architecture/testing/categories/integration/README.md`
  - Integration test standards
  - Docker requirements
  - Test patterns (setup, cleanup, isolation)
  - Environment variable usage
  - Code examples for each pattern
  - Unique resource naming convention
  - Cleanup best practices

- [ ] Create `docs/architecture/testing/categories/integration/docker-setup.md`
  - Docker Desktop/Engine installation
  - Port requirements and conflicts
  - Disk space requirements
  - Network configuration
  - Performance optimization

- [ ] Create `docs/architecture/testing/categories/integration/writing-tests.md`
  - Step-by-step guide to write Integration tests
  - Environment variable patterns
  - Unique resource naming (`IntegrationTest_{Guid}`)
  - Cleanup strategy ([TestCleanup])
  - Connection retry logic
  - Common pitfalls

- [ ] Create `docs/architecture/testing/categories/integration/examples.md`
  - Complete working examples for each service
  - Database tests (SQL Server, MongoDB)
  - Messaging tests (RabbitMQ)
  - Search tests (OpenSearch, Qdrant)
  - Document processing (Apache Tika)
  - Email tests (SMTP4Dev)

#### Stack Documentation (11 Docker-based stacks)

**Database:**
- [ ] `docs/architecture/testing/stacks/database/sql-server.md`
  - Image: `mcr.microsoft.com/mssql/server:2022-latest`
  - Port: 1433
  - Connection string pattern
  - Database cleanup pattern
  - DacFx deployment examples

- [ ] `docs/architecture/testing/stacks/database/mongodb.md`
  - Image: `mongo:latest`
  - Port: 27017
  - Connection string pattern
  - Database/collection cleanup
  - CRUD operation examples

**Messaging:**
- [ ] `docs/architecture/testing/stacks/messaging/rabbitmq.md`
  - Image: `rabbitmq:latest`
  - Ports: 5672 (AMQP), 15672 (Management)
  - Queue/exchange management
  - Cleanup patterns
  - Message publishing/consuming examples

**Search:**
- [ ] `docs/architecture/testing/stacks/search/opensearch.md`
  - Image: `opensearchproject/opensearch:latest`
  - Ports: 9200 (HTTP), 9600 (Performance)
  - Index management
  - Cleanup patterns
  - Search/indexing examples

- [ ] `docs/architecture/testing/stacks/search/qdrant.md`
  - Image: `qdrant/qdrant`
  - Ports: 6333 (HTTP), 6334 (gRPC)
  - Collection management
  - Vector operations
  - Cleanup patterns

**Document Processing:**
- [ ] `docs/architecture/testing/stacks/document-processing/apache-tika.md`
  - Image: `apache/tika`
  - Port: 9998
  - Document parsing
  - Metadata extraction
  - Stateless service (no cleanup)

**Email:**
- [ ] `docs/architecture/testing/stacks/email/smtp.md`
  - Image: `rnwood/smtp4dev`
  - Ports: 25 (SMTP), 7777 (Web UI)
  - Email sending
  - Web UI verification
  - Stateless service

**Cloud Emulation:**
- [ ] `docs/architecture/testing/stacks/cloud-emulation/azurite.md`
  - Image: `mcr.microsoft.com/azure-storage/azurite`
  - Ports: 10000 (Blob), 10001 (Queue), 10002 (Table)
  - Blob storage operations
  - Queue operations
  - Connection string pattern

- [ ] `docs/architecture/testing/stacks/cloud-emulation/localstack.md`
  - Image: `localstack/localstack`
  - Port: 4566
  - AWS service emulation
  - S3, SQS, SNS examples
  - Configuration

**Identity:**
- [ ] `docs/architecture/testing/stacks/identity/keycloak.md`
  - Image: Custom (realm import)
  - Port: 8081
  - Realm configuration
  - User/client management
  - Authentication flows

**AI/ML:**
- [ ] `docs/architecture/testing/stacks/ai-ml/sbert.md`
  - Image: Custom (Python + transformers)
  - Port: 5080
  - Embedding generation
  - Model: all-MiniLM-L6-v2
  - CPU-only configuration

- [ ] `docs/architecture/testing/stacks/ai-ml/ollama.md`
  - Image: ollama/ollama:latest
  - Port: 11434
  - LLM inference (phi3 model)
  - CPU-only configuration
  - Automated model pulling
  - Stateless service (no cleanup)

#### Docker Infrastructure Documentation

- [ ] Create `docs/architecture/testing/docker-infrastructure.md`
  - Docker compose architecture
  - Service definitions using `extends` pattern
  - Container networking (integration-test-net)
  - Volume management (ephemeral strategy)
  - Health checks and startup sequences
  - Performance optimization
  - Troubleshooting common issues

#### PlantUML Diagrams (Docker-focused)

- [ ] Create `docs/architecture/testing/diagrams/docker-network-topology.puml`
  - Container networking diagram
  - Service dependencies
  - Port mappings
  - Volume mounts
  - Health check flow

- [ ] Create `docs/architecture/testing/diagrams/service-dependency-matrix.puml`
  - Which tests depend on which services
  - Service startup order
  - Health check dependencies

---

---

## Notes

**Services with no or few tests yet:** Service Bus emulator, Azurite beyond blobs, Moto beyond SQS (S3 needed).

**Not in Integration testing:** WkHtmlToPdf (in-process library), ParadeDB and Kafka (in compose files elsewhere, no tests).

**Long-term goals:** zero flaky tests (10 consecutive runs pass); Integration run under 5 minutes; all remaining `DevLocal` tests moved to Integration or LiveIntegration; quarterly review of reliability and coverage.
