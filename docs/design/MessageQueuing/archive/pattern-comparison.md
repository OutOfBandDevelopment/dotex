# Message Queue Pattern Comparison

**Date:** 2026-01-20
**Purpose:** Compare Generic Channel-Based vs Context-Based patterns for message queue migration
**Decision:** Choose pattern for AWS SQS and Azure Service Bus migration

---

## Executive Summary

|  | Generic Channel-Based | Context-Based |
|--|----------------------|---------------|
| **Origin** | SharedFramework (incoming) | Main codebase (current) |
| **Complexity** | ⭐⭐⭐⭐ High | ⭐⭐ Low |
| **Type Safety** | ⭐⭐⭐⭐⭐ Compile-time | ⭐⭐ Runtime |
| **Flexibility** | ⭐⭐ Low | ⭐⭐⭐⭐⭐ High |
| **Missing Code** | ~200-300 LOC + tests | None (uses existing) |
| **Migration Effort** | ~500-800 LOC total | ~200-300 LOC |
| **Existing Example** | ❌ None | ✅ RabbitMQ provider |
| **Multi-Provider** | ⚠️ Complex | ✅ Excellent |

**Recommendation:** **Context-Based Pattern**
- Consistent with existing RabbitMQ implementation
- No missing abstractions
- Better for multi-provider scenarios
- Less migration effort
- Proven in production

---

## Side-by-Side Code Comparison

### Provider Implementation

#### Generic Pattern (SharedFramework)
```csharp
[MessageQueue(QueueType = QueueTypes.AmazonSimpleQueue)]
public class AmazonSqsMessageSender<TChannel> : IMessageSenderProvider<TChannel>
{
    private readonly IAmazonSqsFactory _factory;
    private readonly IQueueResolver<TChannel> _resolver;       // ❌ Missing
    private readonly IObjectSerializer _serializer;            // ❌ Missing
    private readonly IQueueConfig<TChannel> _config;

    public async Task<string> SendAsync<T>(
        T message,
        string messageId,
        IDictionary<string, object> properties) where T : class
    {
        var queueName = _resolver.GetQueueName();
        var (contentType, data) = _serializer.Serialize(message);

        var client = _factory.Create(
            _config.AccessKeyId ?? throw new Exception(),
            _config.SecretAccessKey ?? throw new Exception(),
            _config.Region);

        // ... send to SQS
    }
}
```

#### Context Pattern (Main Codebase)
```csharp
public class AmazonSqsMessageProvider : IMessageSenderProvider
{
    private readonly IJsonSerializer _serializer;              // ✅ Exists
    private readonly IAmazonSQS _sqsClient;

    public async Task<string?> SendAsync(
        object message,
        IMessageContext context)                               // ✅ Exists
    {
        var queueName = context.Config["QueueName"];
        var delaySeconds = int.Parse(context.Config["DelaySeconds"] ?? "0");

        // Serialize with existing IJsonSerializer
        using var stream = new MemoryStream();
        await _serializer.SerializeAsync(message, stream, default);

        // ... send to SQS
        // Use context.Headers for custom properties
        // Use context.CorrelationId for tracking
    }
}
```

---

### Dependency Injection

#### Generic Pattern
```csharp
// Complex generic registrations
services.AddTransient(typeof(IMessageSenderProvider<>), typeof(AmazonSqsMessageSender<>));
services.TryAddTransient(typeof(IQueueConfig<>), typeof(QueueConfig<>));

// Need resolver for EACH channel type
services.TryAddTransient<IQueueResolver<OrderChannel>, OrderChannelResolver>();
services.TryAddTransient<IQueueResolver<NotificationChannel>, NotificationChannelResolver>();
services.TryAddTransient<IQueueResolver<PaymentChannel>, PaymentChannelResolver>();

// Need to define channel marker classes
public class OrderChannel { }
public class NotificationChannel { }
public class PaymentChannel { }
```

