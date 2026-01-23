# Document Composition Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Composition Service
**Last Updated:** 2026-01-23

---

## Core Interfaces

```csharp
namespace OoBDev.System.Documents.Composition;

public interface IDocumentCompositionService
{
    Task<CompositionResult> MergeAsync(
        IEnumerable<byte[]> documents,
        string format,
        CompositionContext? context = null);

    Task<CompositionResult> ComposeFromPagesAsync(
        IEnumerable<PageSource> pageSources,
        string format,
        CompositionContext? context = null);

    Task<CompositionResult> InsertAsync(
        byte[] targetDocument,
        byte[] insertDocument,
        int insertPosition,
        CompositionContext? context = null);

    Task<CompositionResult> AppendAsync(
        byte[] targetDocument,
        IEnumerable<byte[]> appendDocuments,
        CompositionContext? context = null);
}

public class CompositionContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public bool PreserveBookmarks { get; set; } = true;
    public bool PreserveMetadata { get; set; } = true;
    public bool GenerateToC { get; set; } = false;
    public bool InsertBlankPagesBetweenSources { get; set; } = false;
    public PageNumberingStrategy? NumberingStrategy { get; set; }
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}

public class CompositionResult
{
    public bool Success { get; set; }
    public byte[]? ComposedDocument { get; set; }
    public int TotalPages { get; set; }
    public int SourceDocumentCount { get; set; }
    public TimeSpan Duration { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public string? ErrorMessage { get; set; }
}
```

---

## Usage Examples

### Example 1: Merge PDFs

```csharp
var documents = new[]
{
    await File.ReadAllBytesAsync("doc1.pdf"),
    await File.ReadAllBytesAsync("doc2.pdf"),
    await File.ReadAllBytesAsync("doc3.pdf")
};

var result = await _compositionService.MergeAsync(documents, "pdf");
await File.WriteAllBytesAsync("merged.pdf", result.ComposedDocument!);
```

### Example 2: Compose from Specific Pages

```csharp
var pageSources = new[]
{
    new PageSource { DocumentContent = doc1, Format = "pdf", PageNumbers = new[] { 1, 2, 3 } },
    new PageSource { DocumentContent = doc2, Format = "pdf", PageNumbers = new[] { 5, 6 } }
};

var result = await _compositionService.ComposeFromPagesAsync(pageSources, "pdf");
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
