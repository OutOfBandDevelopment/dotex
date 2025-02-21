using OoBDev.Apache.Tika.Detectors;
using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Detectors;

[TestClass]
public class TikaContentTypeDetectorTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task DetectContentTypeAsyncTest()
    {
        var stream = new MemoryStream();
        var expectedContentType = "fake/type";

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockClient = mockRepo.Create<IApacheTikaClient>();

        mockClient.Setup(s => s.DetectStreamAsync(stream)).Returns(ValueTask.FromResult(expectedContentType));

        var detector = new TikaContentTypeDetector(
            mockClient.Object,
            TestLogger.CreateLogger<TikaContentTypeDetector>()
            );
        var contentType = await detector.DetectContentTypeAsync(stream);

        Assert.AreEqual(expectedContentType, contentType);

        mockRepo.VerifyAll();
    }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("accessible_epub_3.epub", "application/epub+zip")]
    [DataRow("Sample.pdf", "application/pdf")]
    [DataRow("sample1.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [DataRow("sample-1.rtf", "application/rtf")]
    [DataRow("sample2.doc", "application/msword")]
    [DataRow("sample2.odt", "application/vnd.oasis.opendocument.text")]
    public async Task DevLocalDetectContentTypeAsyncTest(string resourceName, string expectedContentType)
    {
        using var stream = GetType().Assembly.GetManifestResourceStream($"OoBDev.Apache.Tika.Tests.TestData.{resourceName}")
            ?? throw new NotSupportedException($"Not found {resourceName}");

        var serviceProvider = ApacheTikaTestHarness.GetServiceProvider(new Dictionary<string, string?>()
        {
            { "ApacheTikaClientOptions:Url","http://127.0.0.1:9998"}
        });

        var detector = ActivatorUtilities.CreateInstance<TikaContentTypeDetector>(serviceProvider);
        var contentType = await detector.DetectContentTypeAsync(stream);

        Assert.AreEqual(expectedContentType, contentType);
    }
}
