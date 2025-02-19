using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace OoBDev.System.Tests.Linq.Expressions;

[TestClass]
public class SkipMemberOnNullExpressionVisitorTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void NullableCollectionQueryTest_Composite()
    {
        var query = new SearchQuery
        {
            SearchTerm = "Hello",
        };
        TestContext.AddResult(query);

        var rawData = new[]
        {
            new {Items= (string[]?)["Hello"] , Name=(string?)""},
            new {Items= (string[]?)[] , Name=(string?)"" },
            new {Items= (string[]?)null  , Name=(string?)""},
            new {Items= (string[]?)[]  , Name=(string?)null},
        }.AsQueryable();
        TestContext.AddResult(rawData);

        var queryResults = QueryBuilder.Execute(rawData, query, new SkipMemberOnNullExpressionVisitor());
        TestContext.AddResult(queryResults);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void NullableCollectionQueryTest_Array()
    {
        var query = new SearchQuery
        {
            SearchTerm = "Hello",
        };
        TestContext.AddResult(query);

        var rawData = new[]
        {
                new {Items= (string[]?)[]},
                 //new {Name=(string?)null},
        }.AsQueryable();
        TestContext.AddResult(rawData);

        var queryResults = QueryBuilder.Execute(rawData, query, new SkipMemberOnNullExpressionVisitor());
        TestContext.AddResult(queryResults);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void NullableCollectionQueryTest_Element()
    {
        var query = new SearchQuery
        {
            SearchTerm = "Hello",
        };
        TestContext.AddResult(query);

        var rawData = new[]
        {
             new {Name=(string?)null},
        }.AsQueryable();
        TestContext.AddResult(rawData);

        var queryResults = QueryBuilder.Execute(rawData, query, new SkipMemberOnNullExpressionVisitor());
        TestContext.AddResult(queryResults);
    }
}
