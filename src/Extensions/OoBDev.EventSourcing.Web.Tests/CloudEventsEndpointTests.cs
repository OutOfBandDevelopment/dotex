using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.EventSourcing.Web.Tests;

[TestClass]
[TestCategory("Unit")]
public class CloudEventsEndpointTests
{
    public required TestContext TestContext { get; set; }

    private sealed class Host(WebApplication app, HttpClient client, InMemoryEventStore store) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public InMemoryEventStore Store { get; } = store;

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }

    private static async Task<Host> StartAsync(bool requireAuthorization = false)
    {
        var store = new InMemoryEventStore(TimeProvider.System);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddKeyedSingleton<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey, store);
        if (requireAuthorization)
        {
            builder.Services.AddAuthentication("none").AddScheme<AuthenticationSchemeOptions, DenyHandler>("none", null);
            builder.Services.AddAuthorization();
        }

        var app = builder.Build();
        if (requireAuthorization)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapCloudEvents().RequireAuthorization();
        }
        else
        {
            app.MapCloudEvents();
        }

        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return new Host(app, new HttpClient { BaseAddress = new Uri(address) }, store);
    }

    private sealed class DenyHandler(Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options, Microsoft.Extensions.Logging.ILoggerFactory logger, global::System.Text.Encodings.Web.UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
    }

    private static string Event(string stream, long sequence, string type = "Thing.Happened") => JsonSerializer.Serialize(new Dictionary<string, object>
    {
        ["specversion"] = "1.0",
        ["id"] = $"{stream}:{sequence}",
        ["source"] = "test",
        ["type"] = type,
        ["subject"] = stream,
        ["time"] = "2026-11-16T10:00:00Z",
        ["sequence"] = sequence,
        ["data"] = new { value = 1 },
    });

    private Task<HttpResponseMessage> Post(Host host, string body, string contentType = "application/cloudevents+json")
    {
        var content = new StringContent(body, Encoding.UTF8);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        return host.Client.PostAsync("/events", content, TestContext.CancellationToken);
    }

    [TestMethod]
    public async Task Post_ValidEvent_Is202AndAppended()
    {
        await using var host = await StartAsync();

        using var response = await Post(host, Event("s1", 1));

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        var stored = await host.Store.ReadAsync("s1", 1, TestContext.CancellationToken);
        Assert.AreEqual(1, stored.Count);
        Assert.AreEqual("Thing.Happened", stored[0].Type);
    }

    [TestMethod]
    public async Task Post_NextInStream_Appends()
    {
        await using var host = await StartAsync();
        (await Post(host, Event("s2", 1))).Dispose();

        using var response = await Post(host, Event("s2", 2));

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        Assert.AreEqual(2, (await host.Store.ReadAsync("s2", 1, TestContext.CancellationToken)).Count);
    }

    [TestMethod]
    public async Task Post_SequenceGap_Is409Problem()
    {
        await using var host = await StartAsync();

        using var response = await Post(host, Event("s3", 5));

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public async Task Post_OtherContentType_Is415Problem()
    {
        await using var host = await StartAsync();

        using var response = await Post(host, Event("s4", 1), "application/json");

        Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    [DataRow("not json")]
    [DataRow("{\"specversion\":\"2.0\",\"id\":\"a:1\",\"type\":\"t\"}")]
    [DataRow("{\"specversion\":\"1.0\",\"type\":\"t\"}")]
    public async Task Post_InvalidEvent_Is400Problem(string body)
    {
        await using var host = await StartAsync();

        using var response = await Post(host, body);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public async Task Post_WithPolicyAndNoUser_IsRejected()
    {
        await using var host = await StartAsync(requireAuthorization: true);

        using var response = await Post(host, Event("s5", 1));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual(0, (await host.Store.ReadAsync("s5", 1, TestContext.CancellationToken)).Count);
    }
}
