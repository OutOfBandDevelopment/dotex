# Document Composition Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Composition Service
**Last Updated:** 2026-01-23

---

## Architectural Overview

The Document Composition Service implements a **Provider Pattern** with **Context-Based Merging** for combining multiple documents into a single output with bookmark preservation, metadata consolidation, and intelligent page ordering.

```plantuml
@startuml
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Application Layer" as App <<layer>>
rectangle "IDocumentCompositionService\n- MergeAsync(documents[], context)\n- ComposeFromPagesAsync(pageSources[], context)\n- InsertAsync(target, insert, position, context)" as Svc <<service>>
rectangle "PDFSharp\nProvider" as P1 <<provider>>
rectangle "iText\nProvider" as P2 <<provider>>
rectangle "PDFBox\nProvider" as P3 <<provider>>
rectangle "Custom\nProvider" as P4 <<provider>>
App --> Svc : MergeAsync(documents[], context)
Svc --> P1
Svc --> P2
Svc --> P3
Svc --> P4
@enduml
```

*Figure 1 — Composition service and providers*

---

## Core Components

### 1. DocumentCompositionService

**Implementation Pattern:**
```csharp
public class DocumentCompositionService : IDocumentCompositionService
{
    public async Task<CompositionResult> MergeAsync(
        IEnumerable<byte[]> documents,
        string format,
        CompositionContext? context = null)
    {
        context ??= new CompositionContext();
        var provider = await SelectProviderAsync(format, context);

        var result = await provider.MergeAsync(documents, format, context);

        if (context.GenerateToC)
        {
            result.ComposedDocument = await GenerateTableOfContentsAsync(
                result.ComposedDocument!, context);
        }

        return result;
    }
}
```

---

## Design Patterns

1. **Provider Pattern** - Multiple composition engines
2. **Strategy Pattern** - Different merge strategies
3. **Builder Pattern** - Complex document composition

---

## Related Documents

- [Requirements](requirements.md)
- [API Design](api-design.md)
- [Testing Strategy](testing-strategy.md)
- [Epic 6 Overview](../README.md)
