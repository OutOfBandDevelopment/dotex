using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class WebhooksControllerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static WebhooksController Controller(Mock<IReceiveService> receive)
    {
        var http = new DefaultHttpContext();
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"event\":\"delivered\"}"));
        http.Request.Headers["X-Signature"] = "abc";
        http.Request.QueryString = new QueryString("?token=t");
        return new WebhooksController(receive.Object, new FakeTimeProvider(Now)) { ControllerContext = new ControllerContext { HttpContext = http } };
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Receive_Processed_ReturnsNoContentAndPassesTheRawRequest()
    {
        var receive = new Mock<IReceiveService>();
        WebhookRequest? seen = null;
        receive.Setup(r => r.HandleWebhookAsync("mail", It.IsAny<WebhookRequest>(), It.IsAny<CancellationToken>()))
            .Callback<string, WebhookRequest, CancellationToken>((_, request, _) => seen = request)
            .ReturnsAsync(new WebhookResult(true, 1, [], null));

        var result = await Controller(receive).Receive("mail", CancellationToken.None);

        Assert.IsInstanceOfType<NoContentResult>(result);
        Assert.AreEqual("{\"event\":\"delivered\"}", Encoding.UTF8.GetString(seen!.Body.Span));
        Assert.AreEqual("abc", seen.Headers["x-signature"]);
        Assert.AreEqual("t", seen.Query["token"]);
        Assert.AreEqual(Now, seen.ReceivedAt);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Channel 'x' not found", StatusCodes.Status404NotFound)]
    [DataRow("Invalid signature", StatusCodes.Status400BadRequest)]
    public async Task Receive_Failed_ReturnsProblemWithMatchingStatus(string error, int status)
    {
        var receive = new Mock<IReceiveService>();
        receive.Setup(r => r.HandleWebhookAsync(It.IsAny<string>(), It.IsAny<WebhookRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebhookResult(false, 0, [], error));

        var result = await Controller(receive).Receive("mail", CancellationToken.None);

        Assert.AreEqual(status, ((ObjectResult)result).StatusCode);
    }
}
