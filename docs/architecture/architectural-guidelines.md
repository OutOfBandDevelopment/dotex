# Architectural Guidelines

**Version:** 1.0
**Last Updated:** 2026-01-12
**Project:** OoBDev (dotex) - .NET Extensions Framework

---

## Overview

This document establishes the core architectural guidelines and design philosophy for the OoBDev framework. These guidelines ensure consistency, maintainability, and extensibility across all projects in the solution.

---

## Core Principles

### 1. Layered Architecture

**Principle:** Organize code into distinct layers with clear responsibilities and dependencies.

**Layers (Top to Bottom):**
1. **Common** - Orchestration and all-in-one packages
2. **Framework** - Core domain libraries and business logic
3. **Extensions** - Custom .NET system extensions
4. **ExternalServices** - Third-party integrations and wrappers

**Dependency Rule:** Dependencies flow downward. Lower layers cannot depend on higher layers.

**Rationale:**
- Reduces coupling between components
- Enables independent testing of layers
- Allows selective deployment (use only what you need)
- Facilitates parallel development

**Example:**
```
❌ BAD: OoBDev.MongoDB (ExternalServices) → OoBDev.Common (Common)
✅ GOOD: OoBDev.Common (Common) → OoBDev.MongoDB (ExternalServices)
```

---

### 2. Separation of Concerns

**Principle:** Each project should have a single, well-defined responsibility.

**Implementation:**
- Separate abstractions from implementations
- Separate domain logic from infrastructure
- Separate orchestration from execution
- Separate testing from production code

**Project Organization Pattern:**
```
OoBDev.{Domain}.Abstractions    ← Interfaces, models, enums
OoBDev.{Domain}                 ← Core implementation
OoBDev.{Domain}.Hosting         ← Background service hosting (if applicable)
OoBDev.{Domain}.Tests           ← Test project
```

**Example - Message Queueing:**
```
OoBDev.MessageQueueing.Abstractions  ← IMessageQueueHandler<T>, IMessageQueue
OoBDev.MessageQueueing               ← Core message queue implementation
OoBDev.MessageQueueing.Hosting       ← MessageReceiverHost background service
OoBDev.MessageQueueing.Tests         ← Unit and integration tests
```

**Rationale:**
- Easier to understand and maintain
- Simpler to test in isolation
- Reduces risk of breaking changes
- Enables targeted documentation

---

### 3. Dependency Inversion

**Principle:** Depend on abstractions, not concretions.

**Implementation:**
- All external services are accessed through interfaces
- All integrations implement provider interfaces
- Configuration is injected via IOptions<T>
- Factories create instances from configuration

**Pattern:**
```csharp
// Abstraction layer
public interface IMyService { }
public interface IMyServiceProvider { }
public interface IMyServiceProviderFactory { }

// Implementation layer
public class MyServiceProvider : IMyServiceProvider
{
    private readonly IOptions<MyServiceOptions> _options;

    public MyServiceProvider(IOptions<MyServiceOptions> options)
    {
        _options = options;
    }
}

// Factory layer
public class MyServiceProviderFactory : IMyServiceProviderFactory
{
    public IMyServiceProvider Create(string key)
    {
        // Create provider based on configuration
    }
}

// Consumer
public class MyConsumer
{
    public MyConsumer(IMyService service) // ✅ Depends on abstraction
    {
    }
}
```

**Rationale:**
- Enables dependency injection and testability
- Allows swapping implementations without code changes
- Facilitates mocking in tests
- Supports multi-tenant scenarios with keyed services

---

### 4. Provider/Factory Pattern

**Principle:** Use consistent provider/factory abstraction for all integrations.

**Hierarchy:** Abstraction → Provider → Factory

**Implementation:**
```csharp
// Step 1: Define abstraction
public interface IVectorStore<T> where T : class
{
    Task<SearchResultModel> SearchAsync(string query);
}

// Step 2: Define provider interface
public interface IVectorStoreProvider
{
    IVectorStore<T> GetVectorStore<T>(string collectionName) where T : class;
}

// Step 3: Define factory interface
public interface IVectorStoreProviderFactory
{
    IVectorStoreProvider Create(string providerKey);
}

// Step 4: Implement provider
public class QdrantVectorStoreProvider : IVectorStoreProvider
{
    // Implementation
}

// Step 5: Implement factory
public class VectorStoreProviderFactory : IVectorStoreProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public VectorStoreProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IVectorStoreProvider Create(string providerKey)
    {
        return _serviceProvider.GetRequiredKeyedService<IVectorStoreProvider>(providerKey);
    }
}
```

