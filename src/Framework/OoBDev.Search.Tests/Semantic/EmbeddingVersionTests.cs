using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Models;
using OoBDev.Search.Semantic;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Search.Tests.Semantic;

[TestClass]
public class EmbeddingVersionTests
{
    private static SearchResultModel Result(string id, IDictionary<string, object>? meta) =>
        new() { ItemId = id, Score = 1f, MetaData = meta };

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Tag_CombinesModelAndDimensions() =>
        Assert.AreEqual("all-MiniLM-L6-v2/384", EmbeddingVersion.Tag("all-MiniLM-L6-v2", 384));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Tag_BlankModel_Throws() =>
        Assert.Throws<ArgumentException>(() => EmbeddingVersion.Tag(" ", 384));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Stamp_KeepsExistingMetadata_AndDoesNotChangeTheInput()
    {
        var input = new Dictionary<string, object> { ["title"] = "a" };

        var stamped = EmbeddingVersion.Stamp(input, "m/3");

        Assert.AreEqual("m/3", stamped[EmbeddingVersion.MetadataKey]);
        Assert.AreEqual("a", stamped["title"]);
        Assert.HasCount(1, input);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Stamp_ReplacesAnOlderTag()
    {
        var stamped = EmbeddingVersion.Stamp(new Dictionary<string, object> { [EmbeddingVersion.MetadataKey] = "old/1" }, "new/2");

        Assert.AreEqual("new/2", stamped[EmbeddingVersion.MetadataKey]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void StaleIds_ReturnsUntaggedAndOtherModelRows()
    {
        var results = new[]
        {
            Result("current", EmbeddingVersion.Stamp(null, "m/3")),
            Result("older", EmbeddingVersion.Stamp(null, "m/2")),
            Result("untagged", new Dictionary<string, object> { ["title"] = "x" }),
            Result("no-metadata", null),
        };

        CollectionAssert.AreEqual(new[] { "older", "untagged", "no-metadata" }, EmbeddingVersion.StaleIds(results, "m/3").ToArray());
    }
}