#### Context Pattern
```csharp
// Simple, straightforward registrations
services.TryAddSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>();
services.TryAddSingleton<IMessageSenderProvider, AzureServiceBusMessageProvider>();

// Or keyed for explicit selection
services.AddKeyedSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>("sqs");
services.AddKeyedSingleton<IMessageSenderProvider, AzureServiceBusMessageProvider>("servicebus");

// No channel marker classes needed
// No per-channel resolvers needed
```

---

### Usage in Services

#### Generic Pattern
```csharp
public class OrderService
{
    private readonly IMessageSenderProvider<OrderChannel> _orderQueue;
    private readonly IMessageSenderProvider<NotificationChannel> _notifyQueue;

    // ✅ Clear from signature which queues are used
    // ✅ Compile-time type safety
    // ❌ Can't select queue at runtime
    // ❌ Hard to support multi-tenant queues
    public OrderService(
        IMessageSenderProvider<OrderChannel> orderQueue,
        IMessageSenderProvider<NotificationChannel> notifyQueue)
    {
        _orderQueue = orderQueue;
        _notifyQueue = notifyQueue;
    }

    public async Task ProcessOrder(Order order)
    {
        await _orderQueue.SendAsync(
            message: order,
            messageId: order.Id.ToString(),
            properties: new Dictionary<string, object> { { "Priority", "High" } });
    }
}
```

#### Context Pattern
```csharp
public class OrderService
{
    private readonly IMessageSenderProviderFactory _senderFactory;
    private readonly IMessageContextFactory _contextFactory;

    // ❌ Not obvious which queues are used from signature
    // ✅ Can select queue at runtime
    // ✅ Easy to support multi-tenant queues
    // ✅ Flexible routing
    public OrderService(
        IMessageSenderProviderFactory senderFactory,
        IMessageContextFactory contextFactory)
    {
        _senderFactory = senderFactory;
        _contextFactory = contextFactory;
    }

    public async Task ProcessOrder(Order order)
    {
        var context = _contextFactory.Create("OrderQueue", typeof(Order).FullName);
        context.Headers["Priority"] = "High";
        context.CorrelationId = order.Id.ToString();

        var sender = _senderFactory.GetProvider(context);
        await sender.SendAsync(order, context);
    }
}
```

---

### Configuration

#### Generic Pattern
```json
{
  "MessageQueuing": {
    "OrderChannel": {
      "QueueName": "production-orders",
      "ConnectionString": "AccessKeyId=...;SecretAccessKey=...;Region=us-east-1",
      "MaxNumberOfMessages": 10,
      "WaitTimeSeconds": 20
    },
    "NotificationChannel": {
      "QueueName": "production-notifications",
      "ConnectionString": "AccessKeyId=...;SecretAccessKey=...;Region=us-west-2"
    }
  }
}
```

#### Context Pattern
```json
{
  "AWS": {
    "AccessKeyId": "AKIA...",
    "SecretAccessKey": "abc123...",
    "Region": "us-east-1"
  },
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "sqs",
      "QueueName": "production-orders",
      "DelaySeconds": 0
    },
    "NotificationQueue": {
      "Provider": "servicebus",
      "QueueName": "production-notifications",
      "ConnectionString": "Endpoint=sb://..."
    }
  }
}
```

---

## Detailed Comparison Matrix

