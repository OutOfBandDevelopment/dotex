using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Models;
using OoBDev.Microsoft.Azure.StorageAccount.BlobStorage;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.StorageAccount.Tests.BlobStorage;

/// <summary>
/// Integration tests for <see cref="AzureBlobContainerProvider"/> against the Azurite emulator in the Docker test stack.
/// </summary>
[TestClass]
public class AzureBlobContainerProviderIntegrationTests
{
    private const string DefaultAzuriteConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    private BlobServiceClient? _client;
    private AzureBlobContainerProvider? _provider;

    public required TestContext TestContext { get; set; }

    private AzureBlobContainerProvider CreateProvider()
    {
        var connectionString = TestContext.GetPropertyOrDefault("AZURITE_CONNECTION_STRING", DefaultAzuriteConnectionString);
        _client = new BlobServiceClient(connectionString);
        var options = Options.Create(new AzureBlobProviderOptions { ConnectionString = connectionString, EnsureContainerExists = true });
        _provider = new AzureBlobContainerProvider(_client, $"test-{Guid.NewGuid():N}", options, NullLoggerFactory.Instance);
        return _provider;
    }

    private static ContentReference Text(string fileName, string text) => new()
    {
        Content = new MemoryStream(Encoding.UTF8.GetBytes(text)),
        ContentType = "text/plain",
        FileName = fileName,
    };

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (_client != null && _provider != null)
            _ = await _client.DeleteBlobContainerAsync(_provider.ContainerName);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreContentAsync_ThenGetContentAsync_ReturnsSameContentAndType()
    {
        // Stage
        var provider = CreateProvider();
        await provider.StoreContentAsync(Text("a.txt", "hello"), new Dictionary<string, string> { ["owner"] = "test" });

        // Test
        var content = await provider.GetContentAsync("a.txt");

        // Assert
        Assert.IsNotNull(content);
        Assert.AreEqual("text/plain", content.ContentType);
        using var reader = new StreamReader(content.Content);
        Assert.AreEqual("hello", await reader.ReadToEndAsync());

        // Verify
        var metaData = await provider.GetContentMetaDataAsync("a.txt");
        Assert.AreEqual("test", metaData!.MetaData!["owner"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task GetContentAsync_MissingBlob_ReturnsNull()
    {
        // Stage
        var provider = CreateProvider();

        // Test
        var content = await provider.GetContentAsync("missing.txt");

        // Assert
        Assert.IsNull(content);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreContentAsync_ExistingBlobWithoutOverwrite_Throws()
    {
        // Stage
        var provider = CreateProvider();
        await provider.StoreContentAsync(Text("a.txt", "one"));

        // Test
        var second = async () => await provider.StoreContentAsync(Text("a.txt", "two"));

        // Assert
        await Assert.ThrowsExactlyAsync<global::Azure.RequestFailedException>(second);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreContentMetaDataAsync_MissingBlob_ReturnsFalse()
    {
        // Stage
        var provider = CreateProvider();

        // Test
        var stored = await provider.StoreContentMetaDataAsync(new ContentMetaDataReference { FileName = "missing.txt", ContentType = "text/plain", MetaData = new Dictionary<string, string>() });

        // Assert
        Assert.IsFalse(stored);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task QueryContent_TwoBlobs_ListsBothNames()
    {
        // Stage
        var provider = CreateProvider();
        await provider.StoreContentAsync(Text("a.txt", "one"));
        await provider.StoreContentAsync(Text("b.txt", "two"));

        // Test
        var names = provider.QueryContent().Select(c => c.FileName).OrderBy(n => n, StringComparer.Ordinal).ToList();

        // Assert
        CollectionAssert.AreEqual(new[] { "a.txt", "b.txt" }, names);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task DeleteContentAsync_ExistingBlob_RemovesIt()
    {
        // Stage
        var provider = CreateProvider();
        await provider.StoreContentAsync(Text("a.txt", "one"));

        // Test
        await provider.DeleteContentAsync("a.txt");

        // Assert
        Assert.IsNull(await provider.GetContentAsync("a.txt"));
    }
}
