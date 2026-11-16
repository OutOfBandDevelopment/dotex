using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents;
using OoBDev.Documents.Markdown;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Office.Tests;

[TestClass]
public sealed class OfficeFileMarkdownReaderTests
{
    private sealed class FakeConversion(bool succeeds = true) : IDocumentConversion
    {
        public string? SourceType { get; private set; }
        public string? DestinationType { get; private set; }
        public string? SourceText { get; private set; }

        public async Task<bool> ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
        {
            SourceType = sourceContentType;
            DestinationType = destinationContentType;
            using var reader = new StreamReader(source, Encoding.UTF8, leaveOpen: true);
            SourceText = await reader.ReadToEndAsync();
            if (succeeds)
            {
                await destination.WriteAsync(Encoding.UTF8.GetBytes("# Title\r\n\r\ntext"));
            }

            return succeeds;
        }
    }

    private string WriteFile(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"office_{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, "content");
        return path;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [DataRow(".DOC", "application/msword")]
    [DataRow(".odt", "application/vnd.oasis.opendocument.text")]
    [DataRow(".rtf", "application/rtf")]
    [DataRow(".epub", "application/epub+zip")]
    public void Read_KnownExtension_AsksTheChainForMarkdownFromThatType(string extension, string contentType)
    {
        var conversion = new FakeConversion();
        var path = WriteFile(extension);
        try
        {
            var markdown = new OfficeFileMarkdownReader(conversion).Read(path, CancellationToken.None);

            Assert.AreEqual(contentType, conversion.SourceType);
            Assert.AreEqual("text/markdown", conversion.DestinationType);
            Assert.AreEqual("content", conversion.SourceText);
            Assert.AreEqual("# Title\r\n\r\ntext", markdown);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_ChainReturnsFalse_IsEmpty()
    {
        var path = WriteFile(".docx");
        try
        {
            Assert.AreEqual(string.Empty, new OfficeFileMarkdownReader(new FakeConversion(false)).Read(path, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_UnknownExtension_Throws() =>
        Assert.Throws<NotSupportedException>(() => new OfficeFileMarkdownReader(new FakeConversion()).Read("a.xyz", CancellationToken.None));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_Cancelled_Throws()
    {
        var path = WriteFile(".docx");
        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.Throws<OperationCanceledException>(() => new OfficeFileMarkdownReader(new FakeConversion()).Read(path, cts.Token));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddOfficeMarkdown_RegistersOneReaderEvenWhenCalledTwice()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDocumentConversion>(new FakeConversion());
        services.TryAddOfficeMarkdown().TryAddOfficeMarkdown();

        using var provider = services.BuildServiceProvider();

        Assert.AreEqual(1, provider.GetServices<IFileMarkdownReader>().OfType<OfficeFileMarkdownReader>().Count());
    }
}
