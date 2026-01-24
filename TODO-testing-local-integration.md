# TODO - Local Integration Testing (Docker) Epic

**Last Updated:** 2026-01-21

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

### Week 1: Infrastructure Setup (COMPLETED - 2026-01-19)

**Docker Integration Test Stack** - 14 services for Integration test category

**Files Created:**
- [x] `/containers/testing/docker-compose.integration-tests.yml` (14 services - see list below)
- [x] `/containers/testing/.env.integration` - Environment configuration
- [x] `/containers/testing/README.md` - 500+ line guide with PlantUML deployment diagram
- [x] Cross-platform scripts: `integration-up.sh/.bat`, `integration-down.sh/.bat`, `wait-for-services.sh/.bat`
- [x] `/containers/testing/scripts/setup-ollama.sh/.bat` - Automated model pulling (phi3)
- [x] `/containers/testing/TESTING-CHECKLIST.md` - Local validation guide
- [x] `/containers/testing/STATUS.md` - Implementation tracker

**Test Categories Enhancement:**
- [x] Updated `TestCategories.cs` with clear Integration category documentation
- [x] Clear distinction: Integration (Docker-based) vs LiveIntegration (Cloud-based)

**CI/CD Pipeline Implementation:**
- [x] Completed `.github/workflows/integration-tests.yml` (Docker startup, health checks, tests, cleanup)
- [x] Configured all environment variables for 14 services
- [x] Test result upload (30-day retention)
- [x] Validated tag creation (`validated-v{version}`)
- [x] **Workflow DISABLED** - Triggers commented out until local Docker testing validates infrastructure

**Ollama Integration Automation (2026-01-21):**
- [x] Automated phi3 model pulling in integration-up scripts
- [x] Model setup runs automatically after all services are healthy
- [x] Fixed Windows batch file container detection regex
- [x] 4 tests migrated to Integration category

**14 Docker Services:**
1. **Apache Tika** (Document processing) - Port 9998
2. **SMTP4Dev** (Email testing) - Ports 25, 7777
3. **MongoDB** (NoSQL database) - Port 27017
4. **SQL Server** (Relational database) - Port 1433
5. **RabbitMQ** (Message queue) - Ports 5673, 15672
6. **Redis** (Cache store) - Port 6379
7. **OpenSearch** (Search engine) - Ports 9200, 9600
8. **Qdrant** (Vector database) - Ports 6333, 6334
9. **Azurite** (Azure Storage emulator) - Ports 10000-10002
10. **LocalStack** (AWS emulator - SQS, S3, etc.) - Port 4566
11. **Azure Service Bus Emulator** (Message queue) - Port 5672
12. **Keycloak** (Identity & Access Management) - Port 8081
13. **SBert** (Sentence embeddings - CPU only) - Port 5080
14. **Ollama** (LLM inference - CPU only) - Port 11434

---

## Completed Work ✓ (continued)

### Local Testing Validation (COMPLETED - 2026-01-21)

**✅ All Integration tests validated and passing**

**Prerequisites:**
- [x] Docker Desktop/Engine installed and running
- [x] Required ports available (1433, 5672, 6333, 8081, 9200, 9998, 10000-10002, 27017)
- [x] At least 10GB disk space available

**Validation Steps:**
- [x] Follow `/containers/testing/TESTING-CHECKLIST.md` step-by-step
- [x] Start services: `cd containers/testing && ./scripts/integration-up.sh --wait`
- [x] Verify all 14 services become healthy within 2 minutes
- [x] Test individual service health (curl commands in checklist)
- [x] Run all Integration tests - **ALL PASSING**
- [x] Test cleanup: `./scripts/integration-down.sh --clean`
- [x] Verify clean restart works correctly

**Success Criteria:**
- [x] All 14 containers start successfully
- [x] All health checks pass within 2 minutes
- [x] All Integration tests passing
- [x] No port conflicts or errors
- [x] Cleanup removes all volumes
- [x] Restart from clean state works

**Result:** ✅ **VALIDATED** - Ready for CI/CD enablement

---

## Pending Work

### Week 2: Test Migration (COMPLETED - 2026-01-21)

**✅ Migrated 23 tests from DevLocal to Integration category**

