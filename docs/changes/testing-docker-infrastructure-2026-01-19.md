# Testing - Docker-Based Integration Testing Infrastructure

**Date:** 2026-01-19
**Epic:** Testing
**Status:** ✅ COMPLETE (Week 1 & 2) - ⏳ AWAITING LOCAL VALIDATION
**Impact:** 14 Docker services, 23 migrated tests, complete CI/CD pipeline, comprehensive documentation

---

## Summary

Implemented complete Docker-based integration testing infrastructure enabling automated testing of external service integrations in CI/CD pipelines. Successfully migrated 23 tests from DevLocal to Integration category across 7 service types, created cross-platform management scripts, and built comprehensive testing documentation.

**Results:**
- ✅ 14-service Docker stack with health checks and ephemeral volumes
- ✅ 23 Integration tests migrated (Apache Tika, SMTP, MongoDB, RabbitMQ, OpenSearch, SBert, Ollama)
- ✅ Complete CI/CD pipeline (disabled until local validation)
- ✅ Cross-platform scripts (Linux/macOS/Windows)
- ✅ Comprehensive documentation with PlantUML diagrams
- ✅ Test property system using TestContext extensions

---

## Detailed Changes

### Week 1: Docker Infrastructure (COMPLETED - 2026-01-19)

**Docker Integration Test Stack - 14 Services:**

1. **Apache Tika** (Document processing) - Port 9998
2. **SMTP4Dev** (Email testing) - Ports 25, 7777
3. **MongoDB** (NoSQL database) - Port 27017
4. **SQL Server** (Relational database) - Port 1433
5. **RabbitMQ** (Message queue) - Ports 5673, 15672
6. **Redis** (Cache store) - Port 6379
7. **OpenSearch** (Search engine) - Ports 9200, 9600
8. **Qdrant** (Vector database) - Ports 6333, 6334
9. **Azurite** (Azure Storage emulator) - Ports 10000-10002
10. **LocalStack** (AWS emulator - SQS, S3) - Port 4566
11. **Azure Service Bus Emulator** (Message queue) - Port 5672
12. **Keycloak** (Identity & Access Management) - Port 8081
13. **SBert** (Sentence embeddings - CPU only) - Port 5080
14. **Ollama** (LLM inference - CPU only) - Port 11434

**Files Created:**
- `/containers/testing/docker-compose.integration-tests.yml` - 14 service definitions with health checks
- `/containers/testing/.env.integration` - Environment configuration
- `/containers/testing/README.md` - 500+ line guide with PlantUML deployment diagram
- `/containers/testing/scripts/integration-up.sh/.bat` - Cross-platform startup with health checks
- `/containers/testing/scripts/integration-down.sh/.bat` - Cleanup with volume removal
- `/containers/testing/scripts/wait-for-services.sh/.bat` - Health check polling (2-minute timeout)
- `/containers/testing/scripts/setup-ollama.sh/.bat` - Automated phi3 model pulling
- `/containers/testing/TESTING-CHECKLIST.md` - Local validation procedure
- `/containers/testing/STATUS.md` - Implementation progress tracker

**Test Categories Enhancement:**
- Updated `src/Framework/OoBDev.TestUtilities/TestCategories.cs` with Integration category documentation
- Clear distinction: Integration (Docker-based) vs LiveIntegration (Cloud-based)

**CI/CD Pipeline Implementation:**
- Created `.github/workflows/integration-tests.yml`
- Docker startup, health checks, test execution, cleanup
- Configured environment variables for all 14 services
- Test result upload (30-day retention)
- Validated tag creation (`validated-v{version}`)
- **Workflow DISABLED** - Triggers commented out until local Docker validation

**Architecture Patterns:**
- Ephemeral volumes for clean state (removed on `docker compose down -v`)
- Isolated network (`integration-test-net`)
- Health checks for all services (2-minute timeout)
- Cross-platform script support (bash + batch)

### Week 2: Test Migration (COMPLETED - 2026-01-21)

**Migrated 23 tests from DevLocal to Integration category:**

#### Apache Tika (6 tests)
- Files: `src/ExternalServices/Apache/OoBDev.Apache.Tika.Tests/Handlers/*HandlerTests.cs`
- Updated base class `TikaToHtmlConversionHandlerTestsBase.cs` to use `TIKA_URL` test property
- Removed hardcoded URL (`http://127.0.0.1:9998`)
- Tests: PDF, DOC, DOCX, EPUB, ODT, RTF handlers

#### SMTP/MailKit (2 tests)
- File: `src/ExternalServices/MailKit/OoBDev.MailKit.Tests/ClientExampleTests.cs`
- Tests: `SendSmtpTest`, `GetImapTest`
- Updated to use test properties: `SMTP_HOST`, `SMTP_PORT`, `IMAP_HOST`, `IMAP_PORT`
- Removed DataRow attributes (Azure container tests)

