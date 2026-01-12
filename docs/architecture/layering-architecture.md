# Layering Architecture

**Version:** 1.0
**Last Updated:** 2026-01-12
**Project:** OoBDev (dotex) - .NET Extensions Framework

---

## Overview

The OoBDev framework is organized into four distinct layers, each with specific responsibilities and dependencies. This document provides detailed information about each layer, including purpose, contents, dependencies, and guidelines.

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                 Common Layer                            │
│  (Orchestration, All-in-One Packages)                   │
│  - OoBDev.Common.Abstractions                           │
│  - OoBDev.Common                                        │
│  - OoBDev.Common.AspNetCore                             │
│  - OoBDev.Common.Complete                               │
│  - OoBDev.Common.Extensions                             │
│  - OoBDev.Common.Hosting                                │
└─────────────────────────────────────────────────────────┘
                          ↓ depends on
┌─────────────────────────────────────────────────────────┐
│                Framework Layer                          │
│  (Domain-Specific Libraries, Core Business Logic)       │
│  - AI, AspNetCore, Communications                       │
│  - Database, Documents, Identity                        │
│  - MessageQueueing, Search, System                      │
│  - TestUtilities                                        │
└─────────────────────────────────────────────────────────┘
                          ↓ depends on
┌─────────────────────────────────────────────────────────┐
│               Extensions Layer                          │
│  (Custom .NET System Extensions)                        │
│  - OoBDev.Data.Vectors                                  │
│  - OoBDev.System.Text.Html                              │
│  - OoBDev.System.Text.Markdown                          │
│  - OoBDev.System.Text.Yaml                              │
└─────────────────────────────────────────────────────────┘
                          ↓ depends on
┌─────────────────────────────────────────────────────────┐
│            ExternalServices Layer                       │
│  (Third-Party Integrations, Vendor Wrappers)            │
│  - Apache, Azure, GroqCloud, Handlebars                 │
│  - Keycloak, MailKit, Markdig, Microsoft                │
│  - MongoDB, Ollama, OpenSearch, Qdrant                  │
│  - RabbitMQ, SBert, WkHtmlToPdf                         │
└─────────────────────────────────────────────────────────┘
```

**Dependency Rule:** Dependencies flow downward only. Lower layers cannot depend on higher layers.

---

## 1. Common Layer

### 1.1 Purpose

The Common layer provides orchestration and all-in-one packages that aggregate framework components for easy consumption. It serves as the entry point for applications using the OoBDev framework.

### 1.2 Responsibilities

- **Service Orchestration** - Wire up all framework services
- **Dependency Aggregation** - Collect abstractions and implementations
- **Extension Registration** - Provide TryAdd* methods for all framework services
- **Configuration Integration** - Bind configuration to options
- **Convenience Packages** - All-in-one packages for common scenarios

### 1.3 Projects

| Project | Purpose | Key Features |
|---------|---------|--------------|
| **OoBDev.Common.Abstractions** | All framework abstractions in one package | Aggregates all `*.Abstractions` projects |
| **OoBDev.Common** | Core framework services orchestrator | System extensions, serialization, LINQ |
| **OoBDev.Common.AspNetCore** | ASP.NET Core integrations | Middleware, filters, Swagger config |
| **OoBDev.Common.Complete** | All-in-one framework package | Everything in one NuGet package |
| **OoBDev.Common.Extensions** | External service wiring | Extension methods for RabbitMQ, MongoDB, etc. |
| **OoBDev.Common.Hosting** | Background service hosting | Message queue receivers, scheduled tasks |

### 1.4 Wildcard Project References

**Unique Feature:** Common projects use wildcard project references to automatically include all framework projects.

**Example:**
```xml
<!-- src/Common/OoBDev.Common.Abstractions/OoBDev.Common.Abstractions.csproj -->
<ItemGroup>
  <!-- Automatically includes all framework abstractions -->
  <ProjectReference Include="..\..\Framework\**\OoBDev.*.Abstractions.csproj" />
