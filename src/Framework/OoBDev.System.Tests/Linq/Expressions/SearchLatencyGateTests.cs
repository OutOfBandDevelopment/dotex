using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.Tests.Linq.TestTargets;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.System.Tests.Linq.Expressions;

/// <summary>Performance gate for the search syntax: log over 500 ms, fail over 750 ms (testing rule).</summary>
[TestClass]
public class SearchLatencyGateTests
{
    private const int LogMilliseconds = 500;
    private const int FailMilliseconds = 750;

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public void SearchAndFilter_FiftyThousandRows_StaysUnderTheLatencyGate()
    {
        // Stage
        var rows = Enumerable.Range(0, 50_000).Select(i => new TestTargetModel(i)).ToList();
        var builder = new ExpressionTreeBuilder<TestTargetModel>();

        // Test
        var stopwatch = global::System.Diagnostics.Stopwatch.StartNew();
        var search = builder.BuildExpression("Name49999", global::System.StringComparison.OrdinalIgnoreCase, isSearchTerm: true)!;
        var filter = builder.GetPredicateExpression(
            nameof(TestTargetModel.Name),
            new FilterParameter { EqualTo = "Name49999" },
            global::System.StringComparison.OrdinalIgnoreCase,
            isSearchTerm: false)!;
        var hits = rows.AsQueryable().Where(search).Where(filter).ToList();
        stopwatch.Stop();

        // Assert
        TestContext.WriteLine($"search + filter over {rows.Count} rows: {stopwatch.ElapsedMilliseconds} ms ({hits.Count} hits)");
        if (stopwatch.ElapsedMilliseconds > LogMilliseconds)
            TestContext.WriteLine($"WARNING: over the {LogMilliseconds} ms log threshold");
        Assert.IsGreaterThan(0, hits.Count);
        Assert.IsLessThan(FailMilliseconds, stopwatch.ElapsedMilliseconds, "search latency gate");
    }
}