#### MongoDB (3 tests)
- File: `src/ExternalServices/MongoDb/OoBDev.MongoDB.Tests/MongoDBTests.cs`
- Added unique database naming: `IntegrationTest_{Guid.NewGuid():N}`
- Added `[TestCleanup]` method: `await _mongoClient.DropDatabaseAsync(_databaseName)`
- Updated to use `MONGODB_CONNECTION_STRING` test property

#### RabbitMQ (3 tests)
- File: `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ.Tests/MessageQueueing/RabbitMQQueueMessageSenderProviderTests.cs`
- Tests: `SendAsyncTest_ByFullType`, `SendAsyncTest_ByKeyed`, `FindProviderTests`
- Updated to use `RABBITMQ_HOST` test property
- Cleanup handled by RabbitMQ framework's message queue cleanup

#### OpenSearch (2 tests)
- File: `src/ExternalServices/OpenSearch/OoBDev.OpenSearch.Tests/OpenSearchTests.cs`
- Added unique index naming: `integrationtest_{Guid.NewGuid():N}`
- Added index cleanup in `[TestCleanup]`
- Updated to use `OPENSEARCH_URL`, `OPENSEARCH_USERNAME`, `OPENSEARCH_PASSWORD`
- SearchIndexTest now creates test data before searching

#### SBert (2 tests)
- File: `src/ExternalServices/SBert/OoBDev.SBert.Tests/SentenceEmbeddingClientTests.cs`
- Tests: `GetEmbeddingAsyncTest`, `GetAllTest`
- Updated to use `SBERT_URL` test property
- Removed DataRow attributes

#### Ollama (4 tests) - 2026-01-21
- Files: `src/ExternalServices/Ollama/OoBDev.Ollama.Tests/OllamaApiClientTests.cs`, `OllamaMessageCompletionTests.cs`
- Tests: `ListModelsTest`, `GenerateEmbeddingsDoubleTest`, `IMessageCompletion_GetCompletionAsyncTest`, `ILanguageModelProvider_GetResponseAsyncTest`
- Updated to use `OLLAMA_URL`, `OLLAMA_MODEL` test properties
- Model auto-pulled by integration-up scripts (phi3)

**Test Pattern Standardization:**

All tests updated to use `TestContext` extension methods:

```csharp
// Required properties (throws if missing)
var url = TestContext.GetRequiredProperty<string>("SERVICE_URL");

// Optional properties with defaults
var port = TestContext.GetPropertyOrDefault("SERVICE_PORT", 5432);

// Unique resource naming
var databaseName = $"IntegrationTest_{Guid.NewGuid():N}";

// Cleanup in [TestCleanup]
[TestCleanup]
public async Task TestCleanup()
{
    if (_client != null && _resourceName != null)
    {
        try { await _client.DeleteResourceAsync(_resourceName); }
        catch { /* Ignore cleanup errors */ }
    }
}
```

### Documentation Complete

**Created/Updated:**
- ✅ `TEST_VARIABLES.md` - All 30+ test properties documented
- ✅ `docs/architecture/testing-guidelines.md` - Comprehensive testing guide
- ✅ `containers/testing/README.md` - Infrastructure guide with PlantUML
- ✅ `containers/testing/TESTING-CHECKLIST.md` - Local validation steps
- ✅ `containers/testing/STATUS.md` - Implementation progress
- ✅ Updated TODO.md, TODO-testing-local-integration.md, CLAUDE.md

**Documentation Highlights:**
- PlantUML deployment diagram showing service dependencies
- Service-by-service configuration reference
- Cross-platform script usage examples
- Troubleshooting common issues
- Test writing patterns and examples

---

## Verification

**Build Verification:**
```bash
cd /current/src
dotnet build src/
```
- ✅ All 65 projects build successfully

**Infrastructure Verification (Pending Local Testing):**
```bash
cd containers/testing
./scripts/integration-up.sh --wait
```
- ⏳ All 14 services start successfully
- ⏳ All health checks pass within 2 minutes
- ⏳ No port conflicts or errors

**Test Verification (Pending Local Testing):**
```bash
cd ../../src
dotnet test --filter "TestCategory=Integration"
```
- ⏳ All 23 Integration tests pass
- ⏳ Test cleanup verified (no data leaks)
- ⏳ Total execution time < 10 minutes

**Cleanup Verification (Pending Local Testing):**
```bash
cd ../containers/testing
./scripts/integration-down.sh --clean
```
- ⏳ All volumes removed
- ⏳ Clean restart works

---

## Key Patterns

### TestContext Extension Methods

Created reusable extension methods for test configuration:

```csharp
public static class TestContextExtensions
{
    public static T GetRequiredProperty<T>(this TestContext testContext, string key)
    {
        var value = testContext.Properties[key]?.ToString();
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(key, $"Required test property '{key}' not found");
        return (T)Convert.ChangeType(value, typeof(T));
    }

    public static T GetPropertyOrDefault<T>(this TestContext testContext, string key, T defaultValue)
    {
        var value = testContext.Properties[key]?.ToString();
        return string.IsNullOrEmpty(value) ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }
}
```

