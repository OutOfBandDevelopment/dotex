using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OpenSearch.Net;
using System.Linq;

namespace OoBDev.OpenSearch.Tests;

[TestClass]
public class OpenSearchClientFactoryTests
{
    private static OpenSearchClientFactory CreateFactory(bool useTls = false, bool allowInvalidCertificate = false) =>
        new(Options.Create(new OpenSearchOptions
        {
            HostName = "search.test",
            Port = 9201,
            IndexName = "docs",
            UseTls = useTls,
            AllowInvalidCertificate = allowInvalidCertificate,
        }));

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Create_Default_ConnectsOverHttp()
    {
        // Stage
        var factory = CreateFactory();

        // Test
        var client = (OpenSearchLowLevelClient)factory.Create();

        // Assert
        var node = client.Settings.ConnectionPool.Nodes.First();
        Assert.AreEqual("http://search.test:9201/", node.Uri.ToString());
        Assert.IsNull(client.Settings.ServerCertificateValidationCallback);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Create_UseTls_ConnectsOverHttps()
    {
        // Stage
        var factory = CreateFactory(useTls: true);

        // Test
        var client = (OpenSearchLowLevelClient)factory.Create();

        // Assert
        var node = client.Settings.ConnectionPool.Nodes.First();
        Assert.AreEqual("https://search.test:9201/", node.Uri.ToString());
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Create_AllowInvalidCertificate_SetsCertificateCallback()
    {
        // Stage
        var factory = CreateFactory(useTls: true, allowInvalidCertificate: true);

        // Test
        var client = (OpenSearchLowLevelClient)factory.Create();

        // Assert
        Assert.IsNotNull(client.Settings.ServerCertificateValidationCallback);
    }
}
