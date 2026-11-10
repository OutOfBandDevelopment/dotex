using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class EndpointRightCatalogTests
{
    private static ActionDescriptor Action(string? route, string[]? methods, params object[] metadata) => new()
    {
        AttributeRouteInfo = route is null ? null : new AttributeRouteInfo { Template = route },
        ActionConstraints = methods is null ? null : [new HttpMethodActionConstraint(methods)],
        EndpointMetadata = metadata,
    };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_RightsAttribute_ListsTheRights()
    {
        var rows = new EndpointRightCatalog().Describe([Action("api/x", ["GET"], new ApplicationRightAttribute("a", "b"), new ApplicationRightAttribute("a", "c"))]);

        var row = rows.Single();
        Assert.AreEqual("rights", row.Access);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, row.Rights);
        Assert.AreEqual("/api/x", row.Route);
        Assert.AreEqual("GET", row.Method);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_AllowAnonymousBeatsRights_ReportsAnonymous()
    {
        var rows = new EndpointRightCatalog().Describe([Action("api/x", ["GET"], new AllowAnonymousAttribute(), new ApplicationRightAttribute("a"))]);

        Assert.AreEqual("anonymous", rows.Single().Access);
        Assert.AreEqual(0, rows.Single().Rights.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_AuthorizeOnly_ReportsAuthenticated()
    {
        var rows = new EndpointRightCatalog().Describe([Action("api/x", ["GET"], new AuthorizeAttribute())]);

        Assert.AreEqual("authenticated", rows.Single().Access);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_NoAttributes_ReportsAnonymous()
    {
        var rows = new EndpointRightCatalog().Describe([Action("api/x", ["GET"])]);

        Assert.AreEqual("anonymous", rows.Single().Access);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_NoMethodConstraint_ReportsAny()
    {
        var rows = new EndpointRightCatalog().Describe([Action("api/x", null)]);

        Assert.AreEqual("ANY", rows.Single().Method);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_TwoMethodsAndRoutes_OneRowEachOrderedByRoute()
    {
        var rows = new EndpointRightCatalog().Describe([Action("api/b", ["GET"]), Action("api/a", ["POST", "PUT"]), Action(null, ["GET"])]);

        CollectionAssert.AreEqual(new[] { "/api/a POST", "/api/a PUT", "/api/b GET" }, rows.Select(r => $"{r.Route} {r.Method}").ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Describe_NullActions_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new EndpointRightCatalog().Describe((IEnumerable<ActionDescriptor>)null!));
    }
}
