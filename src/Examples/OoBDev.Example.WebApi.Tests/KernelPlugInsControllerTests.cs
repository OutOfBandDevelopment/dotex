using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.SemanticKernel;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class KernelPlugInsControllerTests
{
    private static KernelPlugInsController Build()
    {
        var kernel = new Kernel();
        kernel.Plugins.AddFromObject(new CurrentUserPlugIn(NullLogger<CurrentUserPlugIn>.Instance), "CurrentUserPlugIn");
        return new KernelPlugInsController(kernel);
    }

    [TestMethod]
    public void List_RegisteredPlugIn_ReturnsItsFunctions()
    {
        var functions = Build().List().ToList();

        Assert.IsTrue(functions.Any(f => f.Name == "current_user" && f.PlugIn == "CurrentUserPlugIn"));
    }

    [TestMethod]
    public async Task Invoke_KnownFunction_ReturnsItsResult()
    {
        var result = await Build().Invoke(new KernelInvokeRequestModel { PlugIn = "CurrentUserPlugIn", Function = "current_user" }, default);

        Assert.AreEqual(global::System.Environment.UserName, result.Value!.Result);
    }

    [TestMethod]
    public async Task Invoke_UnknownFunction_IsNotFound()
    {
        var result = await Build().Invoke(new KernelInvokeRequestModel { PlugIn = "x", Function = "y" }, default);

        Assert.AreEqual(404, ((ObjectResult)result.Result!).StatusCode);
    }
}
