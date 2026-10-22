using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using OoBDev.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Authorization;
using OoBDev.System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Authorization;

[TestClass]
public class RoleRightTests
{
    private static RoleRightOptions Options() => new()
    {
        Roles = new Dictionary<string, string[]>
        {
            ["order-clerk"] = ["orders.read"],
            ["order-manager"] = ["orders.read", "orders.approve"],
        },
    };

    private static ConfigurationRoleRightMapper Mapper(RoleRightOptions? options = null) => new(new FixedMonitor(options ?? Options()));

    private static ClaimsPrincipal User(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));

    [TestMethod]
    [TestCategory("Unit")]
    public void MapRights_SeveralRoles_ReturnsUnionCaseInsensitive()
    {
        // Stage
        var mapper = Mapper();

        // Test
        var rights = mapper.MapRights(["ORDER-CLERK", "order-manager"]);

        // Assert
        CollectionAssert.AreEquivalent(new[] { "orders.read", "orders.approve" }, rights.ToArray());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void MapRights_UnmappedRole_GrantsNothing()
    {
        // Stage
        var mapper = Mapper();

        // Test
        var rights = mapper.MapRights(["stranger"]);

        // Assert
        Assert.AreEqual(0, rights.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_RoleClaim_AddsRightsOnceAsync()
    {
        // Stage
        var options = Options();
        var transformation = new RoleRightClaimsTransformation(Mapper(options), new FixedMonitor(options), NullLogger<RoleRightClaimsTransformation>.Instance);
        var user = User(new Claim(ClaimTypes.Role, "order-manager"));

        // Test
        var first = await transformation.TransformAsync(user);
        var second = await transformation.TransformAsync(first);

        // Assert
        var rights = second.FindAll(CommonClaims.ApplicationRight).Select(c => c.Value).OrderBy(v => v).ToArray();
        CollectionAssert.AreEqual(new[] { "orders.approve", "orders.read" }, rights);
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Messages.Add($"{logLevel}: {formatter(state, exception)}");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_GrantedRights_LogsCountsAtDebugAsync()
    {
        // Stage
        var options = Options();
        var logger = new CapturingLogger<RoleRightClaimsTransformation>();
        var transformation = new RoleRightClaimsTransformation(Mapper(options), new FixedMonitor(options), logger);

        // Test
        await transformation.TransformAsync(User(new Claim(ClaimTypes.Role, "order-manager")));

        // Assert
        CollectionAssert.AreEqual(new[] { "Debug: Granted 2 application rights from 1 roles" }, logger.Messages);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_CustomClaimTypeAndExistingRight_MergesWithoutDuplicatesAsync()
    {
        // Stage
        var options = Options();
        options.RoleClaimTypes = ["groups"];
        var transformation = new RoleRightClaimsTransformation(Mapper(options), new FixedMonitor(options), NullLogger<RoleRightClaimsTransformation>.Instance);
        var user = User(new Claim("groups", "order-clerk"), new Claim("roles", "order-manager"), new Claim(CommonClaims.ApplicationRight, "orders.read"));

        // Test
        var result = await transformation.TransformAsync(user);

        // Assert
        var rights = result.FindAll(CommonClaims.ApplicationRight).Select(c => c.Value).ToArray();
        CollectionAssert.AreEqual(new[] { "orders.read" }, rights);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_Unauthenticated_ReturnsSamePrincipalAsync()
    {
        // Stage
        var options = Options();
        var transformation = new RoleRightClaimsTransformation(Mapper(options), new FixedMonitor(options), NullLogger<RoleRightClaimsTransformation>.Instance);
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        // Test
        var result = await transformation.TransformAsync(anonymous);

        // Assert
        Assert.AreSame(anonymous, result);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Check_Decisions_FollowDenyByDefault()
    {
        // Stage
        var checker = new ApplicationRightChecker();
        var holder = User(new Claim(CommonClaims.ApplicationRight, "orders.read"));

        // Test and Assert
        Assert.AreEqual(RightCheckResult.Unauthenticated, checker.Check(new ClaimsPrincipal(new ClaimsIdentity()), ["orders.read"]));
        Assert.AreEqual(RightCheckResult.Forbidden, checker.Check(holder, ["orders.approve"]));
        Assert.AreEqual(RightCheckResult.Allowed, checker.Check(holder, ["orders.approve", "orders.read"]));
        Assert.AreEqual(RightCheckResult.Allowed, checker.Check(holder, []));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Options_BlankRight_FailsValidation()
    {
        // Stage
        var options = new RoleRightOptions { Roles = new Dictionary<string, string[]> { ["clerk"] = [" "] } };

        // Test
        var results = options.Validate(new(options)).ToList();

        // Assert
        Assert.AreEqual(1, results.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryAddRoleRightMapping_ExistingMapper_Wins()
    {
        // Stage
        var configuration = new ConfigurationBuilder().Build();
        var custom = new CustomMapper();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IRoleRightMapper>(custom);

        // Test
        services.TryAddRoleRightMapping(configuration);
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.AreSame(custom, provider.GetRequiredService<IRoleRightMapper>());
        Assert.IsInstanceOfType<IClaimsTransformation>(provider.GetRequiredService<IClaimsTransformation>());
        Assert.IsNotNull(provider.GetRequiredService<IApplicationRightChecker>());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Filter_Decisions_MapToChallengeForbidAndPass()
    {
        // Stage
        static AuthorizationFilterContext Context(ClaimsPrincipal user)
        {
            var http = new DefaultHttpContext { User = user };
            return new AuthorizationFilterContext(new ActionContext(http, new RouteData(), new ActionDescriptor()), []);
        }
        var filter = new ApplicationRightRequirementFilter(["orders.read"]);
        var anonymous = Context(new ClaimsPrincipal(new ClaimsIdentity()));
        var stranger = Context(User());
        var holder = Context(User(new Claim(CommonClaims.ApplicationRight, "orders.read")));

        // Test
        filter.OnAuthorization(anonymous);
        filter.OnAuthorization(stranger);
        filter.OnAuthorization(holder);

        // Assert
        Assert.IsInstanceOfType<ChallengeResult>(anonymous.Result);
        Assert.IsInstanceOfType<ForbidResult>(stranger.Result);
        Assert.IsNull(holder.Result);
    }

    private sealed class CustomMapper : IRoleRightMapper
    {
        public IReadOnlySet<string> MapRights(IEnumerable<string> roles) => new HashSet<string>();
    }

    private sealed class FixedMonitor(RoleRightOptions value) : IOptionsMonitor<RoleRightOptions>
    {
        public RoleRightOptions CurrentValue => value;
        public RoleRightOptions Get(string? name) => value;
        public global::System.IDisposable? OnChange(global::System.Action<RoleRightOptions, string?> listener) => null;
    }
}
