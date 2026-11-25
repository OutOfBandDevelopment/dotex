using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.AuthGateway.Tests;

[TestClass]
public class AuthGatewayAuthorizerTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_ValidApiKey_ReturnsBearerFromTokenEndpoint()
    {
        // Stage
        var host = new GatewayTestHost();
        var authorizer = host.BuildAuthorizer();

        // Test
        var decision = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");
        TestContext.WriteLine($"status {decision.StatusCode}, caller {decision.Caller}");

        // Assert
        Assert.IsTrue(decision.Allowed);
        Assert.AreEqual("token-1", decision.BearerToken);
        Assert.AreEqual("caller-one", decision.Caller!.Subject);
        StringAssert.Contains(host.Endpoint.Bodies[0], "grant_type=client_credentials");
        StringAssert.Contains(host.Endpoint.Bodies[0], "audience=orders-api");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_MissingOrWrongKey_Is401AndNoOutboundCall()
    {
        var host = new GatewayTestHost();
        var authorizer = host.BuildAuthorizer();

        var none = await authorizer.AuthorizeAsync(GatewayTestHost.Request(), "orders");
        var wrong = await authorizer.AuthorizeAsync(GatewayTestHost.Request("not-the-key"), "orders");

        Assert.AreEqual(StatusCodes.Status401Unauthorized, none.StatusCode);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, wrong.StatusCode);
        Assert.AreEqual(0, host.Endpoint.Calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_UnknownTarget_Is403()
    {
        var host = new GatewayTestHost();
        var authorizer = host.BuildAuthorizer();

        var unknown = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "billing");
        var unnamed = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), null);

        Assert.AreEqual(StatusCodes.Status403Forbidden, unknown.StatusCode);
        Assert.AreEqual(StatusCodes.Status403Forbidden, unnamed.StatusCode);
        Assert.AreEqual(0, host.Endpoint.Calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_TokenEndpointFails_Is502()
    {
        var host = new GatewayTestHost();
        host.Endpoint.Status = HttpStatusCode.InternalServerError;

        var decision = await host.BuildAuthorizer().AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");

        Assert.AreEqual(StatusCodes.Status502BadGateway, decision.StatusCode);
        Assert.IsFalse(decision.Allowed);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_SameCaller_CachesUntilNearExpiryThenRefreshesOnce()
    {
        var host = new GatewayTestHost();
        host.Endpoint.ExpiresIn = 120;
        var authorizer = host.BuildAuthorizer();

        var first = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");
        host.Time.Advance(TimeSpan.FromSeconds(80));
        var cached = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");
        host.Time.Advance(TimeSpan.FromSeconds(20));
        var refreshed = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");

        Assert.AreEqual("token-1", first.BearerToken);
        Assert.AreEqual("token-1", cached.BearerToken);
        Assert.AreEqual("token-2", refreshed.BearerToken);
        Assert.AreEqual(2, host.Endpoint.Calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_ConcurrentRequests_FetchOneToken()
    {
        var host = new GatewayTestHost();
        host.Endpoint.Delay = TimeSpan.FromMilliseconds(100);
        var authorizer = host.BuildAuthorizer();

        var decisions = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders")));

        Assert.AreEqual(1, host.Endpoint.Calls);
        Assert.IsTrue(decisions.All(d => d.BearerToken == "token-1"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_TokenExchange_SendsInboundBearerAsSubjectToken()
    {
        var host = new GatewayTestHost(OutboundGrant.TokenExchange);
        var authorizer = host.BuildAuthorizer();

        var decision = await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey, bearer: "inbound.jwt"), "orders");

        Assert.IsTrue(decision.Allowed);
        StringAssert.Contains(host.Endpoint.Bodies[0], "grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Atoken-exchange");
        StringAssert.Contains(host.Endpoint.Bodies[0], "subject_token=inbound.jwt");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_TokenExchangeWithoutInboundBearer_Is502()
    {
        var host = new GatewayTestHost(OutboundGrant.TokenExchange);

        var decision = await host.BuildAuthorizer().AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");

        Assert.AreEqual(StatusCodes.Status502BadGateway, decision.StatusCode);
        Assert.AreEqual(0, host.Endpoint.Calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Authorize_Logs_NeverContainKeyOrSecretOrToken()
    {
        var host = new GatewayTestHost();
        var authorizer = host.BuildAuthorizer();

        await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");
        await authorizer.AuthorizeAsync(GatewayTestHost.Request("bad-key-value"), "orders");
        host.Endpoint.Status = HttpStatusCode.BadRequest;
        host.Time.Advance(TimeSpan.FromHours(2));
        await authorizer.AuthorizeAsync(GatewayTestHost.Request(GatewayTestHost.ApiKey), "orders");

        var all = string.Join('\n', host.Logs.Lines);
        TestContext.WriteLine(all);
        Assert.IsTrue(host.Logs.Lines.Count >= 3);
        foreach (var secret in new[] { GatewayTestHost.ApiKey, "bad-key-value", "super-secret", "token-1" })
        {
            Assert.IsFalse(all.Contains(secret, StringComparison.Ordinal), secret);
        }
    }
}