</ItemGroup>
```

**Benefits:**
- Automatic inclusion of new framework projects
- Single NuGet package for all abstractions
- No need to manually update references

**Reference:** `src/Common/OoBDev.Common.Abstractions/OoBDev.Common.Abstractions.csproj`

### 1.5 Dependencies

**Allowed:**
- ✅ Framework projects (all)
- ✅ Extensions projects (all)
- ✅ ExternalServices projects (all)
- ✅ Third-party NuGet packages

**Not Allowed:**
- ❌ Examples projects
- ❌ Tools projects
- ❌ Other Common projects (circular dependency)

### 1.6 Usage Guidelines

**When to Use Common:**
- ✅ Building an application that uses multiple framework features
- ✅ Want all-in-one package for convenience
- ✅ Need easy service registration

**When to Use Framework Directly:**
- ✅ Only need specific features (e.g., just message queueing)
- ✅ Want to minimize dependencies
- ✅ Building a library that extends OoBDev

**Example:**
```csharp
// Using Common for all-in-one registration
var builder = WebApplication.CreateBuilder(args);

builder.Services.TryAddSystemExtensions(builder.Configuration);
builder.Services.TryAddMessageQueueing(builder.Configuration);
builder.Services.TryAddDocumentServices(builder.Configuration);

var app = builder.Build();
```

---

## 2. Framework Layer

### 2.1 Purpose

The Framework layer contains domain-specific libraries that provide core business logic and abstractions. This is the heart of the OoBDev framework.

### 2.2 Responsibilities

- **Domain Logic** - Core business functionality
- **Abstractions** - Interfaces and models
- **Implementation** - Concrete implementations of abstractions
- **Patterns** - Provider/factory implementations
- **Testing** - Comprehensive test coverage

### 2.3 Organization

Framework projects are organized by domain:

#### AI/ML (2 projects)
- **OoBDev.AI.Abstractions** - LLM and embedding interfaces
- **OoBDev.SemanticKernel.** - Microsoft Semantic Kernel integration

**Key Interfaces:**
- `ILanguageModelProvider` - LLM abstraction
- `IEmbeddingProvider` - Embedding generation
- `IMessageCompletion` - Completion requests

**Reference:** `src/Framework/OoBDev.AI.Abstractions/`

#### ASP.NET Core (4 projects)
- **OoBDev.AspNetCore.Abstractions** - Web framework abstractions
- **OoBDev.AspNetCore.Mvc** - Query support, Swagger extensions
- **OoBDev.AspNetCore.JwtAuthentication** - JWT/OAuth2
- **OoBDev.AspNetCore.Tests** - ASP.NET Core tests

**Key Features:**
- Search query middleware (`ISearchQuery`, `IFilterQuery`, `ISortQuery`, `IPageQuery`)
- Swagger/OpenAPI integration
- JWT authentication
- Culture info middleware
- Correlation ID middleware

**Reference:** `src/Framework/OoBDev.AspNetCore/`

#### Communications (3 projects)
- **OoBDev.Communications.Abstractions** - Email/SMS models
- **OoBDev.Communications** - Core implementation
- **OoBDev.Communications.MessageQueueing** - Message queue integration

**Key Interfaces:**
- `ICommunicationSender<T>` - Send communication
- `EmailMessageModel` - Email message
- `SmsMessageModel` - SMS message

**Reference:** `src/Framework/OoBDev.Communications.Abstractions/`

#### Database (6 projects)
- **OoBDev.DacFx.** - SQL Server deployment (2)
- **OoBDev.Data.Common.** - Common data abstractions (2)
- SQL CLR support (2)

**Key Features:**
- DacFx integration for SQL Server deployment
- Vector data types for SQL CLR
- Common data abstractions

**Reference:** `src/Framework/OoBDev.DacFx.*/`

#### Documents (3 projects)
- **OoBDev.Documents.Abstractions** - Blob storage, conversion
- **OoBDev.Documents** - Implementation
- **OoBDev.Documents.Tests** - Document tests

**Key Interfaces:**
- `IBlobContainer<T>` - Blob storage abstraction
- `IBlobContainerProvider` - Provider for blob containers
- `IDocumentConversion` - Document conversion
- `IDocumentConversionHandler` - Conversion handler
- `IContentTypeDetector` - Content type detection

**Reference:** `src/Framework/OoBDev.Documents.Abstractions/`

#### Identity (3 projects)
- **OoBDev.Identity.Abstractions** - Identity abstractions
- **OoBDev.Identity** - Implementation
- **OoBDev.Identity.Tests** - Identity tests

**Key Interfaces:**
- `IIdentityManager` - Identity management
- `IUserManagementProvider` - User management
- `UserIdentityModel` - User identity model

**Reference:** `src/Framework/OoBDev.Identity.Abstractions/`

#### Message Queueing (4 projects)
- **OoBDev.MessageQueueing.Abstractions** - Message queue abstractions
- **OoBDev.MessageQueueing** - Core implementation
- **OoBDev.MessageQueueing.Hosting** - Background processing
- **OoBDev.MessageQueueing.Tests** - Message queue tests

**Key Interfaces:**
- `IMessageQueueHandler<TChannel, TMessage>` - Message handler
- `IMessageQueueSender<TChannel>` - Send messages
- `IMessageSenderProvider` - Provider for senders
- `IMessageReceiverProvider` - Provider for receivers

**Key Features:**
- Type-safe message handling
- Attribute-based handler discovery: `[MessageQueue(SimpleName = "...")]`
- Background hosted service (MessageReceiverHost)
- Multiple provider support (RabbitMQ, Azure Queue, In-Process)
- Correlation ID tracking

**Coverage:** 90.4% (excellent)

**Reference:** `src/Framework/OoBDev.MessageQueueing/`, `/docs/Framework/MessageQueueing.md`

#### Search (3 projects)
- **OoBDev.Search.Abstractions** - Vector store, search
- **OoBDev.Search** - Implementation
- **OoBDev.Search.Tests** - Search tests

**Key Interfaces:**
- `IVectorStore<T>` - Vector storage
- `IVectorStoreProvider` - Provider for vector stores
- `SearchResultModel` - Search results
- `SearchTypes` - Search type enumeration (Semantic, Lexical, Hybrid)

**Reference:** `src/Framework/OoBDev.Search.Abstractions/`

#### System (11+ projects)
- **OoBDev.System.** - Core utilities (6)
- **OoBDev.System.IO.** - Pipelines, Ports, USB HID (3)
- **OoBDev.System.Linq** - LINQ/query building
- **OoBDev.Extensions** - General extensions

**Key Features:**
- Configuration (command-line provider, hierarchical)
- Serialization (JSON, BSON, XML)
- LINQ extensions (async enumerable, expression building)
- I/O (streams, temp files, pipelines, USB HID)
- Reflection (embedded resources, type resolution)
- Text templating (XSLT, Handlebars)

**Coverage:**
- OoBDev.System.Linq: 90.5% (highest in framework)
- OoBDev.System: 71%

**Reference:** `src/Framework/OoBDev.System/`, `src/Framework/OoBDev.System.Linq/`

#### Test Utilities (2 projects)
- **OoBDev.TestUtilities** - Test helpers
- **OoBDev.TestUtilities.Tests** - Test utility tests

**Key Features:**
- TestContextExtensions - Attachment support
- TestLogger - Test logging
- Result capture utilities

**Coverage:** 78.5%

**Reference:** `src/Framework/OoBDev.TestUtilities/`

### 2.4 Project Structure Pattern

All framework projects follow this pattern:

```
OoBDev.{Domain}/
├── OoBDev.{Domain}.Abstractions/      ← Interfaces, models, enums
│   ├── I{Service}.cs                  ← Service interfaces
│   ├── I{Service}Provider.cs          ← Provider interfaces
│   ├── I{Service}ProviderFactory.cs   ← Factory interfaces
│   └── {Model}.cs                     ← Domain models
├── OoBDev.{Domain}/                   ← Implementation
│   ├── {Service}.cs                   ← Service implementations
│   ├── {Provider}.cs                  ← Provider implementations
│   ├── {ProviderFactory}.cs           ← Factory implementations
│   └── Extensions/                    ← DI registration extensions
│       └── ServiceCollectionExtensions.cs
├── OoBDev.{Domain}.Hosting/           ← Background services (optional)
│   └── {Service}Host.cs               ← BackgroundService implementation
└── OoBDev.{Domain}.Tests/             ← Tests
    ├── {Service}Tests.cs              ← Unit tests
    └── Integration/                   ← Integration tests (optional)