**Usage:**
```csharp
// Registration
services.AddKeyedSingleton<IVectorStoreProvider, QdrantProvider>("qdrant");
services.AddKeyedSingleton<IVectorStoreProvider, OpenSearchProvider>("opensearch");

// Consumption
var factory = serviceProvider.GetRequiredService<IVectorStoreProviderFactory>();
var provider = factory.Create("qdrant");
var vectorStore = provider.GetVectorStore<MyDocument>("my-collection");
```

**Rationale:**
- Consistent pattern across all integrations
- Supports multiple providers for the same abstraction
- Configuration-driven provider selection
- Testable through mocking

**When to Use:**
- ✅ Database integrations (MongoDB, SQL Server)
- ✅ Cloud services (Azure Blob, AWS S3)
- ✅ Message queues (RabbitMQ, Azure Queue)
- ✅ Search engines (Qdrant, OpenSearch)
- ✅ LLM providers (Ollama, GroqCloud)
- ❌ Simple utilities (no multiple implementations)
- ❌ Internal helpers (no external configuration)

---

### 5. Explicit Over Implicit

**Principle:** Make dependencies, configurations, and behaviors explicit.

**Implementation:**

**No Implicit Usings:**
```xml
<PropertyGroup>
  <ImplicitUsings>disable</ImplicitUsings>
</PropertyGroup>
```

**Explicit Extension Registration:**
```csharp
// ✅ GOOD: Explicit registration
builder.Services.TryAddSystemExtensions(builder.Configuration);
builder.Services.TryAddHandlebarServices();

// ❌ BAD: Magic auto-registration
builder.Services.AddOoBDev(); // What does this register?
```

**Explicit Configuration:**
```csharp
// ✅ GOOD: Explicit configuration
services.TryAddMessageQueueing(configuration, builder =>
{
    builder.AddRabbitMQ("rabbitmq");
    builder.AddAzureQueue("azure-queue");
});

// ❌ BAD: Hidden configuration
services.AddMessageQueueing(); // What providers are registered?
```

**Rationale:**
- No surprises or hidden behavior
- Easier to understand what code is doing
- Simpler to debug configuration issues
- Better IDE support and discoverability

---

### 6. Type Safety

**Principle:** Leverage C#'s type system for compile-time safety.

**Implementation:**

**Generic Constraints:**
```csharp
public interface IMessageQueueHandler<TChannel, TMessage>
    where TChannel : IMessageChannel
    where TMessage : class
{
    Task HandleAsync(TMessage message, IMessageContext context);
}
```

**Strongly-Typed Configuration:**
```csharp
// ✅ GOOD: Strongly-typed options
public class RabbitMQOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

// ❌ BAD: String-based configuration
var hostName = configuration["RabbitMQ:HostName"]; // No compile-time safety
```

**Attribute-Based Type Safety:**
```csharp
[MessageQueue(SimpleName = "orders")]
public class OrderHandler : IMessageQueueHandler<OrderChannel, OrderMessage>
{
    public Task HandleAsync(OrderMessage message, IMessageContext context)
    {
        // message is strongly typed as OrderMessage
    }
}
```

**Rationale:**
- Catch errors at compile time instead of runtime
- Better IDE support (IntelliSense)
- Self-documenting code
- Refactoring safety

---

### 7. Open/Closed Principle

**Principle:** Open for extension, closed for modification.

**Implementation:**

**Extensibility Points:**
```csharp
// ✅ GOOD: New providers can be added without modifying framework
public interface ITemplateProvider
{
    bool CanHandle(string fileExtension);
    Task<string> RenderAsync(string template, object model);
}

// Register new provider
services.AddSingleton<ITemplateProvider, MyCustomTemplateProvider>();
```

**Attribute-Based Extension:**
```csharp
// ✅ GOOD: New handlers discovered via attributes
[MessageQueue(SimpleName = "new-feature")]
public class NewFeatureHandler : IMessageQueueHandler<MyChannel, MyMessage>
{
    // Implementation
}
```

**Visitor Pattern for Extension:**
```csharp
// ✅ GOOD: Expression trees can be modified without changing core logic
public interface IPostBuildExpressionVisitor
{
    Expression Visit(Expression expression);
}
```

**Rationale:**
- Add features without risking existing functionality
- No need to retest existing code
- Third parties can extend the framework
- Supports plugin architectures

---

### 8. Convention Over Configuration (Where Appropriate)

**Principle:** Use sensible defaults, allow overrides.

**Implementation:**

**Default Values:**
```csharp
public class MessageQueueOptions
{
    public int MaxConcurrentHandlers { get; set; } = 10; // Sensible default
    public TimeSpan MessageTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
```

**Attribute-Based Conventions:**
```csharp
[MessageQueue(SimpleName = "orders")] // Convention: queue name from attribute
[BlobContainer(ContainerName = "documents")] // Convention: container name from attribute
```

