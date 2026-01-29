# Integration Test Infrastructure - Auto-Initialization

**Last Updated**: 2026-01-29

---

## Overview

The integration test infrastructure automatically initializes all required resources when containers start. This ensures tests can run immediately after the stack reports healthy, without requiring external setup scripts.

---

## Auto-Initialized Services

### 1. Ollama (LLM Inference)

**What Gets Initialized:**
- phi3 model (small, CPU-friendly, ~2.3GB) - baked into image at build time

**How It Works:**
- Custom Dockerfile: `ollama/Dockerfile`
- Model is pulled during image build: `RUN ollama serve & ... && ollama pull phi3`
- Model is part of the image, not downloaded at runtime
- Container starts immediately with model already available

**Configuration:**
```yaml
# docker-compose.integration-tests.yml
ollama:
  build:
    context: ./ollama
    dockerfile: Dockerfile
  image: oobdev/ollama-phi3:latest
```

**First Run:**
- Image build: ~2-3 minutes (downloads phi3 model)
- Subsequent runs: ~5-10 seconds (image cached)
- Use `--build` flag with integration-up script to rebuild

**Tests Can:**
- Assume phi3 model is always available
- Use any other Ollama-supported models (tests pull them dynamically)

---

### 2. LocalStack (AWS Emulator)

**What Gets Initialized:**
- SQS queue: `integration-test-queue`

**How It Works:**
- LocalStack init hooks: `localstack-init/01-create-sqs-queues.sh`
- Runs automatically when LocalStack reaches "ready" state
- Uses `awslocal` CLI to create queues

**Configuration:**
```yaml
# docker-compose.integration-tests.yml
localstack:
  environment:
    - AWS_DEFAULT_REGION=us-east-1
  volumes:
    - ./localstack-init:/etc/localstack/init/ready.d:ro
```

**Tests Can:**
- Use pre-created `integration-test-queue`
- Create additional queues with unique names (timestamps/GUIDs)
- Create FIFO queues, DLQs, etc. dynamically

**Example:**
```csharp
// Use pre-created queue
var queueName = "integration-test-queue";

// Or create unique queue for parallel tests
var queueName = $"test-queue-{DateTime.UtcNow:yyyyMMddHHmmss}";
await sqsClient.CreateQueueAsync(queueName);
```

---

### 3. Service Bus Emulator (Azure)

**What Gets Initialized:**
- Namespace: `sbemulatorns`
- Queue: `integration-test-queue`
- Topic: `integration-test-topic`
- Subscription: `test-subscription` (on topic)

**How It Works:**
- Configuration file: `servicebus-config/Config.json`
- Mounted as volume into container
- Emulator reads config on startup and creates entities

**Configuration:**
```yaml
# docker-compose.integration-tests.yml
servicebus-emulator:
  volumes:
    - ./servicebus-config/Config.json:/ServiceBus_Emulator/ConfigFiles/Config.json:ro
```

**Config File:**
```json
{
  "UserConfig": {
    "Namespaces": [
      {
        "Name": "sbemulatorns",
        "Queues": [
          { "Name": "integration-test-queue", ... }
        ],
        "Topics": [
          { "Name": "integration-test-topic", ... }
        ]
      }
    ]
  }
}
```

**Tests Can:**
- Use pre-created queue and topic
- Note: Emulator doesn't support management API (can't create entities dynamically)
- Use unique message properties (CorrelationId, SessionId) for test isolation

---

## Services Without Auto-Initialization

These services don't require initialization (stateless) or tests create resources dynamically:

| Service | Initialization Approach |
|---------|------------------------|
| **Apache Tika** | Stateless - ready immediately |
| **SMTP4Dev** | Stateless - ready immediately |
| **MongoDB** | Tests create unique databases: `IntegrationTest_{Guid.NewGuid():N}` |
| **SQL Server** | Tests create unique databases dynamically |
| **RabbitMQ** | Tests create queues/exchanges as needed |
| **Redis** | Stateless cache - tests use unique keys |
| **OpenSearch** | Tests create indices with unique names |
| **Qdrant** | Tests create collections with unique names |
| **Azurite** | Emulator ready - tests create containers/blobs |
| **Keycloak** | Imports `integration-test-realm.json` on startup |
| **SBert** | Model loaded - ready immediately |
| **Azurinsight** | SQLite DB created - ready immediately |

