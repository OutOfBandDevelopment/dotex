using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AspNetCore.Authorization;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class RightsControllerTests
{
    private static RightsController Controller(ClaimsPrincipal user) => new(
        Mock.Of<IActionDescriptorCollectionProvider>(),
        new EndpointRightCatalog(),
        new ApplicationRightChecker())
    {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
    };

    private static ClaimsPrincipal Signed(params string[] rights)
    {
        var identity = new ClaimsIdentity(rights.Select(r => new Claim(CommonClaims.ApplicationRight, r)), "test");
        return new ClaimsPrincipal(identity);
    }

    private static RightCheckModel Value(ActionResult<RightCheckModel> result) => (RightCheckModel)((OkObjectResult)result.Result!).Value!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Check_HoldsOneRight_Allowed()
    {
        var model = Value(Controller(Signed("b", "a")).Check(["a", "z"]));

        Assert.AreEqual("Allowed", model.Result);
        CollectionAssert.AreEqual(new[] { "a", "b" }, model.Held);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Check_HoldsNone_Forbidden()
    {
        Assert.AreEqual("Forbidden", Value(Controller(Signed("b")).Check(["a"])).Result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Check_Anonymous_Unauthenticated()
    {
        Assert.AreEqual("Unauthenticated", Value(Controller(new ClaimsPrincipal(new ClaimsIdentity())).Check(["a"])).Result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Protected_ReturnsConfirmation()
    {
        var result = Controller(Signed("rights.demo")).Protected();

        Assert.IsInstanceOfType<OkObjectResult>(result.Result);
    }
}
