using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.Example.WebApi.Tests;

/// <summary>Fails when a controller action of the example API states no access at all, so a new endpoint cannot ship unprotected by accident.</summary>
[TestClass]
public class EndpointRightCoverageTests
{
    private static IEnumerable<(Type Controller, MethodInfo Action)> Actions() =>
        typeof(RightsController).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true } && typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes<HttpMethodAttribute>(true).Any() && !m.IsDefined(typeof(NonActionAttribute), true))
                .Select(m => (t, m)));

    private static bool StatesAccess(Type controller, MethodInfo action)
    {
        static bool Has(ICustomAttributeProvider p) =>
            p.IsDefined(typeof(ApplicationRightAttribute), true)
            || p.IsDefined(typeof(AllowAnonymousAttribute), true)
            || p.IsDefined(typeof(AuthorizeAttribute), true);
        return Has(action) || Has(controller);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Actions_AreDiscovered()
    {
        Assert.IsGreaterThan(20, Actions().Count(), "The scan found too few actions to prove anything.");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EveryAction_StatesItsAccess()
    {
        var missing = Actions()
            .Where(a => !StatesAccess(a.Controller, a.Action))
            .Select(a => $"{a.Controller.Name}.{a.Action.Name}")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.AreEqual(0, missing.Length, "Actions without [ApplicationRight], [Authorize] or [AllowAnonymous]: " + string.Join(", ", missing));
    }
}
