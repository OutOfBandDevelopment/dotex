using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Amazon.Ses.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class SesChannelProviderTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed record Channel(string Name, string Protocol, string Provider) : IChannel;

    private static readonly IChannel Chan = new Channel("mail", "email", AwsSesGlobals.ProviderName);

    private static (SesChannelProvider Provider, Mock<IAmazonSimpleEmailServiceV2> Client) Create(SesOptions? options = null)
    {
        var client = new Mock<IAmazonSimpleEmailServiceV2>();
        var factory = new Mock<ISesClientFactory>();
        factory.Setup(f => f.Create()).Returns(client.Object);
        var provider = new SesChannelProvider(factory.Object, Options.Create(options ?? new SesOptions { DefaultFromAddress = "default@example.test" }), new FakeTimeProvider(new DateTimeOffset(2026, 11, 6, 0, 0, 0, TimeSpan.Zero)), NullLogger<SesChannelProvider>.Instance);
        return (provider, client);
    }

    private static EmailMessage Email(params IEmailAttachment[] attachments) => new()
    {
        MessageId = "m1",
        To = ["to@example.test"],
        Cc = ["cc@example.test"],
        Subject = "Hi",
        TextContent = "text",
        HtmlContent = "<b>html</b>",
        ReplyTo = "reply@example.test",
        Attachments = attachments,
    };

    [TestMethod]
    public async Task SendAsync_Email_MapsRequestAndReturnsProviderIdAsync()
    {
        // Stage
        var (provider, client) = Create(new SesOptions { DefaultFromAddress = "default@example.test", ConfigurationSetName = "events" });
        SendEmailRequest? sent = null;
        client.Setup(c => c.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SendEmailRequest, CancellationToken>((r, _) => sent = r)
            .ReturnsAsync(new SendEmailResponse { MessageId = "ses-1" });

        // Test
        var result = await provider.SendAsync(Chan, Email(), TestContext.CancellationToken);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("ses-1", result.ProviderMessageId);
        Assert.IsNotNull(sent);
        Assert.AreEqual("default@example.test", sent.FromEmailAddress);
        Assert.AreEqual("to@example.test", sent.Destination.ToAddresses.Single());
        Assert.AreEqual("cc@example.test", sent.Destination.CcAddresses.Single());
        Assert.AreEqual("reply@example.test", sent.ReplyToAddresses.Single());
        Assert.AreEqual("Hi", sent.Content.Simple.Subject.Data);
        Assert.AreEqual("<b>html</b>", sent.Content.Simple.Body.Html.Data);
        Assert.AreEqual("events", sent.ConfigurationSetName);
        Assert.AreEqual("m1", sent.EmailTags.Single().Value);
    }

    [TestMethod]
    public async Task SendAsync_Attachments_AreSentAsRawContentAsync()
    {
        // Stage
        var (provider, client) = Create();
        SendEmailRequest? sent = null;
        client.Setup(c => c.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SendEmailRequest, CancellationToken>((r, _) => sent = r)
            .ReturnsAsync(new SendEmailResponse { MessageId = "ses-2" });

        // Test
        await provider.SendAsync(Chan, Email(EmailAttachment.FromBytes("a.bin", "application/octet-stream", [1, 2, 3]), EmailAttachment.FromBytes("logo.png", "image/png", [9], "logo1")), TestContext.CancellationToken);

        // Assert
        var attachments = sent!.Content.Simple.Attachments;
        Assert.AreEqual(2, attachments.Count);
        Assert.AreEqual("a.bin", attachments[0].FileName);
        Assert.AreEqual(AttachmentContentDisposition.ATTACHMENT, attachments[0].ContentDisposition);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, ((MemoryStream)attachments[0].RawContent).ToArray());
        Assert.AreEqual(AttachmentContentDisposition.INLINE, attachments[1].ContentDisposition);
        Assert.AreEqual("logo1", attachments[1].ContentId);
    }

    [TestMethod]
    public async Task SendAsync_NotAnEmail_IsRejectedAsync()
    {
        // Stage
        var (provider, _) = Create();
        var message = new Mock<IMessage>();
        message.SetupGet(m => m.MessageId).Returns("x");

        // Test
        var result = await provider.SendAsync(Chan, message.Object, TestContext.CancellationToken);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure!.Kind);
        Assert.IsFalse(await provider.CanSendAsync(Chan, message.Object, TestContext.CancellationToken));
        Assert.IsTrue(await provider.CanSendAsync(Chan, Email(), TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task SendAsync_NoBodyOrNoSender_IsPermanentAsync()
    {
        // Stage
        var (provider, _) = Create(new SesOptions());

        // Test
        var noSender = await provider.SendAsync(Chan, Email(), TestContext.CancellationToken);
        var noBody = await provider.SendAsync(Chan, new EmailMessage { From = "a@example.test", To = ["b@example.test"] }, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(SendFailureKind.Permanent, noSender.Failure!.Kind);
        Assert.AreEqual(SendFailureKind.Permanent, noBody.Failure!.Kind);
    }

    [TestMethod]
    public async Task SendAsync_ServiceErrors_AreClassifiedAsync()
    {
        // Stage
        var (provider, client) = Create();
        client.SetupSequence(c => c.SendEmailAsync(It.IsAny<SendEmailRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TooManyRequestsException("slow down"))
            .ThrowsAsync(new MessageRejectedException("no"))
            .ThrowsAsync(new AmazonServiceException("boom") { StatusCode = HttpStatusCode.ServiceUnavailable })
            .ThrowsAsync(new AmazonServiceException("denied") { StatusCode = HttpStatusCode.Forbidden });

        // Test
        var kinds = new SendFailureKind[4];
        for (var i = 0; i < kinds.Length; i++)
        {
            kinds[i] = (await provider.SendAsync(Chan, Email(), TestContext.CancellationToken)).Failure!.Kind;
        }

        // Assert
        CollectionAssert.AreEqual(new[] { SendFailureKind.RateLimited, SendFailureKind.Rejected, SendFailureKind.Transient, SendFailureKind.Permanent }, kinds);
    }

    [TestMethod]
    public void Metadata_NamesTheProvider()
    {
        // Test
        var (provider, _) = Create();

        // Assert
        Assert.AreEqual("aws-ses", provider.ProviderName);
        CollectionAssert.AreEqual(new[] { "email" }, provider.SupportedProtocols);
    }
}
