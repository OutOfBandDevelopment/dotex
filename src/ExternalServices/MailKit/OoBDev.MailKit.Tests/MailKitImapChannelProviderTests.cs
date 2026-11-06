using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
public class MailKitImapChannelProviderTests
{
    private sealed record TestChannel(string Name, string Protocol, string Provider) : IChannel;

    private static readonly TestChannel Channel = new("inbox", MailKitGlobals.EmailProtocol, MailKitGlobals.ImapProviderName);

    private sealed class Fixture
    {
        public Mock<IMailFolder> Inbox { get; } = new();
        public Mock<IImapClient> Client { get; } = new();
        public MailKitImapChannelProvider Provider { get; }

        public Fixture(params uint[] unread)
        {
            Inbox.SetupGet(i => i.Name).Returns("INBOX");
            Inbox.SetupGet(i => i.UidValidity).Returns(7u);
            Inbox.Setup(i => i.OpenAsync(It.IsAny<FolderAccess>(), It.IsAny<CancellationToken>())).ReturnsAsync(FolderAccess.ReadOnly);
            Inbox.Setup(i => i.SearchAsync(It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<UniqueId>)unread.Select(u => new UniqueId(u)).ToList());
            Inbox.Setup(i => i.GetMessageAsync(It.IsAny<UniqueId>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
                .ReturnsAsync((UniqueId id, CancellationToken _, ITransferProgress? _) => new MimeMessage { MessageId = $"m{id.Id}", Subject = $"S{id.Id}" });
            Client.SetupGet(c => c.Inbox).Returns(Inbox.Object);

            var factory = new Mock<IImapClientFactory>();
            factory.Setup(f => f.CreateAsync()).ReturnsAsync(Client.Object);
            var mime = new Mock<IMimeMessageFactory>();
            mime.Setup(m => m.ToReceived(It.IsAny<MimeMessage>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((MimeMessage m, string server, string path) => new ReceivedEmailMessageModel
                {
                    ReferenceId = m.MessageId,
                    Subject = m.Subject,
                    Server = server,
                    Path = path,
                });
            Provider = new MailKitImapChannelProvider(
                factory.Object,
                mime.Object,
                Options.Create(new MailKitImapClientOptions { Host = "imap.example.test", Port = 993 }),
                new FakeTimeProvider(),
                NullLogger<MailKitImapChannelProvider>.Instance);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_ReturnsUnreadMessagesWithReceiptsWithoutMarkingThemRead()
    {
        var f = new Fixture(3, 4);

        var polled = await f.Provider.PollAsync(Channel, 10);

        Assert.AreEqual(2, polled.Count);
        Assert.AreEqual("m3", polled[0].Message.MessageId);
        Assert.AreEqual("7:3", polled[0].Receipt);
        var email = (IEmailMessage)polled[1].Message;
        Assert.AreEqual("S4", email.Subject);
        Assert.AreEqual("imap.example.test:993", email.Metadata["server"]);
        Assert.AreEqual("INBOX", email.Metadata["path"]);
        f.Inbox.Verify(i => i.OpenAsync(FolderAccess.ReadOnly, It.IsAny<CancellationToken>()), Times.Once);
        f.Inbox.Verify(i => i.StoreAsync(It.IsAny<IList<UniqueId>>(), It.IsAny<IStoreFlagsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_LimitsToMaxMessages()
    {
        var f = new Fixture(1, 2, 3);

        var polled = await f.Provider.PollAsync(Channel, 2);

        Assert.AreEqual(2, polled.Count);
        f.Inbox.Verify(i => i.GetMessageAsync(It.IsAny<UniqueId>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Exactly(2));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AcknowledgeAsync_MarksMatchingReceiptsSeen_AndSkipsStaleOnes()
    {
        var f = new Fixture();
        IList<UniqueId>? stored = null;
        f.Inbox.Setup(i => i.StoreAsync(It.IsAny<IList<UniqueId>>(), It.IsAny<IStoreFlagsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IList<UniqueId>, IStoreFlagsRequest, CancellationToken>((ids, _, _) => stored = ids)
            .ReturnsAsync((IList<UniqueId>)[]);

        await f.Provider.AcknowledgeAsync(Channel, ["7:3", "7:4", "6:9", "junk"]);

        CollectionAssert.AreEqual(new uint[] { 3, 4 }, stored!.Select(u => u.Id).ToArray());
        f.Inbox.Verify(i => i.OpenAsync(FolderAccess.ReadWrite, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AcknowledgeAsync_NoReceipts_DoesNotConnect()
    {
        var f = new Fixture();

        await f.Provider.AcknowledgeAsync(Channel, []);

        f.Client.VerifyGet(c => c.Inbox, Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_IsRejected_AndCanSendIsFalse()
    {
        var f = new Fixture();
        var message = new EmailMessage { To = ["a@example.test"] };

        var result = await f.Provider.SendAsync(Channel, message);

        Assert.AreEqual(SendFailureKind.Rejected, result.Failure?.Kind);
        Assert.IsFalse(await f.Provider.CanSendAsync(Channel, message));
        Assert.AreEqual("mailkit-imap", f.Provider.ProviderName);
    }
}
