using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Amazon.S3.Tests;

/// <summary>Stores, reads, queries and deletes objects through the Moto emulator; there is no test against real S3 (see the project readme).</summary>
[TestClass]
[TestCategory(TestCategories.Integration)]
public class S3BlobMotoIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [IntegrationTestMethod("oobd-test-moto")]
    public async Task RoundTrip_WithMoto_StoresReadsQueriesAndDeletesAsync()
    {
        // Stage
        var options = Options.Create(new S3BlobOptions
        {
            Region = TestContext.GetRequiredProperty<string>("AWS_REGION"),
            AccessKeyId = TestContext.GetRequiredProperty<string>("AWS_ACCESS_KEY_ID"),
            SecretAccessKey = TestContext.GetRequiredProperty<string>("AWS_SECRET_ACCESS_KEY"),
            ServiceUrl = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("MOTO_URL")),
            EnsureContainerExists = true,
        });
        using var client = new S3ClientFactory(options).Create();
        var bucket = $"test-{Guid.NewGuid():N}";
        var provider = new S3BlobContainerProvider(client, bucket, options, NullLoggerFactory.Instance);
        TestContext.WriteLine($"bucket: {bucket}, endpoint: {options.Value.ServiceUrl}");

        try
        {
            // Test
            using var body = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
            await provider.StoreContentAsync(
                new ContentReference { Content = body, ContentType = "text/plain", FileName = "dir/a.txt" },
                new Dictionary<string, string> { ["owner"] = "me" });
            var content = await provider.GetContentAsync("dir/a.txt");
            var meta = await provider.GetContentMetaDataAsync("dir/a.txt");
            var listed = provider.QueryContent().ToList();
            var updated = await provider.StoreContentMetaDataAsync(new ContentMetaDataReference
            {
                ContentType = "text/plain",
                FileName = "dir/a.txt",
                MetaData = new Dictionary<string, string> { ["owner"] = "you" },
            });
            var after = await provider.GetContentMetaDataAsync("dir/a.txt");
            await provider.DeleteContentAsync("dir/a.txt");
            var gone = await provider.GetContentAsync("dir/a.txt");

            // Assert
            Assert.IsNotNull(content);
            using var reader = new StreamReader(content.Content);
            Assert.AreEqual("hello", await reader.ReadToEndAsync(TestContext.CancellationToken));
            Assert.AreEqual("a.txt", content.FileName);
            Assert.AreEqual("me", meta?.MetaData?["owner"]);
            Assert.AreEqual("dir/a.txt", listed.Single().FileName);
            Assert.IsTrue(updated);
            Assert.AreEqual("you", after?.MetaData?["owner"]);
            Assert.IsNull(gone);
        }
        finally
        {
            await client.DeleteBucketAsync(bucket, TestContext.CancellationToken);
        }
    }
}
