# Azure Service Bus Provider - Integration Testing Guide

This guide explains how to test the Azure Service Bus provider using Microsoft's official Service Bus emulator.

## Prerequisites

- Docker Desktop or Docker Engine
- Integration test stack running

## Quick Start

### 1. Start Integration Test Containers

```bash
# From repository root
cd containers/testing

# Start all services including Service Bus emulator
./scripts/integration-up.sh --wait
```

The Azure Service Bus emulator will start automatically as part of the integration test stack.

### 2. Get Connection String

```bash
# Display emulator configuration
./scripts/setup-servicebus-emulator.sh
```

### 3. Run Integration Tests

```bash
# Run Service Bus integration tests
cd ../../src
export PATH="$HOME/.dotnet:$PATH"

# Set connection string environment variable
export SERVICEBUS_CONNECTION_STRING="Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"

# Run tests
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~ServiceBus"
```

### 4. Cleanup

```bash
# Stop and remove containers
cd ../containers/testing
./scripts/integration-down.sh --clean
```

## Configuration for Tests

### Environment Variable

```bash
export SERVICEBUS_CONNECTION_STRING="Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
```

### Pre-configured Entities

The emulator starts with these entities configured:
- **Queue**: `integration-test-queue`
- **Topic**: `integration-test-topic`

## Example Integration Test

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.MessageQueueing.Services;
using OoBDev.Microsoft.Azure.ServiceBus;
using OoBDev.System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.Tests;

[TestClass]
public class AzureServiceBusIntegrationTests
{
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToEmulatorQueue_Succeeds()
    {
        // Arrange - Service Bus emulator configuration
        var connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING")
            ?? "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "MessageQueuing:TestQueue:ConnectionString", connectionString },
            { "MessageQueuing:TestQueue:QueueName", "integration-test-queue" },
        });

        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAzureServiceBusServices();
        services.TryAddJsonSerializer();
        services.TryAddSingleton<IConfiguration>(config);
        services.TryAddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act - Send test message
        var context = contextFactory.Create("TestQueue", typeof(TestMessage).FullName);
        context.Headers["TestHeader"] = "TestValue";
        context.Headers["CustomProperty"] = "CustomValue";

        var testMessage = new TestMessage
        {
            Id = 123,
            Content = "Integration Test Message"
        };

        var correlationId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(correlationId);
        Assert.IsFalse(string.IsNullOrEmpty(correlationId));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToTopic_Succeeds()
    {
        // Arrange - Topic configuration
        var connectionString = Environment.GetEnvironmentVariable("SERVICEBUS_CONNECTION_STRING")
            ?? "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "MessageQueuing:TestTopic:ConnectionString", connectionString },
            { "MessageQueuing:TestTopic:TopicName", "integration-test-topic" },
        });

        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAzureServiceBusServices();
        services.TryAddJsonSerializer();
        services.TryAddSingleton<IConfiguration>(config);
        services.TryAddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act - Send to topic
        var context = contextFactory.Create("TestTopic", typeof(TestMessage).FullName);

        var testMessage = new TestMessage
        {
            Id = 456,
            Content = "Topic Test Message"
        };

        var correlationId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(correlationId);
    }

    private record TestMessage
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
    }
}
```

## Testing with Sessions

Session-based messaging requires a session-enabled queue:

```csharp
var configBuilder = new ConfigurationBuilder();
configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "MessageQueuing:SessionQueue:ConnectionString", connectionString },
    { "MessageQueuing:SessionQueue:QueueName", "session-enabled-queue" },
    { "MessageQueuing:SessionQueue:SessionId", "session-123" },
});

var context = contextFactory.Create("SessionQueue", typeof(Order).FullName);
var correlationId = await sender.SendAsync(order, context);
```

**Note**: The queue must be created with session support enabled.

## Testing Application Properties

```csharp
var context = contextFactory.Create("TestQueue", typeof(Order).FullName);
context.Headers["CustomerId"] = "customer-456";
context.Headers["OrderType"] = "Premium";
context.Headers["Priority"] = "High";

var correlationId = await sender.SendAsync(order, context);
```

All headers are automatically converted to Service Bus ApplicationProperties.

## Emulator Limitations

The Azure Service Bus emulator is a preview feature and has some limitations:

- **Limited Protocol Support**: AMQP only (no HTTP/REST management operations)
- **No Management API**: Cannot create/delete queues via SDK
- **Pre-configuration Required**: Entities must be defined via environment variables
- **Basic Features**: Limited to core messaging (no advanced features like duplicate detection)

For more details, see: https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator

## Troubleshooting

### Emulator Not Running

**Error**: Connection refused to localhost:5672

**Solution**:
```bash
cd containers/testing
./scripts/integration-up.sh --wait
```

### Entity Not Found

**Error**: The messaging entity could not be found

**Solution**: Entities must be pre-configured via environment variables in docker-compose. Check `/containers/testing/docker-compose.integration-tests.yml`:

```yaml
servicebus-emulator:
  environment:
    - SERVICEBUS_SERVER__ENTITYNAME__0=integration-test-queue
    - SERVICEBUS_SERVER__ENTITYNAME__1=integration-test-topic
    # Add more entities as needed
```

### Invalid Connection String

**Error**: Connection string is not valid

**Solution**: Use the emulator-specific connection string format:

```
Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;
```

## Creating Additional Queues/Topics

To add more entities to the emulator, update the docker-compose file:

```yaml
servicebus-emulator:
  environment:
    - ACCEPT_EULA=Y
    - SERVICEBUS_SERVER__ENTITYNAME__0=integration-test-queue
    - SERVICEBUS_SERVER__ENTITYNAME__1=integration-test-topic
    - SERVICEBUS_SERVER__ENTITYNAME__2=my-custom-queue       # Add here
    - SERVICEBUS_SERVER__ENTITYNAME__3=my-custom-topic       # Add here
```

Then restart the containers:

```bash
cd containers/testing
./scripts/integration-down.sh
./scripts/integration-up.sh --wait
```

## Testing Against Real Azure Service Bus

For comprehensive testing, you can use real Azure Service Bus by setting the `SERVICEBUS_CONNECTION_STRING` environment variable to your Azure connection string:

```bash
# Set real Azure connection string
export SERVICEBUS_CONNECTION_STRING="Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=..."

# Run tests against real Azure (use LiveIntegration category)
dotnet test --filter "TestCategory=LiveIntegration&FullyQualifiedName~ServiceBus"
```

## See Also

- [Azure Service Bus Emulator Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator)
- [Azure Service Bus Documentation](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
- [Integration Test Infrastructure](../../../../containers/testing/README.md)
