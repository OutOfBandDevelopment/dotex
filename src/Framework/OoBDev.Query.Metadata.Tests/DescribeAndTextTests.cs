using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class DescribeAndTextTests
{
    [TestMethod]
    public void Describe_ListsKindsFacetsAndNeverSql()
    {
        var json = JsonSerializer.Serialize(Describer.Describe(Support.Registry));

        StringAssert.Contains(json, "photos");
        StringAssert.Contains(json, "album");
        Assert.IsFalse(json.Contains("r.name", StringComparison.Ordinal));
        Assert.IsFalse(json.Contains("JOIN", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Describe_Predicates_ListValues()
    {
        var photos = Describer.Describe(Support.Registry).Kinds.Single(k => k.Name == "photos");

        CollectionAssert.AreEquivalent(new[] { "hide", "only" }, photos.Predicates["deleted"]);
        CollectionAssert.Contains(photos.Operators, "tag");
    }

    [TestMethod]
    public void OpenApi_HasKindEnumAndFacetExtensions()
    {
        var doc = OpenApiBuilder.Build(Support.Registry, "Test", "1").ToJsonString();

        StringAssert.Contains(doc, "\"3.1.0\"");
        StringAssert.Contains(doc, "PhotoRow");
        StringAssert.Contains(doc, "x-filterable");
        StringAssert.Contains(doc, "/q");
        Assert.IsFalse(doc.Contains("r.name", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Text_KindAndSearchAndClauses_ParseToRequest()
    {
        var r = QueryText.Parse("photos beach \"red car\" by -taken,name top 25 count", Support.Registry, "photos");

        Assert.AreEqual("photos", r.From);
        Assert.AreEqual("beach \"red car\"", r.Search);
        CollectionAssert.AreEqual(new[] { "-taken", "name" }, r.Sort);
        Assert.AreEqual(25, r.Top);
        Assert.IsTrue(r.Count);
    }

    [TestMethod]
    public void Text_WithoutKindWord_UsesDefault()
    {
        var r = QueryText.Parse("tag:beach", Support.Registry, "photos");

        Assert.AreEqual("photos", r.From);
        Assert.AreEqual("tag:beach", r.Search);
    }

    [TestMethod]
    public void Text_AsFacetsBy_SetsShape()
    {
        var r = QueryText.Parse("photos as facets by album", Support.Registry, "photos");

        Assert.AreEqual("facets", r.Shape);
        CollectionAssert.AreEqual(new[] { "album" }, r.By);
    }

    [TestMethod]
    public void Text_AndJsonForms_CompileToSameSql()
    {
        var text = Support.Compile(QueryText.Parse("photos tag:beach by -taken top 10", Support.Registry, "photos"));
        var json = Support.Compile(new QueryRequest { From = "photos", Search = "tag:beach", Sort = ["-taken"], Top = 10 });

        Assert.AreEqual(json.Sql, text.Sql);
    }

    [TestMethod]
    public void Text_BadClauses_Are400()
    {
        foreach (var s in new[] { "photos top", "photos top x", "photos by", "photos as", "photos as pivot", "photos as facets", "photos top 5 junk" })
            Assert.ThrowsExactly<QueryException>(() => QueryText.Parse(s, Support.Registry, "photos"), s);
    }

    [TestMethod]
    public void Registry_KindsAreScanned()
    {
        CollectionAssert.IsSubsetOf(new[] { "photos", "admin" }, Support.Registry.Kinds.Select(k => k.Name).ToArray());
    }

    [TestMethod]
    public void AddMetadataQuery_ResolvesEngineWithValidatedOptions()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Query:Metadata:CursorKey"] = Support.Key }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IQueryDialect, TestDialect>();
        services.AddSingleton<IQueryExecutor, FakeExecutor>();
        services.TryAddMetadataQuery(config, typeof(PhotoRow).Assembly);

        using var provider = services.BuildServiceProvider();

        Assert.IsNotNull(provider.GetRequiredService<QueryEngine>());
    }
}
