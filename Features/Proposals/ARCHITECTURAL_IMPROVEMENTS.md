# Architectural Improvements - SharedFramework Redesign

**Date:** 2026-01-22
**Purpose:** Document key architectural decisions that improve upon SharedFramework's original design

---

## Overview

During the design documentation process, several critical architectural improvements were identified that better align with **separation of concerns**, **reusability**, and **modern .NET patterns**.

---

## Improvement 1: Data Enhancement as Separate Pipeline

### Original SharedFramework Design ❌
```
Communications Component (1,145 LOC)
    ├─ Data Enhancement (tightly coupled)
    ├─ Template Loading (tightly coupled)
    ├─ Message Composition (tightly coupled)
    └─ Channel Routing & Sending
```

**Problems:**
- Data enhancement logic locked inside Communications
- Cannot reuse enhancement for Templating, Reporting, or Document Generation
- Violates Single Responsibility Principle
- Testing is harder (cannot test enhancement in isolation)

### New Architecture ✅
```
Epic 11: Data Enhancement Pipeline (400 LOC - reusable)
    ├─ IDataEnhancementPipeline
    ├─ IDataEnhancementProvider
    ├─ Attribute-based discovery
    └─ Context-based enhancement (not message-specific)

Consumers:
    ├─ Message Composition Service
    ├─ Template Engine
    ├─ Reporting Services
    └─ Document Generation
```

**Benefits:**
- ✅ Enhancement providers registered ONCE, used EVERYWHERE
- ✅ Template engine can use same enhancement logic
- ✅ Reporting can enrich data without duplicating code
- ✅ Clear separation: Enhancement → Composition → Delivery
- ✅ Testable in isolation

**Key Change:**
- Use `context` parameter (generic) instead of `messageType` (domain-specific)
- Examples: `"order.confirmation"`, `"monthly-report"`, `"invoice.pdf"`

---

## Improvement 2: IMessageData Instead of JObject

### Original SharedFramework Design ❌
```csharp
// Tightly coupled to Newtonsoft.Json (third-party)
public interface IDataEnhancementProvider
{
    Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, JObject data);
}

// Usage requires Newtonsoft.Json knowledge
data["Customer"]["Email"] = "user@example.com";
var email = data["Customer"]["Email"].Value<string>();
```

**Problems:**
- Hard dependency on third-party library (Newtonsoft.Json)
- JObject is weakly typed (runtime errors, no IntelliSense)
- Cannot swap implementations (locked to JSON)
- Not idiomatic for .NET 10.0 (System.Text.Json is standard)

### New Architecture ✅
```csharp
// Framework-agnostic abstraction
public interface IMessageData
{
    T? GetValue<T>(string path);
    void SetValue(string path, object? value);
    bool TryGetValue<T>(string path, out T? value);
    bool ContainsPath(string path);
    IMessageData Clone();
    IDictionary<string, object?> ToDictionary();
    string ToJson();
}

// Default implementation using System.Text.Json
public class JsonMessageData : IMessageData { /* ... */ }

// Usage is type-safe and clean
data.SetValue("Customer.Email", "user@example.com");
var email = data.GetValue<string>("Customer.Email");
```

**Benefits:**
- ✅ No third-party dependencies (System.Text.Json is in .NET runtime)
- ✅ Type-safe via generics (`GetValue<T>`, `TryGetValue<T>`)
- ✅ Path navigation: `"Customer.Address.City"` (creates intermediate objects)
- ✅ Swappable implementations (JSON, Dictionary, Dynamic, custom)
- ✅ Modern .NET 10.0 idioms

**Alternative Implementations Possible:**
```csharp
public class DictionaryMessageData : IMessageData { /* ... */ }
public class DynamicMessageData : IMessageData { /* ... */ }
```

---

## Improvement 3: Communications Simplified

### Original SharedFramework Design ❌
```
Communications (does EVERYTHING)
    ├─ Data Enhancement
    ├─ Template Loading
    ├─ Variable Substitution
    ├─ Message Composition
    ├─ Channel Preference Lookup
    ├─ Channel Routing
    └─ Provider Sending

LOC: ~1,145
Complexity: HIGH
Testability: LOW (too many responsibilities)
```

