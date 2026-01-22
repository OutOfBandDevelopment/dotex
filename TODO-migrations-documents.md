# Design Documentation - Document Services

**Epic:** 6 - Document Services
**Status:** 📝 DESIGN PHASE (Replaces code migration)
**Priority:** MEDIUM
**Strategy:** Design-first approach with comprehensive documentation before implementation

---

## Overview

**Strategic Change (2026-01-22):** Instead of migrating code from SharedFramework, we are creating comprehensive design documentation following the Epic 11 pattern. This ensures:
- Clean architecture from first principles
- Modern .NET 10.0 patterns throughout
- Proper integration with IDataContainer, schema discovery, and path translation
- No technical debt from legacy code
- Complete test coverage from day one

---

## Documentation Tasks

### ✅ Completed
- Architecture captured in `Features/Proposals/REVISIONS_SUMMARY.md` (Revision 8: Document Context)
- High-level design in `Features/Proposals/CONSOLIDATED_DESIGN.md` (Epic 6)

### 🔄 In Progress
Creating detailed 4-document sets for each feature:

**Feature 1: Document Packaging** (4 documents)
- [ ] `Features/Proposals/06-Documents/DocumentPackaging/requirements.md`
- [ ] `Features/Proposals/06-Documents/DocumentPackaging/architecture.md`
- [ ] `Features/Proposals/06-Documents/DocumentPackaging/api-design.md`
- [ ] `Features/Proposals/06-Documents/DocumentPackaging/testing-strategy.md`

**Feature 2: Document Resolvers** (4 documents)
- [ ] `Features/Proposals/06-Documents/DocumentResolvers/requirements.md`
- [ ] `Features/Proposals/06-Documents/DocumentResolvers/architecture.md`
- [ ] `Features/Proposals/06-Documents/DocumentResolvers/api-design.md`
- [ ] `Features/Proposals/06-Documents/DocumentResolvers/testing-strategy.md`

**Feature 3: Storage Abstraction** (4 documents)
- [ ] `Features/Proposals/06-Documents/StorageAbstraction/requirements.md`
- [ ] `Features/Proposals/06-Documents/StorageAbstraction/architecture.md`
- [ ] `Features/Proposals/06-Documents/StorageAbstraction/api-design.md`
- [ ] `Features/Proposals/06-Documents/StorageAbstraction/testing-strategy.md`

**Feature 4: Context-Based Services** (4 documents)
- [ ] `Features/Proposals/06-Documents/ContextServices/requirements.md`
- [ ] `Features/Proposals/06-Documents/ContextServices/architecture.md`
- [ ] `Features/Proposals/06-Documents/ContextServices/api-design.md`
- [ ] `Features/Proposals/06-Documents/ContextServices/testing-strategy.md`

---

## Key Architectural Decisions

**From REVISIONS_SUMMARY.md:**

### Document Context (Revision 8)
- `IDocumentContext` for runtime document type detection
- Context-based service resolution (Invoice → invoice services)
- 11 context-specific services (invoices, orders, reports, etc.)
- Metadata-driven document classification

### Document Packaging
- `IDocumentPackager` for bundling related documents
- Support for ZIP, PDF portfolios, multi-part documents
- Metadata preservation across package boundaries
- Integration with storage providers

### Document Resolvers
- `IDocumentResolver` for locating documents by various criteria
- Path-based resolution (file system, URLs, storage keys)
- Metadata-based resolution (tags, categories, versions)
- Content-based resolution (hash, signature)

### Storage Abstraction
- Generic storage provider pattern
- Azure Blob Storage as first provider
- File system, S3, and other providers
- Consistent interface across storage types

### Integration Points
- **IDataContainer**: Document metadata uses container for dynamic data
- **Apache Tika**: Document conversion and content extraction
- **WkHtmlToPdf**: HTML to PDF conversion
- **HtmlToOpenXml**: HTML to Word conversion
- **Context Services**: 11 specialized document service types

---

## Architecture Highlights

