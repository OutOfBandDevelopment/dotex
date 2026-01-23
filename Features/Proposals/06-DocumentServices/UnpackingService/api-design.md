# Document Unpacking Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Unpacking Service
**Last Updated:** 2026-01-23

---

## Core Interfaces

```csharp
namespace OoBDev.System.Documents.Unpacking;

public interface IDocumentUnpackingService
{
    Task<UnpackingResult> UnpackAsync(byte[] archiveContent, string archiveFormat, UnpackingContext? context = null);
    Task<UnpackingResult> UnpackSelectiveAsync(byte[] archiveContent, string archiveFormat, string[] filePatterns, UnpackingContext? context = null);
    Task<IEnumerable<ArchiveEntry>> ListContentsAsync(byte[] archiveContent, string archiveFormat);
}

public class UnpackingContext
{
    public string? Password { get; set; }
    public bool PreserveMetadata { get; set; } = true;
    public bool PreserveFolderStructure { get; set; } = true;
}

public class UnpackingResult
{
    public bool Success { get; set; }
    public IEnumerable<ExtractedFile> Files { get; set; } = Array.Empty<ExtractedFile>();
    public int FileCount { get; set; }
    public long TotalSize { get; set; }
}
```

---

## Usage Examples

```csharp
// Extract all files
var archiveContent = await File.ReadAllBytesAsync("documents.zip");
var result = await _unpackingService.UnpackAsync(archiveContent, "zip");

foreach (var file in result.Files)
{
    await File.WriteAllBytesAsync(file.Name, file.Content);
}

// Extract specific files
var patterns = new[] { "*.pdf", "*.docx" };
var selectiveResult = await _unpackingService.UnpackSelectiveAsync(archiveContent, "zip", patterns);

// List contents without extracting
var entries = await _unpackingService.ListContentsAsync(archiveContent, "zip");
foreach (var entry in entries)
{
    Console.WriteLine($"{entry.Name}: {entry.UncompressedSize} bytes");
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
