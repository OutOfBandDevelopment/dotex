using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.MessageQueueing.Tests;

[TestClass]
public class EmailMessageHandlerTests
{
    private static IMessageContext Context(string? correlation = "corr")
    {
        var context = new Mock<IMessageContext>();
        context.SetupProperty(c => c.CorrelationId, correlation);
        return context.Object;
    }

    private static EmailMessageHandler Handler(ISendService? send, string channel = "mail")
        => new(NullLogger<EmailMessageHandler>.Instance, Options.Create(new EmailMessageHandlerOptions { ChannelName = channel }), send);

    private static EmailMessageModel Model() => new()
    {
        FromAddress = "from@example.test",
        ToAddresses = ["to@example.test"],
        CcAddresses = ["cc@example.test"],
        Subject = "Hello",
        TextContent = "Hi",
        Headers = { ["Reply-To"] = "reply@example.test" },
    };

    private static SendResult Result(bool success)
        => new(success, "id", null, null, DateTimeOffset.UnixEpoch, success ? null : new SendFailure(SendFailureKind.Permanent, "bad"));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_Model_SendsMappedEmailOnConfiguredChannel()
    {
        var send = new Mock<ISendService>();
        IMessage? sent = null;
        send.Setup(s => s.SendAsync("mail", It.IsAny<IMessage>(), null, It.IsAny<CancellationToken>()))
            .Callback<string, IMessage, SendOptions?, CancellationToken>((_, m, _, _) => sent = m)
            .ReturnsAsync(Result(true));

        await Handler(send.Object).HandleAsync(Model(), Context());

        var email = (IEmailMessage)sent!;
        Assert.AreEqual("corr", email.MessageId);
        Assert.AreEqual("corr", email.CorrelationId);
        Assert.AreEqual("from@example.test", email.From);
        CollectionAssert.AreEqual(new[] { "to@example.test" }, email.To);
        CollectionAssert.AreEqual(new[] { "cc@example.test" }, email.Cc);
        Assert.AreEqual("reply@example.test", email.ReplyTo);
        Assert.AreEqual("Hello", email.Subject);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_ReferenceId_BecomesMessageId()
    {
        var send = new Mock<ISendService>();
        send.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<IMessage>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(Result(true));
        var model = Model();
        model.ReferenceId = "ref-1";

        await Handler(send.Object).HandleAsync(model, Context());

        send.Verify(s => s.SendAsync("mail", It.Is<IMessage>(m => m.MessageId == "ref-1"), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_FailedSend_Throws()
    {
        var send = new Mock<ISendService>();
        send.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<IMessage>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(Result(false));

        await Assert.ThrowsAsync<InvalidOperationException>(() => Handler(send.Object).HandleAsync(Model(), Context()));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_NoSendService_DropsTheMessage()
        => await Handler(null).HandleAsync(Model(), Context());

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_OtherPayload_IsIgnored()
    {
        var send = new Mock<ISendService>(MockBehavior.Strict);

        await Handler(send.Object).HandleAsync(new object(), Context());

        send.VerifyNoOtherCalls();
    }
}