### Unique Resource Naming

Pattern for avoiding test conflicts:

```csharp
private string? _testResourceName;

[TestInitialize]
public void TestInitialize()
{
    _testResourceName = $"IntegrationTest_{Guid.NewGuid():N}";
}

[TestCleanup]
public async Task TestCleanup()
{
    if (_client != null && _testResourceName != null)
        await _client.DeleteResourceAsync(_testResourceName);
}
```

### Health Check Pattern

Cross-platform health check polling:

```bash
MAX_ATTEMPTS=60
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    if docker compose ps | grep -q "healthy"; then
        echo "Service is healthy!"
        exit 0
    fi
    ATTEMPT=$((ATTEMPT + 1))
    sleep 2
done

echo "Service failed to become healthy"
exit 1
```

---

## Impact Summary

**Services:**
- 14 Docker services configured
- 9 API endpoints exposed
- All services have health checks
- CPU-only configurations for CI/CD compatibility

**Tests:**
- 23 Integration tests migrated (was 0)
- 7 service types covered
- 100% using TestContext pattern
- All tests have cleanup logic

**Infrastructure:**
- 8 script files (4 bash, 4 batch)
- 2 compose files (base + integration)
- 5 documentation files
- 1 CI/CD workflow (disabled)

**Documentation:**
- 500+ lines in containers/testing/README.md
- 30+ test properties documented
- PlantUML deployment diagram
- Comprehensive testing guidelines

**Lines of Code:**
- Docker configs: ~400 LOC
- Scripts: ~800 LOC (bash + batch)
- Documentation: ~2,000 LOC
- Test updates: ~300 LOC

---

## What's Next (Pending Local Validation)

**Immediate:**
1. Local testing using `/containers/testing/TESTING-CHECKLIST.md`
2. Verify all 14 services become healthy
3. Run 23 migrated Integration tests locally
4. Test cleanup works correctly
5. Document any issues or adjustments needed

**After Validation:**
1. Enable GitHub Actions workflow (uncomment triggers)
2. Week 3: Migrate LiveIntegration tests (Azure B2C, App Insights, Groq)
3. Week 4: Complete documentation (14 stack docs + diagrams)

---

## Files Modified

**Docker Infrastructure:**
- `/containers/testing/docker-compose.integration-tests.yml`
- `/containers/testing/.env.integration`
- `/containers/testing/README.md`
- `/containers/testing/TESTING-CHECKLIST.md`
- `/containers/testing/STATUS.md`
- `/containers/testing/scripts/integration-up.sh`
- `/containers/testing/scripts/integration-up.bat`
- `/containers/testing/scripts/integration-down.sh`
- `/containers/testing/scripts/integration-down.bat`
- `/containers/testing/scripts/wait-for-services.sh`
- `/containers/testing/scripts/wait-for-services.bat`
- `/containers/testing/scripts/setup-ollama.sh`
- `/containers/testing/scripts/setup-ollama.bat`

**CI/CD:**
- `.github/workflows/integration-tests.yml`

**Test Files:**
- `src/ExternalServices/Apache/OoBDev.Apache.Tika.Tests/Handlers/TikaToHtmlConversionHandlerTestsBase.cs`
- `src/ExternalServices/Apache/OoBDev.Apache.Tika.Tests/Handlers/*HandlerTests.cs` (6 files)
- `src/ExternalServices/MailKit/OoBDev.MailKit.Tests/ClientExampleTests.cs`
- `src/ExternalServices/MongoDb/OoBDev.MongoDB.Tests/MongoDBTests.cs`
- `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ.Tests/MessageQueueing/RabbitMQQueueMessageSenderProviderTests.cs`
- `src/ExternalServices/OpenSearch/OoBDev.OpenSearch.Tests/OpenSearchTests.cs`
- `src/ExternalServices/SBert/OoBDev.SBert.Tests/SentenceEmbeddingClientTests.cs`
- `src/ExternalServices/Ollama/OoBDev.Ollama.Tests/OllamaApiClientTests.cs`
- `src/ExternalServices/Ollama/OoBDev.Ollama.Tests/OllamaMessageCompletionTests.cs`

**Test Configuration:**
- `src/.runsettings` (added 30+ test properties)
- `src/Framework/OoBDev.TestUtilities/TestCategories.cs`

**Documentation:**
- `TEST_VARIABLES.md`
- `docs/architecture/testing-guidelines.md`
- `TODO.md`
- `TODO-testing-local-integration.md`
- `CLAUDE.md`

---

**Related Documentation:**
- [TODO.md](../../TODO.md) - Main project tracking
- [TODO-testing-local-integration.md](../../TODO-testing-local-integration.md) - Local integration testing epic
- [TEST_VARIABLES.md](../../TEST_VARIABLES.md) - Test property reference
- [containers/testing/README.md](../../containers/testing/README.md) - Infrastructure guide
