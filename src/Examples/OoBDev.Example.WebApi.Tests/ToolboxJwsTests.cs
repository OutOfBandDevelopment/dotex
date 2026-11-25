using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ToolboxJwsTests
{
    private const string Secret = "0123456789abcdef0123456789abcdef";
    private readonly ToolboxController _controller = new(null!);

    private string Sign(string payload = "{\"sub\":\"ann\"}") =>
        _controller.JwsSign(new JwsSignRequestModel { Payload = payload, Secret = Secret }).Value!.Result;

    [TestMethod]
    public void JwsRead_SignedWithSecret_IsVerifiedAndShowsThePayload()
    {
        // Stage
        var token = Sign();

        // Test
        var read = _controller.JwsRead(new JwsReadRequestModel { Token = token, Secret = Secret }).Value!;

        // Assert
        Assert.IsTrue(read.Verified);
        Assert.AreEqual("{\"sub\":\"ann\"}", read.Payload);
        StringAssert.Contains(read.Header, "HS256");
    }

    [TestMethod]
    public void JwsRead_WrongSecret_IsNotVerified()
    {
        var read = _controller.JwsRead(new JwsReadRequestModel { Token = Sign(), Secret = "ffffffffffffffffffffffffffffffff" }).Value!;

        Assert.IsFalse(read.Verified);
    }

    [TestMethod]
    public void JwsRead_NoSecret_DecodesWithoutVerifying()
    {
        var read = _controller.JwsRead(new JwsReadRequestModel { Token = Sign() }).Value!;

        Assert.IsNull(read.Verified);
    }

    [TestMethod]
    public void JwsRead_NotAToken_IsBadRequest()
    {
        var result = _controller.JwsRead(new JwsReadRequestModel { Token = "abc" });

        Assert.AreEqual(400, ((ObjectResult)result.Result!).StatusCode);
    }

    [TestMethod]
    public void JwsSign_ShortSecret_IsBadRequest()
    {
        var result = _controller.JwsSign(new JwsSignRequestModel { Payload = "{}", Secret = "short" });

        Assert.AreEqual(400, ((ObjectResult)result.Result!).StatusCode);
    }
}
