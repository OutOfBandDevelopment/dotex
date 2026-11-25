using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class SearchScoringTests
{
    private static readonly RagPassage[] Passages =
    [
        new() { Id = "p1", Text = "Messages are put on a queue by an adapter." },
        new() { Id = "p2", Text = "Templates are files named after the template." },
        new() { Id = "p3", Text = "A queue adapter exists for RabbitMQ and a second queue adapter for SQS." },
    ];

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Tokenize_MixedCaseAndPunctuation_ReturnsLowerCaseWords()
        => CollectionAssert.AreEqual(new[] { "hello", "world", "2" }, SearchScoring.Tokenize("Hello, WORLD! 2").ToArray());

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Lexical_TermInSomePassages_RanksMoreFrequentFirstAndDropsNonMatches()
    {
        var hits = SearchScoring.Lexical("queue adapter", Passages);

        CollectionAssert.AreEqual(new[] { "p3", "p1" }, hits.Select(h => h.Id).ToArray());
        Assert.IsTrue(hits[0].Score > hits[1].Score);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Lexical_NoSharedTermOrEmptyQuery_ReturnsNothing()
    {
        Assert.AreEqual(0, SearchScoring.Lexical("zebra", Passages).Count);
        Assert.AreEqual(0, SearchScoring.Lexical("  ", Passages).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Fuse_PassageInBothLists_BeatsPassagesInOne()
    {
        var left = new List<SearchHitModel> { new() { Id = "a", Text = "a" }, new() { Id = "b", Text = "b" } };
        var right = new List<SearchHitModel> { new() { Id = "c", Text = "c" }, new() { Id = "b", Text = "b" } };

        var fused = SearchScoring.Fuse(left, right);

        Assert.AreEqual("b", fused[0].Id);
        Assert.AreEqual(3, fused.Count);
        Assert.AreEqual((1.0 / 62) + (1.0 / 62), fused[0].Score, 1e-12);
    }
}
