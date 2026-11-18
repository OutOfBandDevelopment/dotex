using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Authorization;

[TestClass]
public class RightAuditEndpointTests
{
    public required TestContext TestContext { get; set; }

    private sealed class FakeQuery : IRightAuditQuery
    {
        public RightAuditFilter? Last { get; private set; }

        public Task<IReadOnlyList<RightAuditEntry>> QueryAsync(RightAuditFilter filter, CancellationToken cancellationToken = default)
        {
            Last = filter;
            return Task.FromResult<IReadOnlyList<RightAuditEntry>>([new RightAuditEntry(DateTimeOffset.UnixEpoch, "u-1", "orders.read", "Allowed")]);
        }
    }

    private static DefaultHttpContext Request(FakeQuery query, ClaimsPrincipal? user, string queryString = "")
    {
        var services = new ServiceCollection();
        services.AddSingleton<IApplicationRightChecker, ApplicationRightChecker>();
        services.AddSingleton<IRightAuditQuery>(query);
        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.QueryString = new QueryString(queryString);
        if (user is not null)
        {
            context.User = user;
        }

        return context;
    }

    private static ClaimsPrincipal Admin(string right) => new(new ClaimsIdentity([new Claim(CommonClaims.NameIdentifier, "admin"), new Claim(CommonClaims.ApplicationRight, right)], "test"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Query_AnonymousCaller_Is401AndNeverReadsTheTrail()
    {
        var query = new FakeQuery();

        var result = await RightAuditEndpointRouteBuilderExtensions.QueryAsync(Request(query, null), ["audit.read"], CancellationToken.None);

        TestContext.WriteLine("Result: {0}", result.GetType().Name);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, ((ProblemHttpResult)result).StatusCode);
        Assert.IsNull(query.Last);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Query_CallerWithoutTheRight_Is403()
    {
        var query = new FakeQuery();

        var result = await RightAuditEndpointRouteBuilderExtensions.QueryAsync(Request(query, Admin("orders.read")), ["audit.read"], CancellationToken.None);

        Assert.AreEqual(StatusCodes.Status403Forbidden, ((ProblemHttpResult)result).StatusCode);
        Assert.IsNull(query.Last);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Query_WithRight_PassesTheFilterAndCapsThePageSize()
    {
        var query = new FakeQuery();
        var context = Request(query, Admin("audit.read"), "?subject=u-1&result=Forbidden&from=2026-11-01T00:00:00Z&maxCount=99999");

        var result = await RightAuditEndpointRouteBuilderExtensions.QueryAsync(context, ["audit.read"], CancellationToken.None);

        TestContext.WriteLine("Filter: {0}", query.Last);
        var ok = (Ok<IReadOnlyList<RightAuditEntry>>)result;
        Assert.AreEqual(1, ok.Value!.Count);
        Assert.AreEqual("u-1", query.Last!.Subject);
        Assert.AreEqual("Forbidden", query.Last.Result);
        Assert.AreEqual(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero), query.Last.From);
        Assert.AreEqual(RightAuditEndpointRouteBuilderExtensions.MaxPageSize, query.Last.MaxCount);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("?from=yesterday")]
    [DataRow("?maxCount=0")]
    [DataRow("?maxCount=abc")]
    public async Task Query_BadParameters_Are400(string queryString)
    {
        var query = new FakeQuery();

        var result = await RightAuditEndpointRouteBuilderExtensions.QueryAsync(Request(query, Admin("audit.read"), queryString), ["audit.read"], CancellationToken.None);

        Assert.AreEqual(StatusCodes.Status400BadRequest, ((ProblemHttpResult)result).StatusCode);
        Assert.IsNull(query.Last);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Map_WithoutRights_Throws()
    {
        var endpoints = new Moq.Mock<IEndpointRouteBuilder>().Object;

        Assert.ThrowsExactly<ArgumentException>(() => endpoints.MapRightAuditQuery([]));
    }
}
