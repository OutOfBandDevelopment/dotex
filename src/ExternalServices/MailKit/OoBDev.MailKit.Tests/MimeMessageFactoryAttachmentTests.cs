using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.MailKit.Services;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.MailKit.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class MimeMessageFactoryAttachmentTests
{
    private static MimeMessageFactory Factory() =>
        new(Options.Create(new MailKitSmtpClientOptions { Host = "smtp.example.test", DefaultFromEmailAddress = "from@example.test" }));

    private static EmailMessageModel Model() => new()
    {
        ToAddresses = ["to@example.test"],
        Subject = "Report",
        TextContent = "See attached",
    };

    private static MimePart[] Parts(MimeMessage message) => [.. message.BodyParts.OfType<MimePart>().Where(p => p.IsAttachment || p.ContentId is not null)];

    [TestMethod]
    public async Task CreateAsync_BytesAndMemoryAttachments_AreAddedAsync()
    {
        // Stage
        byte[] bytes = [1, 2, 3];
        ReadOnlyMemory<byte> memory = new byte[] { 4, 5 };
        IEmailAttachment[] attachments =
        [
            EmailAttachment.FromBytes("a.bin", "application/octet-stream", bytes),
            EmailAttachment.FromMemory("b.txt", "text/plain", memory),
        ];

        // Test
        var message = await Factory().CreateAsync(Model(), attachments, TestContext.CancellationToken);

        // Assert
        var parts = Parts(message);
        Assert.AreEqual(2, parts.Length);
        Assert.AreEqual("a.bin", parts[0].FileName);
        Assert.AreEqual("text/plain", parts[1].ContentType.MimeType);
        using var stream = new MemoryStream();
        await parts[0].Content.DecodeToAsync(stream, TestContext.CancellationToken);
        CollectionAssert.AreEqual(bytes, stream.ToArray());
    }

    [TestMethod]
    public async Task CreateAsync_FileAttachment_ReadsTheTempFileAsync()
    {
        // Stage
        var factory = new TempFileFactory(new ServiceCollection().BuildServiceProvider());
        using var directory = factory.GetTempDirectory();
        var path = Path.Combine(directory.DirectoryPath, "data.csv");
        await File.WriteAllTextAsync(path, "a,b", TestContext.CancellationToken);

        // Test
        var message = await Factory().CreateAsync(Model(), [EmailAttachment.FromFile("data.csv", "text/csv", path)], TestContext.CancellationToken);

        // Assert
        var part = Parts(message).Single();
        using var stream = new MemoryStream();
        await part.Content.DecodeToAsync(stream, TestContext.CancellationToken);
        Assert.AreEqual("a,b", global::System.Text.Encoding.UTF8.GetString(stream.ToArray()));
    }

    [TestMethod]
    public async Task CreateAsync_ContentId_IsInlineAsync()
    {
        // Test
        var message = await Factory().CreateAsync(Model(), [EmailAttachment.FromBytes("logo.png", "image/png", [9], "logo1")], TestContext.CancellationToken);

        // Assert
        var part = Parts(message).Single();
        Assert.AreEqual("logo1", part.ContentId);
        Assert.IsFalse(part.IsAttachment);
    }

    [TestMethod]
    public async Task CreateAsync_NoAttachments_MatchesCreateAsync()
    {
        // Test
        var message = await Factory().CreateAsync(Model(), [], TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(0, Parts(message).Length);
        Assert.AreEqual("Report", message.Subject);
    }

    [TestMethod]
    public void EmailAttachment_BlankName_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => EmailAttachment.FromBytes(" ", "text/plain", []));

    public TestContext TestContext { get; set; } = null!;
}
