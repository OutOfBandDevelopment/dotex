# AWS SQS Provider - Integration Testing Guide

This guide explains how to test the AWS SQS provider using LocalStack emulation.

## Prerequisites

- Docker Desktop or Docker Engine
- AWS CLI (for queue setup)
- Integration test stack running

## Quick Start

### 1. Start Integration Test Containers

```bash
# From repository root
cd containers/testing

# Start all services including LocalStack
./scripts/integration-up.sh --wait
```

### 2. Setup SQS Queues in LocalStack

```bash
# Setup test queues
./scripts/setup-localstack-sqs.sh
```

This creates:
- **Standard Queue**: `integration-test-queue`
- **FIFO Queue**: `integration-test-fifo.fifo`
- **Dead Letter Queue**: `integration-test-dlq`

### 3. Run Integration Tests

```bash
# Run SQS integration tests
cd ../../src
export PATH="$HOME/.dotnet:$PATH"
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~Sqs"
```

### 4. Cleanup

```bash
# Stop and remove containers
cd ../containers/testing
./scripts/integration-down.sh --clean
```

## Configuration for Tests

### Environment Variables

```bash
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=us-east-1
export SQS_QUEUE_URL=http://localhost:4566/000000000000/integration-test-queue
```

### Queue URLs

LocalStack uses a consistent URL pattern:

```
Standard: http://localhost:4566/000000000000/{queue-name}
FIFO:     http://localhost:4566/000000000000/{queue-name}.fifo
```

**Available Test Queues:**
- `http://localhost:4566/000000000000/integration-test-queue`
- `http://localhost:4566/000000000000/integration-test-fifo.fifo`
- `http://localhost:4566/000000000000/integration-test-dlq`

## Example Integration Test

```csharp
using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Amazon.Sqs;
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.Tests;

[TestClass]
public class AmazonSqsIntegrationTests
{
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToLocalStack_Succeeds()
    {
        // Arrange - LocalStack configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "MessageQueuing:TestQueue:QueueUrl", "http://localhost:4566/000000000000/integration-test-queue" },
            { "MessageQueuing:TestQueue:Region", "us-east-1" },
            { "MessageQueuing:TestQueue:AccessKeyId", "test" },
            { "MessageQueuing:TestQueue:SecretAccessKey", "test" },
        });

        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAmazonSqsServices();
        services.TryAddJsonSerializer();
        services.TryAddSingleton<IConfiguration>(config);
        services.TryAddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act - Send test message
        var context = contextFactory.Create("TestQueue", typeof(TestMessage).FullName);
        context.Headers["TestHeader"] = "TestValue";

        var testMessage = new TestMessage
        {
            Id = 123,
            Content = "Integration Test Message"
        };

        var messageId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(messageId);
        Assert.IsFalse(string.IsNullOrEmpty(messageId));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToFifoQueue_Succeeds()
    {
        // Arrange - FIFO queue configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "MessageQueuing:FifoQueue:QueueUrl", "http://localhost:4566/000000000000/integration-test-fifo.fifo" },
            { "MessageQueuing:FifoQueue:Region", "us-east-1" },
            { "MessageQueuing:FifoQueue:AccessKeyId", "test" },
            { "MessageQueuing:FifoQueue:SecretAccessKey", "test" },
            { "MessageQueuing:FifoQueue:MessageGroupId", "test-group" },
        });

        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAmazonSqsServices();
        services.TryAddJsonSerializer();
        services.TryAddSingleton<IConfiguration>(config);
        services.TryAddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act - Send to FIFO queue
        var context = contextFactory.Create("FifoQueue", typeof(TestMessage).FullName);

        var testMessage = new TestMessage
        {
            Id = 456,
            Content = "FIFO Test Message"
        };

        var messageId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(messageId);
    }

    private record TestMessage
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
    }
}
```

## Testing FIFO Queues

FIFO queues require `MessageGroupId`:

```csharp
var configBuilder = new ConfigurationBuilder();
configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "MessageQueuing:FifoQueue:QueueUrl", "http://localhost:4566/000000000000/test.fifo" },
    { "MessageQueuing:FifoQueue:MessageGroupId", "my-group" },
    { "MessageQueuing:FifoQueue:AccessKeyId", "test" },
    { "MessageQueuing:FifoQueue:SecretAccessKey", "test" },
});
```

## Testing Message Attributes

```csharp
var context = contextFactory.Create("TestQueue", typeof(Order).FullName);
context.Headers["CustomerId"] = "customer-123";
context.Headers["Priority"] = "High";
context.Headers["Region"] = "US-EAST";

var messageId = await sender.SendAsync(order, context);
```

All headers are automatically converted to SQS message attributes.

## Troubleshooting

### LocalStack Not Running

**Error**: Connection refused to localhost:4566

**Solution**:
```bash
cd containers/testing
./scripts/integration-up.sh --wait
```

### Queue Not Found

**Error**: Queue does not exist

**Solution**:
```bash
# Create queue manually
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name your-queue-name

# Or run setup script
./scripts/setup-localstack-sqs.sh
```

### Invalid Credentials

**Error**: The security token included in the request is invalid

**Solution**:
```bash
# LocalStack accepts any credentials
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
```

## Verifying Messages in LocalStack

```bash
# List queues
aws --endpoint-url=http://localhost:4566 sqs list-queues

# Get queue attributes
aws --endpoint-url=http://localhost:4566 sqs get-queue-attributes \
    --queue-url http://localhost:4566/000000000000/integration-test-queue \
    --attribute-names All

# Receive messages
aws --endpoint-url=http://localhost:4566 sqs receive-message \
    --queue-url http://localhost:4566/000000000000/integration-test-queue
```

## See Also

- [LocalStack Documentation](https://docs.localstack.cloud/user-guide/aws/sqs/)
- [AWS SQS Documentation](https://docs.aws.amazon.com/sqs/)
- [Integration Test Infrastructure](../../../../containers/testing/README.md)
