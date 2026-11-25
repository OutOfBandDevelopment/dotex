using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class DataAccessControllerTests
{
    public required TestContext TestContext { get; set; }

    private static Mock<ReadOnlyQueryRunner> Runner(bool configured)
    {
        var runner = new Mock<ReadOnlyQueryRunner>(Options.Create(new DataAccessOptions())) { CallBase = false };
        runner.SetupGet(r => r.IsConfigured).Returns(configured);
        return runner;
    }

    private static int? Status(ActionResult<DataQueryResultModel> result) => (result.Result as ObjectResult)?.StatusCode;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Query_NotConfigured_ReturnsNotFoundAndRunsNothing()
    {
        // Stage
        var runner = Runner(false);

        // Test
        var result = await new DataAccessController(runner.Object).Query(new DataQueryRequestModel { Sql = "SELECT 1" }, CancellationToken.None);
        TestContext.WriteLine($"status {Status(result)}");

        // Assert
        Assert.AreEqual(StatusCodes.Status404NotFound, Status(result));
        runner.Verify(r => r.RunAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("DROP TABLE t")]
    [DataRow("SELECT 1; SELECT 2")]
    [DataRow("SELECT * INTO x FROM t")]
    public async Task Query_NotASingleSelect_ReturnsBadRequestAndRunsNothing(string sql)
    {
        // Stage
        var runner = Runner(true);

        // Test
        var result = await new DataAccessController(runner.Object).Query(new DataQueryRequestModel { Sql = sql }, CancellationToken.None);
        TestContext.WriteLine($"{sql} -> {Status(result)}");

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, Status(result));
        runner.Verify(r => r.RunAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Query_Select_ReturnsTheRunnerResult()
    {
        // Stage
        var runner = Runner(true);
        var expected = new DataQueryResultModel { Columns = ["n"], Rows = [new List<string?> { "1" }] };
        runner.Setup(r => r.RunAsync("SELECT 1 AS n", It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        // Test
        var result = await new DataAccessController(runner.Object).Query(new DataQueryRequestModel { Sql = "SELECT 1 AS n" }, CancellationToken.None);

        // Assert
        Assert.AreSame(expected, result.Value);
    }
}
