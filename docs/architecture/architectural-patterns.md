# Architectural Patterns

**Version:** 1.0
**Last Updated:** 2026-01-12
**Project:** OoBDev (dotex) - .NET Extensions Framework

---

## Overview

This document catalogs the key architectural and design patterns used throughout the OoBDev framework. Each pattern includes intent, implementation details, code examples with file references, benefits, drawbacks, and usage guidelines.

---

## Pattern Index

### Architectural Patterns
1. [Layered Architecture](#1-layered-architecture)
2. [Provider/Factory Pattern](#2-providerfactory-pattern)
3. [Dependency Injection Pattern](#3-dependency-injection-pattern)

### Design Patterns
4. [Handler Pattern](#4-handler-pattern)
5. [Middleware Pattern](#5-middleware-pattern)
6. [Visitor Pattern](#6-visitor-pattern)
7. [Strategy Pattern](#7-strategy-pattern)
8. [Builder Pattern](#8-builder-pattern)

### Code Organization Patterns
9. [Attribute-Based Configuration](#9-attribute-based-configuration)
10. [Options Pattern](#10-options-pattern)
11. [Extension Method Pattern](#11-extension-method-pattern)

---

## 1. Layered Architecture

**Category:** Architectural
**Confidence:** High
**Evidence:** `src/Common/`, `src/Framework/`, `src/Extensions/`, `src/ExternalServices/`

### Intent

Organize the codebase into distinct layers with clear responsibilities and unidirectional dependencies, enabling independent development, testing, and deployment of components.

### Implementation

**Layer Structure:**
```
┌─────────────────────────────────────┐
│   Common (Orchestration Layer)     │  ← All-in-one packages, service wiring
├─────────────────────────────────────┤
│   Framework (Domain Libraries)     │  ← Core business logic, abstractions
├─────────────────────────────────────┤
│   Extensions (System Extensions)   │  ← Custom .NET extensions
├─────────────────────────────────────┤
│  ExternalServices (Integrations)   │  ← Third-party service wrappers
└─────────────────────────────────────┘
```

**Dependency Rule:** Dependencies flow downward only.

### Example

**Common Layer - Orchestration:**
```csharp
// src/Common/OoBDev.Common/OoBDev.Common.csproj
<ItemGroup>
  <!-- Wildcard references to all framework abstractions -->
  <ProjectReference Include="..\..\Framework\**\OoBDev.*.Abstractions.csproj" />
</ItemGroup>
```

**Framework Layer - Domain Logic:**
```csharp
// src/Framework/OoBDev.MessageQueueing/MessageQueueSender.cs
public class MessageQueueSender<TChannel> : IMessageQueueSender<TChannel>
    where TChannel : IMessageChannel
{
    public Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // Core message queue implementation
    }
}
```

**External Services Layer - Integration:**
```csharp
// src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ/RabbitMQMessageSenderProvider.cs
public class RabbitMQMessageSenderProvider : IMessageSenderProvider
{
    // RabbitMQ-specific implementation
}
```

### Benefits

- **Independent Development** - Teams can work on different layers in parallel
- **Testability** - Each layer can be tested in isolation
- **Flexibility** - Swap implementations without affecting other layers
- **Reusability** - Framework layer reused across multiple applications
- **Clear Ownership** - Each layer has clear responsibilities

### Drawbacks

- **Indirection** - More layers means more code to navigate
- **Performance Overhead** - Additional abstraction layers may add latency
- **Learning Curve** - Developers must understand layer boundaries

### Usage Guidelines

**When to Use:**
- ✅ Large codebases with multiple concerns
- ✅ Multiple implementation options for same abstraction
- ✅ Need to swap implementations based on configuration

**When to Avoid:**
- ❌ Simple utilities with single responsibility
- ❌ Prototypes or proof-of-concept code

**Related Patterns:** Provider/Factory Pattern, Dependency Injection Pattern

---

## 2. Provider/Factory Pattern

**Category:** Architectural
**Confidence:** High
**Evidence:** All `*Provider` and `*ProviderFactory` classes throughout framework

### Intent

Provide a consistent abstraction for creating and managing service instances, enabling multiple implementations to be swapped via configuration without code changes.

### Implementation

**Hierarchy:** Abstraction → Provider → Factory

**Three-Tier Structure:**
1. **Abstraction** - Interface for the service
2. **Provider** - Interface for creating instances of the service
3. **Factory** - Creates providers based on configuration keys

### Example

**Step 1: Service Abstraction**
```csharp
// src/Framework/OoBDev.Search.Abstractions/IVectorStore.cs
public interface IVectorStore<T> where T : class
{
    Task<SearchResultModel> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task AddAsync(T item, CancellationToken cancellationToken = default);
}
```

**Step 2: Provider Interface**
```csharp
// src/Framework/OoBDev.Search.Abstractions/IVectorStoreProvider.cs
public interface IVectorStoreProvider
{
    IVectorStore<T> GetVectorStore<T>(string collectionName) where T : class;
}
```

**Step 3: Factory Interface**
```csharp
// src/Framework/OoBDev.Search.Abstractions/IVectorStoreProviderFactory.cs
public interface IVectorStoreProviderFactory
{
    IVectorStoreProvider Create(string providerKey);
}
```

**Step 4: Concrete Provider**
```csharp
// src/ExternalServices/Qdrant/OoBDev.Qdrant/QdrantVectorStoreProvider.cs
public class QdrantVectorStoreProvider : IVectorStoreProvider
{
    private readonly QdrantClient _client;

    public IVectorStore<T> GetVectorStore<T>(string collectionName) where T : class
    {
        return new QdrantVectorStore<T>(_client, collectionName);
    }
}
```

**Step 5: Factory Implementation**
```csharp
// src/Framework/OoBDev.Search/VectorStoreProviderFactory.cs
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

**Step 6: Registration**
```csharp
// Registration in DI container
services.AddKeyedSingleton<IVectorStoreProvider, QdrantProvider>("qdrant");
services.AddKeyedSingleton<IVectorStoreProvider, OpenSearchProvider>("opensearch");
services.AddSingleton<IVectorStoreProviderFactory, VectorStoreProviderFactory>();
```

**Step 7: Usage**
```csharp
public class DocumentSearchService
{
    private readonly IVectorStore<Document> _vectorStore;

    public DocumentSearchService(IVectorStoreProviderFactory factory, IConfiguration configuration)
    {
        var providerKey = configuration["VectorStore:Provider"] ?? "qdrant";
        var provider = factory.Create(providerKey);
        _vectorStore = provider.GetVectorStore<Document>("documents");
    }
}
```

### Benefits

- **Swappable Implementations** - Change provider via configuration
- **Multi-Tenant Support** - Different tenants use different providers
- **Testability** - Easy to mock providers in tests
- **Consistency** - Same pattern across all integrations
- **Type Safety** - Strongly-typed throughout

### Drawbacks

- **Boilerplate** - Requires multiple interfaces and classes
- **Complexity** - More layers of indirection
- **Discovery** - May be harder to navigate for newcomers

### Usage Guidelines

**When to Use:**
- ✅ Multiple implementations of same abstraction (databases, message queues, cloud services)
- ✅ Configuration-driven provider selection
- ✅ Multi-tenant scenarios

**When to Avoid:**
- ❌ Single implementation (use direct dependency injection)
- ❌ Simple utilities with no configuration

**Related Patterns:** Dependency Injection, Strategy Pattern

**Examples in Codebase:**
- `IVectorStoreProvider` / `IVectorStoreProviderFactory` - Vector search
- `IMessageSenderProvider` / `IMessageSenderProviderFactory` - Message queuing
- `IBlobContainerProvider` / `IBlobContainerProviderFactory` - Blob storage
- `ILanguageModelProvider` - LLM providers

---

## 3. Dependency Injection Pattern

**Category:** Architectural
**Confidence:** High
**Evidence:** `src/Common/*/Extensions/*.cs`, all `TryAdd*` extension methods

### Intent

Invert control of dependency creation, enabling loose coupling, testability, and flexibility in component composition.

### Implementation

**Extension Builder Pattern:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddMyExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MyExtensionBuilder>? configure = null)
    {
        // Register services with TryAdd* to avoid conflicts
        services.TryAddSingleton<IMyService, MyService>();
        services.TryAddScoped<IMyFactory, MyFactory>();

        // Configure options
        services.Configure<MyOptions>(configuration.GetSection("MyService"));

        // Optional builder configuration
        var builder = new MyExtensionBuilder(services, configuration);
        configure?.Invoke(builder);

        return services;
    }
}
```

### Example

**Extension Registration:**
```csharp
// src/Common/OoBDev.Common/Extensions/SystemExtensions.cs
public static IServiceCollection TryAddSystemExtensions(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
    services.TryAddSingleton<IGuidProvider, GuidProvider>();
    services.TryAddSingleton<IJsonSerializer, JsonSerializer>();
    services.TryAddSingleton<ITempFileFactory, TempFileFactory>();

    return services;
}
```

**Builder Pattern:**
```csharp
// src/Framework/OoBDev.MessageQueueing/Extensions/MessageQueueExtensionBuilder.cs
public class MessageQueueExtensionBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public MessageQueueExtensionBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    public MessageQueueExtensionBuilder AddRabbitMQ(string key = "rabbitmq")
    {
        _services.AddKeyedSingleton<IMessageSenderProvider, RabbitMQProvider>(key);
        return this;
    }

    public MessageQueueExtensionBuilder AddAzureQueue(string key = "azure-queue")
    {
        _services.AddKeyedSingleton<IMessageSenderProvider, AzureQueueProvider>(key);
        return this;
    }
}
```

**Usage:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register with builder configuration
builder.Services.TryAddMessageQueueing(builder.Configuration, mq =>
{
    mq.AddRabbitMQ("rabbitmq");
    mq.AddAzureQueue("azure-queue");
});

var app = builder.Build();
```

### Benefits

- **Loose Coupling** - Components depend on interfaces, not implementations
- **Testability** - Easy to mock dependencies
- **Flexibility** - Swap implementations via registration
- **Lifetime Management** - Container manages object lifecycles
- **Configuration** - Centralized service configuration

### Drawbacks

- **Complexity** - Requires understanding of DI container
- **Runtime Errors** - Missing registrations fail at runtime
- **Performance** - Small overhead for service resolution

### Usage Guidelines

**When to Use:**
- ✅ All framework services
- ✅ External service integrations
- ✅ Configurable components

**Service Lifetimes:**
- **Singleton** - Shared instance (stateless services, factories)
- **Scoped** - Per-request instance (database contexts)
- **Transient** - New instance each time (lightweight, stateful)

**Registration Methods:**
- `TryAdd*` - Register if not already registered (preferred)
- `Add*` - Always register (throws if duplicate)
- `AddKeyed*` - Register with key for multiple implementations

**Related Patterns:** Provider/Factory Pattern, Options Pattern

---

## 4. Handler Pattern

**Category:** Design
**Confidence:** High
**Evidence:** `IMessageQueueHandler<TChannel, TMessage>`, `IDocumentConversionHandler`

### Intent

Decouple the sender of a request from its receiver by encapsulating processing logic in handler classes, discovered and invoked via attributes or registration.

### Implementation

**Message Queue Handlers:**
```csharp
// Handler interface
public interface IMessageQueueHandler<TChannel, TMessage>
    where TChannel : IMessageChannel
    where TMessage : class
{
    Task HandleAsync(TMessage message, IMessageContext context, CancellationToken cancellationToken = default);
}

// Attribute for discovery
[AttributeUsage(AttributeTargets.Class)]
public class MessageQueueAttribute : Attribute
{
    public string SimpleName { get; set; } = string.Empty;
}
```

### Example

**Defining a Handler:**
```csharp
// src/Examples/OoBDev.WebApi/Handlers/OrderHandler.cs
[MessageQueue(SimpleName = "orders")]
public class OrderHandler : IMessageQueueHandler<OrderChannel, OrderMessage>
{
    private readonly ILogger<OrderHandler> _logger;

    public OrderHandler(ILogger<OrderHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(
        OrderMessage message,
        IMessageContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing order {OrderId}", message.OrderId);

        // Process order
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Order {OrderId} processed", message.OrderId);
    }
}
```

**Handler Discovery and Invocation:**
```csharp
// src/Framework/OoBDev.MessageQueueing.Hosting/MessageReceiverHost.cs
public class MessageReceiverHost : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Discover handlers via reflection
        var handlers = DiscoverHandlers();

        foreach (var handler in handlers)
        {
            var queueName = handler.GetCustomAttribute<MessageQueueAttribute>()?.SimpleName;

            // Subscribe to queue and invoke handler
            await SubscribeAsync(queueName, async (message, context) =>
            {
                await handler.HandleAsync(message, context, stoppingToken);
            }, stoppingToken);
        }
    }
}
```

**Document Conversion Handlers:**
```csharp
// src/Framework/OoBDev.Documents.Abstractions/IDocumentConversionHandler.cs
public interface IDocumentConversionHandler
{
    int Priority { get; }
    bool CanConvert(string sourceFormat, string targetFormat);
    Task<Stream> ConvertAsync(Stream source, string sourceFormat, string targetFormat, CancellationToken cancellationToken = default);
}

// Chain of responsibility for conversion
public class DocumentConversionService
{
    private readonly IEnumerable<IDocumentConversionHandler> _handlers;

    public async Task<Stream> ConvertAsync(Stream source, string sourceFormat, string targetFormat)
    {
        // Find handler with highest priority that can convert
        var handler = _handlers
            .Where(h => h.CanConvert(sourceFormat, targetFormat))
            .OrderByDescending(h => h.Priority)
            .FirstOrDefault();

        if (handler == null)
            throw new NotSupportedException($"No handler for {sourceFormat} to {targetFormat}");

        return await handler.ConvertAsync(source, sourceFormat, targetFormat);
    }
}
```

### Benefits

- **Decoupling** - Sender doesn't know about handler implementation
- **Extensibility** - New handlers added without modifying framework
- **Discoverability** - Attribute-based discovery
- **Type Safety** - Strongly-typed message handling
- **Testability** - Handlers tested in isolation

### Drawbacks

- **Reflection Overhead** - Discovery uses reflection
- **Runtime Discovery** - Handlers found at runtime, not compile time
- **Debugging** - Harder to trace message flow

### Usage Guidelines

**When to Use:**
- ✅ Message queue processing
- ✅ Event handling
- ✅ Command/query handling
- ✅ Document conversion pipeline

**When to Avoid:**
- ❌ Simple method calls
- ❌ Synchronous, in-process operations

**Related Patterns:** Chain of Responsibility, Strategy Pattern

---

## 5. Middleware Pattern

**Category:** Design
**Confidence:** High
**Evidence:** `src/Framework/OoBDev.AspNetCore/Middleware/*.cs`

### Intent

Build a pipeline of processing components that can inspect, modify, or short-circuit HTTP requests and responses.

### Implementation

**Middleware Interface:**
```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;

    public MyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Before logic

        await _next(context); // Call next middleware

        // After logic
    }
}
```

### Example

**CultureInfo Middleware:**
```csharp
// src/Framework/OoBDev.AspNetCore/Middleware/CultureInfoMiddleware.cs
public class CultureInfoMiddleware
{
    private readonly RequestDelegate _next;

    public CultureInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Read Accept-Language header
        var acceptLanguage = context.Request.Headers["Accept-Language"].ToString();

        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            var culture = new CultureInfo(acceptLanguage.Split(',')[0]);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        // Continue pipeline
        await _next(context);

        // Set Content-Language header on response
        context.Response.Headers["Content-Language"] = CultureInfo.CurrentCulture.Name;
    }
}
```

**Registration:**
```csharp
// Extension method for registration
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCultureInfo(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CultureInfoMiddleware>();
    }
}

// Usage in Program.cs
var app = builder.Build();
app.UseCultureInfo();
```

**Correlation ID Middleware:**
```csharp
// src/Framework/OoBDev.AspNetCore/Middleware/CorrelationInfoMiddleware.cs
public class CorrelationInfoMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or read correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Add to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Store in HttpContext for logging
        context.Items["CorrelationId"] = correlationId;

        await _next(context);
    }
}
```

### Benefits

- **Separation of Concerns** - Each middleware has single responsibility
- **Reusability** - Middleware reused across applications
- **Composability** - Build complex pipelines from simple components
- **Testability** - Test middleware in isolation
- **Order Control** - Explicit pipeline ordering

### Drawbacks

- **Order Dependency** - Middleware order matters
- **Performance** - Each middleware adds overhead
- **Complexity** - Long pipelines harder to reason about

### Usage Guidelines

**Common Middleware:**
- Authentication/Authorization
- Logging/Telemetry
- Request/Response transformation
- Error handling
- Compression
- Caching
- CORS

**Ordering Rules:**
1. Exception handling (first)
2. HTTPS redirection
3. Static files
4. Routing
5. CORS
6. Authentication
7. Authorization
8. Custom middleware (CultureInfo, Correlation)
9. Endpoint execution (last)

**Related Patterns:** Chain of Responsibility, Decorator Pattern

---

## 6. Visitor Pattern

**Category:** Design
**Confidence:** Medium
**Evidence:** `src/Framework/OoBDev.System.Linq/ExpressionVisitors/*.cs`

### Intent

Separate an algorithm from the object structure it operates on, allowing new operations to be added without modifying the structures.

### Implementation

Used for expression tree manipulation in LINQ queries.

### Example

**String Comparison Visitor:**
```csharp
// src/Framework/OoBDev.System.Linq/ExpressionVisitors/StringComparisonReplacementExpressionVisitor.cs
public class StringComparisonReplacementExpressionVisitor : ExpressionVisitor
{
    private readonly StringComparison _comparison;

    public StringComparisonReplacementExpressionVisitor(StringComparison comparison)
    {
        _comparison = comparison;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Replace string comparison methods
        if (node.Method.DeclaringType == typeof(string))
        {
            if (node.Method.Name == "Equals" || node.Method.Name == "Contains")
            {
                // Inject StringComparison parameter
                return Expression.Call(
                    node.Object,
                    node.Method.DeclaringType.GetMethod(node.Method.Name, new[] { typeof(string), typeof(StringComparison) })!,
                    node.Arguments[0],
                    Expression.Constant(_comparison));
            }
        }

        return base.VisitMethodCall(node);
    }
}
```

**Null-Safety Visitor:**
```csharp
// src/Framework/OoBDev.System.Linq/ExpressionVisitors/SkipMemberOnNullExpressionVisitor.cs
public class SkipMemberOnNullExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitMember(MemberExpression node)
    {
        // Add null-check before member access
        if (node.Expression != null)
        {
            return Expression.Condition(
                Expression.Equal(node.Expression, Expression.Constant(null)),
                Expression.Default(node.Type),
                node);
        }

        return base.VisitMember(node);
    }
}
```

**Usage:**
```csharp
// Build expression tree
Expression<Func<User, bool>> predicate = u => u.Name.Contains("John");

// Apply visitor to modify expression
var visitor = new StringComparisonReplacementExpressionVisitor(StringComparison.OrdinalIgnoreCase);
var modifiedPredicate = (Expression<Func<User, bool>>)visitor.Visit(predicate);

// Use modified expression
var users = await dbContext.Users.Where(modifiedPredicate).ToListAsync();
```

### Benefits

- **Extensibility** - New operations without modifying expression structures
- **Separation of Concerns** - Visiting logic separate from data structure
- **Type Safety** - Compiler-checked transformations
- **Reusability** - Visitors reused across different expression trees

### Drawbacks

- **Complexity** - Requires understanding of expression trees
- **Verbosity** - More code than direct modification
- **Performance** - Overhead of visitor traversal

### Usage Guidelines

**When to Use:**
- ✅ LINQ expression tree manipulation
- ✅ Complex transformations of structured data
- ✅ Need to add operations without modifying core classes

**When to Avoid:**
- ❌ Simple transformations (use LINQ directly)
- ❌ Performance-critical paths

**Related Patterns:** Strategy Pattern, Interpreter Pattern

---

## 7. Strategy Pattern

**Category:** Design
**Confidence:** High
**Evidence:** `ITemplateProvider`, `IDocumentConversionHandler`, priority-based selection

### Intent

Define a family of algorithms, encapsulate each one, and make them interchangeable via a common interface.

### Implementation

**Template Providers:**
```csharp
// Strategy interface
public interface ITemplateProvider
{
    int Priority { get; }
    bool CanHandle(string fileExtension);
    Task<string> RenderAsync(string template, object model, CancellationToken cancellationToken = default);
}

// Concrete strategy: XSLT
public class XsltTemplateProvider : ITemplateProvider
{
    public int Priority => 10;

    public bool CanHandle(string fileExtension)
    {
        return fileExtension.Equals(".xslt", StringComparison.OrdinalIgnoreCase)
            || fileExtension.Equals(".xsl", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> RenderAsync(string template, object model, CancellationToken cancellationToken = default)
    {
        // XSLT transformation logic
    }
}

// Concrete strategy: Handlebars
public class HandlebarsTemplateProvider : ITemplateProvider
{
    public int Priority => 20;

    public bool CanHandle(string fileExtension)
    {
        return fileExtension.Equals(".hbs", StringComparison.OrdinalIgnoreCase)
            || fileExtension.Equals(".handlebars", StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> RenderAsync(string template, object model, CancellationToken cancellationToken = default)
    {
        // Handlebars rendering logic
    }
}
```

### Example

**Template Engine Context:**
```csharp
// src/Framework/OoBDev.System/Text/Templates/TemplateEngine.cs
public class TemplateEngine : ITemplateEngine
{
    private readonly IEnumerable<ITemplateProvider> _providers;

    public TemplateEngine(IEnumerable<ITemplateProvider> providers)
    {
        _providers = providers;
    }

    public async Task<string> RenderAsync(string templatePath, object model, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(templatePath);

        // Select strategy based on file extension and priority
        var provider = _providers
            .Where(p => p.CanHandle(extension))
            .OrderByDescending(p => p.Priority)
            .FirstOrDefault();

        if (provider == null)
            throw new NotSupportedException($"No template provider found for {extension}");

        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        return await provider.RenderAsync(template, model, cancellationToken);
    }
}
```

**Usage:**
```csharp
// Register all strategies
services.AddSingleton<ITemplateProvider, XsltTemplateProvider>();
services.AddSingleton<ITemplateProvider, HandlebarsTemplateProvider>();
services.AddSingleton<ITemplateEngine, TemplateEngine>();

// Use template engine (strategy selection is automatic)
var templateEngine = serviceProvider.GetRequiredService<ITemplateEngine>();
var result = await templateEngine.RenderAsync("template.hbs", model);
```

### Benefits

- **Flexibility** - Swap algorithms at runtime
- **Extensibility** - Add new strategies without modifying context
- **Encapsulation** - Each algorithm encapsulated in separate class
- **Testability** - Test strategies independently

### Drawbacks

- **Complexity** - More classes than direct implementation
- **Selection Logic** - Need to decide which strategy to use

### Usage Guidelines

**When to Use:**
- ✅ Multiple algorithms for same task
- ✅ Algorithm selection based on input
- ✅ Need to extend algorithms without modifying existing code

**Examples in Codebase:**
- Template rendering (XSLT, Handlebars)
- Document conversion (priority-based handler selection)
- Serialization (JSON, BSON, XML)

**Related Patterns:** Provider/Factory Pattern, Handler Pattern

---

## 8. Builder Pattern

**Category:** Design
**Confidence:** High
**Evidence:** `*ExtensionBuilder` classes, `IExpressionTreeBuilder<T>`

### Intent

Separate the construction of a complex object from its representation, allowing the same construction process to create different representations.

### Implementation

**Extension Builder:**
```csharp
public class MessageQueueExtensionBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public MessageQueueExtensionBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    public MessageQueueExtensionBuilder AddRabbitMQ(string key = "rabbitmq")
    {
        _services.AddKeyedSingleton<IMessageSenderProvider, RabbitMQProvider>(key);
        _services.Configure<RabbitMQOptions>(_configuration.GetSection($"MessageQueue:{key}"));
        return this;
    }

    public MessageQueueExtensionBuilder AddAzureQueue(string key = "azure-queue")
    {
        _services.AddKeyedSingleton<IMessageSenderProvider, AzureQueueProvider>(key);
        _services.Configure<AzureQueueOptions>(_configuration.GetSection($"MessageQueue:{key}"));
        return this;
    }

    public MessageQueueExtensionBuilder AddInProcess(string key = "in-process")
    {
        _services.AddKeyedSingleton<IMessageSenderProvider, InProcessProvider>(key);
        return this;
    }
}
```

### Example

**Usage:**
```csharp
// Fluent API for building message queue configuration
builder.Services.TryAddMessageQueueing(builder.Configuration, mq =>
{
    mq.AddRabbitMQ("rabbitmq")
      .AddAzureQueue("azure-queue")
      .AddInProcess("in-process");
});
```

**Expression Tree Builder:**
```csharp
// src/Framework/OoBDev.System.Linq/ExpressionTreeBuilder.cs
public class ExpressionTreeBuilder<T> : IExpressionTreeBuilder<T>
{
    private Expression? _expression;

    public ExpressionTreeBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _expression = _expression == null
            ? predicate.Body
            : Expression.AndAlso(_expression, predicate.Body);

        return this;
    }

    public ExpressionTreeBuilder<T> OrWhere(Expression<Func<T, bool>> predicate)
    {
        _expression = _expression == null
            ? predicate.Body
            : Expression.OrElse(_expression, predicate.Body);

        return this;
    }

    public Expression<Func<T, bool>> Build()
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        return Expression.Lambda<Func<T, bool>>(_expression!, parameter);
    }
}
```

**Usage:**
```csharp
// Build complex predicate dynamically
var builder = new ExpressionTreeBuilder<User>();

if (!string.IsNullOrEmpty(searchTerm))
    builder.Where(u => u.Name.Contains(searchTerm));

if (isActive)
    builder.Where(u => u.IsActive);

var predicate = builder.Build();
var users = await dbContext.Users.Where(predicate).ToListAsync();
```

### Benefits

- **Fluent API** - Readable, chainable configuration
- **Flexibility** - Different configurations from same builder
- **Validation** - Validate configuration before building
- **Immutability** - Build immutable objects step by step

### Drawbacks

- **Verbosity** - More code than direct construction
- **Overhead** - Additional builder class

### Usage Guidelines

**When to Use:**
- ✅ Complex object construction
- ✅ Many optional parameters
- ✅ Step-by-step configuration
- ✅ Fluent API desired

**Examples in Codebase:**
- Extension builders (MessageQueueExtensionBuilder, AspNetCoreExtensionBuilder)
- Expression tree builders (ExpressionTreeBuilder<T>)
- Query builders

**Related Patterns:** Fluent Interface, Factory Pattern

---

## 9. Attribute-Based Configuration

**Category:** Code Organization
**Confidence:** High
**Evidence:** `[MessageQueue]`, `[BlobContainer]`, `[ApplicationRight]`, etc.

### Intent

Use attributes to declaratively configure classes, enabling discovery and configuration without explicit registration.

### Implementation

**Attribute Definition:**
```csharp
[AttributeUsage(AttributeTargets.Class)]
public class MessageQueueAttribute : Attribute
{
    public string SimpleName { get; set; } = string.Empty;
    public int MaxConcurrentHandlers { get; set; } = 10;
}

[AttributeUsage(AttributeTargets.Class)]
public class BlobContainerAttribute : Attribute
{
    public string ContainerName { get; set; } = string.Empty;
}

[AttributeUsage(AttributeTargets.Method)]
public class ApplicationRightAttribute : Attribute
{
    public string Rights { get; set; } = string.Empty;
}
```

### Example

**Message Queue Handler:**
```csharp
[MessageQueue(SimpleName = "orders", MaxConcurrentHandlers = 5)]
public class OrderHandler : IMessageQueueHandler<OrderChannel, OrderMessage>
{
    public async Task HandleAsync(OrderMessage message, IMessageContext context, CancellationToken cancellationToken = default)
    {
        // Handle order
    }
}
```

**Blob Container:**
```csharp
[BlobContainer(ContainerName = "documents")]
public class DocumentModel
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
```

**Application Rights:**
```csharp
[HttpGet("users")]
[ApplicationRight(Rights = "admin.users.read")]
public async Task<IActionResult> GetUsers()
{
    // Only users with "admin.users.read" right can access
}
```

**Attribute Discovery:**
```csharp
// Discover handlers via reflection
var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .Where(t => t.GetCustomAttribute<MessageQueueAttribute>() != null);

foreach (var handlerType in handlerTypes)
{
    var attribute = handlerType.GetCustomAttribute<MessageQueueAttribute>();
    var queueName = attribute!.SimpleName;
    var maxConcurrent = attribute.MaxConcurrentHandlers;

    // Register handler
}
```

### Benefits

- **Declarative** - Configuration at point of definition
- **Discoverability** - Automatic discovery via reflection
- **Type Safety** - Attribute properties are typed
- **Centralized Metadata** - Configuration lives with the class

### Drawbacks

- **Reflection Overhead** - Discovery uses reflection
- **Runtime Configuration** - Not compile-time validated
- **Hidden Behavior** - Behavior configured by attributes may not be obvious

### Usage Guidelines

**When to Use:**
- ✅ Handler discovery
- ✅ Container/queue naming
- ✅ Authorization requirements
- ✅ Validation rules

**When to Avoid:**
- ❌ Complex configuration (use options pattern)
- ❌ Dynamic configuration (use configuration files)

**Examples in Codebase:**
- `[MessageQueue(SimpleName = "...")]`
- `[BlobContainer(ContainerName = "...")]`
- `[VectorStore(CollectionName = "...")]`
- `[ApplicationRight(Rights = "...")]`

**Related Patterns:** Handler Pattern, Options Pattern

---

## 10. Options Pattern

**Category:** Code Organization
**Confidence:** High
**Evidence:** All `*Options` classes, `IOptions<T>` usage throughout

### Intent

Provide strongly-typed configuration objects that are validated and injected via dependency injection.

### Implementation

**Options Class:**
```csharp
public class RabbitMQOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? VirtualHost { get; set; }
    public bool AutomaticRecoveryEnabled { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrEmpty(HostName))
            throw new InvalidOperationException("HostName is required");

        if (Port < 1 || Port > 65535)
            throw new InvalidOperationException("Port must be between 1 and 65535");
    }
}
```

### Example

**Configuration File:**
```json
{
  "RabbitMQ": {
    "HostName": "rabbitmq.example.com",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "AutomaticRecoveryEnabled": true
  }
}
```

**Registration:**
```csharp
// Register options
services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));

// Optional: Add validation
services.AddOptions<RabbitMQOptions>()
    .Bind(configuration.GetSection("RabbitMQ"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

**Usage:**
```csharp
public class RabbitMQProvider
{
    private readonly RabbitMQOptions _options;

    public RabbitMQProvider(IOptions<RabbitMQOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _options.Validate(); // Fail fast on invalid configuration
    }
}
```

**IOptionsSnapshot for Reload:**
```csharp
// Use IOptionsSnapshot for configuration that can be reloaded
public class MyService
{
    private readonly IOptionsSnapshot<MyOptions> _options;

    public MyService(IOptionsSnapshot<MyOptions> options)
    {
        _options = options;
    }

    public void DoWork()
    {
        // Gets current options (supports configuration reload)
        var currentOptions = _options.Value;
    }
}
```

### Benefits

- **Type Safety** - Compile-time checked configuration
- **IntelliSense** - IDE support for configuration
- **Validation** - Validate configuration on startup
- **Testability** - Easy to provide test configuration
- **Reloadability** - Support for configuration reload (with IOptionsSnapshot)

### Drawbacks

- **Boilerplate** - Options class for each configuration section
- **Complexity** - More indirection than direct configuration access

### Usage Guidelines

**When to Use:**
- ✅ All configuration (strongly preferred)
- ✅ Complex configuration structures
- ✅ Configuration validation required

**Options Interface:**
- `IOptions<T>` - Singleton, no reload support
- `IOptionsSnapshot<T>` - Scoped, supports reload
- `IOptionsMonitor<T>` - Singleton, supports reload + change notifications

**Examples in Codebase:**
- `RabbitMQOptions`
- `AzureBlobProviderOptions`
- `MailKitSmtpClientOptions`
- `QdrantOptions`
- All `*Options` classes

**Related Patterns:** Dependency Injection, Configuration Pattern

---

## 11. Extension Method Pattern

**Category:** Code Organization
**Confidence:** High
**Evidence:** All `TryAdd*` extension methods, middleware extensions

### Intent

Add methods to existing types without modifying them or creating derived types.

### Implementation

**Extension Method Definition:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSystemExtensions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.TryAddSingleton<IGuidProvider, GuidProvider>();
        return services;
    }
}
```

### Example

**Middleware Extensions:**
```csharp
// src/Framework/OoBDev.AspNetCore/Extensions/MiddlewareExtensions.cs
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCultureInfo(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CultureInfoMiddleware>();
    }

    public static IApplicationBuilder UseCorrelationInfo(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationInfoMiddleware>();
    }

    public static IApplicationBuilder UseSearchQuery(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SearchQueryMiddleware>();
    }
}
```

**LINQ Extensions:**
```csharp
// src/Framework/OoBDev.System.Linq/Extensions/AsyncEnumerableExtensions.cs
public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }
        return list;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            return item;
        }
        return default;
    }
}
```

**Usage:**
```csharp
// Service registration
builder.Services.TryAddSystemExtensions(builder.Configuration);
builder.Services.TryAddMessageQueueing(builder.Configuration);

// Middleware
app.UseCultureInfo();
app.UseCorrelationInfo();

// LINQ
var list = await asyncEnumerable.ToListAsync();
var first = await asyncEnumerable.FirstOrDefaultAsync();
```

### Benefits

- **Discoverability** - IntelliSense shows extension methods
- **Fluent API** - Chainable method calls
- **Non-Invasive** - Don't modify original types
- **Namespace Organization** - Group related extensions

### Drawbacks

- **Namespace Required** - Must import namespace to use extensions
- **No Override** - Can't override instance methods
- **Static** - Can't use polymorphism

### Usage Guidelines

**When to Use:**
- ✅ DI registration (`TryAdd*` methods)
- ✅ Middleware registration (`Use*` methods)
- ✅ LINQ-style operations
- ✅ Utility methods on common types

**Naming Conventions:**
- `TryAdd*` - DI registration (non-replacing)
- `Use*` - Middleware registration
- `To*` - Conversion methods
- `*Async` - Async operations

**Examples in Codebase:**
- `ServiceCollectionExtensions` (all `TryAdd*` methods)
- `MiddlewareExtensions` (all `Use*` methods)
- `AsyncEnumerableExtensions` (LINQ extensions)
- `DictionaryExtensions` (dictionary helpers)

**Related Patterns:** Fluent Interface, Builder Pattern

---

## Summary

The OoBDev framework leverages a consistent set of architectural and design patterns:

**Core Architectural Patterns:**
- **Layered Architecture** - Clear separation of concerns
- **Provider/Factory** - Swappable implementations
- **Dependency Injection** - Loose coupling and testability

**Key Design Patterns:**
- **Handler** - Message processing and document conversion
- **Middleware** - HTTP request pipeline
- **Visitor** - Expression tree manipulation
- **Strategy** - Algorithm selection
- **Builder** - Fluent configuration

**Code Organization:**
- **Attribute-Based Configuration** - Declarative metadata
- **Options Pattern** - Strongly-typed configuration
- **Extension Methods** - Fluent API and discoverability

These patterns work together to create a consistent, extensible, and maintainable framework.

---

## Related Documentation

- [architectural-guidelines.md](./architectural-guidelines.md) - High-level principles
- [architectural-standards.md](./architectural-standards.md) - Concrete standards
- [layering-architecture.md](./layering-architecture.md) - Layer details
- [provider-factory-pattern.md](./provider-factory-pattern.md) - Detailed provider pattern guide

---

## Change Log

- 2026-01-12 v1.0: Initial architectural patterns documented