| Feature | Generic Pattern | Context Pattern | Winner |
|---------|----------------|-----------------|--------|
| **Type Safety** | ⭐⭐⭐⭐⭐ Compile-time | ⭐⭐ Runtime strings | Generic |
| **Dependency Injection Clarity** | ⭐⭐⭐⭐ Very clear | ⭐⭐ Requires docs | Generic |
| **Runtime Flexibility** | ⭐⭐ Limited | ⭐⭐⭐⭐⭐ Excellent | Context |
| **Multi-Tenant Support** | ⭐⭐ Difficult | ⭐⭐⭐⭐⭐ Easy | Context |
| **Multi-Provider Support** | ⭐⭐ Complex | ⭐⭐⭐⭐⭐ Simple | Context |
| **Configuration Flexibility** | ⭐⭐⭐ Good | ⭐⭐⭐⭐⭐ Excellent | Context |
| **Learning Curve** | ⭐⭐ Harder (generics) | ⭐⭐⭐⭐ Easier | Context |
| **Code Required** | ⭐⭐ Need abstractions | ⭐⭐⭐⭐⭐ Uses existing | Context |
| **Consistency** | ⭐ New pattern | ⭐⭐⭐⭐⭐ Matches RabbitMQ | Context |
| **Refactoring Safety** | ⭐⭐⭐⭐⭐ Compiler catches | ⭐⭐ Runtime errors | Generic |
| **Migration Scenarios** | ⭐⭐ Hard | ⭐⭐⭐⭐⭐ Easy | Context |
| **Bridge Patterns** | ⭐⭐ Complex | ⭐⭐⭐⭐⭐ Natural | Context |

---

## Use Case Suitability

### When Generic Pattern Is Better

✅ **Fixed, Well-Known Channels (5-10 queues)**
```csharp
// You know exactly which queues exist
public enum Queues { Order, Payment, Notification, Audit, Email }

// Each has specific type
IMessageSenderProvider<OrderChannel>
IMessageSenderProvider<PaymentChannel>
```

✅ **Compile-Time Type Safety Critical**
```csharp
// Financial/medical systems
// Wrong queue = regulatory violation
public class PaymentProcessor
{
    // ✅ Compiler ensures correct queue
    public PaymentProcessor(IMessageSenderProvider<PaymentChannel> paymentQueue) { }
}
```

✅ **Single Provider Per Channel**
```csharp
// Each channel uses exactly one provider
OrderChannel → SQS only
PaymentChannel → ServiceBus only
NotificationChannel → RabbitMQ only
```

✅ **Self-Documenting Code Priority**
```csharp
// Dependencies show exact queues used
public class OrderService
{
    public OrderService(
        IMessageSenderProvider<OrderChannel> orderQueue,
        IMessageSenderProvider<AuditChannel> auditQueue)
    {
        // Very clear what queues this service touches
    }
}
```

---

### When Context Pattern Is Better

✅ **Dynamic Queue Management**
```csharp
// Multi-tenant: per-tenant queues
var queueName = $"tenant-{tenantId}-orders";
var context = _factory.Create(queueName, typeof(Order).FullName);

// Queue names from database
var queueName = await _db.GetQueueForCustomer(customerId);
var context = _factory.Create(queueName, typeof(Order).FullName);
```

✅ **Multiple Providers / Migration**
```csharp
// Easy to use multiple providers simultaneously
[FromKeyedServices("sqs")] IMessageSenderProvider sqsSender,
[FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitSender,
[FromKeyedServices("servicebus")] IMessageSenderProvider serviceBusSender

// Bridge: read from SQS, write to RabbitMQ
await _rabbitSender.SendAsync(messageFromSqs, context);
```

✅ **Configuration-Driven Systems**
```csharp
// Change provider without code changes
{
  "OrderQueue": {
    "Provider": "sqs",        // Change to "rabbitmq" - no code change!
    "QueueName": "orders-v2"  // Change queue name - no code change!
  }
}
```

✅ **Existing Codebase Consistency**
```csharp
// Already have RabbitMQ using context pattern
public class RabbitMQQueueMessageProvider : IMessageSenderProvider
{
    public Task<string?> SendAsync(object message, IMessageContext context) { }
}

// New providers follow same pattern
public class AmazonSqsMessageProvider : IMessageSenderProvider
{
    public Task<string?> SendAsync(object message, IMessageContext context) { }
}
```

---

## Migration Effort Comparison

### Generic Pattern Migration

