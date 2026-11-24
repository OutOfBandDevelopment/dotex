using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;

namespace OoBDev.Amazon.S3.Tests;

[TestClass]
public class S3BlobContainerProviderTests
{
    public TestContext TestContext { get; set; } = null!;

    private static S3BlobContainerProvider Build(IAmazonS3 client, string name = "Invoices") =>
        new(client, name, Options.Create(new S3BlobOptions()), NullLoggerFactory.Instance);

    private static AmazonS3Exception NotFound() => new("missing", global::Amazon.Runtime.ErrorType.Sender, "NoSuchKey", "id", HttpStatusCode.NotFound);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ContainerName_IsTheLowerCasedBucketName()
    {
        var provider = Build(Mock.Of<IAmazonS3>());

        TestContext.WriteLine($"container: {provider.ContainerName}");

        Assert.AreEqual("invoices", provider.ContainerName);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetContentAsync_MissingKey_ReturnsNull()
    {
        // Stage
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectAsync("invoices", "a.txt", It.IsAny<CancellationToken>())).ThrowsAsync(NotFound());

        // Test
        var content = await Build(client.Object).GetContentAsync("a.txt");

        // Assert
        Assert.IsNull(content);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetContentMetaDataAsync_MissingKey_ReturnsNull()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectMetadataAsync("invoices", "a.txt", It.IsAny<CancellationToken>())).ThrowsAsync(NotFound());

        Assert.IsNull(await Build(client.Object).GetContentMetaDataAsync("a.txt"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_ExistingKeyWithoutOverwrite_Throws()
    {
        // Stage
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectMetadataAsync("invoices", "a.txt", It.IsAny<CancellationToken>())).ReturnsAsync(new GetObjectMetadataResponse());
        using var stream = new MemoryStream([1]);

        // Test
        var store = () => Build(client.Object).StoreContentAsync(new ContentReference { Content = stream, ContentType = "text/plain", FileName = "a.txt" });

        // Assert
        await Assert.ThrowsExactlyAsync<IOException>(store);
        client.Verify(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsync_NewKey_PutsTheObjectWithMetadata()
    {
        // Stage
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectMetadataAsync("invoices", "a.txt", It.IsAny<CancellationToken>())).ThrowsAsync(NotFound());
        PutObjectRequest? put = null;
        client.Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutObjectRequest, CancellationToken>((r, _) => put = r)
            .ReturnsAsync(new PutObjectResponse());
        using var stream = new MemoryStream([1, 2]);

        // Test
        await Build(client.Object).StoreContentAsync(
            new ContentReference { Content = stream, ContentType = "text/plain", FileName = "a.txt" },
            new Dictionary<string, string> { ["owner"] = "me" });

        // Assert
        Assert.IsNotNull(put);
        Assert.AreEqual("invoices", put.BucketName);
        Assert.AreEqual("a.txt", put.Key);
        Assert.AreEqual("text/plain", put.ContentType);
        Assert.AreEqual("me", put.Metadata["owner"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreContentMetaDataAsync_MissingKey_ReturnsFalse()
    {
        var client = new Mock<IAmazonS3>();
        client.Setup(c => c.GetObjectMetadataAsync("invoices", "a.txt", It.IsAny<CancellationToken>())).ThrowsAsync(NotFound());

        var stored = await Build(client.Object).StoreContentMetaDataAsync(new ContentMetaDataReference { ContentType = "text/plain", FileName = "a.txt" });

        Assert.IsFalse(stored);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void QueryContent_ListsEveryPage()
    {
        // Stage
        var client = new Mock<IAmazonS3>();
        client.SetupSequence(c => c.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListObjectsV2Response { S3Objects = [new S3Object { Key = "a" }], IsTruncated = true, NextContinuationToken = "t" })
            .ReturnsAsync(new ListObjectsV2Response { S3Objects = [new S3Object { Key = "b" }], IsTruncated = false });
        client.Setup(c => c.GetObjectMetadataAsync("invoices", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetObjectMetadataResponse());

        // Test
        var names = Build(client.Object).QueryContent().Select(m => m.FileName).ToList();

        // Assert
        CollectionAssert.AreEqual(new[] { "a", "b" }, names);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(null, null, false)]
    [DataRow("us-east-1", null, true)]
    [DataRow(null, "http://127.0.0.1:4566", true)]
    public void Factory_OffersTheProviderOnlyWhileConfigured(string? region, string? url, bool offered)
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["S3BlobOptions:Region"] = region,
            ["S3BlobOptions:ServiceUrl"] = url,
        }).Build();
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Mock.Of<IAmazonS3>())
            .TryAddAwsS3BlobServices(configuration)
            .BuildServiceProvider();

        // Test
        var created = provider.GetRequiredService<IBlobContainerProviderFactory>().Create("Invoices");

        // Assert
        TestContext.WriteLine($"region: {region}, url: {url}, offered: {created is not null}");
        Assert.AreEqual(offered, created is not null);
        if (offered)
        {
            Assert.AreEqual("invoices", created!.ContainerName);
        }
    }
}