### New Architecture ✅
```
Communications Platform (SIMPLIFIED)
    ├─ Channel Preference Lookup
    ├─ Channel Routing
    ├─ Quiet Hours Management
    ├─ Deferral Scheduling
    └─ Provider Sending

LOC: ~300 (down from 1,145)
Complexity: MEDIUM
Testability: HIGH (single responsibility)
```

**Responsibilities Moved:**
- **Data Enhancement** → Epic 11: Data Enhancement Pipeline
- **Template Loading** → Epic 10: Text Templating
- **Variable Substitution** → Epic 10: Text Templating
- **Message Composition** → Message Composition Service (Epic 12)

**New Flow:**
```
Application Service
    ↓
Message Composition Service (Epic 12)
    ├─ Data Enhancement Pipeline (Epic 11)
    ├─ Template Engine (Epic 10)
    └─ Produces: IEmailMessage, ISmsMessage (pre-formatted)
    ↓
Communications Platform (Epic 2)
    ├─ Channel Routing
    └─ Provider Sending
```

**Benefits:**
- ✅ Each component has ONE responsibility
- ✅ Communications receives **pre-formatted** messages
- ✅ Template engine reusable for non-communication scenarios
- ✅ Enhancement reusable across features
- ✅ Easier to test (smaller, focused components)

---

## Improvement 4: Document Management Split

### Original SharedFramework Design ❌
```
DocumentCenter (911 LOC - monolithic)
    ├─ Storage (mixed with conversion)
    ├─ Conversion (mixed with packaging)
    ├─ Packaging (mixed with storage)
    └─ Everything tightly coupled
```

**Problems:**
- Cannot use storage without conversion logic
- Cannot convert documents without storage overhead
- Cannot package documents independently
- Violates Single Responsibility Principle

### New Architecture ✅
```
Epic 6: Document Management (split into 3 features)

Feature 1: Persistence & Retrieval (~300 LOC)
    ├─ IDocumentRepository
    ├─ IDocumentStore (DB, file system, S3, Azure Blob)
    ├─ Query/search by metadata
    └─ Version control

Feature 2: Conversion Pipelines (~400 LOC)
    ├─ IDocumentConverter
    ├─ IConversionPipeline
    ├─ Format transformations (PDF ↔ Word, HTML → PDF)
    ├─ Text extraction
    └─ OCR processing

Feature 3: Pack/Unpack (~200 LOC)
    ├─ IDocumentPacker
    ├─ IPackageManager
    ├─ ZIP/TAR support
    └─ Package metadata
```

**Benefits:**
- ✅ Use persistence **without** needing conversion
- ✅ Convert documents **without** storing them
- ✅ Package documents from **any source**
- ✅ Compose features as needed
- ✅ Each feature testable independently

**Usage Examples:**
```csharp
// Just storage
await _documentStore.SaveAsync(document);

// Just conversion
var pdf = await _converter.ConvertToPdfAsync(wordDoc);

// Just packaging
var package = await _packer.CreatePackageAsync(documents);

// Compose as needed
var wordDoc = await _documentStore.GetAsync(docId);
var pdf = await _converter.ConvertToPdfAsync(wordDoc);
var package = await _packer.CreatePackageAsync(new[] { pdf });
await _documentStore.SaveAsync(package);
```

---

## Improvement 5: Generic "Context" Instead of "MessageType"

### Original SharedFramework Design ❌
```csharp
public interface IDataEnhancementProvider
{
    // Assumes enhancement is ONLY for messages
    Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, JObject data);
}
```

**Problems:**
- Name `messageType` implies enhancement is only for messages
- Cannot use for reports, documents, exports, etc.
- Limits reusability

### New Architecture ✅
```csharp
public interface IDataEnhancementProvider
{
    // Generic "context" works for ANY enhancement scenario
    Task<IMessageData> EnhanceAsync(
        string context,
        IMessageData data,
        IDictionary<string, object?>? metadata = null);
}
```

**Context Examples:**
- `"order.confirmation"` - Email/SMS message
- `"monthly-sales-report"` - Report generation
- `"invoice.pdf"` - Document generation
- `"customer.export"` - Data export
- `"dashboard.widgets"` - Dashboard data loading

**Benefits:**
- ✅ More reusable across features
- ✅ Clearer separation of concerns
- ✅ Same providers work for messages, reports, documents, exports

---

## Improvement 6: Provider Discovery via Attributes

### Kept from SharedFramework ✅