---

## Test Best Practices

### 1. Use Pre-Created Resources

For services with auto-initialization:

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task SendToPreCreatedQueue()
{
    var queueName = "integration-test-queue";  // Pre-created
    // ... test logic
}
```

### 2. Create Unique Resources

For parallel test execution:

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task SendToUniqueQueue()
{
    // Create unique queue for this test
    var queueName = $"test-{Guid.NewGuid():N}";
    await sqsClient.CreateQueueAsync(queueName);

    try
    {
        // ... test logic
    }
    finally
    {
        // Cleanup
        await sqsClient.DeleteQueueAsync(queueUrl);
    }
}
```

### 3. Handle Initialization in Tests

Some tests need to ensure resources exist:

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task EnsureQueueExists()
{
    var queueName = TestContext.GetRequiredProperty<string>("SQS_TEST_QUEUE");

    string queueUrl;
    try
    {
        var response = await sqsClient.GetQueueUrlAsync(queueName);
        queueUrl = response.QueueUrl;
    }
    catch (QueueDoesNotExistException)
    {
        // Create if doesn't exist
        var createResponse = await sqsClient.CreateQueueAsync(queueName);
        queueUrl = createResponse.QueueUrl;
        TestContext.WriteLine($"Created queue: {queueUrl}");
    }

    // ... test logic
}
```

### 4. Always Clean Up

Use `[TestCleanup]` or `try-finally` blocks:

```csharp
private string? _databaseName;

[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task TestOperation()
{
    _databaseName = $"IntegrationTest_{Guid.NewGuid():N}";
    // ... test logic
}

[TestCleanup]
public async Task Cleanup()
{
    if (_databaseName != null)
    {
        await client.DropDatabaseAsync(_databaseName);
    }
}
```

---

## Troubleshooting

### Ollama Model Not Downloaded

**Symptoms:**
- Tests fail with "model not found"
- Ollama container restarted before model download completed

**Solution:**
```bash
# Check if model exists
docker exec oobd-test-ollama ollama list

# Manually pull model
docker exec oobd-test-ollama ollama pull phi3

# Check logs
docker logs oobd-test-ollama
```

### LocalStack Queues Not Created

**Symptoms:**
- Tests fail with "queue does not exist"
- Init script didn't run

**Solution:**
```bash
# Check LocalStack health
curl http://localhost:4566/_localstack/health

# List queues
aws --endpoint-url=http://localhost:4566 sqs list-queues --region us-east-1

# Manually create queue
aws --endpoint-url=http://localhost:4566 sqs create-queue \
    --queue-name integration-test-queue --region us-east-1

# Check init logs
docker logs oobd-test-localstack | grep -A 10 "Initializing"
```

### Service Bus Entities Not Created

**Symptoms:**
- Tests fail with "entity does not exist"
- Config.json not loaded

**Solution:**
```bash
# Check container logs
docker logs oobd-test-servicebus

# Verify config mounted
docker exec oobd-test-servicebus cat /ServiceBus_Emulator/ConfigFiles/Config.json

# Restart with clean state
cd containers/testing
./scripts/integration-down.sh --clean
./scripts/integration-up.sh --wait
```

---

## Manual Setup Scripts (Deprecated)

The following scripts are **no longer needed** but kept for troubleshooting:

- `scripts/setup-ollama.sh` - Use for manual model management
- `scripts/setup-localstack-sqs.sh` - Use to verify queue creation
- `scripts/setup-servicebus-emulator.sh` - Displays connection info only

**Note:** Integration-up scripts no longer call these - initialization is automatic.

---

## Related Documentation

- [README.md](./README.md) - Complete infrastructure guide
- [STATUS.md](./STATUS.md) - Implementation progress
- [TEST_VARIABLES.md](../../TEST_VARIABLES.md) - Test configuration reference
- [docker-compose.integration-tests.yml](./docker-compose.integration-tests.yml) - Service definitions

---

**Summary**: All integration test services automatically initialize required resources on startup. Tests can assume these resources exist and create additional unique resources for parallel execution.
