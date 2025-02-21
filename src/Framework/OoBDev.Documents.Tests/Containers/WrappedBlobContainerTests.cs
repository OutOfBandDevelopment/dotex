using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Documents.Tests.Containers;

[TestClass]
public class WrappedBlobContainerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task DeleteContentAsyncTest()
    {
        var path = "test path";

        var mockrepo = new MockRepository(MockBehavior.Strict);
        var mockFactory = mockrepo.Create<IBlobContainerFactory>();
        var mockBlobContainer = mockrepo.Create<IBlobContainer<NoTag>>();

        mockFactory.Setup(s => s.Create<NoTag>()).Returns(mockBlobContainer.Object);
        mockBlobContainer.Setup(s => s.DeleteContentAsync(path)).Returns(Task.CompletedTask);

        var container = new WrappedBlobContainer<NoTag>(mockFactory.Object);

        await container.DeleteContentAsync(path);

        mockrepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetContentAsyncTest()
    {
        var path = "test path";
        var data = new ContentReference()
        {
            Content = new MemoryStream(),
            FileName = "file name",
            ContentType = "content type",
        };

        var mockrepo = new MockRepository(MockBehavior.Strict);
        var mockFactory = mockrepo.Create<IBlobContainerFactory>();
        var mockBlobContainer = mockrepo.Create<IBlobContainer<NoTag>>();

        mockFactory.Setup(s => s.Create<NoTag>()).Returns(mockBlobContainer.Object);
        mockBlobContainer.Setup(s => s.GetContentAsync(path)).Returns(Task.FromResult<ContentReference?>(data));

        var container = new WrappedBlobContainer<NoTag>(mockFactory.Object);

        var result = await container.GetContentAsync(path);

        Assert.AreEqual(data, result);

        mockrepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetContentMetaDataAsyncTest()
    {
        var path = "test path";
        var data = new ContentMetaDataReference()
        {
            FileName = "file name",
            ContentType = "content type",
        };

        var mockrepo = new MockRepository(MockBehavior.Strict);
        var mockFactory = mockrepo.Create<IBlobContainerFactory>();
        var mockBlobContainer = mockrepo.Create<IBlobContainer<NoTag>>();

        mockFactory.Setup(s => s.Create<NoTag>()).Returns(mockBlobContainer.Object);
        mockBlobContainer.Setup(s => s.GetContentMetaDataAsync(path)).Returns(Task.FromResult<ContentMetaDataReference?>(data));

        var container = new WrappedBlobContainer<NoTag>(mockFactory.Object);

        var result = await container.GetContentMetaDataAsync(path);

        Assert.AreEqual(data, result);

        mockrepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreContentMetaDataAsyncTest()
    {
        var data = new ContentMetaDataReference()
        {
            FileName = "file name",
            ContentType = "content type",
        };

        var mockrepo = new MockRepository(MockBehavior.Strict);
        var mockFactory = mockrepo.Create<IBlobContainerFactory>();
        var mockBlobContainer = mockrepo.Create<IBlobContainer<NoTag>>();

        mockFactory.Setup(s => s.Create<NoTag>()).Returns(mockBlobContainer.Object);
        mockBlobContainer.Setup(s => s.StoreContentMetaDataAsync(data)).Returns(Task.FromResult(true));

        var container = new WrappedBlobContainer<NoTag>(mockFactory.Object);

        var result = await container.StoreContentMetaDataAsync(data);

        Assert.IsTrue(result);

        mockrepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreContentAsyncTest()
    {
        var data = new ContentReference()
        {
            Content = new MemoryStream(),
            FileName = "file name",
            ContentType = "content type",
        };
        var dict = new Dictionary<string, string>();

        var mockrepo = new MockRepository(MockBehavior.Strict);
        var mockFactory = mockrepo.Create<IBlobContainerFactory>();
        var mockBlobContainer = mockrepo.Create<IBlobContainer<NoTag>>();

        mockFactory.Setup(s => s.Create<NoTag>()).Returns(mockBlobContainer.Object);
        mockBlobContainer.Setup(s => s.StoreContentAsync(data, dict, true)).Returns(Task.CompletedTask);

        var container = new WrappedBlobContainer<NoTag>(mockFactory.Object);

        await container.StoreContentAsync(data, dict, true);

        mockrepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void QueryContentTest()
    {
        var data = new[]{ new ContentMetaDataReference()
        {
            FileName = "file name",
            ContentType = "content type",
        }}.AsQueryable();

        var mockrepo = new MockRepository(MockBehavior.Strict);
        var mockFactory = mockrepo.Create<IBlobContainerFactory>();
        var mockBlobContainer = mockrepo.Create<IBlobContainer<NoTag>>();

        mockFactory.Setup(s => s.Create<NoTag>()).Returns(mockBlobContainer.Object);
        mockBlobContainer.Setup(s => s.QueryContent()).Returns(data);

        var container = new WrappedBlobContainer<NoTag>(mockFactory.Object);

        var result = container.QueryContent();

        CollectionAssert.AreEquivalent(data.ToArray(), result.ToArray());

        mockrepo.VerifyAll();
    }

    public class NoTag
    {
    }
}