```

### 2.5 Dependencies

**Allowed:**
- ✅ Other Framework projects (abstractions preferred)
- ✅ Extensions projects
- ✅ ExternalServices projects (abstractions only)
- ✅ Third-party NuGet packages

**Not Allowed:**
- ❌ Common projects (circular dependency)
- ❌ Examples projects
- ❌ Tools projects

### 2.6 Usage Guidelines

**When to Add Framework Project:**
- ✅ New domain-specific functionality
- ✅ Core business logic (not vendor-specific)
- ✅ Multiple potential implementations
- ✅ Abstractions used by multiple applications

**When to Add ExternalServices Instead:**
- ✅ Vendor-specific implementation
- ✅ Third-party service wrapper
- ✅ Single implementation

---

## 3. Extensions Layer

### 3.1 Purpose

The Extensions layer provides custom .NET system extensions that enhance base .NET capabilities without external dependencies.

### 3.2 Responsibilities

- **System Extensions** - Extend base .NET types and functionality
- **Custom Data Types** - Vector operations, custom collections
- **Text Processing** - HTML, Markdown, YAML
- **No External Services** - Pure .NET extensions (may depend on NuGet packages)

### 3.3 Projects

#### Data & Vectors (5 projects)
- **OoBDev.Data.Vectors** - Vector operations
- **OoBDev.Data.Vectors.DB** - Database integration
- **OoBDev.Data.Vectors.Hosting** - Hosting support
- **OoBDev.Data.Vectors.Net481** - SQL CLR support (.NET 4.8.1)
- **OoBDev.Data.Vectors.Tests** - Vector tests

**Key Features:**
- Vector data type
- Distance calculations (Euclidean, Cosine, Dot Product)
- SQL CLR functions for SQL Server
- Vector normalization and operations

**Reference:** `src/Extensions/OoBDev.Data.Vectors/`, `/docs/Framework/VectorSqlClr.md`

#### Text Processing (6 projects)
- **OoBDev.System.Text.Html.** (2) - HTML processing
- **OoBDev.System.Text.Markdown.** (2) - Markdown processing
- **OoBDev.System.Text.Yaml.** (2) - YAML processing

**Key Features:**
- HTML parsing and manipulation
- Markdown to HTML conversion
- YAML serialization/deserialization

**Reference:** `src/Extensions/OoBDev.System.Text.*/`

### 3.4 Dependencies

**Allowed:**
- ✅ ExternalServices projects (for implementations)
- ✅ Third-party NuGet packages
- ✅ .NET BCL

**Not Allowed:**
- ❌ Common projects
- ❌ Framework projects
- ❌ Other Extensions projects (minimize coupling)

### 3.5 Usage Guidelines

**When to Add Extension Project:**
- ✅ Enhancing base .NET functionality
- ✅ Custom data types (vectors, spatial)
- ✅ Text processing (HTML, Markdown, YAML)
- ✅ No external service dependencies

**When to Add Framework Instead:**
- ✅ Domain-specific business logic
- ✅ Multiple provider implementations
- ✅ External service abstractions

---

## 4. ExternalServices Layer

### 4.1 Purpose

The ExternalServices layer contains vendor-specific implementations and third-party service wrappers. Each integration is isolated in its own project pair (Abstractions + Implementation).

### 4.2 Responsibilities

- **Vendor Integration** - Wrap third-party services
- **API Abstraction** - Hide vendor-specific details
- **Configuration** - Vendor-specific options
- **Error Handling** - Convert vendor errors to framework exceptions
- **No Business Logic** - Pure integration code

### 4.3 Organization

Projects are organized by vendor/technology:

#### Apache (2 projects)
- **OoBDev.Apache.Tika.Abstractions**
- **OoBDev.Apache.Tika**

**Purpose:** Content type detection and document parsing

**Reference:** `src/ExternalServices/Apache/OoBDev.Apache.Tika/`

#### Azure (2 projects)
- **OoBDev.Azure.StorageAccount.Abstractions**
- **OoBDev.Azure.StorageAccount**

**Purpose:** Azure Blob Storage and Queue Storage

**Reference:** `src/ExternalServices/Azure/OoBDev.Azure.StorageAccount/`

#### GroqCloud (2 projects)
- **OoBDev.GroqCloud.Abstractions**
- **OoBDev.GroqCloud**

**Purpose:** Cloud LLM provider

**Reference:** `src/ExternalServices/GroqCloud/OoBDev.GroqCloud/`

#### Handlebars (2 projects)
- **OoBDev.Handlebars.Abstractions**
- **OoBDev.Handlebars**

**Purpose:** Handlebars.Net template engine

**Coverage:** 85%

**Reference:** `src/ExternalServices/Handlebars/OoBDev.Handlebars/`

#### HtmlToOpenXml (2 projects)
- **OoBDev.HtmlToOpenXml.Abstractions**
- **OoBDev.HtmlToOpenXml**

**Purpose:** HTML to DOCX conversion

**Reference:** `src/ExternalServices/HtmlToOpenXml/OoBDev.HtmlToOpenXml/`

#### Keycloak (2 projects)
- **OoBDev.Keycloak.Abstractions**
- **OoBDev.Keycloak**

**Purpose:** Keycloak identity provider

**Reference:** `src/ExternalServices/Keycloak/OoBDev.Keycloak/`

#### MailKit (3 projects)
- **OoBDev.MailKit.Abstractions**
- **OoBDev.MailKit**
- **OoBDev.MailKit.Tests** (optional test project)

**Purpose:** SMTP/IMAP email integration

**Reference:** `src/ExternalServices/MailKit/OoBDev.MailKit/`

#### Markdig (2 projects)
- **OoBDev.Markdig.Abstractions**
- **OoBDev.Markdig**

**Purpose:** Markdown to HTML conversion

**Reference:** `src/ExternalServices/Markdig/OoBDev.Markdig/`

#### Microsoft (8 projects)
- **OoBDev.Microsoft.ApplicationInsights.** (2)
- **OoBDev.Microsoft.B2C.** (2)
- **OoBDev.Microsoft.SqlServer.DacFx.** (2)
- **OoBDev.Microsoft.SqlServer.Server.** (2)

**Purpose:**
- Application Insights (telemetry)
- Azure AD B2C (identity)
- SQL Server DacFx (deployment)
- SQL Server CLR (server-side code)

**Reference:** `src/ExternalServices/Microsoft/*/`

#### MongoDB (2 projects)
- **OoBDev.MongoDB.Abstractions**
- **OoBDev.MongoDB**

**Purpose:** MongoDB database integration

**Reference:** `src/ExternalServices/MongoDB/OoBDev.MongoDB/`, `/docs/Framework/MongoDbExtensions.md`

#### MysticMind (2 projects)
- **OoBDev.MysticMind.Abstractions**
- **OoBDev.MysticMind**

**Purpose:** HTML to Markdown conversion

**Reference:** `src/ExternalServices/MysticMind/OoBDev.MysticMind/`

#### Ollama (2 projects)
- **OoBDev.Ollama.Abstractions**
- **OoBDev.Ollama**

**Purpose:** Local LLM provider

**Reference:** `src/ExternalServices/Ollama/OoBDev.Ollama/`

#### OpenSearch (2 projects)
- **OoBDev.OpenSearch.Abstractions**
- **OoBDev.OpenSearch**

**Purpose:** OpenSearch search engine

**Reference:** `src/ExternalServices/OpenSearch/OoBDev.OpenSearch/`

#### Qdrant (2 projects)
- **OoBDev.Qdrant.Abstractions**
- **OoBDev.Qdrant**

**Purpose:** Qdrant vector database

**Reference:** `src/ExternalServices/Qdrant/OoBDev.Qdrant/`

#### RabbitMQ (2 projects)
- **OoBDev.RabbitMQ.Abstractions**
- **OoBDev.RabbitMQ**

**Purpose:** RabbitMQ message broker

**Reference:** `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ/`

#### SBert (3 projects)
- **OoBDev.SBert.Abstractions**
- **OoBDev.SBert**
- **OoBDev.SBert.Tests** (optional)

**Purpose:** Sentence-BERT embeddings

**Reference:** `src/ExternalServices/SBert/OoBDev.SBert/`

#### WkHtmlToPdf (2 projects)
- **OoBDev.WkHtmlToPdf.Abstractions**
- **OoBDev.WkHtmlToPdf**

**Purpose:** HTML to PDF conversion

**Reference:** `src/ExternalServices/WkHtmlToPdf/OoBDev.WkHtmlToPdf/`

### 4.4 Project Structure Pattern

All ExternalServices projects follow this pattern:

```
OoBDev.{Vendor}/
├── OoBDev.{Vendor}.Abstractions/
│   ├── I{Service}.cs              ← Service interface
│   ├── I{Service}Provider.cs      ← Provider interface
│   └── {Vendor}Options.cs         ← Configuration options
└── OoBDev.{Vendor}/
    ├── {Service}.cs               ← Service implementation
    ├── {Service}Provider.cs       ← Provider implementation
    ├── Extensions/
    │   └── ServiceCollectionExtensions.cs
    └── README.md                  ← Integration documentation
```

### 4.5 Dependencies

**Allowed:**
- ✅ Third-party vendor NuGet packages
- ✅ Framework abstractions (implement interfaces)
- ✅ .NET BCL

**Not Allowed:**
- ❌ Common projects
- ❌ Framework implementation projects
- ❌ Extensions projects
- ❌ Other ExternalServices projects (minimize coupling)

### 4.6 Usage Guidelines

**When to Add ExternalServices Project:**
- ✅ Integrating a third-party service
- ✅ Vendor-specific implementation
- ✅ Single implementation (not multiple providers)
- ✅ Wrapping external API

**Naming Convention:**
- Use vendor name: `OoBDev.{Vendor}.{Subcategory}`
- Examples: `OoBDev.RabbitMQ`, `OoBDev.Azure.StorageAccount`, `OoBDev.Microsoft.B2C`

**Integration Checklist:**
- [ ] Create `OoBDev.{Vendor}.Abstractions` project
- [ ] Create `OoBDev.{Vendor}` project
- [ ] Implement framework interfaces (IProvider, IProviderFactory, etc.)
- [ ] Create `{Vendor}Options` configuration class
- [ ] Create `TryAdd{Vendor}Services` extension method
- [ ] Add README with usage examples
- [ ] Add to `/docs/Libraries/{vendor}.md`
- [ ] Create test project (optional but recommended)

---

## 5. Supporting Layers

### 5.1 Examples Layer

**Purpose:** Sample applications and usage examples

**Projects:**
- **OoBDev.Example.DB** - Database example
- **OoBDev.Examples.Tests** - Example tests
- **OoBDev.WebApi** - Full ASP.NET Core Web API example

**Dependencies:** Can depend on any layer

**Reference:** `src/Examples/`

### 5.2 Tools Layer

**Purpose:** Command-line utilities and build tools

**Projects:**
- **OoBDev.DacPacCompiler.Cli** - Database deployment
- **OoBDev.DocumentConverter.Cli** - Batch conversion
- **OoBDev.FileRagEngine.Cli** - RAG indexing
- **OoBDev.FixSourceLinks.Cli** - Source link correction
- **OoBDev.TemplateEngine.Cli** - Template processing
- **OoBDev.MigrationHelper.Cli** - Namespace migration

**Dependencies:** Can depend on any layer

**Reference:** `src/Tools/`

---

## 6. Layer Interaction Patterns

### 6.1 Application → Common → Framework

**Most Common Pattern:**
```csharp
// Application code
var builder = WebApplication.CreateBuilder(args);

// Common layer provides easy registration
builder.Services.TryAddSystemExtensions(builder.Configuration);
builder.Services.TryAddMessageQueueing(builder.Configuration, mq =>
{
    // Common.Extensions provides external service wiring
    mq.AddRabbitMQ("rabbitmq");
});

// Framework interfaces are used in application code
public class OrderService
{
    private readonly IMessageQueueSender<OrderChannel> _sender;

    public OrderService(IMessageQueueSender<OrderChannel> sender)
    {
        _sender = sender; // Framework abstraction
    }
}
```

### 6.2 Framework → ExternalServices

**Provider Pattern:**
```csharp
// Framework defines abstraction
public interface IMessageSenderProvider
{
    IMessageQueueSender<TChannel> GetSender<TChannel>() where TChannel : IMessageChannel;
}

// ExternalServices implements abstraction
public class RabbitMQMessageSenderProvider : IMessageSenderProvider
{
    // RabbitMQ-specific implementation
}

// Framework factory uses external service provider
public class MessageSenderProviderFactory : IMessageSenderProviderFactory
{
    public IMessageSenderProvider Create(string providerKey)
    {
        // Resolves to RabbitMQMessageSenderProvider or other implementations
        return _serviceProvider.GetRequiredKeyedService<IMessageSenderProvider>(providerKey);
    }
}
```

### 6.3 Framework → Extensions

**Composition:**
```csharp
// Extensions provide custom data types
public class Vector
{
    public float[] Values { get; set; }
    public float CosineSimilarity(Vector other) { }
}

// Framework uses extension types
public interface IVectorStore<T> where T : class
{
    Task AddAsync(T item, Vector embedding);
    Task<SearchResultModel> SearchAsync(Vector queryEmbedding);
}
```

---

## 7. Dependency Validation

### 7.1 Valid Dependencies

```
Common:
  ✅ → Framework
  ✅ → Extensions
  ✅ → ExternalServices

Framework:
  ✅ → Framework (other projects)
  ✅ → Extensions
  ✅ → ExternalServices (abstractions only)
  ❌ → Common (circular dependency)

Extensions:
  ✅ → ExternalServices
  ❌ → Common
  ❌ → Framework
  ⚠️  → Extensions (minimize)

ExternalServices:
  ✅ → Third-party NuGet
  ❌ → Common
  ❌ → Framework (implementation)
  ❌ → Extensions
  ❌ → ExternalServices (other)
```

### 7.2 Validation Script

Future: Create automated validation script

```bash
# Pseudocode for dependency validation
for project in src/Framework/*/*.csproj; do
  if grep -q "Common" "$project"; then
    echo "ERROR: Framework project references Common: $project"
  fi
