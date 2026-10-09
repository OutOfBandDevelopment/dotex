# Testing - Local Integration Testing (completed work)

**Date:** 2026-10-09
**Epic:** Testing
**Status:** ✅ COMPLETE
**Impact:** Archived from `TODO-testing-local-integration.md`; records the Docker stack build-out and test migration

---

## Summary

The completed sections of the local integration testing TODO, moved here verbatim to keep the active TODO short. Service names and ports in the text are as of 2026-01-24. Since then Moto replaced LocalStack, Grafana LGTM (`otel-lgtm`) replaced Azurinsight, and the test Ollama is published on host port 11435. See [the OpenTelemetry change](migration-opentelemetry-2026-10-09.md) and [the Moto change](testing-vectors-sqs-moto-ci-2026-10-08.md).

---

## Archived Sections

### Completed Work

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

### Completed Work (continued)

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

### Week 2 and later

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

### Script Enhancements & Health Check Fixes (2026-01-24)

**Goal:** Improve integration-up scripts and fix Docker health checks for all 15 services

**Completed:**
- [x] **Script Enhancements:**
  - [x] Added `--build` flag support to `integration-up.sh/.bat` for rebuilding images
  - [x] Fixed Windows batch file path resolution (using `PUSHD` instead of character counting)
  - [x] Updated all 15 services in startup script output display
  - [x] Updated port lists in script headers (all 17 required ports)
  - [x] Added all 4 missing services to `wait-for-services.sh/.bat` (Redis, Service Bus, Ollama, Azurinsight)

- [x] **Health Check Fixes:**
  - [x] Updated all health checks to use bash TCP (`</dev/tcp/HOST/PORT`) instead of curl/wget/nc
  - [x] Apache Tika: `timeout 2 bash -c '</dev/tcp/localhost/9998'` ✅ healthy
  - [x] Qdrant: bash TCP check ✅ healthy
  - [x] Azurite: Node.js socket check ✅ healthy
  - [x] Keycloak: bash TCP check ✅ healthy
  - [x] SBert: bash TCP check ✅ healthy
  - [x] Ollama: bash TCP check ✅ healthy
  - [x] Service Bus: bash TCP check (⏳ starting - 30s period)
  - [x] Azurinsight: bash TCP check (⚠️ unhealthy - needs investigation)

- [x] **Protocol Updates:**
  - [x] Updated `.claude/protocols/software/integration-test-maintenance.md` to v1.1.0
  - [x] Added checklist items for startup scripts (integration-up.sh/.bat)
  - [x] Added checklist items for health check scripts (wait-for-services.sh/.bat)
  - [x] Added checklist items for .env.integration and CI/CD workflow
  - [x] Expanded file reference quick links

**Status:** 13/15 services healthy, 2 remaining (servicebus starting, azurinsight needs fix)

**Next Steps:**
- [ ] Investigate azurinsight health check failure
- [ ] Verify servicebus completes startup successfully
- [ ] Complete final validation with all 15 services healthy

---

### Week 2 and later
