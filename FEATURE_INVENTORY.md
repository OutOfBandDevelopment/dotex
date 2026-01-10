# dotex Feature Inventory

**Purpose:** Reference document for comparing other projects against dotex to identify functionality for migration.

**Last Updated:** 2026-01-10

---

## Project Overview

- **Total Projects:** 100+
- **Total C# Files:** 965
- **Test Projects:** 36
- **Code Coverage:** 42.8% lines, 42.4% branches
- **Primary Framework:** .NET 9.0
- **Secondary Framework:** .NET 4.8.1 (SQL CLR)
- **Solution File:** `/current/src/dotex/src/OoBDev.sln`

---

## 1. Core Architecture Components

### 1.1 Project Organization

| Category | Count | Location | Purpose |
|----------|-------|----------|---------|
| **Common** | 6 | `src/Common/` | Core services orchestration |
| **Framework** | 39 | `src/Framework/` | Domain-specific libraries |
| **External Services** | 40+ | `src/ExternalServices/` | Third-party integrations |
| **Extensions** | 6 | `src/Extensions/` | Custom system extensions |
| **Examples** | 3 | `src/Examples/` | Sample applications |
| **Tools** | 5+ | `src/Tools/` | CLI utilities |

### 1.2 Core Common Projects

- **OoBDev.Common** - Root common services orchestrator
- **OoBDev.Common.Abstractions** - Base abstractions
- **OoBDev.Common.AspNetCore** - ASP.NET Core integrations
- **OoBDev.Common.Complete** - All-in-one package
- **OoBDev.Common.Extensions** - External service wiring
- **OoBDev.Common.Hosting** - Background service hosting

### 1.3 Key Framework Projects

| Project | Purpose | Key Classes |
|---------|---------|-------------|
| **AI.Abstractions** | AI/LLM interfaces | `ILanguageModelProvider`, `IEmbeddingProvider` |
| **AspNetCore.*** | Web framework | Middleware, filters, JWT auth |
| **Communications.*** | Communication abstractions | `ICommunicationSender<T>`, Email/SMS models |
| **Documents.*** | Document management | `IBlobContainer<T>`, `IDocumentConversion` |
| **Identity.*** | Identity management | `IIdentityManager`, `IUserManagementProvider` |
| **MessageQueueing.*** | Event messaging | `IMessageQueueHandler<T,M>`, `IMessageQueueSender<T>` |
| **Search.*** | Search capabilities | `IVectorStore<T>`, search abstractions |
| **System.*** | Core utilities | Serialization, I/O, reflection, LINQ |
| **System.Linq** | LINQ/query building | `IQueryBuilder<T>`, expression tree builders |
| **TestUtilities** | Testing helpers | Test extensions, logging |

---

## 2. Feature Checklist

### 2.1 Message Queueing System ✓

**Capabilities:**
- [x] Type-safe message handlers with generics
- [x] Correlation ID tracking
- [x] Configuration-based routing
- [x] Background hosted service for processing
- [x] Attribute-based handler registration (`[MessageQueue]`)
- [x] Message context with headers and metadata

**Built-in Providers:**
- [x] In-Process `ConcurrentQueue` (default)
- [x] Azure Storage Queues
- [x] RabbitMQ

**Key Interfaces:**
- `IMessageQueueHandler<TChannel, TMessage>`
- `IMessageQueueSender<TChannel>`
- `IMessageContext`
- `IQueueMessage`

**Coverage:** 90.4% line coverage

### 2.2 Text Templating Engine ✓

**Capabilities:**
- [x] Extensible provider architecture
- [x] File-based template sources
- [x] Content type detection
- [x] Priority-based template selection
- [x] Sandbox path support

**Built-in Providers:**
- [x] XSLT 1.0
- [x] Handlebars.Net

**Handlebars Helpers:**
- `DateNow`, `GuidNew`, `Get`, `Set`, `Hash`, `StringReplace`

**Key Interfaces:**
- `ITemplateEngine`, `ITemplateProvider`, `ITemplateSource`

### 2.3 ASP.NET Core Extensions ✓

**Middleware:**
- [x] `CultureInfoMiddleware` - Accept-Language/Content-Language
- [x] `SearchQueryMiddleware` - IQueryable query parameter binding
- [x] `CorrelationInfoMiddleware` - Request correlation tracking

