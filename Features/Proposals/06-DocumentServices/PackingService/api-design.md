# Document Packing Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Packing Service
**Last Updated:** 2026-01-23

---

## Core Interfaces

```csharp
namespace OoBDev.System.Documents.Packing;

public interface IDocumentPackingService
{
    Task<PackingResult> PackAsync(IEnumerable<PackingItem> items, string archiveFormat, PackingContext? context = null);
    Task<PackingResult> PackFromDirectoryAsync(string directoryPath, string archiveFormat, PackingContext? context = null);
}

public class PackingContext
{
    public CompressionLevel Compression { get; set; } = CompressionLevel.Normal;
    public string? Password { get; set; }
    public bool PreserveMetadata { get; set; } = true;
}

public class PackingResult
{
    public bool Success { get; set; }
    public byte[]? Archive { get; set; }
    public int ItemCount { get; set; }
    public long CompressedSize { get; set; }
    public double CompressionRatio { get; set; }
}
```

---

## Usage Examples

```csharp
var items = new[]
{
    new PackingItem { Content = doc1, EntryName = "document1.pdf" },
    new PackingItem { Content = doc2, EntryName = "document2.pdf" }
};

var context = new PackingContext
{
    Compression = CompressionLevel.Maximum,
    Password = "secret123"
};

var result = await _packingService.PackAsync(items, "zip", context);
await File.WriteAllBytesAsync("archive.zip", result.Archive!);
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
