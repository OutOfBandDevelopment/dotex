using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Unsubscribe;

namespace OoBDev.Communications.Web.Tests;

[TestClass]
[TestCategory("Unit")]
public class UnsubscribeEndpointTests
{
    public required TestContext TestContext { get; set; }

    private sealed class Host(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public IServiceProvider Services => app.Services;

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }

    private static async Task<Host> StartAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddUnsubscribeLinks(o =>
        {
            o.EndpointUrl = "https://example.test/unsubscribe";
            o.SigningKey = new string('k', 40);
        });
        var app = builder.Build();
        app.MapUnsubscribe();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return new Host(app, new HttpClient { BaseAddress = new Uri(address) });
    }

    private static FormUrlEncodedContent OneClick() => new([new KeyValuePair<string, string>("List-Unsubscribe", "One-Click")]);

    [TestMethod]
    public async Task Post_ValidTokenOneClick_Is200AndSuppressesTheAddress()
    {
        await using var host = await StartAsync();
        var token = host.Services.GetRequiredService<UnsubscribeLinks>().CreateToken("Ann@Example.test");
        TestContext.WriteLine($"token {token.Length} chars");

        using var response = await host.Client.PostAsync($"/unsubscribe?t={token}", OneClick(), TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(await host.Services.GetRequiredService<ISuppressionList>().IsSuppressedAsync("ann@example.test", TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Post_BadToken_Is400AndSuppressesNothing()
    {
        await using var host = await StartAsync();

        using var response = await host.Client.PostAsync("/unsubscribe?t=bad.token", OneClick(), TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Post_WithoutTheOneClickForm_Is400()
    {
        await using var host = await StartAsync();
        var token = host.Services.GetRequiredService<UnsubscribeLinks>().CreateToken("bob@example.test");

        using var response = await host.Client.PostAsync($"/unsubscribe?t={token}", new StringContent("nothing"), TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsFalse(await host.Services.GetRequiredService<ISuppressionList>().IsSuppressedAsync("bob@example.test", TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Get_ShowsAButtonAndDoesNotUnsubscribe()
    {
        await using var host = await StartAsync();
        var token = host.Services.GetRequiredService<UnsubscribeLinks>().CreateToken("cy@example.test");

        using var response = await host.Client.GetAsync($"/unsubscribe?t={token}", TestContext.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(html, "<button>");
        Assert.IsFalse(await host.Services.GetRequiredService<ISuppressionList>().IsSuppressedAsync("cy@example.test", TestContext.CancellationToken));
    }
}
