using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ToolboxCodecTests
{
    private readonly ToolboxController _controller = new(null!);

    private ActionResult<ToolResultModel> Run(string codec, string value, bool decode = false) =>
        _controller.Codec(new CodecRequestModel { Codec = codec, Value = value, Decode = decode });

    [TestMethod]
    public void Codec_RomanEncode_ReturnsNumeral() =>
        Assert.AreEqual("MCMXCIV", Run("roman", "1994").Value!.Result);

    [TestMethod]
    public void Codec_RomanDecode_ReturnsNumber() =>
        Assert.AreEqual("1994", Run("roman", "mcmxciv", decode: true).Value!.Result);

    [TestMethod]
    public void Codec_Base32RoundTrips()
    {
        var encoded = Run("base32", "hello").Value!.Result;

        Assert.AreEqual("hello", Run("base32", encoded, decode: true).Value!.Result);
    }

    [TestMethod]
    public void Codec_MorseRoundTrips()
    {
        var encoded = Run("morse", "sos").Value!.Result;

        Assert.AreEqual("... --- ...", encoded);
        Assert.AreEqual("SOS", Run("morse", encoded, decode: true).Value!.Result);
    }

    [TestMethod]
    public void Codec_UnknownCodec_IsBadRequest() =>
        Assert.AreEqual(400, ((ObjectResult)Run("rot13", "x").Result!).StatusCode);

    [TestMethod]
    public void Codec_RomanEncodeNotANumber_IsBadRequest() =>
        Assert.AreEqual(400, ((ObjectResult)Run("roman", "abc").Result!).StatusCode);
}
