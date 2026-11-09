using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Documents;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class DocumentControllerTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_StoredDocuments_ReturnsNameAndTypeOrderedByName()
    {
        // Stage
        var docs = new Mock<IBlobContainer<DocumentController.Documents>>();
        docs.Setup(d => d.QueryContent()).Returns(new[]
        {
            new ContentMetaDataReference { FileName = "b.pdf", ContentType = "application/pdf" },
            new ContentMetaDataReference { FileName = "A.txt", ContentType = "text/plain" },
        }.AsQueryable());
        var controller = new DocumentController(
            Mock.Of<IDocumentConversion>(),
            Mock.Of<IDocumentTypeTools>(),
            NullLogger<DocumentController>.Instance,
            docs.Object,
            Mock.Of<IBlobContainer<DocumentController.Summaries>>());

        // Test
        var list = controller.List();

        // Assert
        CollectionAssert.AreEqual(new[] { "A.txt", "b.pdf" }, list.Select(d => d.FileName).ToArray());
        Assert.AreEqual("application/pdf", list[1].ContentType);
    }
}
