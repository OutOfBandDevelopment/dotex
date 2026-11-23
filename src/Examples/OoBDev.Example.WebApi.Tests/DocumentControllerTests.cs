using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Documents;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Documents.Packaging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Mock.Of<IBlobContainer<DocumentController.Summaries>>(),
            Mock.Of<OoBDev.Documents.Packaging.IDocumentPackager>());

        // Test
        var list = controller.List();

        // Assert
        CollectionAssert.AreEqual(new[] { "A.txt", "b.pdf" }, list.Select(d => d.FileName).ToArray());
        Assert.AreEqual("application/pdf", list[1].ContentType);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Package_StoredDocuments_ReturnsAZipWithEachOne()
    {
        // Stage
        var docs = new Mock<IBlobContainer<DocumentController.Documents>>();
        docs.Setup(d => d.QueryContent()).Returns(new[]
        {
            new ContentMetaDataReference { FileName = "b.txt", ContentType = "text/plain" },
            new ContentMetaDataReference { FileName = "a.txt", ContentType = "text/plain" },
        }.AsQueryable());
        docs.Setup(d => d.GetContentAsync(It.IsAny<string>())).Returns((string n) => Task.FromResult<ContentReference?>(
            new ContentReference { Content = new MemoryStream(Encoding.UTF8.GetBytes("text of " + n)), ContentType = "text/plain", FileName = n }));
        var services = new ServiceCollection();
        services.TryAddDocumentServices();
        var controller = new DocumentController(
            Mock.Of<IDocumentConversion>(),
            Mock.Of<IDocumentTypeTools>(),
            NullLogger<DocumentController>.Instance,
            docs.Object,
            Mock.Of<IBlobContainer<DocumentController.Summaries>>(),
            services.BuildServiceProvider().GetRequiredService<IDocumentPackager>());

        // Test
        var result = await controller.Package(CancellationToken.None);

        // Assert
        var file = (FileStreamResult)result;
        Assert.AreEqual("application/zip", file.ContentType);
        using var zip = new ZipArchive(file.FileStream);
        CollectionAssert.AreEqual(new[] { "a.txt", "b.txt" }, zip.Entries.Select(e => e.FullName).ToArray());
        using var reader = new StreamReader(zip.GetEntry("a.txt")!.Open());
        Assert.AreEqual("text of a.txt", await reader.ReadToEndAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Package_NothingStored_IsNotFound()
    {
        var docs = new Mock<IBlobContainer<DocumentController.Documents>>();
        docs.Setup(d => d.QueryContent()).Returns(Array.Empty<ContentMetaDataReference>().AsQueryable());
        var controller = new DocumentController(Mock.Of<IDocumentConversion>(), Mock.Of<IDocumentTypeTools>(), NullLogger<DocumentController>.Instance, docs.Object, Mock.Of<IBlobContainer<DocumentController.Summaries>>(), Mock.Of<IDocumentPackager>());
        Assert.IsInstanceOfType<NotFoundResult>(await controller.Package(CancellationToken.None));
    }
}