**Naming Conventions:**
```csharp
// Convention: Async methods end with "Async"
Task<Result> SendAsync(Message message);

// Convention: Factory methods named "Create"
IProvider Create(string key);

// Convention: Extension methods start with "TryAdd" for non-replacing registration
IServiceCollection TryAddSystemExtensions(this IServiceCollection services);
```

**When to Use:**
- ✅ Default configuration values
- ✅ Naming patterns (Async suffix, Create methods)
- ✅ Attribute-based discovery
- ❌ Critical security settings (must be explicit)
- ❌ Data loss scenarios (explicit confirmation required)

**Rationale:**
- Reduces boilerplate configuration
- Faster onboarding for new developers
- Consistent patterns across codebase
- Still allows explicit overrides when needed

---

### 9. Fail Fast

**Principle:** Detect errors as early as possible.

**Implementation:**

**Validation at Boundaries:**
```csharp
public class MyService
{
    public MyService(IOptions<MyServiceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var config = options.Value;
        if (string.IsNullOrEmpty(config.HostName))
            throw new InvalidOperationException("HostName is required");
    }
}
```

**Build-Time Validation:**
```xml
<!-- Custom MSBuild task to validate README exists -->
<Target Name="CheckReadMe" BeforeTargets="PrepareForBuild">
  <Error Condition="!Exists('README.md')" Code="OBDPK002"
         Text="README.md is required for all projects" />
</Target>
```

**Compile-Time Safety:**
```csharp
// ✅ GOOD: Generic constraint enforces type safety at compile time
public interface IVectorStore<T> where T : class
{
}

// ❌ BAD: Runtime type checking
public interface IVectorStore
{
    void Add(object item); // What if item is wrong type?
}
```

**Rationale:**
- Faster feedback loop
- Easier to diagnose issues
- Prevents cascading failures
- Better developer experience

---

### 10. Testability

**Principle:** Design for testability from the start.

**Implementation:**

**Dependency Injection:**
```csharp
// ✅ GOOD: Dependencies injected, easy to mock
public class OrderService
{
    private readonly IMessageQueueSender<OrderChannel> _sender;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IMessageQueueSender<OrderChannel> sender,
        ILogger<OrderService> logger)
    {
        _sender = sender;
        _logger = logger;
    }
}

// Test
var mockSender = new Mock<IMessageQueueSender<OrderChannel>>();
var service = new OrderService(mockSender.Object, NullLogger<OrderService>.Instance);
```

**Abstraction of External Dependencies:**
```csharp
// ✅ GOOD: Time is abstracted for testing
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

// Test with fixed time
var mockTime = new Mock<IDateTimeProvider>();
mockTime.Setup(x => x.UtcNow).Returns(new DateTime(2026, 1, 12));
```

**Test Utilities:**
```csharp
// Framework provides test utilities
using OoBDev.TestUtilities;

[TestClass]
public class MyTests
{
    [TestMethod]
    public void TestMethod()
    {
        // Use test utilities
        TestContext.AddAttachment("result.json", result);
    }
}
```

**Coverage Goals:**
- Framework projects: >80%
- LINQ/Query projects: >90%
- External service integrations: >50%
- Overall: >60%

**Rationale:**
- Confidence in refactoring
- Faster development iteration
- Documentation through tests
- Regression prevention

---

### 11. Documentation as Code

**Principle:** Documentation lives with the code and is validated during build.

**Implementation:**

**Required README:**
```xml
<!-- Build fails if README.md is missing -->
<Target Name="CheckReadMe" BeforeTargets="PrepareForBuild">
  <Error Condition="!Exists('README.md')" Code="OBDPK002"
         Text="README.md is required" />
</Target>
```

**Embedded Examples:**
```xml
<ItemGroup>
  <EmbeddedResource Include="Examples\*.txt" />
  <EmbeddedResource Include="Examples\*.json" />
  <EmbeddedResource Include="Examples\*.sql" />
</ItemGroup>
```

**XML Documentation:**
```csharp
/// <summary>
/// Sends a message to the specified queue.
/// </summary>
/// <typeparam name="TMessage">The message type.</typeparam>
/// <param name="message">The message to send.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public Task SendAsync<TMessage>(
    TMessage message,
    CancellationToken cancellationToken = default) where TMessage : class;
```

**PlantUML Diagrams:**
```markdown
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml
' Diagram definition
@enduml
```
```

**Rationale:**
- Documentation stays up to date
- Examples are validated by build
- Single source of truth
- Better onboarding experience

---

### 12. Performance Awareness

**Principle:** Design for performance without premature optimization.

**Implementation:**

**Async/Await Throughout:**
```csharp
// ✅ GOOD: Async for I/O operations
public async Task<Result> ProcessAsync(Message message)
{
    var data = await _repository.GetAsync(message.Id);
    await _sender.SendAsync(data);
}

