using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Fusion;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;

namespace OoBDev.Search.Tests;

[TestClass]
public class FusionTests
{
    private static SearchItem Item(string id, double score, int? vector = null) =>
        new(id, score, new SearchExplanation(Field: "f", VectorIndex: vector));

    private static FusionInput Input(string source, params SearchItem[] items) => new(source, items);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MaxNormalised_TwoSources_GoldenScores()
    {
        var result = new MaxNormalisedFusion().Fuse(
        [
            Input("keyword", Item("a", 10), Item("b", 5)),
            Input("vector", Item("b", 0.8), Item("c", 0.4)),
        ]);

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, result.Select(r => r.ItemId).ToArray());
        Assert.AreEqual(1.0, result[0].Score, 1e-9);
        Assert.AreEqual(1.0, result[1].Score, 1e-9);
        Assert.AreEqual(0.5, result[2].Score, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MaxNormalised_ItemInBothSources_ListsBothScoresAndFusion()
    {
        var result = new MaxNormalisedFusion().Fuse(
        [
            Input("keyword", Item("b", 5)),
            Input("vector", Item("b", 0.8, vector: 3)),
        ]);

        var explanation = result.Single().Explanation;
        Assert.AreEqual(5, explanation.Scores!["keyword"]);
        Assert.AreEqual(0.8, explanation.Scores["vector"]);
        Assert.AreEqual(FusionNames.MaxNormalised, explanation.Fusion);
        Assert.AreEqual(3, explanation.VectorIndex);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MaxNormalised_EmptyAndZeroSources_ScoreZero()
    {
        var result = new MaxNormalisedFusion().Fuse([Input("keyword"), Input("vector", Item("z", 0))]);

        Assert.AreEqual(0, result.Single().Score);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReciprocalRank_GoldenScores()
    {
        var result = new ReciprocalRankFusion(60).Fuse(
        [
            Input("keyword", Item("a", 99), Item("b", 1)),
            Input("vector", Item("b", 0.9), Item("a", 0.1)),
        ]);

        var expected = (1.0 / 61) + (1.0 / 62);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(expected, result[0].Score, 1e-12);
        Assert.AreEqual(expected, result[1].Score, 1e-12);
        CollectionAssert.AreEqual(new[] { "a", "b" }, result.Select(r => r.ItemId).ToArray());
        Assert.AreEqual(FusionNames.ReciprocalRank, result[0].Explanation.Fusion);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReciprocalRank_SingleSourceItem_ScoresOneContribution()
    {
        var result = new ReciprocalRankFusion(10).Fuse([Input("keyword", Item("a", 1), Item("b", 1))]);

        Assert.AreEqual(1.0 / 11, result[0].Score, 1e-12);
        Assert.AreEqual(1.0 / 12, result[1].Score, 1e-12);
    }
}
