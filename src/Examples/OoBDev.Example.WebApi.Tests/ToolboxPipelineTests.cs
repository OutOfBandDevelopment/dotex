using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ToolboxPipelineTests
{
    public required TestContext TestContext { get; set; }

    private readonly ToolboxController _controller = new(null!);

    private ActionResult<IReadOnlyList<PipelineStepResultModel>> Run(string value, params string[] steps) =>
        _controller.Pipeline(new PipelineRequestModel { Value = value, Steps = steps });

    [TestMethod]
    public void Pipeline_EncodeThenDecode_ReturnsTheInput()
    {
        var result = ((OkObjectResult)Run("hello world", "gzip", "base64", "base64-decode", "gunzip").Result!).Value as List<PipelineStepResultModel>;

        TestContext.WriteLine($"steps={string.Join(",", result!.Select(r => r.Step))}");
        Assert.AreEqual(4, result!.Count);
        Assert.AreEqual("hello world", result[^1].Text);
    }

    [TestMethod]
    public void Pipeline_Sha256Hex_MatchesKnownDigest()
    {
        var result = ((OkObjectResult)Run("abc", "sha256", "hex").Result!).Value as List<PipelineStepResultModel>;

        Assert.AreEqual("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", result![^1].Text);
    }

    [TestMethod]
    public void Pipeline_BinaryResult_HasNoText()
    {
        var result = ((OkObjectResult)Run("abc", "sha256").Result!).Value as List<PipelineStepResultModel>;

        Assert.IsNull(result![0].Text);
        Assert.AreEqual(32, result[0].Length);
    }

    [TestMethod]
    public void Pipeline_UnknownStep_Is400()
        => Assert.AreEqual(400, ((ObjectResult)Run("abc", "rot13").Result!).StatusCode);

    [TestMethod]
    public void Pipeline_BadInputForStep_Is400()
        => Assert.AreEqual(400, ((ObjectResult)Run("not hex", "hex-decode").Result!).StatusCode);

    [TestMethod]
    public void Pipeline_NoSteps_Is400()
        => Assert.AreEqual(400, ((ObjectResult)Run("abc").Result!).StatusCode);

    [TestMethod]
    public void Pipeline_Gunzip_OfNonGzip_Is400()
        => Assert.AreEqual(400, ((ObjectResult)Run("abc", "gunzip").Result!).StatusCode);

    [TestMethod]
    public void PipelineSteps_ListsEveryKey()
        => CollectionAssert.Contains(_controller.PipelineSteps().ToList(), "gzip");
}
