using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Html.Tests;

[TestClass]
public class JsonPathQueryTests
{
    private static readonly JsonPathResolver Resolver = new(JsonNode.Parse("""
        { "a": { "b": [10, 20, 30], "s": "x", "n": null, "t": true }, "list": [ { "k": 1 }, { "k": 2, "name": "two" } ] }
        """));

    private static async Task<List<JsonNode?>> Select(string path) =>
        [.. (IReadOnlyList<JsonNode?>)await Resolver.ItemSelector(path).ConfigureAwait(false)];

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Select_DottedIndexedAndWildcardPaths()
    {
        Assert.AreEqual(20, (int)(await Select("$.a.b[1]"))[0]!);
        Assert.AreEqual(3, (await Select("a.b[*]")).Count);
        Assert.AreEqual(2, (await Select("list[*].k")).Count);
        Assert.AreEqual("x", (string)(await Select("@['a'].s"))[0]!);
        Assert.AreEqual(1, (await Select("")).Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Select_MissingGivesNoMatchAndJsonNullGivesNullMatch()
    {
        Assert.AreEqual(0, (await Select("a.missing")).Count);
        Assert.AreEqual(0, (await Select("a.b[9]")).Count);
        var nulls = await Select("a.n");
        Assert.AreEqual(1, nulls.Count);
        Assert.IsNull(nulls[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Select_FilterComparesNumbersAndStrings()
    {
        Assert.AreEqual(1, (await Select("list[?(@.k > 1)]")).Count);
        Assert.AreEqual(1, (await Select("list[?(@.name == 'two')]")).Count);
        Assert.AreEqual(1, (await Select("list[?(@.name != 'two')]")).Count);
        Assert.AreEqual(1, (await Select("list[?(@.name)]")).Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Test_HoldsForTheNodeOrAnyDescendant()
    {
        Assert.IsTrue(await Resolver.Test("@.a.t == true"));
        Assert.IsTrue(await Resolver.Test("@.k == 2"));
        Assert.IsFalse(await Resolver.Test("@.k == 3"));
        Assert.IsTrue(await Resolver.Test("a.s"));
        Assert.IsFalse(await Resolver.Test("a.n"));
        Assert.IsTrue(await Resolver.Test(""));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Select_UnsupportedSegmentThrows() =>
        await Assert.ThrowsExactlyAsync<FormatException>(() => Select("a[bad]"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Factory_ServesResolversAndRejectsTheRest()
    {
        var factory = new JsonInstanceFactory();
        var resolver = await factory.GetPathResolver(new { Name = "n" });
        Assert.AreEqual("n", (string)((IReadOnlyList<JsonNode?>)await resolver.ItemSelector("Name")).Single()!);
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => factory.GetFormatter(new object()));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => factory.GetTemplateTransform("text/html"));
    }
}
