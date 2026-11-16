using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Evaluation;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;

namespace OoBDev.Search.Tests.Evaluation;

[TestClass]
public class RelevanceMetricsTests
{
    private static readonly string[] _ranked = ["a", "b", "c", "d"];

    private static HashSet<string> Set(params string[] ids) => new(ids, StringComparer.Ordinal);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void RecallAtK_CountsOnlyTheLeadingResults()
    {
        Assert.AreEqual(0.5, RelevanceMetrics.RecallAtK(_ranked, Set("a", "d"), 2));
        Assert.AreEqual(1.0, RelevanceMetrics.RecallAtK(_ranked, Set("a", "d"), 4));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void RecallAtK_NothingRelevant_IsOne() =>
        Assert.AreEqual(1.0, RelevanceMetrics.RecallAtK(_ranked, Set(), 3));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void RecallAtK_ZeroK_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => RelevanceMetrics.RecallAtK(_ranked, Set("a"), 0));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReciprocalRank_IsOneOverTheFirstHitRank()
    {
        Assert.AreEqual(1.0 / 3, RelevanceMetrics.ReciprocalRank(_ranked, Set("c", "d")), 1e-12);
        Assert.AreEqual(0.0, RelevanceMetrics.ReciprocalRank(_ranked, Set("z")));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MeanReciprocalRank_AveragesTheQueries()
    {
        var mrr = RelevanceMetrics.MeanReciprocalRank(
        [
            (_ranked, Set("a")),
            (_ranked, Set("b")),
        ]);

        Assert.AreEqual(0.75, mrr, 1e-12);
        Assert.AreEqual(0.0, RelevanceMetrics.MeanReciprocalRank([]));
    }
}