#### Priority 1: Stateless Services ✅ COMPLETE

**Apache Tika (6 tests)** - ✅ COMPLETED
- [x] File: `src/ExternalServices/Apache/OoBDev.Apache.Tika.Tests/Handlers/*HandlerTests.cs`
- [x] Changed `[TestCategory(TestCategories.DevLocal)]` → `[TestCategory(TestCategories.Integration)]`
- [x] Updated base test class to use `TIKA_URL` test property
  ```csharp
  var tikaUrl = TestContext.GetRequiredProperty<string>("TIKA_URL");
  ```
- [x] Removed hardcoded URL (`http://127.0.0.1:9998`) from `TikaToHtmlConversionHandlerTestsBase.cs`
- [x] All 6 handler tests (PDF, DOC, DOCX, EPUB, ODT, RTF) migrated

**SMTP/MailKit (2 tests)** - ✅ COMPLETED
- [x] File: `src/ExternalServices/MailKit/OoBDev.MailKit.Tests/ClientExampleTests.cs`
- [x] Changed category to Integration for both `SendSmtpTest` and `GetImapTest`
- [x] Updated to use test properties:
  ```csharp
  var smtpHost = TestContext.GetRequiredProperty<string>("SMTP_HOST");
  var smtpPort = TestContext.GetPropertyOrDefault("SMTP_PORT", 25);
  var imapHost = TestContext.GetRequiredProperty<string>("IMAP_HOST");
  var imapPort = TestContext.GetPropertyOrDefault("IMAP_PORT", 143);
  ```
- [x] Removed DataRow attributes (Azure container tests moved to local Docker focus)

#### Priority 2: Stateful Services ✅ COMPLETE

**MongoDB (3 tests)** - ✅ COMPLETED
- [x] File: `src/ExternalServices/MongoDb/OoBDev.MongoDB.Tests/MongoDBTests.cs`
- [x] Changed category to Integration for all 3 test methods
- [x] Added unique database name pattern:
  ```csharp
  private string? _databaseName;
  [TestInitialize]
  public void TestInitialize() { _databaseName = $"IntegrationTest_{Guid.NewGuid():N}"; }
  ```
- [x] Added `[TestCleanup]` method:
  ```csharp
  [TestCleanup]
  public async Task TestCleanup()
  {
      if (_mongoClient != null && _databaseName != null)
          await _mongoClient.DropDatabaseAsync(_databaseName);
  }
  ```
- [x] Updated connection string to use test property in all 3 tests:
  ```csharp
  var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");
  ```

**SQL Server DacFx** - ⏭️ SKIPPED (No integration tests to migrate)
- File: `src/ExternalServices/Microsoft/OoBDev.Microsoft.SqlServer.DacFx.Tests/Class1.cs`
- Contains only unit test that builds DacPac in memory (no external database required)
- No DevLocal tests found that need migration to Integration category

**RabbitMQ (3 tests)** - ✅ COMPLETED
- [x] File: `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ.Tests/MessageQueueing/RabbitMQQueueMessageSenderProviderTests.cs`
- [x] Changed category to Integration for all 3 test methods
- [x] Updated connection to use test property in all 3 tests:
  ```csharp
  var rabbitMQHost = TestContext.GetRequiredProperty<string>("RABBITMQ_HOST");
  ```
- [x] Tests: `SendAsyncTest_ByFullType`, `SendAsyncTest_ByKeyed`, `FindProviderTests`
- Note: Cleanup handled by RabbitMQ framework's message queue cleanup

#### Priority 3: Search Services ✅ COMPLETE

**OpenSearch (2 tests)** - ✅ COMPLETED
- [x] File: `src/ExternalServices/OpenSearch/OoBDev.OpenSearch.Tests/OpenSearchTests.cs`
- [x] Changed category to Integration for both tests
- [x] Added index cleanup logic in `[TestCleanup]`:
  ```csharp
  [TestCleanup]
  public async Task TestCleanup()
  {
      if (_client != null && _testIndexName != null)
      {
          try { await _client.Indices.DeleteAsync<StringResponse>(_testIndexName); }
          catch { /* Ignore cleanup errors */ }
      }
  }
  ```
