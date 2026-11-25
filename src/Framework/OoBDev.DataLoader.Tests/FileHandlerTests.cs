using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Documents.Models;
using OoBDev.Documents.Store;

namespace OoBDev.DataLoader.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class FileHandlerTests
{
    public TestContext TestContext { get; set; } = null!;

    private string folder = string.Empty;

    [TestInitialize]
    public void Init()
    {
        folder = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "body.txt"), "hello text");
        TestContext.WriteLine($"folder {folder}");
    }

    [TestCleanup]
    public void Clean() => Directory.Delete(folder, recursive: true);

    private static RowContext Row(params (string, string?)[] values)
    {
        var dict = new Dictionary<string, string?>();
        foreach (var (k, v) in values)
        {
            dict[k] = v;
        }

        return new RowContext(1, dict);
    }

    [TestMethod]
    public async Task TextFile_NamedFile_ReplacesTheColumnWithItsText()
    {
        var handler = new TextFileHandler(1, new SourceFileResolver(folder), new Dictionary<string, string> { ["bodyFile"] = "body" });
        var row = Row(("bodyFile", "body.txt"));

        var outcome = await handler.HandleAsync(row, CancellationToken.None);

        Assert.IsTrue(outcome.Accepted);
        Assert.AreEqual("hello text", row.Values["body"]);
        Assert.IsFalse(row.Values.ContainsKey("bodyFile"));
    }

    [TestMethod]
    public async Task TextFile_MissingOrOutsideFile_RejectsWhenRequired()
    {
        var handler = new TextFileHandler(1, new SourceFileResolver(folder), new Dictionary<string, string> { ["bodyFile"] = "body" });

        var missing = await handler.HandleAsync(Row(("bodyFile", "nope.txt")), CancellationToken.None);
        var outside = await handler.HandleAsync(Row(("bodyFile", "../outside.txt")), CancellationToken.None);
        TestContext.WriteLine($"{missing.Error} | {outside.Error}");

        Assert.IsFalse(missing.Accepted);
        StringAssert.Contains(missing.Error, "not found");
        Assert.IsFalse(outside.Accepted);
        StringAssert.Contains(outside.Error, "outside the data folder");
    }

    [TestMethod]
    public async Task TextFile_MissingFileNotRequired_DropsTheColumn()
    {
        var handler = new TextFileHandler(1, new SourceFileResolver(folder), new Dictionary<string, string> { ["bodyFile"] = "body" }, required: false);
        var row = Row(("bodyFile", "nope.txt"));

        var outcome = await handler.HandleAsync(row, CancellationToken.None);

        Assert.IsTrue(outcome.Accepted);
        Assert.IsFalse(row.Values.ContainsKey("body"));
        Assert.IsFalse(row.Values.ContainsKey("bodyFile"));
    }

    [TestMethod]
    public async Task Upload_NamedFile_StoresItAndSetsKeyAndName()
    {
        string? seenType = null;
        string? seenName = null;
        string? seenText = null;
        var store = new Mock<IDocumentStore>();
        store.Setup(s => s.StoreAsync(It.IsAny<ContentReference>())).Returns((ContentReference c) =>
        {
            seenType = c.ContentType;
            seenName = c.FileName;
            using var reader = new StreamReader(c.Content, Encoding.UTF8);
            seenText = reader.ReadToEnd();
            return Task.FromResult("key-1");
        });
        var handler = new DocumentUploadHandler(10, new SourceFileResolver(folder), store.Object);
        var row = Row(("__SourceFilePath", "body.txt"), ("__SourceFileContent", "text/plain"), ("Name", null));

        var outcome = await handler.HandleAsync(row, CancellationToken.None);
        TestContext.WriteLine($"{seenName} {seenType} key {row.Values["StorageKey"]}");

        Assert.IsTrue(outcome.Accepted);
        Assert.AreEqual("key-1", row.Values["StorageKey"]);
        Assert.AreEqual("body.txt", row.Values["Name"]);
        Assert.AreEqual("text/plain", seenType);
        Assert.AreEqual("hello text", seenText);
        Assert.IsFalse(row.Values.ContainsKey("__SourceFilePath"));
    }

    [TestMethod]
    public async Task Upload_RowWithAKey_IsNotUploadedAgain()
    {
        var store = new Mock<IDocumentStore>(MockBehavior.Strict);
        var handler = new DocumentUploadHandler(10, new SourceFileResolver(folder), store.Object);
        var row = Row(("__SourceFilePath", "body.txt"), ("StorageKey", "old"));

        var outcome = await handler.HandleAsync(row, CancellationToken.None);

        Assert.IsTrue(outcome.Accepted);
        Assert.AreEqual("old", row.Values["StorageKey"]);
    }

    [TestMethod]
    public async Task Upload_RefusedOrMissingContent_RejectsTheRow()
    {
        var store = new Mock<IDocumentStore>();
        store.Setup(s => s.StoreAsync(It.IsAny<ContentReference>())).ThrowsAsync(new InvalidContentException("not allowed"));
        var handler = new DocumentUploadHandler(10, new SourceFileResolver(folder), store.Object);

        var refused = await handler.HandleAsync(Row(("__SourceFilePath", "body.txt")), CancellationToken.None);
        var missing = await handler.HandleAsync(Row(("__SourceFilePath", "gone.bin")), CancellationToken.None);
        TestContext.WriteLine($"{refused.Error} | {missing.Error}");

        Assert.IsFalse(refused.Accepted);
        StringAssert.Contains(refused.Error, "not allowed");
        Assert.IsFalse(missing.Accepted);
    }
}
