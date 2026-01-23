# Message Queue Provider Migration Checklist

**Date:** 2026-01-20
**Pattern:** Context-Based (Following RabbitMQ Implementation)
**Target:** AWS SQS and Azure Service Bus

---

## RabbitMQ Pattern Reference

### Project Structure
```
OoBDev.RabbitMQ/
├── MessageQueueing/
│   ├── IQueueClientFactory.cs
│   ├── QueueClientFactory.cs
│   └── RabbitMQQueueMessageProvider.cs
├── RabbitMQGlobals.cs
├── ServiceCollectionEx.cs
├── OoBDev.RabbitMQ.csproj
└── Readme.RabbitMQ.md
```

### Key Patterns from RabbitMQ Implementation

#### 1. NuGet Packages (.csproj)
```xml
<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>False</GenerateDocumentationFile>
    <PackageReadmeFile>Readme.[Provider].md</PackageReadmeFile>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="GitVersion.MsBuild" Version="6.5.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.2" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.2" />
    <PackageReference Include="[ProviderClient]" Version="[Latest]" />
</ItemGroup>

<ItemGroup>
    <ProjectReference Include="..\..\..\Framework\OoBDev.MessageQueueing.Abstractions\OoBDev.MessageQueueing.Abstractions.csproj" />
    <ProjectReference Include="..\..\..\Framework\OoBDev.System.Abstractions\OoBDev.System.Abstractions.csproj" />
</ItemGroup>
```

#### 2. Globals Class
```csharp
namespace OoBDev.RabbitMQ;

public static class RabbitMQGlobals
{
    public const string MessageProviderKey = "rabbitmq";
}
```

#### 3. Service Registration (ServiceCollectionEx.cs)
```csharp
public static class ServiceCollectionEx
{
    public static IServiceCollection TryAddRabbitMQServices(this IServiceCollection services) =>
        services.TryAddRabbitMQQueueServices();

    public static IServiceCollection TryAddRabbitMQQueueServices(this IServiceCollection services)
    {
        // Register non-keyed (default)
        services.AddTransient<IMessageSenderProvider, RabbitMQQueueMessageProvider>();
        services.AddTransient<IMessageReceiverProvider, RabbitMQQueueMessageProvider>();

        // Register keyed (for multi-provider scenarios)
        services.TryAddKeyedTransient<IMessageSenderProvider, RabbitMQQueueMessageProvider>(RabbitMQGlobals.MessageProviderKey);
        services.TryAddKeyedTransient<IMessageReceiverProvider, RabbitMQQueueMessageProvider>(RabbitMQGlobals.MessageProviderKey);

        // Register factory
        services.TryAddTransient<IQueueClientFactory, QueueClientFactory>();

        return services;
    }
}
```

#### 4. Factory Pattern
```csharp
public interface IQueueClientFactory
{
    Task<(IConnection connection, IChannel channel, string queueName)> CreateAsync(IConfigurationSection config);
}

public class QueueClientFactory : IQueueClientFactory
{
    public async Task<(IConnection connection, IChannel channel, string queueName)> CreateAsync(IConfigurationSection config)
    {
        // Read configuration
        var hostName = config[nameof(ConnectionFactory.HostName)]
            ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(ConnectionFactory.HostName)}");
        var queueName = config["QueueName"]
            ?? throw new ConfigurationMissingException($"{config.Path}:QueueName");

        // Create client
        var factory = new ConnectionFactory() { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        return (connection, channel, queueName);
    }
}
```

#### 5. Provider Implementation
```csharp
public class RabbitMQQueueMessageProvider : IMessageSenderProvider, IMessageReceiverProvider
{
    private readonly IJsonSerializer _serializer;
    private readonly IQueueClientFactory _clientFactory;
    private readonly ILogger<RabbitMQQueueMessageProvider> _logger;

    public RabbitMQQueueMessageProvider(
        IJsonSerializer serializer,
        IQueueClientFactory clientFactory,
        ILogger<RabbitMQQueueMessageProvider> logger)
    {
        _serializer = serializer;
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        // Use factory to create client from context.Config
        var (connection, channel, queueName) = await _clientFactory.CreateAsync(context.Config);

        // Wrap message
        var wrapped = new WrappedQueueMessage
        {
            ContentType = "application/json;",
            PayloadType = message.GetType().AssemblyQualifiedName ?? throw new NotSupportedException(),
            CorrelationId = context.CorrelationId ?? "",
            Payload = message,
            Properties = context.Headers,
        };

        // Serialize using IJsonSerializer
        using var stream = new MemoryStream();
        await _serializer.SerializeAsync(wrapped, stream, default);
        ReadOnlyMemory<byte> body = stream.ToArray();

        // Send to queue
        using (connection)
        using (channel)
        {
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: null,
                mandatory: true,
                body: body);

            return context.CorrelationId;
        }
    }
}
```

