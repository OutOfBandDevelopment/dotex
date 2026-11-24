using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.TestUtilities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.AISearch.Tests;

[TestClass]
public class AzureAISearchVectorStoreCollectionCatalogTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ListCollectionsAsync_ReturnsTheIndexNamesSorted()
    {
        // Stage
        var index = new Mock<SearchIndexClient>();
        index.Setup(i => i.GetIndexNamesAsync(It.IsAny<CancellationToken>()))
            .Returns(AsyncPageable<string>.FromPages([Page<string>.FromValues(["zeta", "alpha"], null, Mock.Of<Response>())]));
        var catalog = new AzureAISearchVectorStoreCollectionCatalog(index.Object);

        // Test
        var names = await catalog.ListCollectionsAsync(TestContext.CancellationToken);

        // Assert
        TestContext.WriteLine($"indexes: {string.Join(", ", names)}");
        CollectionAssert.AreEqual(new[] { "alpha", "zeta" }, names.ToArray());
    }
}
