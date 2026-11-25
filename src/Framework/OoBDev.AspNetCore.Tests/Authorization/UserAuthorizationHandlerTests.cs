using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AspNetCore.Mvc.Authorization;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Authorization;

[TestClass]
[DoNotParallelize]
public class UserAuthorizationHandlerTests
{
    public required TestContext TestContext { get; set; }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(UserAuthorizationHandler.ShowPiiVariable, null);
        IdentityModelEventSource.ShowPII = false;
    }

    private static IHostEnvironment Environment_(string name)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(name);
        return environment.Object;
    }

    private static async Task<bool> AuthorizeAsync(string? claimName, params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity(Array.ConvertAll(claims, c => new Claim(c.Type, c.Value)), "test");
        var requirement = new UserAuthorizationRequirement(claimName);
        var context = new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(identity), null);
        await new UserAuthorizationHandler(NullLogger<UserAuthorizationHandler>.Instance, Environment_("Development")).HandleAsync(context);
        return context.HasSucceeded;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Handle_NoClaimConfigured_DoesNotRequireAUserIdAsync()
    {
        var result = await AuthorizeAsync(null, (CommonClaims.ObjectId, "o1"));
        TestContext.WriteLine("No claim configured, object id only: authorized {0}", result);
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Handle_ClaimConfiguredAndMissing_FailsAsync()
    {
        var result = await AuthorizeAsync("my_user", (CommonClaims.ObjectId, "o1"));
        TestContext.WriteLine("Claim my_user required and absent: authorized {0}", result);
        Assert.IsFalse(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Handle_ConfiguredClaimPresent_SucceedsAsync()
    {
        var result = await AuthorizeAsync("my_user", (CommonClaims.ObjectId, "o1"), ("my_user", "42"));
        TestContext.WriteLine("Claim my_user present: authorized {0}", result);
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_ShowPiiVariableOutsideProduction_EnablesShowPii()
    {
        Environment.SetEnvironmentVariable(UserAuthorizationHandler.ShowPiiVariable, "true");
        _ = new UserAuthorizationHandler(NullLogger<UserAuthorizationHandler>.Instance, Environment_("Development"));
        TestContext.WriteLine("ShowPII after Development start: {0}", IdentityModelEventSource.ShowPII);
        Assert.IsTrue(IdentityModelEventSource.ShowPII);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_ShowPiiVariableInProduction_IsIgnored()
    {
        Environment.SetEnvironmentVariable(UserAuthorizationHandler.ShowPiiVariable, "true");
        _ = new UserAuthorizationHandler(NullLogger<UserAuthorizationHandler>.Instance, Environment_("Production"));
        TestContext.WriteLine("ShowPII after Production start: {0}", IdentityModelEventSource.ShowPII);
        Assert.IsFalse(IdentityModelEventSource.ShowPII);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Create_NoVariable_LeavesShowPiiOff()
    {
        _ = new UserAuthorizationHandler(NullLogger<UserAuthorizationHandler>.Instance, Environment_("Development"));
        Assert.IsFalse(IdentityModelEventSource.ShowPII);
    }
}
