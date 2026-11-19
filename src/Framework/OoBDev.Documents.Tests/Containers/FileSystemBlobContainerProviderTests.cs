using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class FileSystemBlobContainerProviderTests
{
    public required TestContext TestContext { get; set; }

    private static ContentReference Reference(string name, string text, Stream? stream = null) =>
        new() { Content = stream ?? new MemoryStream(global::System.Text.Encoding.UTF8.GetBytes(text)), ContentType = "text/plain", FileName = name };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RoundTrip_StoresReadsQueriesUpdatesAndDeletes()
    {
        // Stage
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        var provider = new FileSystemBlobContainerProvider(temp.DirectoryPath, "Invoices");
        TestContext.WriteLine($"root: {temp.DirectoryPath}");

        // Test
        await provider.StoreContentAsync(Reference("dir/a.txt", "hello"), new Dictionary<string, string> { ["owner"] = "me" });
        var content = await provider.GetContentAsync("dir/a.txt");
        var meta = await provider.GetContentMetaDataAsync("dir/a.txt");
        var listed = provider.QueryContent().ToList();
        var updated = await provider.StoreContentMetaDataAsync(new ContentMetaDataReference { ContentType = "text/plain", FileName = "dir/a.txt", MetaData = new Dictionary<string, string> { ["owner"] = "you" } });
        var after = await provider.GetContentMetaDataAsync("dir/a.txt");
        Assert.IsNotNull(content);
        string text;
        await using (content.Content.ConfigureAwait(false))
        {
            using var reader = new StreamReader(content.Content);
            text = await reader.ReadToEndAsync(TestContext.CancellationToken);
        }

        await provider.DeleteContentAsync("dir/a.txt");

        // Assert
        Assert.AreEqual("hello", text);
        Assert.AreEqual("a.txt", content.FileName);
        Assert.AreEqual("text/plain", content.ContentType);
        Assert.AreEqual("me", meta?.MetaData?["owner"]);
        Assert.AreEqual("dir/a.txt", listed.Single().FileName);
        Assert.IsTrue(updated);
        Assert.AreEqual("you", after?.MetaData?["owner"]);
        Assert.IsNull(await provider.GetContentAsync("dir/a.txt"));
        Assert.AreEqual(0, provider.QueryContent().Count());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_ExistingFileWithoutOverwrite_Throws()
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        var provider = new FileSystemBlobContainerProvider(temp.DirectoryPath, "c");
        await provider.StoreContentAsync(Reference("a.txt", "one"));

        await Assert.ThrowsExactlyAsync<IOException>(() => provider.StoreContentAsync(Reference("a.txt", "two")));
        await provider.StoreContentAsync(Reference("a.txt", "three"), overwrite: true);

        using var reader = new StreamReader((await provider.GetContentAsync("a.txt"))!.Content);
        Assert.AreEqual("three", await reader.ReadToEndAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Missing_ReturnsNullAndFalse()
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        var provider = new FileSystemBlobContainerProvider(temp.DirectoryPath, "c");

        Assert.IsNull(await provider.GetContentAsync("none.txt"));
        Assert.IsNull(await provider.GetContentMetaDataAsync("none.txt"));
        Assert.IsFalse(await provider.StoreContentMetaDataAsync(new ContentMetaDataReference { ContentType = "x", FileName = "none.txt" }));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("../outside.txt")]
    [DataRow("..\\outside.txt")]
    [DataRow("a.txt.blobmeta.json")]
    public async Task Paths_OutsideTheContainerOrSidecars_AreRejected(string path)
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        var provider = new FileSystemBlobContainerProvider(temp.DirectoryPath, "c");

        _ = await Assert.ThrowsExactlyAsync<ArgumentException>(() => provider.GetContentAsync(path));
        Assert.IsFalse(File.Exists(Path.Combine(temp.DirectoryPath, "outside.txt")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ContainerName_OutsideTheRoot_Throws()
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();

        _ = Assert.ThrowsExactly<InvalidOperationException>(() => new FileSystemBlobContainerProvider(temp.DirectoryPath, ".."));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Factory_IsOfferedOnlyWithARootDirectory()
    {
        using var temp = new TempFileFactory(null!).GetTempDirectory();
        foreach (var (root, offered) in new[] { ((string?)null, false), (temp.DirectoryPath, true) })
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["FileSystemBlobOptions:RootDirectory"] = root }).Build();
            using var services = new ServiceCollection().TryAddFileSystemBlobServices(configuration).BuildServiceProvider();

            var created = services.GetRequiredService<IBlobContainerProviderFactory>().Create("Invoices");

            TestContext.WriteLine($"root: {root}, offered: {created is not null}");
            Assert.AreEqual(offered, created is not null);
        }
    }
}
