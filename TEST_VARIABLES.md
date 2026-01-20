# Integration Test Variables

**Last Updated:** 2026-01-20

This document lists all test properties used by Integration tests. These properties can be configured via:
- `.runsettings` file (`TestRunParameters` section)
- Test deployment context
- Environment variables (fallback)

> **Usage in Tests:**
> - Required values: `TestContext.GetRequiredProperty<string>("SERVICE_URL")`
> - Values with defaults: `TestContext.GetPropertyOrDefault("SERVICE_PORT", 8080)`
>
> See [Testing Guidelines](docs/architecture/testing/testing-guidelines.md) for complete patterns.

---

## Table of Contents

1. [Test Categories](#test-categories)
2. [Integration Test Variables](#integration-test-variables)
   - [Apache Tika (Document Processing)](#apache-tika-document-processing)
   - [SMTP/IMAP (Email Testing)](#smtpimap-email-testing)
   - [MongoDB (NoSQL Database)](#mongodb-nosql-database)
   - [SQL Server (Relational Database)](#sql-server-relational-database)
   - [RabbitMQ (Message Queue)](#rabbitmq-message-queue)
   - [Redis (Cache Store)](#redis-cache-store)
   - [OpenSearch (Search Engine)](#opensearch-search-engine)
   - [SBert (Sentence Embeddings)](#sbert-sentence-embeddings---aiml)
   - [Qdrant (Vector Database)](#qdrant-vector-database)
   - [Azurite (Azure Storage Emulator)](#azurite-azure-storage-emulator)
   - [LocalStack (AWS Emulator)](#localstack-aws-emulator)
   - [Keycloak (Identity & Access Management)](#keycloak-identity--access-management)
3. [LiveIntegration Test Variables](#liveintegration-test-variables)
   - [Azure B2C (Identity Provider)](#azure-b2c-identity-provider)
   - [Application Insights (Telemetry)](#application-insights-telemetry)
   - [Groq Cloud (LLM API)](#groq-cloud-llm-api)
4. [Configuration Examples](#configuration-examples)
5. [Test Pattern Guidelines](#test-pattern-guidelines)
6. [Docker Integration Stack](#docker-integration-stack)
7. [Related Documentation](#related-documentation)

---

## Test Categories

### Integration Tests (Docker-Based)
Tests that run against Docker containers. See `/containers/testing/` for Docker infrastructure.

### LiveIntegration Tests (Cloud-Based)
Tests that require live cloud credentials. Manual execution only.

---

## Integration Test Variables

### Apache Tika (Document Processing)

**Service:** Apache Tika document conversion service

| Variable | Default | Description |
|----------|---------|-------------|
| `TIKA_URL` | `http://localhost:9998` | Apache Tika server URL |

**Docker Container:** `apache/tika` (Port 9998)

**Tests Using:**
- `OoBDev.Apache.Tika.Tests` (6 handlers: PDF, DOC, DOCX, EPUB, ODT, RTF)

---

### SMTP/IMAP (Email Testing)

**Service:** SMTP4Dev email testing server

| Variable | Default | Description |
|----------|---------|-------------|
| `SMTP_HOST` | `localhost` | SMTP server hostname |
| `SMTP_PORT` | `25` | SMTP server port |
| `IMAP_HOST` | `localhost` | IMAP server hostname |
| `IMAP_PORT` | `143` | IMAP server port |

**Docker Container:** `rnwood/smtp4dev` (Ports 25 SMTP, 143 IMAP, 7777 Web UI)

**Tests Using:**
- `OoBDev.MailKit.Tests.ClientExampleTests.SendSmtpTest`
- `OoBDev.MailKit.Tests.ClientExampleTests.GetImapTest`

---

### MongoDB (NoSQL Database)

**Service:** MongoDB document database

| Variable | Default | Description |
|----------|---------|-------------|
| `MONGODB_CONNECTION_STRING` | `mongodb://localhost:27017?collation={locale:'en_US',caseLevel:false,strength:2 }` | MongoDB connection string with collation settings |

**Docker Container:** `mongo:latest` (Port 27017)

**Tests Using:**
- `OoBDev.MongoDB.Tests.MongoDBTests.TestMethod1`
- `OoBDev.MongoDB.Tests.MongoDBTests.TestMethod2`
- `OoBDev.MongoDB.Tests.MongoDBTests.TestMethod3`

**Notes:**
- Tests create unique database names: `IntegrationTest_{Guid}`
- Automatic cleanup via `[TestCleanup]` drops test databases

---

### SQL Server (Relational Database)

**Service:** Microsoft SQL Server

| Variable | Default | Description |
|----------|---------|-------------|
| `SQL_CONNECTION_STRING` | `Server=localhost,1433;User Id=sa;Password=IntegrationTest123!;TrustServerCertificate=True` | SQL Server connection string |

**Docker Container:** `mcr.microsoft.com/mssql/server:2022-latest` (Port 1433)

**Tests Using:**
- To be migrated (currently no Integration tests)

**Notes:**
- Use unique database names for test isolation
- Cleanup required in `[TestCleanup]`

---

### RabbitMQ (Message Queue)

**Service:** RabbitMQ message broker

| Variable | Default | Description |
|----------|---------|-------------|
| `RABBITMQ_HOST` | `localhost` | RabbitMQ server hostname |
| `RABBITMQ_PORT` | `5673` | RabbitMQ AMQP port (5672 used by Service Bus emulator) |

**Docker Container:** `rabbitmq:latest` (Ports 5673 AMQP, 15672 Management UI)

**Tests Using:**
- `OoBDev.RabbitMQ.Tests.MessageQueueing.RabbitMQQueueMessageSenderProviderTests.SendAsyncTest_ByFullType`
- `OoBDev.RabbitMQ.Tests.MessageQueueing.RabbitMQQueueMessageSenderProviderTests.SendAsyncTest_ByKeyed`
- `OoBDev.RabbitMQ.Tests.MessageQueueing.RabbitMQQueueMessageSenderProviderTests.FindProviderTests`

**Notes:**
- Default credentials: `guest/guest`
- Queue cleanup handled by framework

---

### Redis (Cache Store)

**Service:** Redis in-memory data store

| Variable | Default | Description |
|----------|---------|-------------|
| `REDIS_CONNECTION_STRING` | `localhost:6379` | Redis connection string |

**Docker Container:** `redis:7-alpine` (Port 6379)

**Tests Using:**
- `OoBDev.Redis.Caching.Tests.Examples.ExampleTests.CachingDesignTest_WithRedisCache`
- Other Redis caching integration tests (to be migrated from DevLocal category)

**Notes:**
- No authentication by default in test container
- Cache keys should be unique per test run
- Automatic cleanup via FLUSHDB or key expiration

---

### OpenSearch (Search Engine)

**Service:** OpenSearch distributed search and analytics engine

| Variable | Default | Description |
|----------|---------|-------------|
| `OPENSEARCH_URL` | `http://localhost:9200` | OpenSearch HTTP endpoint |
| `OPENSEARCH_USERNAME` | `admin` | OpenSearch admin username |
| `OPENSEARCH_PASSWORD` | `admin` | OpenSearch admin password |

**Docker Container:** `opensearchproject/opensearch:latest` (Ports 9200 HTTP, 9600 Performance)

**Tests Using:**
- `OoBDev.OpenSearch.Tests.OpenSearchTests.CreateIndexTest`
- `OoBDev.OpenSearch.Tests.OpenSearchTests.SearchIndexTest`

**Notes:**
- Tests create unique index names: `integrationtest_{Guid}`
- Automatic cleanup via `[TestCleanup]` deletes test indices

---

### SBert (Sentence Embeddings - AI/ML)

**Service:** Sentence-BERT embedding service (CPU-only for CI/CD)

| Variable | Default | Description |
|----------|---------|-------------|
| `SBERT_URL` | `http://localhost:5080` | SBert HTTP API endpoint |

**Docker Container:** Custom SBert image (Port 5080)

**Tests Using:**
- `OoBDev.SBert.Tests.SentenceEmbeddingClientTests.GetEmbeddingAsyncTest`
- `OoBDev.SBert.Tests.SentenceEmbeddingClientTests.GetAllTest`

**Notes:**
- Stateless service (no cleanup needed)
- CPU-only configuration for CI/CD compatibility

---

### Qdrant (Vector Database)

**Service:** Qdrant vector similarity search engine

| Variable | Default | Description |
|----------|---------|-------------|
| `QDRANT_URL` | `http://localhost:6333` | Qdrant HTTP API endpoint |
| `QDRANT_GRPC_URL` | `http://localhost:6334` | Qdrant gRPC endpoint |

**Docker Container:** `qdrant/qdrant` (Ports 6333 HTTP, 6334 gRPC)

**Tests Using:**
- Currently all tests commented out (deferred migration)

**Notes:**
- Tests should use unique collection names: `integration_test_{Guid}`
- Cleanup requires: `DeleteCollectionAsync(_testCollectionName)`

---

### Azurite (Azure Storage Emulator)

**Service:** Azure Storage emulator for local development

| Variable | Default | Description |
|----------|---------|-------------|
| `AZURITE_BLOB_URL` | `http://localhost:10000` | Blob storage endpoint |
| `AZURITE_QUEUE_URL` | `http://localhost:10001` | Queue storage endpoint |
| `AZURITE_TABLE_URL` | `http://localhost:10002` | Table storage endpoint |

**Docker Container:** `mcr.microsoft.com/azure-storage/azurite` (Ports 10000-10002)

**Tests Using:**
- To be migrated

**Notes:**
- Default account: `devstoreaccount1`
- Default key: `Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==`

---

### LocalStack (AWS Emulator)

**Service:** AWS cloud service emulator for local development

| Variable | Default | Description |
|----------|---------|-------------|
| `LOCALSTACK_URL` | `http://localhost:4566` | LocalStack unified endpoint |

**Docker Container:** `localstack/localstack` (Port 4566)

**Tests Using:**
- To be migrated

**Notes:**
- Supports S3, DynamoDB, SQS, SNS, Lambda, and more
- Use AWS SDK with endpoint override

---

### Keycloak (Identity & Access Management)

**Service:** Keycloak IAM and SSO

| Variable | Default | Description |
|----------|---------|-------------|
| `KEYCLOAK_URL` | `http://localhost:8081` | Keycloak server base URL |
| `KEYCLOAK_REALM` | `integration-test` | Keycloak realm name for tests |
| `KEYCLOAK_CLIENT_ID` | `integration-test-client` | OAuth confidential client ID |
| `KEYCLOAK_CLIENT_SECRET` | `test-client-secret-12345` | OAuth client secret |
| `KEYCLOAK_TEST_USERNAME` | `testuser` | Standard test user username |
| `KEYCLOAK_TEST_PASSWORD` | `testpassword` | Standard test user password |
| `KEYCLOAK_ADMIN_USERNAME` | `adminuser` | Admin test user username |
| `KEYCLOAK_ADMIN_PASSWORD` | `adminpassword` | Admin test user password |

**Docker Container:** `quay.io/keycloak/keycloak:latest` (Port 8081)

**Pre-configured Test Realm:** `integration-test`

**Test Users:**
- `testuser/testpassword` - Standard user with `user` role (enabled, verified)
- `adminuser/adminpassword` - Admin user with `user` and `admin` roles (enabled, verified)
- `disableduser/disabledpassword` - Disabled account for negative tests
- `unverifieduser/unverifiedpassword` - Unverified email for email flow tests

**Test Clients:**
- `integration-test-client` (confidential) - For server-side authentication
- `integration-test-public-client` (public) - For browser-based authentication

**Tests Using:**
- To be migrated

**Notes:**
- Admin console: `http://localhost:8081` (admin/admin)
- Realm auto-imported on startup from `keycloak-config/integration-test-realm.json`
- See [KEYCLOAK-TESTING.md](containers/testing/KEYCLOAK-TESTING.md) for detailed testing guide
- Token endpoint: `http://localhost:8081/realms/integration-test/protocol/openid-connect/token`

---

## LiveIntegration Test Variables

### Azure B2C (Identity Provider)

**Service:** Azure Active Directory B2C

| Variable | Default | Description |
|----------|---------|-------------|
| `AZURE_B2C_TENANT_ID` | *(none)* | Azure B2C tenant ID |
| `AZURE_B2C_CLIENT_ID` | *(none)* | Application (client) ID |
| `AZURE_B2C_CLIENT_SECRET` | *(none)* | Client secret |
| `AZURE_B2C_DOMAIN` | *(none)* | B2C domain (e.g., `yourb2c.onmicrosoft.com`) |

**Tests Using:**
- `OoBDev.Microsoft.B2C.Tests` (manual execution only)

**Setup:**
1. Create Azure B2C tenant
2. Register application
3. Generate client secret
4. Configure `.runsettings` with credentials

---

### Application Insights (Telemetry)

**Service:** Microsoft Application Insights

| Variable | Default | Description |
|----------|---------|-------------|
| `APPINSIGHTS_INSTRUMENTATION_KEY` | *(none)* | Instrumentation key |
| `APPINSIGHTS_CONNECTION_STRING` | *(none)* | Connection string |

**Tests Using:**
- `OoBDev.Microsoft.ApplicationInsights.Tests` (manual execution only)

**Setup:**
1. Create Application Insights resource in Azure
2. Copy instrumentation key or connection string
3. Configure `.runsettings`

---

### Groq Cloud (LLM API)

**Service:** Groq Cloud AI inference

| Variable | Default | Description |
|----------|---------|-------------|
| `GROQ_API_KEY` | *(none)* | Groq API key |
| `GROQ_API_URL` | `https://api.groq.com/openai/v1` | Groq API endpoint |

**Tests Using:**
- `OoBDev.Groq.Tests` (manual execution only)

**Setup:**
1. Create Groq account at https://groq.com
2. Generate API key
3. Configure `.runsettings`

---

## Configuration Examples

### .runsettings File

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <TestRunParameters>
    <!-- Apache Tika -->
    <Parameter name="TIKA_URL" value="http://localhost:9998" />

    <!-- SMTP/IMAP -->
    <Parameter name="SMTP_HOST" value="localhost" />
    <Parameter name="SMTP_PORT" value="25" />
    <Parameter name="IMAP_HOST" value="localhost" />
    <Parameter name="IMAP_PORT" value="143" />

    <!-- MongoDB -->
    <Parameter name="MONGODB_CONNECTION_STRING" value="mongodb://localhost:27017?collation={locale:'en_US',caseLevel:false,strength:2 }" />

    <!-- SQL Server -->
    <Parameter name="SQL_CONNECTION_STRING" value="Server=localhost,1433;User Id=sa;Password=IntegrationTest123!;TrustServerCertificate=True" />

    <!-- RabbitMQ -->
    <Parameter name="RABBITMQ_HOST" value="localhost" />
    <Parameter name="RABBITMQ_PORT" value="5673" />

    <!-- OpenSearch -->
    <Parameter name="OPENSEARCH_URL" value="http://localhost:9200" />
    <Parameter name="OPENSEARCH_USERNAME" value="admin" />
    <Parameter name="OPENSEARCH_PASSWORD" value="admin" />

    <!-- SBert -->
    <Parameter name="SBERT_URL" value="http://localhost:5080" />

    <!-- Qdrant -->
    <Parameter name="QDRANT_URL" value="http://localhost:6333" />
    <Parameter name="QDRANT_GRPC_URL" value="http://localhost:6334" />

    <!-- Azurite -->
    <Parameter name="AZURITE_BLOB_URL" value="http://localhost:10000" />
    <Parameter name="AZURITE_QUEUE_URL" value="http://localhost:10001" />
    <Parameter name="AZURITE_TABLE_URL" value="http://localhost:10002" />

    <!-- LocalStack -->
    <Parameter name="LOCALSTACK_URL" value="http://localhost:4566" />

    <!-- Keycloak -->
    <Parameter name="KEYCLOAK_URL" value="http://localhost:8081" />

    <!-- Azure B2C (LiveIntegration) -->
    <Parameter name="AZURE_B2C_TENANT_ID" value="your-tenant-id" />
    <Parameter name="AZURE_B2C_CLIENT_ID" value="your-client-id" />
    <Parameter name="AZURE_B2C_CLIENT_SECRET" value="your-client-secret" />
    <Parameter name="AZURE_B2C_DOMAIN" value="yourb2c.onmicrosoft.com" />

    <!-- Application Insights (LiveIntegration) -->
    <Parameter name="APPINSIGHTS_INSTRUMENTATION_KEY" value="your-instrumentation-key" />
    <Parameter name="APPINSIGHTS_CONNECTION_STRING" value="your-connection-string" />

    <!-- Groq Cloud (LiveIntegration) -->
    <Parameter name="GROQ_API_KEY" value="your-api-key" />
    <Parameter name="GROQ_API_URL" value="https://api.groq.com/openai/v1" />
  </TestRunParameters>
</RunSettings>
```

### Visual Studio

1. **Test → Configure Run Settings → Select Solution Wide runsettings File**
2. Select your `.runsettings` file
3. Run tests as normal

### Command Line

```bash
# Run all Integration tests with custom settings
dotnet test --settings integration.runsettings --filter "TestCategory=Integration"

# Run specific service tests
dotnet test --settings integration.runsettings --filter "FullyQualifiedName~MongoDB"
```

### CI/CD (GitHub Actions)

See `.github/workflows/integration-tests.yml` for environment variable configuration in CI/CD pipelines.

---

## Test Pattern Guidelines

### Using Test Properties in Tests

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task MyIntegrationTest()
{
    // ✅ CORRECT: Use GetRequiredProperty for required values
    var url = TestContext.GetRequiredProperty<string>("SERVICE_URL");
    var username = TestContext.GetRequiredProperty<string>("SERVICE_USERNAME");
    var password = TestContext.GetRequiredProperty<string>("SERVICE_PASSWORD");

    // ✅ CORRECT: Use GetPropertyOrDefault for values with industry-standard defaults
    var port = TestContext.GetPropertyOrDefault("SERVICE_PORT", 8080);

    // ❌ WRONG: Don't use Environment.GetEnvironmentVariable()
    // var url = Environment.GetEnvironmentVariable("SERVICE_URL") ?? "http://localhost:8080";

    // Your test logic...
}
```

### Test Isolation Best Practices

1. **Unique Resource Names:** Use `Guid.NewGuid()` for databases, indices, collections, queues
2. **Cleanup Logic:** Implement `[TestCleanup]` to delete test resources
3. **Independent Tests:** Each test should be runnable in isolation
4. **Idempotent:** Tests should produce same results on repeated runs

---

## Docker Integration Stack

To run Integration tests locally:

```bash
# Start all services
cd containers/testing
./scripts/integration-up.sh --wait

# Run tests
dotnet test --filter "TestCategory=Integration"

# Stop and cleanup
./scripts/integration-down.sh --clean
```

See `/containers/testing/README.md` for detailed Docker infrastructure documentation.

---

## Related Documentation

- [Testing Guidelines](./docs/architecture/testing/testing-guidelines.md) - Testing standards and patterns
- [Testing README](./docs/architecture/testing/README.md) - Testing documentation index
- [Docker Infrastructure](./containers/testing/README.md) - Docker setup and usage
- [Test Categories](./src/Framework/OoBDev.TestUtilities/TestCategories.cs) - Test category definitions
- [.runsettings](./src/.runsettings) - Default test run configuration
- [Integration Test Protocol](./src/.claude/protocols/software/integration-test-maintenance.md) - Maintenance checklist

---

**Maintainers:** Update this file when:
- New Integration tests are added
- New test properties are introduced
- Docker services are added/modified
- Default values change