**Authentication/Authorization:**
- [x] JWT Bearer authentication
- [x] OAuth2 Swagger integration (Azure B2C, Keycloak)
- [x] Claims-based authorization
- [x] Application rights/permissions filtering (`[ApplicationRight]`)

**Query Support:**
- [x] Search term queries
- [x] Filter predicates (EqualTo, LessThan, GreaterThan, InSet, etc.)
- [x] Multi-column sorting
- [x] Pagination
- [x] Swagger integration

**Swagger/OpenAPI:**
- [x] OAuth2 configuration
- [x] Health check endpoints
- [x] Query parameter documentation
- [x] FormFile upload support

### 2.4 Communications Services ✓

**Email (MailKit):**
- [x] SMTP support
- [x] IMAP support
- [x] HTML and plain text content
- [x] Attachments via blob references
- [x] Health checks
- [x] Message queueing integration

**Key Interfaces:**
- `ICommunicationSender<T>`
- `EmailMessageModel`, `SmsMessageModel`

### 2.5 Document Services ✓

**Blob Storage:**
- [x] Provider-agnostic interface
- [x] Azure Blob Storage implementation
- [x] Metadata support
- [x] Query capabilities
- [x] Container-based organization
- [x] Attribute-based container naming (`[BlobContainer]`)

**Document Conversion:**
- [x] Apache Tika (content type detection, conversion)
- [x] WkHtmlToPdf (HTML to PDF)
- [x] Markdig (Markdown to HTML)
- [x] HtmlToOpenXml (HTML to DOCX)
- [x] MysticMind (HTML to Markdown)
- [x] Conversion handler chain pattern

**Key Interfaces:**
- `IBlobContainer<T>`, `IBlobContainerFactory`, `IBlobContainerProvider`
- `IDocumentConversion`, `IDocumentConversionHandler`
- `IContentTypeDetector`

### 2.6 Search Capabilities ✓

**Vector Search:**
- [x] Qdrant integration
- [x] OpenSearch integration
- [x] Semantic search
- [x] Lexical search
- [x] Hybrid search
- [x] Vector storage abstraction
- [x] Collection management

**Embeddings:**
- [x] SBert integration
- [x] AllMiniLML6v2Sharp (local)
- [x] OpenAI embeddings

**Key Interfaces:**
- `IVectorStore<T>`, `IVectorStoreFactory`, `IVectorStoreProvider`
- `SearchResultModel`
- `SearchTypes` enum

### 2.7 AI/LLM Integration ✓

**Providers:**
- [x] Ollama (local LLM)
- [x] GroqCloud
- [x] OpenAI
- [x] Semantic Kernel integration

**Features:**
- [x] Completion requests
- [x] Streaming responses
- [x] Context-aware conversations
- [x] RAG (Retrieval Augmented Generation)
- [x] Embedding generation
- [x] Keyed services for multiple providers

**Key Interfaces:**
- `ILanguageModelProvider`
- `IEmbeddingProvider`
- `IMessageCompletion`
- `CompletionRequest`, `CompletionResponse`

### 2.8 Identity Management ✓

**Providers:**
- [x] Azure B2C
- [x] Keycloak

**Features:**
- [x] User creation/deletion
- [x] Email-based user lookup
- [x] Password management
- [x] Force password change

**Key Interfaces:**
- `IIdentityManager`
- `IUserManagementProvider`
- `UserIdentityModel`, `UserCreateModel`, `UserCreatedModel`

### 2.9 Database Extensions ✓

**MongoDB:**
- [x] Custom conventions (BsonObjectId)
- [x] Database factory
- [x] Dispatch proxy for logging/interception

**SQL Server:**
- [x] DacFx integration for deployment
- [x] SQL CLR vector functions (.NET 4.8.1)
- [x] Vector data type support

### 2.10 System Extensions ✓

**Configuration:**
- [x] Command-line configuration provider
- [x] Hierarchical configuration support
- [x] Strongly-typed options pattern

**Serialization:**
- [x] JSON (System.Text.Json)
- [x] BSON
- [x] XML
- [x] Custom converters (DateTime, ObjectId, Dictionary)

