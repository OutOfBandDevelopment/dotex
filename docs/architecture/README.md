# OoBDev Architecture Documentation

**Last Updated:** 2026-01-12
**Project:** OoBDev (dotex) - .NET Extensions Framework
**Organization:** Out-of-Band Development, LLC

---

## Overview

This directory contains comprehensive architectural documentation for the OoBDev framework, including guidelines, standards, patterns, and design principles that govern the development of this enterprise-grade .NET library suite.

---

## Purpose

The OoBDev framework provides shared libraries and extensions for faster, more consistent .NET development. It consists of 112+ projects organized into a highly modular architecture with:

- **Strong separation of concerns** through layered architecture
- **Extensibility** via provider/factory pattern
- **Type safety** throughout all abstractions
- **Multi-provider support** for cloud services, databases, and external integrations
- **Comprehensive testing** with 36 test projects

---

## Architecture Documents

### Core Architecture

| Document | Purpose |
|----------|---------|
| [architectural-guidelines.md](./architectural-guidelines.md) | High-level principles and design philosophy |
| [architectural-standards.md](./architectural-standards.md) | Concrete rules and requirements |
| [architectural-patterns.md](./architectural-patterns.md) | Documented patterns and practices |
| [layering-architecture.md](./layering-architecture.md) | Layer responsibilities and dependencies |

### Specialized Topics

| Document | Purpose |
|----------|---------|
| provider-factory-pattern.md | Provider/factory pattern implementation |
| dependency-injection.md | DI patterns and builder configuration |
| testing-architecture.md | Testing strategy and standards |
| configuration-system.md | Configuration management approach |

---

## Quick Reference

### Architectural Layers

```
┌─────────────────────────────────────┐
│   Common (Orchestration Layer)     │  ← All-in-one packages
├─────────────────────────────────────┤
│   Framework (Domain Libraries)     │  ← Core business logic
├─────────────────────────────────────┤
│   Extensions (System Extensions)   │  ← Custom .NET extensions
├─────────────────────────────────────┤
│  ExternalServices (Integrations)   │  ← Third-party wrappers
└─────────────────────────────────────┘
```

**Dependency Rule:** Lower layers cannot depend on higher layers.

### Key Patterns

1. **Provider/Factory Pattern** - Abstraction → Provider → Factory
2. **Dependency Injection** - Builder-based configuration with TryAdd* extensions
3. **Attribute-Based Configuration** - `[MessageQueue]`, `[BlobContainer]`, etc.
4. **Handler Pattern** - Message queue handlers, document conversion handlers
5. **Middleware Pattern** - ASP.NET Core middleware pipeline

### Project Statistics

- **Total Projects:** 112+
- **Total C# Files:** 965+
- **Test Projects:** 36
- **Code Coverage:** 42.8% lines, 42.4% branches
- **Documentation Files:** 40+ library docs, 15+ framework docs

---

## Technology Stack

**Primary:**
- C# with .NET 9.0
- MSBuild with custom targets
- MSTest with Coverlet coverage
- GitVersion for semantic versioning

**Key Integrations:**
- AI/ML: Semantic Kernel, Ollama, GroqCloud, SBert
- Cloud: Azure (Blob, B2C, App Insights), AWS (incoming)
- Databases: MongoDB, SQL Server, OpenSearch, Qdrant
- Message Brokers: RabbitMQ, Azure Storage Queues
- Document Processing: Apache Tika, WkHtmlToPdf, Markdig
- Identity: Keycloak, Azure AD B2C

---

## Design Principles

### 1. Separation of Concerns

Each project has a single, well-defined responsibility. Abstractions are separated from implementations.

**Example:**
```
OoBDev.MessageQueueing.Abstractions  ← Interfaces and models
OoBDev.MessageQueueing               ← Core implementation
OoBDev.MessageQueueing.Hosting       ← Background service hosting
OoBDev.RabbitMQ.Abstractions         ← RabbitMQ abstractions
OoBDev.RabbitMQ                      ← RabbitMQ implementation
```

### 2. Dependency Inversion

Depend on abstractions, not concretions. All integrations are behind provider interfaces.

**Example:**
```csharp
IMessageQueueSender<TChannel> → IMessageSenderProvider → IMessageSenderProviderFactory
```

### 3. Open/Closed Principle

Open for extension, closed for modification. New providers can be added without changing existing code.

**Example:**
```csharp
// Add new message queue provider without modifying framework
services.AddKeyedSingleton<IMessageSenderProvider, MyCustomProvider>("custom");
```

### 4. Explicit Over Implicit

No implicit usings, explicit configuration, clear dependencies.

**Example:**
```csharp
// Explicit extension registration
services.TryAddSystemExtensions(configuration);
services.TryAddHandlebarServices();
```

### 5. Type Safety

Strongly-typed throughout. Generic constraints ensure compile-time safety.

**Example:**
```csharp
IMessageQueueHandler<TChannel, TMessage> where TMessage : class
```

---

## Getting Started

### For New Developers

1. Read [architectural-guidelines.md](./architectural-guidelines.md) - Understand the philosophy
2. Read [layering-architecture.md](./layering-architecture.md) - Understand the structure
3. Read [architectural-patterns.md](./architectural-patterns.md) - Learn common patterns
4. Review [architectural-standards.md](./architectural-standards.md) - Follow the rules

### For Adding New Features

1. Determine the appropriate layer (Common, Framework, Extensions, ExternalServices)
2. Follow the [provider-factory-pattern.md](./provider-factory-pattern.md) if adding integrations
3. Use the [dependency-injection.md](./dependency-injection.md) builder pattern
4. Follow [testing-architecture.md](./testing-architecture.md) for test coverage
5. Document according to [../Libraries/](../Libraries/) examples

### For Integrating External Services

1. Create `OoBDev.{Vendor}.Abstractions` project
2. Create `OoBDev.{Vendor}` implementation project
3. Follow provider/factory pattern
4. Add configuration options class
5. Create extension builder
6. Add comprehensive tests
7. Document in `/docs/Libraries/`

---

## Key Differentiators

What makes OoBDev unique:

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
11. **AI-Assisted Development** - Claude protocols for systematic workflows
12. **Migration-Ready Architecture** - Staging area for feature integration

---

## Related Documentation

### Framework Documentation
- [../Framework/MajorFunctionality.md](../Framework/MajorFunctionality.md)
- [../Framework/MessageQueueing.md](../Framework/MessageQueueing.md)
- [../Framework/TextTemplating.md](../Framework/TextTemplating.md)
- [../Framework/DocumentConversion.md](../Framework/DocumentConversion.md)

### Library Documentation
- [../Libraries/](../Libraries/) - 40+ library-specific docs

### Code Documentation
- [../code/](../code/) - Per-project README files

### Feature Tracking
- [../FEATURE_INVENTORY.md](../FEATURE_INVENTORY.md) - Comprehensive feature comparison

---

## Change Log

- 2026-01-12 v1.0: Initial architectural documentation created
