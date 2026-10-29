using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.TestUtilities;
using System.Collections.Generic;

namespace OoBDev.AspNetCore.Tests.Filters;

[TestClass]
public class AcceptsQueryAttributeTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void OnResultExecuting_SearchAction_AddsAcceptQueryHeader()
    {
        // Stage
        var http = new DefaultHttpContext();
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var context = new ResultExecutingContext(action, new List<IFilterMetadata>(), new OkResult(), controller: new object());

        // Test
        new AcceptsQueryAttribute().OnResultExecuting(context);

        // Assert
        Assert.AreEqual("\"application/json\"", http.Response.Headers["Accept-Query"].ToString());
    }
}