**I/O:**
- [x] Stream extensions
- [x] Temporary file management
- [x] Pipelines support
- [x] USB HID device support
- [x] Serial port abstractions

**LINQ Extensions:**
- [x] Async enumerable extensions
- [x] Dictionary extensions
- [x] Expression building and manipulation
- [x] Expression tree visitors

**Reflection:**
- [x] Embedded resource utilities
- [x] Type resolution
- [x] Enum modeling

**Key Interfaces:**
- `ISerializer`, `IJsonSerializer`, `IBsonSerializer`, `IXmlSerializer`
- `IDevice`, `IDeviceAdapter`, `IStreamDevice`
- `ITempFile`, `ITempFileFactory`
- `IDateTimeProvider`, `IGuidProvider`

### 2.11 LINQ and Query Building ✓

**Query Interfaces:**
- [x] `ISearchQuery` - Search term queries
- [x] `IFilterQuery` - Filter predicates
- [x] `ISortQuery` - Sorting specifications
- [x] `IPageQuery` - Pagination

**Expression Building:**
- [x] `IQueryBuilder<T>` - Query execution
- [x] `IExpressionTreeBuilder<T>` - Dynamic expression construction
- [x] `ExpressionTreeBuilder<T>` - Concrete implementation
- [x] Expression visitors for null-safety
- [x] String comparison replacement visitors
- [x] Sort order replacement visitors

**Coverage:** 90.5% line coverage (highest in framework)

---

## 3. Architecture Patterns

### 3.1 Dependency Injection ✓

**Registration Patterns:**
- [x] `TryAdd*Extensions` methods
- [x] Scoped, Singleton, Transient lifetimes
- [x] Keyed services for multiple implementations
- [x] Builder pattern for configuration

**Extension Builders:**
- [x] `SystemExtensionBuilder`
- [x] `AspNetCoreExtensionBuilder`
- [x] `JwtExtensionBuilder`
- [x] `IdentityExtensionBuilder`
- [x] `ExternalExtensionBuilder`
- [x] `HostingBuilder`
- [x] `MiddlewareExtensionBuilder`

### 3.2 Provider/Factory Pattern ✓

**Hierarchy:** Abstraction → Provider → Factory

**Examples:**
- `IVectorStore` → `IVectorStoreProvider` → `IVectorStoreProviderFactory`
- `IBlobContainer` → `IBlobContainerProvider` → `IBlobContainerProviderFactory`
- Message queueing providers and factories

### 3.3 Handler Pattern ✓

**Implementations:**
- [x] `IMessageQueueHandler<TChannel, TMessage>`
- [x] `IDocumentConversionHandler`
- [x] Attribute-based handler registration

### 3.4 Middleware Pattern ✓

**ASP.NET Core Middleware:**
- [x] Culture info
- [x] Correlation tracking
- [x] Search query binding

### 3.5 Visitor Pattern ✓

**Expression Tree Manipulation:**
- [x] `StringComparisonReplacementExpressionVisitor`
- [x] `StringOrderReplacementExpressionVisitor`
- [x] `SkipMemberOnNullExpressionVisitor`

### 3.6 Strategy Pattern ✓

**Implementations:**
- [x] Template providers
- [x] Document conversion handlers
- [x] Serialization providers

### 3.7 Background Services ✓

- [x] `MessageReceiverHost` - IHostedService for message processing
- [x] Graceful start/stop

---

## 4. External Service Integrations

### 4.1 Cloud Services

| Service | Project | Purpose |
|---------|---------|---------|
| Azure Blob Storage | OoBDev.Azure.StorageAccount | Blob container implementation |
| Azure Storage Queues | OoBDev.Azure.StorageAccount | Message queue provider |
| Azure B2C | OoBDev.Microsoft.B2C | Identity provider |
| Application Insights | OoBDev.Microsoft.ApplicationInsights | Telemetry |

### 4.2 Databases and Storage

| Service | Project | Purpose |
|---------|---------|---------|
| MongoDB | OoBDev.MongoDB | NoSQL database |
| SQL Server | OoBDev.Microsoft.SqlServer | Relational database |
| OpenSearch | OoBDev.OpenSearch | Search engine |
| Qdrant | OoBDev.Qdrant | Vector database |