**Key Interfaces:**
```csharp
// Document context
public interface IDocumentContext
{
    string ContextType { get; }  // "invoice", "order", "report", etc.
    IDictionary<string, object> Metadata { get; }
    Task<IDocumentService> ResolveServiceAsync(CancellationToken ct);
}

// Document packaging
public interface IDocumentPackager
{
    Task<IPackage> CreatePackageAsync(IEnumerable<IDocument> documents, PackageOptions options, CancellationToken ct);
    Task<IEnumerable<IDocument>> ExtractPackageAsync(IPackage package, CancellationToken ct);
}

// Document resolvers
public interface IDocumentResolver
{
    Task<IDocument?> ResolveByPathAsync(string path, CancellationToken ct);
    Task<IDocument?> ResolveByMetadataAsync(IDictionary<string, object> metadata, CancellationToken ct);
    Task<IEnumerable<IDocument>> ResolveByContentHashAsync(string hash, CancellationToken ct);
}

// Storage abstraction
public interface IDocumentStorage
{
    string StorageType { get; }
    Task<IDocument> SaveAsync(IDocument document, CancellationToken ct);
    Task<IDocument?> LoadAsync(string documentId, CancellationToken ct);
    Task DeleteAsync(string documentId, CancellationToken ct);
    Task<IEnumerable<IDocument>> ListAsync(DocumentQuery query, CancellationToken ct);
}

// Context-based services (11 types)
public interface IInvoiceService : IDocumentService
{
    Task<InvoiceDocument> GenerateInvoiceAsync(IDataContainer data, CancellationToken ct);
    Task<PaymentStatus> ProcessPaymentAsync(string invoiceId, CancellationToken ct);
}

public interface IOrderService : IDocumentService
{
    Task<OrderDocument> CreateOrderAsync(IDataContainer data, CancellationToken ct);
    Task<OrderStatus> TrackOrderAsync(string orderId, CancellationToken ct);
}

// ... 9 more context-specific service interfaces
```

---

## Implementation Strategy

**Phase 1: Design Documentation (Current)**
1. Complete all 16 design documents (4 per feature)
2. Review and validate designs
3. Get stakeholder approval

**Phase 2: Implementation (Future)**
1. Implement based on approved designs
2. Follow provider/factory pattern consistently
3. Write tests alongside implementation (TDD)
4. Target 85-90% test coverage
5. Integrate with existing Apache Tika, WkHtmlToPdf, HtmlToOpenXml

**Phase 3: Migration Path (Future)**
1. Enhance existing OoBDev.Documents (531 LOC → 1,300+ LOC)
2. Add document packaging system
3. Implement document resolvers
4. Add generic storage abstraction (keep Azure Blob as provider)
5. Implement 11 context-based document services
6. Preserve backward compatibility for existing consumers

---

## Reference Materials

**Design Documents:**
- `Features/Proposals/REVISIONS_SUMMARY.md` - Revision 8: Document Context
- `Features/Proposals/CONSOLIDATED_DESIGN.md` - Epic 6: Document Services
- `Features/Proposals/11-DataEnhancement/` - Pattern reference (Epic 11)

**Existing Implementation:**
- `Framework/OoBDev.Documents.Abstractions/` - Current abstractions (~700 LOC)
- `Framework/OoBDev.Documents/` - Current implementation (531 LOC)
- `ExternalServices/Apache.Tika/OoBDev.Apache.Tika/` - Document conversion
- `ExternalServices/WkHtmlToPdf/OoBDev.WkHtmlToPdf/` - PDF generation
- `ExternalServices/HtmlToOpenXml/OoBDev.HtmlToOpenXml/` - Word generation

---

## Success Criteria

- ✅ 16 comprehensive design documents created (requirements, architecture, api-design, testing-strategy)
- ✅ All architectural patterns consistent with Epic 11
- ✅ Integration points with IDataContainer, Apache Tika, WkHtmlToPdf, HtmlToOpenXml defined
- ✅ Provider pattern for storage (Azure Blob, file system, S3, etc.)
- ✅ 11 context-based document services designed (invoices, orders, reports, etc.)
- ✅ Test coverage targets defined (85-90%)
- ✅ Migration path from current Documents (531 LOC → 1,300+ LOC) planned
- ✅ Backward compatibility strategy for existing consumers

---

## Notes

**Why Design-First?**
- Avoids technical debt from legacy SharedFramework code
- Ensures modern .NET 10.0 patterns throughout
- Integrates cleanly with new Epic 11 features (IDataContainer, context-based services)
- Enables comprehensive test planning upfront
- Documents intent before implementation
- Clean merge strategy for existing documents system

**Benefits:**
- Clean architecture from scratch
- Enhanced documents system (531 LOC → 1,300+ LOC)
- Proper dependency injection from start
- Built-in extensibility with provider pattern
- Complete test coverage planned upfront
- 11 specialized context-based services
- Generic storage abstraction (Azure, file system, S3)

**Key Enhancements:**
- Document packaging (ZIP, PDF portfolios, multi-part)
- Document resolvers (path, metadata, content hash)
- Generic storage abstraction (keep Azure Blob as provider)
- Context-based service resolution (invoice, order, report, etc.)
- Integration with Apache Tika, WkHtmlToPdf, HtmlToOpenXml
- Metadata-driven document classification

**11 Context-Based Services:**
1. Invoice documents
2. Order documents
3. Report documents
4. Contract documents
5. Statement documents
6. Receipt documents
7. Shipping documents
8. Legal documents
9. HR documents
10. Marketing documents
11. Technical documents

---

**See Also:**
- [Design Progress](Features/Proposals/DOCUMENTATION_PROGRESS.md)
- [Epic 11 Documentation](Features/Proposals/11-DataEnhancement/) - Reference implementation
