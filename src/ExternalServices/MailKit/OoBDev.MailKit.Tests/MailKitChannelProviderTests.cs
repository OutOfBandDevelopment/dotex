using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.MailKit.Services;
using OoBDev.TestUtilities;

namespace OoBDev.MailKit.Tests;

[TestClass]
public class MailKitChannelProviderTests
{
    private sealed record TestChannel(string Name, string Protocol, string Provider) : IChannel;

    private static readonly TestChannel Channel = new("mail", MailKitGlobals.EmailProtocol, MailKitGlobals.ProviderName);

    private static (MailKitChannelProvider Provider, Mock<ISmtpClient> Client, Mock<IMimeMessageFactory> Mime) Create()
    {
        var client = new Mock<ISmtpClient>();
        var mime = new Mock<IMimeMessageFactory>();
        mime.Setup(m => m.CreateAsync(It.IsAny<EmailMessageModel>(), It.IsAny<IReadOnlyList<IEmailAttachment>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new MimeMessage());
        var factory = new Mock<ISmtpClientFactory>();
        factory.Setup(f => f.CreateAsync()).ReturnsAsync(client.Object);
        return (new MailKitChannelProvider(mime.Object, factory.Object, new FakeTimeProvider(), NullLogger<MailKitChannelProvider>.Instance), client, mime);
    }

    private static EmailMessage Email() => new()
    {
        MessageId = "m1",
        CorrelationId = "c1",
        To = ["to@example.test"],
        From = "from@example.test",
        ReplyTo = "reply@example.test",
        Subject = "Hello",
        TextContent = "Hi",
    };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_Email_ReturnsSuccessWithProviderReference()
    {
        // Stage
        var (provider, client, mime) = Create();
        client.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>())).ReturnsAsync("250 queued");

        // Test
        var result = await provider.SendAsync(Channel, Email());

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("250 queued", result.ProviderMessageId);
        Assert.AreEqual("c1", result.CorrelationId);
        mime.Verify(m => m.CreateAsync(It.Is<EmailMessageModel>(e => e.ReferenceId == "m1" && e.Headers["Reply-To"] == "reply@example.test" && e.ToAddresses.Count == 1), It.IsAny<IReadOnlyList<IEmailAttachment>>(), It.IsAny<CancellationToken>()), Times.Once);
        client.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_NotAnEmail_IsRejected()
    {
        var (provider, _, _) = Create();

        var result = await provider.SendAsync(Channel, new Mock<IMessage>().Object);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(SendFailureKind.Rejected, result.Failure?.Kind);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CanSendAsync_ChecksMessageType()
    {
        var (provider, _, _) = Create();

        Assert.IsTrue(await provider.CanSendAsync(Channel, Email()));
        Assert.IsFalse(await provider.CanSendAsync(Channel, new Mock<IMessage>().Object));
        Assert.AreEqual("mailkit", provider.ProviderName);
        CollectionAssert.AreEqual(new[] { "email" }, provider.SupportedProtocols);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(SmtpStatusCode.MailboxBusy, SendFailureKind.Transient)]
    [DataRow(SmtpStatusCode.InsufficientStorage, SendFailureKind.Transient)]
    [DataRow(SmtpStatusCode.MailboxUnavailable, SendFailureKind.Transient)]
    [DataRow(SmtpStatusCode.MailboxNameNotAllowed, SendFailureKind.Permanent)]
    public async Task SendAsync_SmtpCommandError_IsClassifiedByStatus(SmtpStatusCode status, SendFailureKind expected)
    {
        var (provider, client, _) = Create();
        client.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .ThrowsAsync(new SmtpCommandException(SmtpErrorCode.UnexpectedStatusCode, status, "nope"));

        var result = await provider.SendAsync(Channel, Email());

        Assert.IsFalse(result.Success);
        Assert.AreEqual(expected, result.Failure?.Kind);
        Assert.AreEqual((int)status, result.Failure?.StatusCode);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_ConnectionDrops_IsTransient()
    {
        var (provider, client, _) = Create();
        client.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .ThrowsAsync(new IOException("reset"));

        var result = await provider.SendAsync(Channel, Email());

        Assert.AreEqual(SendFailureKind.Transient, result.Failure?.Kind);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_MessageWithoutBody_IsPermanent()
    {
        var (provider, _, mime) = Create();
        mime.Setup(m => m.CreateAsync(It.IsAny<EmailMessageModel>(), It.IsAny<IReadOnlyList<IEmailAttachment>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotSupportedException("You must provide Text or Html Body"));

        var result = await provider.SendAsync(Channel, Email());

        Assert.AreEqual(SendFailureKind.Permanent, result.Failure?.Kind);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_Cancelled_Propagates()
    {
        var (provider, client, _) = Create();
        client.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => provider.SendAsync(Channel, Email()));
    }
}