### 4.3 Message Brokers

| Service | Project | Purpose |
|---------|---------|---------|
| RabbitMQ | OoBDev.RabbitMQ | Message broker |
| Azure Storage Queues | OoBDev.Azure.StorageAccount | Cloud queue |
| In-process | OoBDev.MessageQueueing | Local queue (default) |

### 4.4 AI/ML Services

| Service | Project | Purpose |
|---------|---------|---------|
| Ollama | OoBDev.Ollama | Local LLM |
| GroqCloud | OoBDev.GroqCloud | Cloud LLM |
| OpenAI | Via configuration | LLM/Embeddings |
| SBert | OoBDev.SBert | Sentence embeddings |
| AllMiniLML6v2Sharp | OoBDev.System.Text.Embeddings | Local embeddings |
| Semantic Kernel | OoBDev.SemanticKernel | AI orchestration |

### 4.5 Document Processing

| Service | Project | Purpose |
|---------|---------|---------|
| Apache Tika | OoBDev.Apache.Tika | Content type detection, conversion |
| WkHtmlToPdf | OoBDev.WkHtmlToPdf | HTML to PDF |
| Markdig | OoBDev.Markdig | Markdown to HTML |
| HtmlToOpenXml | OoBDev.HtmlToOpenXml | HTML to DOCX |
| MysticMind | OoBDev.MysticMind | HTML to Markdown |

### 4.6 Identity Providers

| Service | Project | Purpose |
|---------|---------|---------|
| Keycloak | OoBDev.Keycloak | Open source identity |
| Azure AD B2C | OoBDev.Microsoft.B2C | Cloud identity |

### 4.7 Communication

| Service | Project | Purpose |
|---------|---------|---------|
| MailKit | OoBDev.MailKit | SMTP/IMAP |

### 4.8 Libraries

| Library | Project | Purpose |
|---------|---------|---------|
| Handlebars.Net | OoBDev.Handlebars | Template engine |
| System.Text.Json | Built-in | JSON serialization |

---

## 5. Configuration System

### 5.1 Configuration Sources

- [x] appsettings.json
- [x] Environment variables
- [x] Command-line arguments
- [x] Hierarchical configuration sections
- [x] Strongly-typed options classes

### 5.2 Key Configuration Sections

| Section | Purpose | Example Options |
|---------|---------|-----------------|
| `ApacheTikaClientOptions` | Tika server | URL |
| `OAuth2SwaggerOptions` | OAuth integration | AuthorizationUrl, TokenUrl |
| `AzureBlobProviderOptions` | Blob storage | Connection strings |
| `MailKitSmtpClientOptions` | Email SMTP | Host, Port, DefaultFrom |
| `MailKitImapClientOptions` | Email IMAP | Host, Port, Username |
| `MicrosoftIdentityOptions` | Azure B2C | TenantId, ClientId |
| `MongoDatabaseOptions` | MongoDB | Connection string |
| `OllamaApiClientOptions` | Ollama | Base URL, Model |
| `OpenSearchOptions` | OpenSearch | URLs, credentials |
| `QdrantOptions` | Qdrant | API key, host |
| `SentenceEmbeddingOptions` | Embeddings | Provider settings |
| `FileTemplateSource` | Templates | Base path |
| `TemplateEngineOptions` | Template engine | Provider settings |

### 5.3 Configuration Binding

- [x] IConfiguration injection
- [x] Options pattern
- [x] IOptions<T> / IOptionsSnapshot<T>

---

## 6. Extensibility Points

### 6.1 Custom Provider Interfaces

| Interface | Purpose | Implementation Required |
|-----------|---------|-------------------------|
| `ITemplateProvider` | Custom template engines | Template compilation and rendering |
| `ITemplateSource` | Custom template sources | Template retrieval |
| `IMessageSenderProvider` | Custom message queues (send) | Message sending logic |
| `IMessageReceiverProvider` | Custom message queues (receive) | Message receiving logic |
| `IBlobContainerProvider` | Custom blob storage | CRUD operations on blobs |
| `IDocumentConversionHandler` | Custom document converters | Format conversion logic |
| `IVectorStoreProvider` | Custom vector stores | Vector CRUD and search |
| `ILanguageModelProvider` | Custom LLM providers | Completion API |
| `IEmbeddingProvider` | Custom embedding generators | Text to vector conversion |

