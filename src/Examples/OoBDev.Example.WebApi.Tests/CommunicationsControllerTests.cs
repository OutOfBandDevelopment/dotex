using System;
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
        var controller = new CommunicationsController(send.Object, Mock.Of<IMessageQueueSender<EmailMessageModel>>());

        var result = await controller.SendAnonymous(Model(), CancellationToken.None);

        Assert.IsTrue(result.Value!.Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAuthorize_Rejected_ReturnsBadGatewayProblem()
    {
        var send = new Mock<ISendService>();
        send.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<IMessage>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(Result(false));
        var controller = new CommunicationsController(send.Object, Mock.Of<IMessageQueueSender<EmailMessageModel>>());

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
        var controller = new CommunicationsController(Mock.Of<ISendService>(), queue.Object);

        var id = await controller.Enqueue(Model());

        Assert.AreEqual("queued-1", id);
    }
}