---

## Latest NuGet Package Versions

### AWS SDK for .NET (SQS)
**Package:** `AWSSDK.SQS`
**Latest Version:** **4.0.2.11** (as of January 2026)
**NuGet:** [AWSSDK.SQS](https://www.nuget.org/packages/AWSSDK.SQS)
**Downloads:** 248.4M+ downloads

**Installation:**
```xml
<PackageReference Include="AWSSDK.SQS" Version="4.0.2.11" />
```

**Compatibility:** .NET 8.0+, .NET Core 3.1, .NET Standard 2.0, .NET Framework 4.7.2+

**Note:** Version 4.x is the latest major version. The AWS SDK v4 includes performance improvements and modernized APIs.

### Azure Service Bus
**Package:** `Azure.Messaging.ServiceBus`
**Latest Version:** **7.20.1** (as of January 2026)
**NuGet:** [Azure.Messaging.ServiceBus](https://www.nuget.org/packages/Azure.Messaging.ServiceBus)

**Installation:**
```xml
<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.20.1" />
```

**⚠️ Important:** The older packages `Microsoft.Azure.ServiceBus` and `WindowsAzure.ServiceBus` are **obsolete** and will no longer be maintained after 9/30/2026. Must use `Azure.Messaging.ServiceBus`.

**Migration:** If incoming code uses old package, must update to new API surface.

### Common Dependencies (Match RabbitMQ)
```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="10.0.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.2" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.2" />
<PackageReference Include="GitVersion.MsBuild" Version="6.5.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

---

## Migration Checklist: AWS SQS

### Project Setup
- [ ] Create `src/ExternalServices/Amazon/OoBDev.Amazon.Sqs/` directory
- [ ] Create `OoBDev.Amazon.Sqs.csproj` with:
  - [ ] TargetFramework: net10.0
  - [ ] Nullable: enable
  - [ ] PackageReadmeFile: Readme.AmazonSqs.md
  - [ ] Latest `AWSSDK.SQS` package
  - [ ] Microsoft.Extensions.* 10.0.2
  - [ ] GitVersion.MsBuild 6.5.1
  - [ ] ProjectReference to OoBDev.MessageQueueing.Abstractions
  - [ ] ProjectReference to OoBDev.System.Abstractions

### Files to Create
- [ ] `MessageQueueing/ISqsClientFactory.cs` (interface)
- [ ] `MessageQueueing/SqsClientFactory.cs` (implementation)
- [ ] `MessageQueueing/AmazonSqsMessageProvider.cs` (implements IMessageSenderProvider)
- [ ] `AmazonSqsGlobals.cs` (contains provider key constant)
- [ ] `ServiceCollectionEx.cs` (DI registration)
- [ ] `Readme.AmazonSqs.md` (provider documentation)

### Implementation Requirements
- [ ] Follow exact pattern from RabbitMQ
- [ ] Use `IJsonSerializer` for serialization
- [ ] Read config from `context.Config` (IConfigurationSection)
- [ ] Throw `ConfigurationMissingException` for missing required config
- [ ] Register both non-keyed and keyed services
- [ ] Support `WrappedQueueMessage` pattern
- [ ] Use context.CorrelationId for message tracking
- [ ] Support context.Headers for custom message attributes

### AWS SQS Specific Features to Support
- [ ] Standard and FIFO queues
- [ ] Message attributes (from context.Headers)
- [ ] Delay seconds (from config)
- [ ] Message group ID (FIFO queues, from config)
- [ ] Deduplication ID (FIFO queues, from config)
- [ ] Queue URL (from config)

### Configuration Pattern
```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789/orders.fifo",
      "MessageGroupId": "order-processing",
      "DeduplicationId": "use-content-hash",
      "DelaySeconds": 0
    }
  },
  "AWS": {
    "AccessKeyId": "AKIA...",
    "SecretAccessKey": "...",
    "Region": "us-east-1"
  }
}
```

---

## Migration Checklist: Azure Service Bus

### Project Setup
- [ ] Create `src/ExternalServices/Microsoft/OoBDev.Microsoft.Azure.ServiceBus/` directory
- [ ] Create `OoBDev.Microsoft.Azure.ServiceBus.csproj` with:
  - [ ] TargetFramework: net10.0
  - [ ] Nullable: enable
  - [ ] PackageReadmeFile: Readme.AzureServiceBus.md
  - [ ] Latest `Azure.Messaging.ServiceBus` package
  - [ ] Microsoft.Extensions.* 10.0.2
  - [ ] GitVersion.MsBuild 6.5.1
  - [ ] ProjectReference to OoBDev.MessageQueueing.Abstractions
  - [ ] ProjectReference to OoBDev.System.Abstractions

### Files to Create
- [ ] `MessageQueueing/IServiceBusClientFactory.cs` (interface)
- [ ] `MessageQueueing/ServiceBusClientFactory.cs` (implementation)
- [ ] `MessageQueueing/AzureServiceBusMessageProvider.cs` (implements IMessageSenderProvider)
- [ ] `AzureServiceBusGlobals.cs` (contains provider key constant)
- [ ] `ServiceCollectionEx.cs` (DI registration)
- [ ] `Readme.AzureServiceBus.md` (provider documentation)

### Implementation Requirements
- [ ] Follow exact pattern from RabbitMQ
- [ ] Use `IJsonSerializer` for serialization
- [ ] Read config from `context.Config` (IConfigurationSection)
- [ ] Throw `ConfigurationMissingException` for missing required config
- [ ] Register both non-keyed and keyed services
- [ ] Support `WrappedQueueMessage` pattern
- [ ] Use context.CorrelationId for message tracking
- [ ] Support context.Headers for application properties

### Azure Service Bus Specific Features to Support
- [ ] Queues and topics
- [ ] Sessions (from config)
- [ ] Partition key (from config)
- [ ] Scheduled enqueue time (from config)
- [ ] Time to live (from config)
- [ ] Subject (from config)
- [ ] Application properties (from context.Headers)

### Configuration Pattern
```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "ConnectionString": "Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...",
      "QueueName": "orders",
      "SessionId": "session-{orderId}",
      "PartitionKey": "{customerId}",
      "TimeToLive": "01:00:00"
    },
    "OrderTopic": {
      "ConnectionString": "Endpoint=sb://...",
      "TopicName": "orders-topic",
      "Subject": "order.created"
    }
  }
}
```

---

## Testing Requirements

### Unit Tests
- [ ] Create test project for each provider
- [ ] Test with mocked dependencies (IJsonSerializer, ILogger, etc.)
- [ ] Test configuration reading from IConfigurationSection
- [ ] Test error handling for missing configuration
- [ ] Test message wrapping with WrappedQueueMessage
- [ ] Test header/properties handling

### Integration Tests (DevLocal)
- [ ] Test sending to actual queue (requires local/dev environment)
- [ ] Test receiving from actual queue
- [ ] Test FIFO queue features (SQS)
- [ ] Test session features (Service Bus)
- [ ] Test topic/subscription features (Service Bus)

### Integration Tests (Docker - Future)
- [ ] Consider adding LocalStack for SQS testing (CI/CD)
- [ ] Azure Service Bus emulator (if available)

---

## Documentation Requirements

### README Files
- [ ] Create Readme.AmazonSqs.md with:
  - [ ] Overview of AWS SQS integration
  - [ ] Installation instructions
  - [ ] Configuration examples (standard queue, FIFO queue)
  - [ ] Usage examples
  - [ ] Feature list (message attributes, delay, etc.)
  - [ ] Troubleshooting

- [ ] Create Readme.AzureServiceBus.md with:
  - [ ] Overview of Azure Service Bus integration
  - [ ] Installation instructions
  - [ ] Configuration examples (queue, topic, sessions)
  - [ ] Usage examples
  - [ ] Feature list (topics, sessions, scheduled messages, etc.)
  - [ ] Troubleshooting

### Architecture Documentation
- [ ] Update `Features/MessageQueuing/pattern-context-based.md` with AWS SQS examples
- [ ] Update `Features/MessageQueuing/pattern-context-based.md` with Azure Service Bus examples
- [ ] Document provider-specific configuration options

---

## Code Quality Requirements

### Follow OoBDev Standards
- [ ] All public APIs have XML documentation
- [ ] Nullable reference types enabled
- [ ] No implicit usings
- [ ] Follow existing naming conventions
- [ ] Use ConfigurationMissingException for missing config (from OoBDev.System)
- [ ] Use ILogger for logging
- [ ] Use IJsonSerializer for serialization (from OoBDev.System)

### Error Handling
- [ ] Validate all required configuration at factory creation
- [ ] Provide clear error messages with config path
- [ ] Handle client creation failures gracefully
- [ ] Log errors with appropriate severity

---

## Migration Steps Order

### Step 1: Prepare Package Information
1. Check latest versions of AWS SDK and Azure Service Bus packages
2. Verify compatibility with .NET 10.0
3. Document breaking changes (if any) from older versions

### Step 2: AWS SQS Migration
1. Create project structure
2. Create .csproj with latest packages
3. Implement factory pattern (ISqsClientFactory)
4. Implement provider (AmazonSqsMessageProvider)
5. Create service registration (ServiceCollectionEx)
6. Create globals (AmazonSqsGlobals)
7. Create README
8. Create unit tests
9. Create integration tests (DevLocal)
10. Add to solution
11. Build and verify

### Step 3: Azure Service Bus Migration
1. Create project structure
2. Create .csproj with latest packages
3. Implement factory pattern (IServiceBusClientFactory)
4. Implement provider (AzureServiceBusMessageProvider)
5. Create service registration (ServiceCollectionEx)
6. Create globals (AzureServiceBusGlobals)
7. Create README
8. Create unit tests
9. Create integration tests (DevLocal)
10. Add to solution
11. Build and verify

### Step 4: Documentation
1. Update main README
2. Update pattern documentation
3. Create usage examples
4. Update TODO-archive/TODO-migrations-message-queues.md

### Step 5: Final Verification
1. Build entire solution
2. Run all unit tests
3. Run integration tests (if environment available)
4. Verify no breaking changes to existing code
5. Update TODO.md with completion

---

## Key Differences from Incoming Code

### What to Change
❌ **Remove:**
- Generic `<TChannel>` parameters
- `IQueueResolver<TChannel>` (doesn't exist in main)
- `IObjectSerializer` (use `IJsonSerializer` instead)
- `IQueueConfig<TChannel>` (use `IConfigurationSection` instead)
- `IRegistrar` pattern (use static extension methods)

✅ **Add:**
- `IJsonSerializer` dependency (from OoBDev.System)
- `IConfigurationSection` parameter (context.Config)
- Keyed service registration
- `ConfigurationMissingException` for validation
- Factory pattern (following RabbitMQ)

### What to Keep
✅ **Keep:**
- Provider-specific client logic
- Message attribute/property handling
- Queue-specific features (FIFO, sessions, etc.)
- Error handling patterns

---

## Success Criteria

### Must Have
- ✅ Matches RabbitMQ pattern exactly
- ✅ Uses latest stable NuGet packages
- ✅ All tests passing
- ✅ Builds successfully in solution
- ✅ README documentation complete
- ✅ Follows OoBDev coding standards

### Should Have
- ✅ Integration tests (DevLocal)
- ✅ Comprehensive XML documentation
- ✅ Configuration validation
- ✅ Logging at appropriate levels

### Nice to Have
- 🎯 Docker-based integration tests
- 🎯 Advanced feature examples
- 🎯 Performance benchmarks

---

**Next Action:** Check latest NuGet package versions and begin AWS SQS migration

**Reference Implementation:** `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ/`