### 6.2 Attribute-Based Configuration

| Attribute | Purpose | Applied To |
|-----------|---------|------------|
| `[MessageQueue(SimpleName = "...")]` | Handler registration | Message handler classes |
| `[BlobContainer(ContainerName = "...")]` | Container naming | Controller/service classes |
| `[VectorStore(CollectionName = "...")]` | Collection naming | Vector store usage |
| `[ApplicationRight(Rights = ...)]` | Authorization | Controller actions |

### 6.3 Expression Visitors

- [x] `IPostBuildExpressionVisitor` - Query customization
- [x] `IExpressionTreeBuilder<T>` - Custom expression building

### 6.4 Middleware

- [x] Custom ASP.NET Core middleware
- [x] Request/response pipeline extensions

---

## 7. Testing Infrastructure

### 7.1 Test Framework

- [x] **MSTest** - Primary test framework
- [x] **Coverlet** - Code coverage (XPlat Code Coverage)
- [x] 36 test projects
- [x] `.runsettings` configuration

### 7.2 Test Categories

- [x] `[TestCategory("Unit")]` - Unit tests
- [x] `[TestCategory("Simulate")]` - Simulation tests
- [x] Integration test support

### 7.3 Test Utilities (OoBDev.TestUtilities)

- [x] `TestContextExtensions` - Attachment support
- [x] `TestLogger` - Test logging
- [x] Result capture utilities

**Coverage:** 78.5% line coverage

### 7.4 Current Coverage

| Project | Line Coverage | Notable |
|---------|---------------|---------|
| System.Linq | 90.5% | ✓ Excellent |
| MessageQueueing | 90.4% | ✓ Excellent |
| Handlebars | 85% | ✓ Good |
| TestUtilities | 78.5% | ✓ Good |
| System | 71% | ✓ Good |
| ASP.NET Core | 0-15% | ⚠ Low |
| External Services | 0-15% | ⚠ Low |

**Overall:** 42.8% line, 42.4% branch

---

## 8. Build and Deployment

### 8.1 Build System

- [x] MSBuild with custom targets
- [x] `Directory.Build.props` - Shared properties
- [x] `Directory.Build.targets` - Custom build tasks
- [x] Multi-targeting (.NET 9.0, .NET 4.8.1)

### 8.2 Versioning

- [x] GitVersion for semantic versioning
- [x] `GitVersion.yml` configuration
- [x] Automatic version tagging

### 8.3 CI/CD Pipeline (GitHub Actions)

**Build Matrix:**
- [x] ubuntu-latest (default)
- [x] windows-latest
- [x] macos-latest

**Pipeline Steps:**
1. Checkout with submodules
2. Install .NET 9.0 SDK
3. GitVersion setup and execution
4. Restore dependencies
5. Build .NET 4.8.1 (SQL CLR)
6. Build main solution
7. Package NuGet packages
8. Run unit tests with coverage
9. Publish test results
10. Tag commit with version

### 8.4 NuGet Packaging

- [x] Automatic package generation
- [x] Output to `publish/packages/`
- [x] Source Link support
- [x] Embedded resources (examples, docs)
- [x] License file inclusion

---

## 9. Documentation

### 9.1 Framework Documentation (/docs)

| Document | Purpose |
|----------|---------|
| `MajorFunctionality.md` | Feature overview |
| `MessageQueueing.md` | Message queue design |
| `TextTemplating.md` | Template engine design |
| `DocumentConversion.md` | Conversion framework |
| `BlobStorage.md` | Blob storage design |
| `CultureInfoMiddleware.md` | Localization |
| `ConfigurationSettings.md` | Settings reference |

### 9.2 Library Documentation

- [x] 40+ library-specific markdown files in `docs/Libraries/`
- [x] Detailed API documentation per library

### 9.3 Code Documentation

- [x] 40+ README files in `docs/code/`
- [x] Per-project documentation
- [x] Extensive XML comments

### 9.4 Examples

- [x] WebApi example with controllers
- [x] Example database project
- [x] Test examples

---

## 10. Command-Line Tools

