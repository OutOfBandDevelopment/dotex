using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class ReadOnlyQueryRunnerIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private ReadOnlyQueryRunner Runner(int maxRows) => new(Options.Create(new DataAccessOptions
    {
        ConnectionString = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING"),
        MaxRows = maxRows,
    }));

    [IntegrationTestMethod("oobd-test-sqlserver")]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_Select_ReturnsColumnsAndRows()
    {
        // Test
        var result = await Runner(10).RunAsync("SELECT 1 AS n, 'a' AS s, NULL AS missing", CancellationToken.None);
        TestContext.WriteLine($"{string.Join(",", result.Columns)} / {string.Join(",", result.Rows[0])} in {result.ElapsedMs} ms");

        // Assert
        Assert.AreEqual("n,s,missing", string.Join(",", result.Columns));
        Assert.HasCount(1, result.Rows);
        Assert.AreEqual("1", result.Rows[0][0]);
        Assert.IsNull(result.Rows[0][2]);
        Assert.IsFalse(result.Truncated);
    }

    [IntegrationTestMethod("oobd-test-sqlserver")]
    [TestCategory(TestCategories.Integration)]
    public async Task RunAsync_MoreRowsThanTheCap_IsTruncated()
    {
        // Test
        var result = await Runner(3).RunAsync("SELECT TOP (10) object_id FROM sys.all_objects", CancellationToken.None);
        TestContext.WriteLine($"{result.Rows.Count} rows, truncated {result.Truncated}");

        // Assert
        Assert.HasCount(3, result.Rows);
        Assert.IsTrue(result.Truncated);
    }
}