- [x] Updated connection to use test properties:
  ```csharp
  var url = TestContext.GetRequiredProperty<string>("OPENSEARCH_URL");
  var username = TestContext.GetRequiredProperty<string>("OPENSEARCH_USERNAME");
  var password = TestContext.GetRequiredProperty<string>("OPENSEARCH_PASSWORD");
  ```
- [x] Added unique index names: `integrationtest_{Guid.NewGuid():N}`
- [x] SearchIndexTest now creates test data before searching

**SBert (2 tests)** - ✅ COMPLETED
- [x] File: `src/ExternalServices/SBert/OoBDev.SBert.Tests/SentenceEmbeddingClientTests.cs`
- [x] Changed category to Integration for both tests
- [x] Updated to use test property:
  ```csharp
  var url = TestContext.GetRequiredProperty<string>("SBERT_URL");
  ```
- [x] Removed DataRow attributes (hardcoded URLs replaced with env vars)
- [x] Tests: `GetEmbeddingAsyncTest`, `GetAllTest`
- [x] No cleanup needed (stateless service)

**Ollama (4 tests)** - ✅ COMPLETED (2026-01-21)
- [x] Files: `src/ExternalServices/Ollama/OoBDev.Ollama.Tests/OllamaApiClientTests.cs`, `OllamaMessageCompletionTests.cs`
- [x] Changed category to Integration for 4 tests (was DevLocal)
- [x] Updated to use test properties:
  ```csharp
  var url = TestContext.GetRequiredProperty<string>("OLLAMA_URL");
  var model = TestContext.GetPropertyOrDefault("OLLAMA_MODEL", "phi3");
  ```
- [x] Tests migrated:
  - `OllamaApiClientTests.ListModelsTest`
  - `OllamaApiClientTests.GenerateEmbeddingsDoubleTest`
  - `OllamaMessageCompletionTests.IMessageCompletion_GetCompletionAsyncTest`
  - `OllamaMessageCompletionTests.ILanguageModelProvider_GetResponseAsyncTest`
- [x] Model auto-pulled by integration-up scripts (phi3)
- [x] No cleanup needed (stateless service)

#### Priority 4: Commented Tests ⏭️ DEFERRED

**Qdrant (commented tests)** - ⏭️ DEFERRED (All tests commented out)
- File: `src/ExternalServices/Qdrant/OoBDev.Qdrant.Tests/QdrantGrpcClientTests.cs`
- Entire file is commented out (lines 1-287)
- Tests depend on Ollama and SBert services (complex setup required)
- Categories used: "setup" and "dev-local" (not DevLocal standard category)
- **Decision:** Leave commented until tests are uncommented and requirements clarified

**Final Migration Count:** 23 tests migrated successfully
- ✅ Apache Tika: 6 tests
- ✅ SMTP/MailKit: 2 tests
- ✅ MongoDB: 3 tests
- ✅ RabbitMQ: 3 tests
- ✅ OpenSearch: 2 tests
- ✅ SBert: 2 tests
- ✅ Ollama: 4 tests
- ⏭️ SQL Server DacFx: 0 tests (none applicable)
- ⏭️ Qdrant: 0 tests (all commented out)

---

## Pending Work

### ⏳ Week 3: CI/CD Pipeline Enablement (NEXT)

**Goal:** Enable automated Integration tests in CI/CD pipeline

**Prerequisites:**
- [x] Local validation complete (ALL TESTS PASSING - 2026-01-21)
- [x] Docker infrastructure stable
- [x] Test cleanup verified

**CI/CD Configuration:**
- [ ] **Review** `.github/workflows/integration-tests.yml` workflow file
- [ ] **Enable** workflow triggers (currently commented out):
  ```yaml
  # Currently disabled:
  # on:
  #   schedule:
  #     - cron: '0 16 * * *'  # Daily at 4:00 PM UTC
  #   workflow_dispatch:      # Manual trigger

  # After validation, uncomment to enable
  ```
- [ ] **Verify** GitHub Actions runner has Docker support
- [ ] **Test** manual workflow trigger (`workflow_dispatch`)
- [ ] **Monitor** first automated daily run
- [ ] **Verify** validated tag creation (`validated-v{version}`)
- [ ] **Document** any CI/CD-specific issues or adjustments needed

