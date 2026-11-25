using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DataLoader;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class DataLoadControllerTests
{
    public required TestContext TestContext { get; set; }

    private static DataLoadController Create() => new([new CsvRowReader(), new JsonRowReader()]);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Preview_CsvWithADuplicateKey_SkipsItAndReportsTheCounts()
    {
        // Test
        var result = await Create().Preview(new DataLoadRequestModel("csv", "id,name\n1,Ann\n2,Bo\n1,Ann again", "id"), CancellationToken.None);
        TestContext.WriteLine($"Read={result.Value?.Read} Written={result.Value?.Written} Skipped={result.Value?.Skipped}");

        // Assert
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(3, result.Value.Read);
        Assert.AreEqual(2, result.Value.Written);
        Assert.AreEqual(1, result.Value.Skipped);
        CollectionAssert.AreEquivalent(new[] { "id", "name" }, new List<string>(result.Value.Columns));
        Assert.AreEqual(2, result.Value.Rows.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Preview_JsonWithReplace_KeepsTheLastRow()
    {
        // Test
        var result = await Create().Preview(new DataLoadRequestModel("json", """[{"id":"1","v":"a"},{"id":"1","v":"b"}]""", "id", "Replace"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(1, result.Value.Replaced);
        var vIndex = new List<string>(result.Value.Columns).IndexOf("v");
        Assert.AreEqual("b", result.Value.Rows[0][vIndex]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Preview_FailPolicy_ReportsTheDuplicateAsAnError()
    {
        // Test
        var result = await Create().Preview(new DataLoadRequestModel("csv", "id\n1\n1", "id", "Fail"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(1, result.Value.Errors.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("csv", "", "id", "Skip")]
    [DataRow("csv", "id\n1", " ", "Skip")]
    [DataRow("csv", "id\n1", "id", "Maybe")]
    [DataRow("xml", "<a/>", "id", "Skip")]
    public async Task Preview_InvalidRequest_Returns400(string format, string text, string key, string duplicates)
    {
        // Test
        var result = await Create().Preview(new DataLoadRequestModel(format, text, key, duplicates), CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<ObjectResult>(result.Result);
        Assert.AreEqual(400, ((ObjectResult)result.Result).StatusCode);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Preview_TextOverTheLimit_Returns400()
    {
        // Test
        var result = await Create().Preview(new DataLoadRequestModel("csv", new string('a', DataLoadController.MaxTextLength + 1), "id"), CancellationToken.None);

        // Assert
        Assert.AreEqual(400, ((ObjectResult)result.Result!).StatusCode);
    }
}