**This pattern is GOOD and preserved:**
```csharp
[EnhancementContext(Context = "order.confirmation", Order = 1)]
public class OrderEnhancementProvider : IDataEnhancementProvider
{
    public async Task<IMessageData> EnhanceAsync(string context, IMessageData data, ...)
    {
        // Enhancement logic
    }
}

// Registration (automatic discovery)
services.DiscoverEnhancementProviders();
```

**Why Keep It:**
- ✅ Attribute-based discovery is a proven pattern
- ✅ Decouples provider registration from consumer code
- ✅ Order-based execution allows layered enhancement
- ✅ Supports multiple contexts per provider

**Improvement:**
- Renamed `[Communication]` → `[EnhancementContext]` (more generic)
- Changed `MessageType` → `Context` property
- Added `Order` property for execution sequencing

---

## Summary of Changes

| Aspect | SharedFramework | New Design | Benefit |
|--------|----------------|------------|---------|
| **Data Enhancement** | Embedded in Communications | Separate pipeline (Epic 11) | Reusable across features |
| **Data Container** | JObject (Newtonsoft.Json) | IMessageData (System.Text.Json) | No third-party deps, type-safe |
| **Communications** | 1,145 LOC (monolithic) | 300 LOC (focused) | Single responsibility |
| **Document Management** | 911 LOC (monolithic) | 3 features (~900 LOC total) | Composable features |
| **Context Naming** | messageType (domain-specific) | context (generic) | Broader reusability |
| **Template Logic** | Embedded in Communications | Separate (Epic 10) | Reusable templating |

---

## Epic Reorganization

### Original Plan (Before Improvements)
1. Communications (monolithic)
2. Spatial Services
3. Distributed Caching
4. Data Loading
5. Document Management (monolithic)
6. Identity & Session
7. Complex Events
8. Test Data Generation
9. Text Templating

### Improved Plan (After Improvements)
1. **Epic 11: Data Enhancement Pipeline** (NEW - foundational)
2. **Epic 10: Text Templating** (promote to earlier - needed by composition)
3. **Epic 12: Message Composition Service** (NEW - combines Epic 11 + Epic 10)
4. **Epic 2: Communications Platform** (SIMPLIFIED - just routing)
5. Epic 3: Spatial Services
6. Epic 4: Distributed Caching
7. Epic 5: Data Loading Pipeline
8. **Epic 6: Document Management** (SPLIT into 3 features)
   - Feature 1: Persistence & Retrieval
   - Feature 2: Conversion Pipelines
   - Feature 3: Pack/Unpack
9. Epic 7: Identity & Session
10. Epic 8: Complex Events
11. Epic 9: Test Data Generation

---

## Implementation Order (Revised)

### Phase 1: Foundation (Weeks 1-2)
1. **Epic 11: Data Enhancement Pipeline** - Core reusable infrastructure
2. **Epic 10: Text Templating** - Template engine (file/DB-based)
3. **Epic 4: Distributed Caching** - Already migrated

### Phase 2: Composition & Communication (Weeks 3-4)
4. **Epic 12: Message Composition Service** - Combines Epic 11 + Epic 10
5. **Epic 2: Communications Platform** - Simplified channel routing

### Phase 3: Domain Features (Weeks 5-7)
6. Epic 3: Spatial Services
7. Epic 5: Data Loading Pipeline
8. Epic 6: Document Management (3 features)

### Phase 4: Advanced Features (Weeks 8-10)
9. Epic 7: Identity & Session
10. Epic 8: Complex Events
11. Epic 9: Test Data Generation

---

## Key Principles Applied

1. **Separation of Concerns** - Each component has ONE responsibility
2. **Reusability** - Components usable across multiple features
3. **Framework Agnostic** - Abstract interfaces, swappable implementations
4. **Modern .NET** - Use System.Text.Json, not third-party libraries
5. **Testability** - Smaller, focused components are easier to test
6. **Composability** - Combine features as needed, not monolithic

---

## Related Documentation

- [Proposals Index](./README.md)
- [Epic 11: Data Enhancement Pipeline](./11-DataEnhancement/README.md)
- [Epic 2: Communications Platform (Revised)](./02-Communications/README-REVISED.md)
- [SharedFramework Migration Plan](../docs/migration/sharedframework-migration-plan.md)
