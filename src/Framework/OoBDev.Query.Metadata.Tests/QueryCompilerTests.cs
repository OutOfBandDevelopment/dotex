using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class QueryCompilerTests
{
    [TestMethod]
    public void Compile_DefaultRequest_SelectsFromBaseWithKeyLastInSort()
    {
        var q = Support.Compile(new QueryRequest { From = "photos" });

        StringAssert.Contains(q.Sql, "FROM photos r");
        StringAssert.Contains(q.Sql, "JOIN albums a");
        StringAssert.Contains(q.Sql, "ORDER BY");
        StringAssert.Contains(q.Sql, "r.id");
        StringAssert.Contains(q.Sql, "LIMIT 61");
        Assert.AreEqual(60, q.Top);
        Assert.IsNull(q.CountSql);
    }

    [TestMethod]
    public void Compile_FilterValue_IsBoundNeverInlined()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Filters = Support.Filters("""{"name":"x'; DROP TABLE photos;--"}""") });

        Assert.IsFalse(q.Sql.Contains("DROP", StringComparison.Ordinal));
        Assert.IsTrue(q.Parameters.Values.Any(v => v is string s && s.Contains("DROP", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Compile_UndeclaredFacet_Is400NamingIt()
    {
        var ex = Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Filters = Support.Filters("""{"nope":1}""") }));

        Assert.AreEqual(400, ex.Status);
        StringAssert.Contains(ex.Message, "nope");
    }

    [TestMethod]
    public void Compile_NotFilterableFacet_Is400()
    {
        var ex = Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Filters = Support.Filters("""{"secret":"a"}""") }));

        StringAssert.Contains(ex.Message, "secret");
    }

    [TestMethod]
    public void Compile_NotSortableFacet_Is400()
    {
        var ex = Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Sort = ["secret"] }));

        StringAssert.Contains(ex.Message, "secret");
    }

    [TestMethod]
    public void Compile_UnknownKind_ListsKnownKinds()
    {
        var ex = Assert.ThrowsExactly<QueryException>(() => Support.Registry.Get("nothing"));

        StringAssert.Contains(ex.Message, "photos");
    }

    [TestMethod]
    public void Compile_Predicate_AddsDeclaredFragment()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Filters = Support.Filters("""{"deleted":"hide"}""") });

        StringAssert.Contains(q.Sql, "r.deleted_at IS NULL");
    }

    [TestMethod]
    public void Compile_PredicateUnknownValue_Is400()
    {
        Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Filters = Support.Filters("""{"deleted":"zzz"}""") }));
    }

    [TestMethod]
    public void Compile_SearchOperator_UsesDeclaredFragmentAndBindsValue()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Search = "tag:beach" });

        StringAssert.Contains(q.Sql, "EXISTS (SELECT 1 FROM tags t");
        Assert.IsFalse(q.Sql.Contains("beach", StringComparison.Ordinal));
        Assert.IsTrue(q.Parameters.Values.Contains("beach"));
    }

    [TestMethod]
    public void Compile_NegatedSearchOperator_IsNotTrue()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Search = "-tag:beach" });

        StringAssert.Contains(q.Sql, "IS NOT TRUE");
    }

    [TestMethod]
    public void Compile_SearchWords_MatchSearchableFacetsOnly()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Search = "sunset" });

        StringAssert.Contains(q.Sql, "r.name ILIKE");
        Assert.IsFalse(q.Sql.Contains("r.secret ILIKE", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Compile_Count_AddsCountSql()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Count = true });

        Assert.IsNotNull(q.CountSql);
        StringAssert.Contains(q.CountSql, "count");
    }

    [TestMethod]
    public void Compile_TopAboveMax_IsClamped()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Top = 100000 });

        Assert.AreEqual(500, q.Top);
    }

    [TestMethod]
    public void Compile_Facets_GroupsByFacet()
    {
        var q = Support.Compile(new QueryRequest { From = "photos", Shape = "facets", By = ["album"] });

        StringAssert.Contains(q.Sql, "GROUP BY");
    }

    [TestMethod]
    public void Compile_FacetsWithAfter_Is400()
    {
        Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Shape = "facets", By = ["album"], After = "x" }));
    }

    [TestMethod]
    public void Compile_UnknownShape_Is400()
    {
        Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Shape = "pivot" }));
    }

    [TestMethod]
    public void Compile_FacetOperators_AllCompile()
    {
        foreach (var json in new[]
        {
            """{"size":{"gt":1}}""", """{"size":{"gte":1}}""", """{"size":{"lt":9}}""", """{"size":{"lte":9}}""",
            """{"name":{"like":"a"}}""", """{"name":{"nlike":"a"}}""", """{"name":{"ne":"a"}}""", """{"taken":{"exists":true}}""",
            """{"rating":{"in":[1,2]}}""", """{"rating":{"between":[1,5]}}""", """{"rating":3}""", """{"starred":true}""", """{"labels":"a"}""",
        })
        {
            var q = Support.Compile(new QueryRequest { From = "photos", Filters = Support.Filters(json) });
            Assert.IsFalse(string.IsNullOrEmpty(q.Sql), json);
        }
    }

    [TestMethod]
    public void Compile_SearchOperatorsOnFacets_AllCompile()
    {
        foreach (var search in new[] { "size:>1mb", "rating:>=3", "starred:true", "taken:2024", "taken:2024-05", "name:abc", "\"two words\" -skip" })
        {
            var q = Support.Compile(new QueryRequest { From = "photos", Search = search });
            Assert.IsFalse(string.IsNullOrEmpty(q.Sql), search);
        }
    }
}