**New Code Required:**
1. `IQueueResolver<TChannel>` interface + implementation (~50 LOC)
2. `IObjectSerializer` interface + implementation (~100 LOC)
3. `IQueueConnectionString` interface + implementation (~30 LOC)
4. Tests for new abstractions (~300 LOC)
5. Channel type definitions (10 LOC per channel)
6. Per-channel resolver implementations (~50 LOC per channel)

**Modify Incoming Code:**
- Keep generic signatures
- Minor namespace updates
- Update project references

**Total Effort:** ~500-800 LOC

---

### Context Pattern Migration

**New Code Required:**
- None (uses existing infrastructure)

**Modify Incoming Code:**
1. Remove `<TChannel>` generics
2. Change `IObjectSerializer` → `IJsonSerializer`
3. Change method signature to `SendAsync(object message, IMessageContext context)`
4. Read config from `context.Config` instead of `IQueueResolver`
5. Use `context.Headers` for custom properties

**Changes Per Provider:**
- AWS SQS: ~100-150 LOC changes
- Azure Service Bus: ~100-150 LOC changes
- Update tests: ~100 LOC

**Total Effort:** ~200-400 LOC

---

## Real-World Scenario Comparison

### Scenario 1: Add New Queue

#### Generic Pattern
```csharp
// 1. Define channel type
public class InvoiceChannel { }

// 2. Create resolver
public class InvoiceChannelResolver : IQueueResolver<InvoiceChannel> { /* 50 LOC */ }

// 3. Register
services.TryAddTransient<IQueueResolver<InvoiceChannel>, InvoiceChannelResolver>();

// 4. Inject
public InvoiceService(IMessageSenderProvider<InvoiceChannel> invoiceQueue) { }
```

#### Context Pattern
```csharp
// 1. Add configuration
{
  "MessageQueuing": {
    "InvoiceQueue": { "Provider": "sqs", "QueueName": "invoices" }
  }
}

// 2. Use immediately
var context = _factory.Create("InvoiceQueue", typeof(Invoice).FullName);
await _sender.SendAsync(invoice, context);
```

**Winner:** Context (less code, faster)

---

### Scenario 2: Bridge SQS → RabbitMQ

#### Generic Pattern
```csharp
// ❌ Complex: Need different channel types for same logical flow
public class BridgeService
{
    public BridgeService(
        IMessageReceiverProvider<IncomingSqsChannel> sqsReceiver,
        IMessageSenderProvider<OutgoingRabbitChannel> rabbitSender)
    {
        // Different channel types required even though it's same message
    }
}

// Must define multiple channel types
public class IncomingSqsChannel { }
public class OutgoingRabbitChannel { }
```

#### Context Pattern
```csharp
// ✅ Natural: Same interface for both providers
public class BridgeService
{
    public BridgeService(
        [FromKeyedServices("sqs-receiver")] IMessageReceiverProvider sqsReceiver,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitSender)
    {
        // Simple and clear
    }
}

// Just create different contexts
var sqsContext = _factory.Create("SQS-Incoming", typeof(Order).FullName);
var rabbitContext = _factory.Create("RabbitMQ-Outgoing", typeof(Order).FullName);
```

**Winner:** Context (simpler, more natural)

See [Multi-Provider Bridge Example](./multi-provider-bridge-example.md) for detailed implementation.

---

### Scenario 3: Multi-Tenant System

#### Generic Pattern
```csharp
// ❌ Can't handle dynamic tenant queues
public class TenantService
{
    // How to inject provider for tenant-specific queue?
    // Would need factory with complex type switching
    public TenantService(???) { }
}

// Tenants have different queues: tenant-1234-orders, tenant-5678-orders
// Can't create channel type for each tenant at compile time
```

#### Context Pattern
```csharp
// ✅ Perfect fit: dynamic queue names
public class TenantService
{
    public async Task SendTenantMessage(string tenantId, Order order)
    {
        var queueName = $"tenant-{tenantId}-orders";  // Runtime queue name
        var context = _factory.Create(queueName, typeof(Order).FullName);
        context.Headers["TenantId"] = tenantId;

        await _sender.SendAsync(order, context);
    }
}
```

