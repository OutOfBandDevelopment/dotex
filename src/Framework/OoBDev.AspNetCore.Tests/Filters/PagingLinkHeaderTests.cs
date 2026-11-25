using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.TestUtilities;

namespace OoBDev.AspNetCore.Tests.Filters;

[TestClass]
public class PagingLinkHeaderTests
{
    private static HttpRequest Request(string query)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/items";
        context.Request.QueryString = new QueryString(query);
        return context.Request;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_SinglePage_ReturnsNull()
    {
        Assert.IsNull(PagingLinkHeader.Build(Request(""), 1, 1));
        Assert.IsNull(PagingLinkHeader.Build(Request(""), 1, 0));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_FirstPage_HasFirstNextLastOnly()
    {
        var header = PagingLinkHeader.Build(Request("?pageSize=10"), 1, 3)!;

        Assert.AreEqual(
            "</api/items?pageSize=10&currentPage=1>; rel=\"first\", </api/items?pageSize=10&currentPage=2>; rel=\"next\", </api/items?pageSize=10&currentPage=3>; rel=\"last\"",
            header);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_MiddlePage_HasPrevAndNext_ReplacesOldPage()
    {
        var header = PagingLinkHeader.Build(Request("?currentPage=2&q=a%20b"), 2, 3)!;

        StringAssert.Contains(header, "currentPage=1>; rel=\"prev\"");
        StringAssert.Contains(header, "currentPage=3>; rel=\"next\"");
        StringAssert.Contains(header, "q=a%20b");
        Assert.IsFalse(header.Contains("currentPage=2"), header);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_LastPage_HasNoNext()
    {
        var header = PagingLinkHeader.Build(Request(""), 3, 3)!;

        StringAssert.Contains(header, "rel=\"prev\"");
        Assert.IsFalse(header.Contains("rel=\"next\""), header);
    }
}