**Success Criteria:**
- [ ] Workflow triggers successfully
- [ ] All 14 Docker services start in CI/CD
- [ ] All Integration tests pass in CI/CD
- [ ] Test results uploaded correctly
- [ ] Validated tag created on success
- [ ] No timeout issues (30-minute limit sufficient)

**Rollback Plan:**
- If CI/CD tests fail, re-disable triggers and investigate
- Local Docker testing remains available for development

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

### Future Service Additions

#### Azurinsight - Application Insights Emulator (PENDING - External Repository)

**Goal:** Add 15th Docker service to enable local Application Insights testing

**Background:**
- Application Insights tests currently in LiveIntegration category (requires cloud credentials)
- [azurinsight](https://github.com/Rahulkumar010/azurinsight) is a lightweight local emulator for Azure Application Insights
- Container being prepared in separate repository

**Integration Tasks (BLOCKED - Awaiting container availability):**

- [ ] **Prerequisites:**
  - [ ] azurinsight Docker image available (separate repository work)
  - [ ] Image published or build instructions available

- [ ] **Docker Infrastructure:**
  - [ ] Add azurinsight service to `docker-compose.integration-tests.yml`
  - [ ] Configure port 5000 (default Application Insights emulator port)
  - [ ] Add health check endpoint
  - [ ] Update `.env.integration` with configuration variables:
    ```bash
    APPINSIGHTS_CONNECTION_STRING=InstrumentationKey=test;IngestionEndpoint=http://localhost:5000
    APPINSIGHTS_URL=http://localhost:5000
    ```
  - [ ] Test service startup with other 14 services

- [ ] **Test Migration:**
  - [ ] Migrate Application Insights tests from LiveIntegration to Integration
  - [ ] Update tests to use `APPINSIGHTS_CONNECTION_STRING` test property
  - [ ] Add cleanup logic in `[TestCleanup]` if needed
  - [ ] Verify tests pass with local emulator

- [ ] **Documentation:**
  - [ ] Update `containers/testing/README.md` with 15th service
  - [ ] Create `docs/architecture/testing/stacks/monitoring/azurinsight.md`
  - [ ] Update TEST_VARIABLES.md with Application Insights properties
  - [ ] Update PlantUML diagrams to include azurinsight

- [ ] **Configuration:**
  - [ ] Update `.github/workflows/integration-tests.yml` environment variables
  - [ ] Update `.runsettings` files with Application Insights settings
  - [ ] Update nginx dashboard configuration

**Protocol Reference:**
- Follow `.claude/protocols/testing/integration-test-maintenance.md` for adding new container

**Benefits:**
- Eliminate need for live Azure Application Insights credentials
- Move tests from manual LiveIntegration to automated Integration category
- Reduce cloud costs during testing
- Enable deterministic telemetry validation
- Complete offline testing capability

**Dependencies:**
- External: azurinsight container from separate repository
- Internal: Current 14-service stack stable and validated

**Estimated Timeline:**
- Container availability: TBD (external dependency)
- Integration work: 1-2 hours after container available
- Test migration: 2-4 hours
- Documentation: 1-2 hours

---

## Enable CI/CD After Validation

**After successful local testing:**

- [ ] Edit `.github/workflows/integration-tests.yml`
- [ ] Uncomment the `schedule` trigger:
  ```yaml
  on:
    schedule:
      - cron: '0 16 * * *'  # Daily at 4 PM UTC
    workflow_dispatch:  # Manual trigger
  ```
- [ ] Remove the temporary `workflow_call` trigger
- [ ] Commit and push changes:
  ```bash
  git add .github/workflows/integration-tests.yml
  git commit -m "Enable integration tests workflow after local validation

  All 11 Docker services validated locally:
  - Health checks pass within 2 minutes
  - Cleanup works correctly
  - Ready for daily CI/CD execution

  Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"
  git push
  ```
- [ ] Manually trigger workflow to verify CI/CD execution:
  ```bash
  gh workflow run integration-tests.yml
  ```
- [ ] Monitor first run and verify:
  - [ ] Docker services start correctly in GitHub Actions
  - [ ] All health checks pass
  - [ ] Integration tests run (once migrated)
  - [ ] Cleanup runs (even on failure)
  - [ ] Validated tag creation on success: `validated-v{version}`

---

## Architecture Highlights

**Shared Infrastructure:**
- Same Docker containers for local development (`docker-compose-cpu.yml`) and CI/CD testing (`testing/docker-compose.integration-tests.yml`)
- Different orchestration files but same base service definitions (using `extends`)
- Environment variable overrides for different contexts

**Health Checks:**
- All 11 services have health check definitions in compose file
- Wait script polls for healthy status (120-second timeout, configurable)
- Tests don't run until all services report healthy
- Fast fail if any service doesn't become healthy

**Clean State Management:**
- Ephemeral volumes destroyed with `docker compose down -v`
- Unique resource names per test run: `IntegrationTest_{Guid.NewGuid():N}`
- Cleanup in `[TestCleanup]` attribute methods (always runs, even on test failure)
- Fresh database/queue/index for each test

**Isolated Network:**
- Dedicated `integration-test-net` bridge network
- No conflicts with development containers
- Container name prefix: `oobd-test-*`

**CI/CD Pipeline Flow:**
```
Build Pipeline (dotnet.yml)
  ├─ Push/PR trigger
  ├─ Build + Unit/Simulate tests
  ├─ Create packages
  ├─ Upload artifacts (90 days)
  └─ Tag: v{version}
          ↓
Daily at 4 PM UTC (after build completes)
          ↓
Integration Tests (integration-tests.yml)
  ├─ Download latest build artifacts
  ├─ Start Docker services (11 containers)
  ├─ Wait for health checks (max 5 minutes)
  ├─ Run Integration tests (filter: TestCategory=Integration)
  ├─ Upload test results (30 days)
  ├─ Stop Docker services (always runs)
  └─ Tag: validated-v{version} (on success)
          ↓
Manual Release (release.yml)
  ├─ Find validated artifact
  └─ Deploy to NuGet
```

---

## Success Criteria

### Week 1: Infrastructure ✅
- ✅ Docker integration stack starts/stops successfully
- ⏳ All 14 services become healthy within 2 minutes (awaiting local test)
- ⏳ Manual workflow trigger works (awaiting local test)
- ✅ Daily schedule configured correctly (disabled until local test)
- ✅ Health check script works correctly
- ✅ Cleanup script removes all volumes
- ✅ Ollama automated model setup (phi3)

### Week 2: Test Migration ✅
- ✅ 23 tests migrated from DevLocal to Integration (Apache Tika, SMTP, MongoDB, RabbitMQ, OpenSearch, SBert, Ollama)
- ⏳ All Integration tests pass locally with Docker stack running (awaiting local test)
- ⏳ All Integration tests pass in CI/CD pipeline (awaiting workflow enable)
- ⏳ Test cleanup verified (no data leaks between runs) (awaiting local test)
- ⏳ Total execution time < 10 minutes (awaiting local test)
- ⏳ Zero flaky tests (10 consecutive runs pass) (awaiting local test)

### Week 4 (Part 1): Documentation
- [ ] Integration category fully documented
- [ ] All 14 Docker stacks documented
- [ ] Code examples for each stack
- [ ] PlantUML diagrams embedded
- [ ] Templates available

---

## Risk Mitigation

### Docker Service Startup Time
**Risk:** Services take too long to start in CI/CD (>5 minutes)

**Mitigation:**
- Parallel startup (Docker Compose default behavior)
- Fast health check intervals (10 seconds)
- Timeout protection (5 minute maximum for health checks)
- Docker layer caching in GitHub Actions
- Pre-pull images in CI/CD setup step

### Test Isolation
**Risk:** Tests contaminate shared resources (databases, queues, indices)

**Mitigation:**
- Unique resource names per test: `IntegrationTest_{Guid.NewGuid():N}`
- Cleanup in `[TestCleanup]` (always runs, even on test failure)
- Ephemeral volumes (`docker compose down -v`)
- Retry logic with exponential backoff for transient failures

### CI/CD Runner Constraints
**Risk:** GitHub Actions runners have limited CPU/memory/disk

**Mitigation:**
- CPU-only stack (no GPU services like CUDA-enabled Ollama)
- Maximum 11 containers (well within GitHub limits)
- Health check timeouts (fail fast if service won't start)
- Service subset optimization (exclude heavy services)
- Monitor resource usage in Actions logs

### Flaky Tests
**Risk:** Tests fail intermittently due to timing issues

**Mitigation:**
- Explicit health checks before ANY tests run
- Connection retry logic in tests (3-5 retries with backoff)
- wait-for-services.sh ensures readiness
- Clear error messages when service unavailable
- No hardcoded sleep/delays (use health checks instead)

### Port Conflicts
**Risk:** Local or CI/CD ports already in use

**Mitigation:**
- Document required ports in TESTING-CHECKLIST.md
- Pre-flight check script to verify port availability
- Clear error messages on port conflicts
- Option to customize ports via .env.integration

---

## Environment Variables

**Pattern:** `{STACK}_{PROPERTY}`

**Complete List:**
```bash
# SQL Server
SQL_CONNECTION_STRING=Server=localhost,1433;User Id=sa;Password=IntegrationTest123!;TrustServerCertificate=True
SQL_SA_PASSWORD=IntegrationTest123!

# MongoDB
MONGODB_CONNECTION_STRING=mongodb://localhost:27017

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_PORT=5673
RABBITMQ_CONNECTION_STRING=amqp://guest:guest@localhost:5673/

# OpenSearch
OPENSEARCH_URL=https://localhost:9200
OPENSEARCH_USERNAME=admin
OPENSEARCH_PASSWORD=IntegrationTest123!

# Qdrant
QDRANT_URL=http://localhost:6333
QDRANT_GRPC_URL=http://localhost:6334

# Apache Tika
TIKA_URL=http://localhost:9998

# SMTP
SMTP_HOST=localhost
SMTP_PORT=25

# Azurite (Azure Storage Emulator)
AZURITE_BLOB_URL=http://localhost:10000
AZURITE_QUEUE_URL=http://localhost:10001
AZURITE_TABLE_URL=http://localhost:10002
AZURITE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;QueueEndpoint=http://localhost:10001/devstoreaccount1;TableEndpoint=http://localhost:10002/devstoreaccount1

# LocalStack (AWS Emulator)
LOCALSTACK_URL=http://localhost:4566
AWS_ACCESS_KEY_ID=test
AWS_SECRET_ACCESS_KEY=test
AWS_DEFAULT_REGION=us-east-1

# Keycloak
KEYCLOAK_URL=http://localhost:8081
KEYCLOAK_REALM=local-dev
KEYCLOAK_CLIENT_ID=test-client

# SBert
SBERT_URL=http://localhost:5080

# Ollama
OLLAMA_URL=http://localhost:11434
OLLAMA_HOST=localhost
OLLAMA_PORT=11434
OLLAMA_MODEL=phi3

# Redis
REDIS_CONNECTION_STRING=localhost:6379

# Azure Service Bus Emulator
AZURE_SERVICE_BUS_CONNECTION_STRING=Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true
```

---

## Documentation References

- `/containers/testing/README.md` - Complete infrastructure guide with PlantUML
- `/containers/testing/TESTING-CHECKLIST.md` - Local validation procedure
- `/containers/testing/STATUS.md` - Implementation progress tracker
- `.github/workflows/integration-tests.yml` - CI/CD pipeline definition
- `src/Framework/OoBDev.TestUtilities/TestCategories.cs` - Integration category definition

---

## Notes

**Services NOT included in Integration testing:**
- **WkHtmlToPdf** - In-process library, no Docker container needed
- **ParadeDB/Kafka** - Already in compose files but no tests exist yet

**Services with no tests yet:**
- **Redis** - Future Integration tests for caching features
- **Azure Service Bus Emulator** - Future Integration tests for Service Bus features
- **Keycloak** - Future Integration tests when identity features implemented
- **Azurite** - Future Integration tests for Azure Storage features
- **LocalStack** - Future Integration tests for AWS features (SQS tests exist, S3 needed)

**Long-Term Goals:**
- 🎯 80% code coverage for Integration tests
- 🎯 Zero flaky tests (10 consecutive runs pass)
- 🎯 Test execution time < 5 minutes (optimized from current ~10 minutes)
- 🎯 All DevLocal tests migrated to Integration or LiveIntegration
- 🎯 Quarterly review of test reliability and coverage