// ❌ BAD: Blocking on async
public Result Process(Message message)
{
    var data = _repository.GetAsync(message.Id).Result; // Blocks thread
}
```

**ConfigureAwait in Libraries:**
```csharp
// In library code, avoid capturing SynchronizationContext
var result = await _httpClient.GetAsync(url).ConfigureAwait(false);
```

**Memory Efficiency:**
```csharp
// ✅ GOOD: Use Span<T> for slicing without allocations
public void ProcessBuffer(ReadOnlySpan<byte> buffer)
{
    var slice = buffer.Slice(0, 10);
}

// ✅ GOOD: Use ArrayPool for temporary buffers
var buffer = ArrayPool<byte>.Shared.Rent(4096);
try
{
    // Use buffer
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer);
}
```

**ValueTask for Hot Paths:**
```csharp
// Use ValueTask for frequently-called methods that often complete synchronously
public ValueTask<Result> TryGetCachedAsync(string key)
{
    if (_cache.TryGetValue(key, out var value))
        return new ValueTask<Result>(value); // No allocation

    return new ValueTask<Result>(LoadFromSourceAsync(key));
}
```

**Rationale:**
- Scalable under load
- Resource-efficient
- Better user experience
- Cost savings in cloud environments

---

### 13. Security by Default

**Principle:** Secure defaults, explicit opt-out if needed.

**Implementation:**

**No Hardcoded Secrets:**
```csharp
// ✅ GOOD: Configuration from IConfiguration
var connectionString = configuration.GetConnectionString("Database");

// ❌ BAD: Hardcoded secrets
var connectionString = "Server=myserver;User=admin;Password=secret123";
```

**Claims-Based Authorization:**
```csharp
[HttpGet]
[ApplicationRight(Rights = "admin.users.read")]
public async Task<IActionResult> GetUsers()
{
    // Only users with "admin.users.read" right can access
}
```

**Secure Configuration:**
```csharp
public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    // Validation ensures secure configuration
    public void Validate()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw new InvalidOperationException("ConnectionString is required");

        if (ConnectionString.Contains("Password=") && !ConnectionString.Contains("Encrypt=true"))
            throw new InvalidOperationException("Encryption is required for password-based connections");
    }
}
```

**Input Validation:**
```csharp
public async Task<Result> ProcessUserInput(string input)
{
    ArgumentNullException.ThrowIfNull(input);

    if (input.Length > 1000)
        throw new ArgumentException("Input too long", nameof(input));

    // Process validated input
}
```

**Rationale:**
- Reduce security vulnerabilities
- Compliance with security standards
- Defense in depth
- Protect against common attacks (injection, XSS, etc.)

---

### 14. Backward Compatibility Awareness

**Principle:** Minimize breaking changes, communicate when unavoidable.

**Implementation:**

**Semantic Versioning:**
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

**Deprecation Strategy:**
```csharp
[Obsolete("Use NewMethod instead. This will be removed in v3.0.")]
public void OldMethod()
{
    NewMethod(); // Call new implementation
}

public void NewMethod()
{
    // New implementation
}
```

**Extension Points:**
```csharp
// ✅ GOOD: Add new optional parameters
public Task ProcessAsync(
    Message message,
    CancellationToken cancellationToken = default,
    ProcessOptions? options = null) // New optional parameter
{
}

// ❌ BAD: Change signature
public Task ProcessAsync(Message message, ProcessOptions options) // Breaking change
{
}
```

**Rationale:**
- Don't break consumer code
- Smooth upgrade path
- Maintain trust with library users
- Reduce support burden

---

## Decision Framework

When making architectural decisions, ask:

1. **Does this follow the layered architecture?**
   - Is the dependency direction correct?
   - Is the responsibility in the right layer?

2. **Is this testable?**
   - Can I mock the dependencies?
   - Can I verify the behavior in isolation?

3. **Is this extensible?**
   - Can new implementations be added without modification?
   - Are extension points clearly defined?

4. **Is this explicit?**
   - Are dependencies clear?
   - Is configuration obvious?
   - Are behaviors predictable?

5. **Is this type-safe?**
   - Are types enforced at compile time?
   - Are generics used appropriately?

6. **Is this secure by default?**
   - No hardcoded secrets?
   - Proper authorization?
   - Input validation?

7. **Is this documented?**
   - README exists?
   - Examples provided?
   - XML documentation for public APIs?

8. **Is this performant enough?**
   - Async where appropriate?
   - No unnecessary allocations?
   - Scales under load?

---

## Related Documentation

- [architectural-standards.md](./architectural-standards.md) - Concrete standards and rules
- [architectural-patterns.md](./architectural-patterns.md) - Documented patterns
- [layering-architecture.md](./layering-architecture.md) - Layer details
- [provider-factory-pattern.md](./provider-factory-pattern.md) - Provider pattern guide

---

## Change Log

- 2026-01-12 v1.0: Initial architectural guidelines created