done

for project in src/ExternalServices/*/*.csproj; do
  if grep -q "Framework.*(?<!Abstractions)" "$project"; then
    echo "ERROR: ExternalServices references Framework implementation: $project"
  fi
done
```

---

## 8. Migration and Evolution

### 8.1 Adding New Layer

**Rare:** Layers are well-established. Adding a new layer requires architectural review.

**Considerations:**
- Does it fit existing responsibilities?
- Can it be a Framework subdomain instead?
- What dependencies does it introduce?
- Impact on existing layers?

### 8.2 Moving Projects Between Layers

**Guidelines:**

**Framework → ExternalServices:**
- Project becomes vendor-specific
- Remove business logic, keep integration only

**ExternalServices → Framework:**
- Multiple implementations needed
- Create abstraction in Framework
- Keep vendor implementation in ExternalServices

**Extensions → Framework:**
- Functionality becomes domain-specific
- Needs provider pattern
- Multiple implementations planned

### 8.3 Deprecating Projects

**Process:**
1. Mark as `[Obsolete]` with migration path
2. Document in CHANGELOG
3. Provide migration guide
4. Remove after major version increment

---

## Summary

The four-layer architecture provides:

1. **Common** - Easy consumption and orchestration
2. **Framework** - Core business logic and abstractions
3. **Extensions** - Custom .NET enhancements
4. **ExternalServices** - Vendor integrations

**Key Principles:**
- Dependencies flow downward only
- Each layer has clear responsibilities
- Abstractions separate from implementations
- Provider/factory pattern for external services

This architecture enables:
- ✅ Independent development and testing
- ✅ Flexible deployment (use only what you need)
- ✅ Swappable implementations
- ✅ Clear separation of concerns
- ✅ Maintainability and extensibility

---

## Related Documentation

- [architectural-guidelines.md](./architectural-guidelines.md) - High-level principles
- [architectural-standards.md](./architectural-standards.md) - Concrete standards
- [architectural-patterns.md](./architectural-patterns.md) - Pattern documentation
- [provider-factory-pattern.md](./provider-factory-pattern.md) - Provider pattern details

---

## Change Log

- 2026-01-12 v1.0: Initial layering architecture documentation