| Tool | Purpose | Location |
|------|---------|----------|
| **DacPacCompiler.Cli** | Database deployment compilation | `src/Tools/` |
| **DocumentConverter.Cli** | Batch document conversion | `src/Tools/` |
| **FileRagEngine.Cli** | RAG indexing and queries | `src/Tools/` |
| **FixSourceLinks.Cli** | Source link correction | `src/Tools/` |
| **TemplateEngine.Cli** | Template processing | `src/Tools/` |

---

## 11. Key Differentiators

### What Makes dotex Unique

1. **Comprehensive Extension System** - Builder-based configuration with optional parameters
2. **Provider/Factory Pattern** - Consistent abstraction across all integrations
3. **Type-Safe Message Queuing** - Attribute-based with multiple backends
4. **Advanced LINQ Query Building** - Expression tree manipulation for dynamic queries
5. **Extensible Template Engine** - Multiple providers (XSLT, Handlebars)
6. **Unified Document Management** - Blob storage + conversion pipeline
7. **Complete ASP.NET Core Integration** - Middleware, filters, Swagger, auth
8. **Multi-LLM Abstraction** - Keyed services, RAG support
9. **Vector Search Capabilities** - Semantic/lexical/hybrid search
10. **Strong Testing Infrastructure** - MSTest, Coverlet, utilities

---

## 12. Migration Comparison Checklist

Use this checklist when comparing another project to dotex:

### Core Capabilities
- [ ] Has message queueing? If yes, what providers?
- [ ] Has templating engine? If yes, which one?
- [ ] Has blob storage? If yes, which provider?
- [ ] Has document conversion? If yes, which formats?
- [ ] Has search capabilities? If yes, what type?
- [ ] Has AI/LLM integration? If yes, which providers?
- [ ] Has identity management? If yes, which providers?

### Architecture Patterns
- [ ] Uses DI/IoC? Which container?
- [ ] Has provider/factory pattern?
- [ ] Has handler pattern?
- [ ] Has middleware?
- [ ] Has background services?

### ASP.NET Core Features
- [ ] Has custom middleware?
- [ ] Has JWT authentication?
- [ ] Has OAuth2/OIDC?
- [ ] Has Swagger/OpenAPI?
- [ ] Has query parameter binding?
- [ ] Has correlation tracking?

### Testing
- [ ] Has unit tests? Which framework?
- [ ] Has integration tests?
- [ ] Has code coverage? What percentage?
- [ ] Has test utilities?

### Build/Deploy
- [ ] Has CI/CD pipeline?
- [ ] Has versioning strategy?
- [ ] Produces NuGet packages?
- [ ] Has multi-targeting?

### Documentation
- [ ] Has README files?
- [ ] Has API documentation?
- [ ] Has example projects?
- [ ] Has configuration documentation?

---

## 13. Common Migration Scenarios

### Scenario 1: Project Uses Similar Features
**Action:** Compare implementation approaches
- Is the implementation better/worse/equivalent?
- Can we extract and migrate the implementation?
- Should we standardize on dotex's approach?

### Scenario 2: Project Has Unique Features
**Action:** Identify for potential integration
- Is this feature valuable to dotex?
- Can it be abstracted into a provider pattern?
- Should it become a new framework library?

### Scenario 3: Project Has Duplicate Features
**Action:** Consolidate on dotex
- Map project classes to dotex equivalents
- Create migration guide for consumers
- Deprecate duplicate implementations

### Scenario 4: Project Has Legacy Patterns
**Action:** Evaluate modernization
- Does dotex have a modern equivalent?
- Is migration worth the effort?
- Can we create adapters/bridges?

---

## Appendix: Full Project List

See `/current/src/dotex/src/OoBDev.sln` for complete project listing.

**Key Project Prefixes:**
- `OoBDev.Common.*` - Common services
- `OoBDev.AI.*` - AI/LLM
- `OoBDev.AspNetCore.*` - Web framework
- `OoBDev.Communications.*` - Communications
- `OoBDev.Documents.*` - Document management
- `OoBDev.Identity.*` - Identity
- `OoBDev.MessageQueueing.*` - Message queues
- `OoBDev.Search.*` - Search
- `OoBDev.System.*` - System utilities
- `OoBDev.<Provider>` - External service integrations

---

**End of Feature Inventory**
