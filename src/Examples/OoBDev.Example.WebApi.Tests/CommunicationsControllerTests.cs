using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Models;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class CommunicationsControllerTests
{
    private static SendResult Result(bool success)
        => new(success, "id", null, null, DateTimeOffset.UnixEpoch, success ? null : new SendFailure(SendFailureKind.Rejected, "no such mailbox"));

    private static EmailMessageModel Model() => new() { FromAddress = "a@example.test", ToAddresses = ["b@example.test"], Subject = "Hi", ReferenceId = "ref-1" };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAnonymous_Accepted_ReturnsTheSendResultOnTheEmailChannel()
    {
        var send = new Mock<ISendService>();
        send.Setup(s => s.SendAsync("email", It.Is<IMessage>(m => m.MessageId == "ref-1"), null, It.IsAny<CancellationToken>())).ReturnsAsync(Result(true));
        var controller = new CommunicationsController(send.Object, Mock.Of<IMessageQueueSender<EmailMessageModel>>(), Mock.Of<IReceiveService>());

        var result = await controller.SendAnonymous(Model(), CancellationToken.None);

        Assert.IsTrue(result.Value!.Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAuthorize_Rejected_ReturnsBadGatewayProblem()
    {
        var send = new Mock<ISendService>();
        send.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<IMessage>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(Result(false));
        var controller = new CommunicationsController(send.Object, Mock.Of<IMessageQueueSender<EmailMessageModel>>(), Mock.Of<IReceiveService>());

        var result = await controller.SendAuthorize(Model(), CancellationToken.None);

        var problem = (ObjectResult)result.Result!;
        Assert.AreEqual(StatusCodes.Status502BadGateway, problem.StatusCode);
        StringAssert.Contains(((ProblemDetails)problem.Value!).Title, "Rejected");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Enqueue_SendsTheModelWithItsReferenceId()
    {
        var queue = new Mock<IMessageQueueSender<EmailMessageModel>>();
        queue.Setup(q => q.SendAsync(It.IsAny<object>(), "ref-1", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>())).ReturnsAsync("queued-1");
        var controller = new CommunicationsController(Mock.Of<ISendService>(), queue.Object, Mock.Of<IReceiveService>());

        var id = await controller.Enqueue(Model());

        Assert.AreEqual("queued-1", id);
    }
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollInbox_MailWaiting_ReturnsModelsWithShortPreview()
    {
        var receive = new Mock<IReceiveService>();
        IReadOnlyList<IMessage> polled = [new EmailMessage { From = "a@example.test", To = ["b@example.test"], Subject = "Hello", TextContent = new string('x', 500) }];
        receive.Setup(r => r.PollAsync("inbox", 5, It.IsAny<CancellationToken>())).ReturnsAsync(polled);
        var controller = new CommunicationsController(Mock.Of<ISendService>(), Mock.Of<IMessageQueueSender<EmailMessageModel>>(), receive.Object);

        var result = await controller.PollInbox(5, CancellationToken.None);

        var item = result.Value!.Single();
        TestContext.WriteLine($"Subject {item.Subject}, preview {item.Preview.Length} chars");
        Assert.AreEqual("Hello", item.Subject);
        Assert.AreEqual(160, item.Preview.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollInbox_MaxOutOfRange_IsClamped()
    {
        var receive = new Mock<IReceiveService>();
        receive.Setup(r => r.PollAsync("inbox", 50, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var controller = new CommunicationsController(Mock.Of<ISendService>(), Mock.Of<IMessageQueueSender<EmailMessageModel>>(), receive.Object);

        var result = await controller.PollInbox(999, CancellationToken.None);

        Assert.IsEmpty(result.Value!);
        receive.Verify(r => r.PollAsync("inbox", 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollInbox_ChannelMissing_ReturnsNotFoundProblem()
    {
        var receive = new Mock<IReceiveService>();
        receive.Setup(r => r.PollAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotSupportedException("no poll"));
        var controller = new CommunicationsController(Mock.Of<ISendService>(), Mock.Of<IMessageQueueSender<EmailMessageModel>>(), receive.Object);

        var result = await controller.PollInbox(10, CancellationToken.None);

        Assert.AreEqual(StatusCodes.Status404NotFound, ((ObjectResult)result.Result!).StatusCode);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollInbox_ServerDown_ReturnsBadGatewayProblem()
    {
        var receive = new Mock<IReceiveService>();
        receive.Setup(r => r.PollAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new global::System.IO.IOException("refused"));
        var controller = new CommunicationsController(Mock.Of<ISendService>(), Mock.Of<IMessageQueueSender<EmailMessageModel>>(), receive.Object);

        var result = await controller.PollInbox(10, CancellationToken.None);

        Assert.AreEqual(StatusCodes.Status502BadGateway, ((ObjectResult)result.Result!).StatusCode);
    }
}