**Winner:** Context (generic pattern doesn't support this)

---

## Decision Matrix

### Choose Generic Pattern If:
- [ ] You have 5-10 well-known, fixed channels
- [ ] Compile-time type safety is critical requirement
- [ ] You're willing to create new abstractions (~500 LOC)
- [ ] Self-documenting code is top priority
- [ ] You don't need multi-provider scenarios
- [ ] You don't need runtime queue selection
- [ ] Team is comfortable with generic types
- [ ] You're okay with inconsistency vs RabbitMQ

**Score:** 0/8 = Not recommended for this migration

---

### Choose Context Pattern If:
- [x] You want consistency with existing RabbitMQ code
- [x] You need multi-provider support (bridges, fan-out, migration)
- [x] You need runtime queue selection
- [x] You want minimal migration effort
- [x] You prefer using existing infrastructure (IJsonSerializer, etc.)
- [x] You need multi-tenant support
- [x] Configuration-driven behavior is important
- [x] Simpler code/DI is preferred

**Score:** 8/8 = ✅ **Strongly recommended**

---

## Recommendation

### ✅ **Use Context-Based Pattern**

**Primary Reasons:**

1. **Consistency** - Matches existing RabbitMQ implementation
   - Same interfaces: `IMessageSenderProvider`, `IMessageContext`
   - Same patterns: Factory, keyed services
   - One pattern to learn across all providers

2. **Zero Missing Infrastructure**
   - `IJsonSerializer` already exists
   - `IMessageContext` already exists
   - `IMessageSenderProviderFactory` already exists
   - No new abstractions needed

3. **Less Migration Effort**
   - ~200-400 LOC changes vs ~500-800 LOC for generic
   - Simpler changes (remove generics, adapt signature)
   - Faster to implement and test

4. **Better Multi-Provider Support**
   - Bridge patterns (SQS → RabbitMQ)
   - Fan-out patterns (send to multiple queues)
   - Migration scenarios (gradual rollout)
   - Multi-cloud hybrid architectures

5. **Proven in Production**
   - RabbitMQ provider works in production
   - Pattern is battle-tested
   - Known to work with existing framework

**Trade-off Accepted:**
- Loss of compile-time type safety for queue selection
- Must rely on configuration validation at runtime
- This is acceptable given the flexibility gains

---

## Implementation Roadmap

### Phase 1: AWS SQS Provider (Week 1)
1. Create `OoBDev.Amazon.Sqs/` project
2. Adapt incoming code to context pattern (~150 LOC changes)
3. Create `AmazonSqsMessageProvider : IMessageSenderProvider`
4. Update tests (~100 LOC)
5. Create README with configuration examples

### Phase 2: Azure Service Bus Provider (Week 1)
1. Create `OoBDev.Microsoft.Azure.ServiceBus/` project
2. Adapt incoming code to context pattern (~150 LOC changes)
3. Create `AzureServiceBusMessageProvider : IMessageSenderProvider`
4. Update tests (~100 LOC)
5. Create README with Service Bus features (topics, sessions, DLQ)

### Phase 3: Integration & Documentation (Week 2)
1. Add both projects to solution
2. Build and test verification
3. Create architecture documentation
4. Create usage examples
5. Update main TODO.md

**Total Timeline:** 1-2 weeks
**Total LOC:** ~400-500 LOC (implementation + tests)

---

## See Also

- [Context-Based Pattern](./pattern-context-based.md) - Detailed documentation
- [Generic Channel-Based Pattern](./pattern-generic-channel-based.md) - Alternative approach
- [Multi-Provider Bridge Example](./multi-provider-bridge-example.md) - Advanced scenarios
- `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ/` - Existing context pattern example

---

**Decision Date:** 2026-01-20
**Decision:** Context-Based Pattern
**Rationale:** Consistency, zero missing infrastructure, multi-provider support, less effort
**Next Step:** Begin AWS SQS provider migration using context pattern
