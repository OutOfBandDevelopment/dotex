# Document Unpacking Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Unpacking Service
**Last Updated:** 2026-01-23

---

## Testing Overview

**Goal:** 80%+ code coverage

**Test Categories:**
- **Unit Tests** - 28+ tests
- **Integration Tests** - 10+ tests

---

## Unit Tests

```csharp
[TestClass]
public class DocumentUnpackingServiceTests
{
    [TestMethod]
    public async Task UnpackAsync_ValidZip_ExtractsFiles()
    {
        var zipContent = CreateTestZip(new[] { "file1.txt", "file2.txt" });
        var result = await _service.UnpackAsync(zipContent, "zip");

        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.FileCount);
    }

    [TestMethod]
    public async Task UnpackSelectiveAsync_FilePattern_ExtractsMatchingFiles()
    {
        var zipContent = CreateTestZip(new[] { "doc1.pdf", "doc2.txt", "doc3.pdf" });
        var result = await _service.UnpackSelectiveAsync(zipContent, "zip", new[] { "*.pdf" });

        Assert.AreEqual(2, result.FileCount);
        Assert.IsTrue(result.Files.All(f => f.Name.EndsWith(".pdf")));
    }
}
```

---

## Integration Tests

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class UnpackingIntegrationTests
{
    [TestMethod]
    public async Task UnpackAsync_PasswordProtectedZip_ExtractsWithPassword()
    {
        var encryptedZip = await File.ReadAllBytesAsync("TestFiles/encrypted.zip");
        var context = new UnpackingContext { Password = "secret" };

        var result = await _service.UnpackAsync(encryptedZip, "zip", context);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.FileCount > 0);
    }

    [TestMethod]
    public async Task ListContentsAsync_Archive_ReturnsAllEntries()
    {
        var zipContent = await File.ReadAllBytesAsync("TestFiles/sample.zip");
        var entries = await _service.ListContentsAsync(zipContent, "zip");

        Assert.IsTrue(entries.Count() > 0);
        Assert.IsTrue(entries.All(e => !string.IsNullOrEmpty(e.Name)));
    }
}
```

---

## Coverage Goals

| Component | Target |
|-----------|--------|
| DocumentUnpackingService | 85%+ |
| Providers | 80%+ |
| Overall | 80%+ |

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
