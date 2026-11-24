using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.AuthGateway.Tests;

[TestClass]
public class UseAuthGatewayPipelineTests
{
    public TestContext TestContext { get; set; } = null!;

    private async Task<(WebApplication Downstream, WebApplication Gateway, HttpClient Client)> StartAsync(StubTokenEndpoint tokens)
    {
        var downstream = WebApplication.CreateBuilder();
        downstream.WebHost.UseUrls("http://127.0.0.1:0");
        var back = downstream.Build();
        back.Run(context => context.Response.WriteAsync($"auth={context.Request.Headers.Authorization};key={context.Request.Headers["X-Api-Key"]}"));
        await back.StartAsync(TestContext.CancellationToken);
        var backUrl = back.Urls.First();
        TestContext.WriteLine($"downstream {backUrl}");

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var hash = Convert.ToHexString(global::System.Security.Cryptography.SHA512.HashData(global::System.Text.Encoding.UTF8.GetBytes(GatewayTestHost.ApiKey)));
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:AuthGateway:ApiKeys:0:Id"] = "one",
            ["OoBDev:AuthGateway:ApiKeys:0:Subject"] = "caller-one",
            ["OoBDev:AuthGateway:ApiKeys:0:Sha512"] = hash,
            ["OoBDev:AuthGateway:Targets:orders:Inbound:0"] = "ApiKey",
            ["OoBDev:AuthGateway:Targets:orders:Outbound:Grant"] = "ClientCredentials",
            ["OoBDev:AuthGateway:Targets:orders:Outbound:TokenEndpoint"] = "https://idp.test/token",
            ["OoBDev:AuthGateway:Targets:orders:Outbound:ClientId"] = "gateway",
            ["OoBDev:AuthGateway:Targets:orders:Outbound:ClientSecret"] = "super-secret",
            ["ReverseProxy:Routes:r1:ClusterId"] = "c1",
            ["ReverseProxy:Routes:r1:Match:Path"] = "/orders/{**rest}",
            ["ReverseProxy:Routes:r1:Metadata:AuthGatewayTarget"] = "orders",
            ["ReverseProxy:Clusters:c1:Destinations:d1:Address"] = backUrl,
        });
        builder.Services.TryAddAuthGateway(builder.Configuration);
        builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        builder.Services.AddHttpClient(OAuthTokenProvider.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => tokens);
        var gateway = builder.Build();
        gateway.MapReverseProxy(p => p.UseAuthGateway());
        await gateway.StartAsync(TestContext.CancellationToken);
        return (back, gateway, gateway.GetTestClient());
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Proxy_ValidApiKey_ForwardsWithBearerAndNoKey()
    {
        var tokens = new StubTokenEndpoint();
        var (back, gateway, client) = await StartAsync(tokens);
        await using var _ = back;
        await using var __ = gateway;

        using var request = new HttpRequestMessage(HttpMethod.Get, "/orders/1");
        request.Headers.Add("X-Api-Key", GatewayTestHost.ApiKey);
        var response = await client.SendAsync(request, TestContext.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
        TestContext.WriteLine($"{(int)response.StatusCode} {body}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("auth=Bearer token-1;key=", body);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Proxy_MissingApiKey_Returns401WithoutReachingDownstream()
    {
        var tokens = new StubTokenEndpoint();
        var (back, gateway, client) = await StartAsync(tokens);
        await using var _ = back;
        await using var __ = gateway;

        var response = await client.GetAsync("/orders/1", TestContext.CancellationToken);
        TestContext.WriteLine($"{(int)response.StatusCode}");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual(0, tokens.Calls);
    }
}
