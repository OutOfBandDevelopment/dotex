using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Security.OAuthClient.Tests;

[TestClass]
public sealed class OAuthClientTests
{
    private const string Discovery = """
        {"authorization_endpoint":"https://idp.test/auth","token_endpoint":"https://idp.test/token",
         "device_authorization_endpoint":"https://idp.test/device","revocation_endpoint":"https://idp.test/revoke"}
        """;

    private sealed class FakeHandler(Func<HttpRequestMessage, string, (HttpStatusCode, string)> respond) : HttpMessageHandler
    {
        public List<(string Url, string Body)> Calls { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Calls.Add((request.RequestUri!.AbsoluteUri, body));
            var (status, text) = respond(request, body);
            return new HttpResponseMessage(status) { Content = new StringContent(text, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class MemoryStore : ISecretStore
    {
        public Dictionary<string, string> Values { get; } = [];
        public Task<string?> ReadAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(Values.GetValueOrDefault(name));
        public Task WriteAsync(string name, string value, CancellationToken cancellationToken = default) { Values[name] = value; return Task.CompletedTask; }
        public Task DeleteAsync(string name, CancellationToken cancellationToken = default) { Values.Remove(name); return Task.CompletedTask; }
    }

    private static OAuthClientOptions Options() => new() { Issuer = new Uri("https://idp.test/"), ClientId = "desktop" };

    private static (PublicOAuthClient Client, MemoryStore Store, FakeTimeProvider Time) Make(FakeHandler handler, OAuthClientOptions? options = null)
    {
        var store = new MemoryStore();
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        return (new PublicOAuthClient(new HttpClient(handler), Microsoft.Extensions.Options.Options.Create(options ?? Options()), store, time), store, time);
    }

    private static (HttpStatusCode, string) Answer(HttpRequestMessage request, string body, string tokenJson) =>
        request.RequestUri!.AbsolutePath.EndsWith("openid-configuration", StringComparison.Ordinal) ? (HttpStatusCode.OK, Discovery) : (HttpStatusCode.OK, tokenJson);

    [TestMethod]
    [TestCategory("Unit")]
    public void Pkce_MatchesTheRfc7636Vector() =>
        Assert.AreEqual("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM", Pkce.ChallengeFor("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"));

    [TestMethod]
    [TestCategory("Unit")]
    public void Pkce_VerifierIsUrlSafeAndLongEnough()
    {
        var verifier = Pkce.CreateVerifier();
        Assert.AreEqual(43, verifier.Length);
        Assert.IsFalse(verifier.Contains('+') || verifier.Contains('/') || verifier.Contains('='));
        Assert.AreNotEqual(verifier, Pkce.CreateVerifier());
        Assert.AreNotEqual(Pkce.CreateState(), Pkce.CreateState());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Endpoints_ParseOptionalAndRequired()
    {
        var endpoints = OAuthEndpoints.Parse(Discovery);
        Assert.AreEqual("https://idp.test/device", endpoints.DeviceAuthorization!.AbsoluteUri);
        Assert.IsNull(OAuthEndpoints.Parse("""{"authorization_endpoint":"https://a/x","token_endpoint":"https://a/t"}""").Revocation);
        Assert.ThrowsExactly<OAuthException>(() => OAuthEndpoints.Parse("{}"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Refresh_StoresTokensAndKeepsTheOldRefreshTokenWhenNoneIsReturned()
    {
        var handler = new FakeHandler((r, b) => Answer(r, b, """{"access_token":"a2","expires_in":300}"""));
        var (client, store, time) = Make(handler);
        var tokens = await client.RefreshAsync("r1");
        Assert.AreEqual("a2", tokens.AccessToken);
        Assert.AreEqual("r1", tokens.RefreshToken);
        Assert.AreEqual(time.GetUtcNow().AddSeconds(300), tokens.ExpiresAt);
        Assert.IsTrue(store.Values.ContainsKey("oauth-tokens"));
        StringAssert.Contains(handler.Calls[1].Body, "grant_type=refresh_token");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Refresh_InvalidGrantClearsTheStoredTokens()
    {
        var handler = new FakeHandler((r, b) => r.RequestUri!.AbsolutePath.EndsWith("token", StringComparison.Ordinal)
            ? (HttpStatusCode.BadRequest, """{"error":"invalid_grant","error_description":"gone"}""") : (HttpStatusCode.OK, Discovery));
        var (client, store, _) = Make(handler);
        store.Values["oauth-tokens"] = "{}";
        var ex = await Assert.ThrowsExactlyAsync<OAuthException>(() => client.RefreshAsync("r1"));
        Assert.AreEqual("invalid_grant", ex.Error);
        Assert.AreEqual(0, store.Values.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task SignOut_ClearsAndRevokes()
    {
        var handler = new FakeHandler((r, b) => Answer(r, b, "{}"));
        var (client, store, _) = Make(handler);
        store.Values["oauth-tokens"] = """{"accessToken":"a","refreshToken":"r9","expiresAt":"2030-01-01T00:00:00+00:00"}""";
        await client.SignOutAsync();
        Assert.AreEqual(0, store.Values.Count);
        Assert.IsTrue(handler.Calls.Exists(c => c.Url.EndsWith("/revoke", StringComparison.Ordinal) && c.Body.Contains("token=r9", StringComparison.Ordinal)));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task DeviceCode_PollsUntilApproved()
    {
        var polls = 0;
        var handler = new FakeHandler((r, b) =>
        {
            var path = r.RequestUri!.AbsolutePath;
            if (path.EndsWith("openid-configuration", StringComparison.Ordinal)) return (HttpStatusCode.OK, Discovery);
            if (path.EndsWith("device", StringComparison.Ordinal))
                return (HttpStatusCode.OK, """{"device_code":"dc","user_code":"ABCD","verification_uri":"https://idp.test/v","expires_in":600,"interval":1}""");
            return ++polls < 3 ? (HttpStatusCode.BadRequest, """{"error":"authorization_pending"}""") : (HttpStatusCode.OK, """{"access_token":"ok","expires_in":60}""");
        });
        var (client, _, time) = Make(handler);
        DeviceCodePrompt? prompt = null;
        var task = client.SignInWithDeviceCodeAsync(p => prompt = p);
        while (!task.IsCompleted)
        {
            await Task.Delay(10);
            time.Advance(TimeSpan.FromSeconds(1));
        }

        var tokens = await task;
        Assert.AreEqual("ok", tokens.AccessToken);
        Assert.AreEqual("ABCD", prompt!.UserCode);
        Assert.AreEqual(3, polls);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task DeviceCode_WithoutEndpointFails()
    {
        var handler = new FakeHandler((r, b) => (HttpStatusCode.OK, """{"authorization_endpoint":"https://a/x","token_endpoint":"https://a/t"}"""));
        var (client, _, _) = Make(handler);
        await Assert.ThrowsExactlyAsync<OAuthException>(() => client.SignInWithDeviceCodeAsync(_ => { }));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task BrowserSignIn_SendsPkceAndExchangesTheCode()
    {
        var handler = new FakeHandler((r, b) => Answer(r, b, """{"access_token":"web","refresh_token":"rr","expires_in":60}"""));
        var (client, store, _) = Make(handler);
        using var http = new HttpClient();
        var tokens = await client.SignInWithBrowserAsync(url =>
        {
            var query = Uri.UnescapeDataString(url.Query);
            StringAssert.Contains(query, "code_challenge_method=S256");
            var redirect = query.Split('&').First(p => p.StartsWith("redirect_uri=", StringComparison.Ordinal))["redirect_uri=".Length..];
            var state = query.Split('&').First(p => p.StartsWith("state=", StringComparison.Ordinal))["state=".Length..];
            _ = http.GetAsync($"{redirect}?code=thecode&state={state}");
        });
        Assert.AreEqual("web", tokens.AccessToken);
        Assert.IsTrue(handler.Calls.Exists(c => c.Body.Contains("code=thecode", StringComparison.Ordinal) && c.Body.Contains("code_verifier=", StringComparison.Ordinal)));
        Assert.IsTrue(store.Values.ContainsKey("oauth-tokens"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Loopback_IgnoresWrongStateAndOtherPaths_ThenTimesOut()
    {
        using var receiver = new LoopbackReceiver();
        using var http = new HttpClient();
        var wait = receiver.WaitForCodeAsync("good", TimeSpan.FromMilliseconds(600));
        Assert.AreEqual(HttpStatusCode.NotFound, (await http.GetAsync(new Uri(receiver.RedirectUri, "/other"))).StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, (await http.GetAsync($"{receiver.RedirectUri}?code=x&state=bad")).StatusCode);
        var ex = await Assert.ThrowsExactlyAsync<OAuthException>(() => wait);
        Assert.AreEqual("timeout", ex.Error);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Loopback_ProviderErrorIsReported()
    {
        using var receiver = new LoopbackReceiver();
        using var http = new HttpClient();
        var wait = receiver.WaitForCodeAsync("s", TimeSpan.FromSeconds(5));
        _ = http.GetAsync($"{receiver.RedirectUri}?error=access_denied&state=s");
        var ex = await Assert.ThrowsExactlyAsync<OAuthException>(() => wait);
        Assert.AreEqual("access_denied", ex.Error);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task TokenCache_RefreshesOnceForConcurrentCallers()
    {
        var refreshes = 0;
        var handler = new FakeHandler((r, b) =>
        {
            if (r.RequestUri!.AbsolutePath.EndsWith("token", StringComparison.Ordinal)) Interlocked.Increment(ref refreshes);
            return Answer(r, b, """{"access_token":"new","expires_in":600}""");
        });
        var (client, store, time) = Make(handler);
        store.Values["oauth-tokens"] = $$"""{"accessToken":"old","refreshToken":"r","expiresAt":"{{time.GetUtcNow().AddSeconds(10):O}}"}""";
        using var cache = new TokenCache(client, Microsoft.Extensions.Options.Options.Create(Options()), time);
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => cache.GetAccessTokenAsync()));
        Assert.IsTrue(results.All(t => t == "new"));
        Assert.AreEqual(1, refreshes);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task TokenCache_ReturnsNullWhenSignedOutOrRefreshRefused()
    {
        var handler = new FakeHandler((r, b) => r.RequestUri!.AbsolutePath.EndsWith("token", StringComparison.Ordinal)
            ? (HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""") : (HttpStatusCode.OK, Discovery));
        var (client, store, time) = Make(handler);
        using var cache = new TokenCache(client, Microsoft.Extensions.Options.Options.Create(Options()), time);
        Assert.IsNull(await cache.GetAccessTokenAsync());
        store.Values["oauth-tokens"] = $$"""{"accessToken":"old","refreshToken":"r","expiresAt":"{{time.GetUtcNow().AddSeconds(-5):O}}"}""";
        Assert.IsNull(await cache.GetAccessTokenAsync());
        Assert.AreEqual(0, store.Values.Count);
    }

    private sealed class StubCache(params string?[] tokens) : ITokenCache
    {
        private int _next;
        public List<string?> Rejected { get; } = [];
        public Task<string?> GetAccessTokenAsync(string? rejectedToken = null, CancellationToken cancellationToken = default)
        {
            Rejected.Add(rejectedToken);
            return Task.FromResult(tokens[Math.Min(_next++, tokens.Length - 1)]);
        }
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task BearerHandler_RetriesOnceAfter401WithTheFreshToken()
    {
        var seen = new List<string?>();
        var inner = new FakeHandler((r, b) =>
        {
            seen.Add(r.Headers.Authorization?.Parameter);
            return (seen.Count == 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK, "{}");
        });
        var cache = new StubCache("t1", "t2");
        using var http = new HttpClient(new BearerTokenHandler(cache) { InnerHandler = inner });
        using var response = await http.PostAsync("https://api.test/x", new StringContent("body"));
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        CollectionAssert.AreEqual(new[] { "t1", "t2" }, seen);
        Assert.AreEqual("t1", cache.Rejected[1]);
        Assert.AreEqual("body", inner.Calls[1].Body);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task BearerHandler_DoesNotRetryWhenTheTokenIsUnchanged()
    {
        var inner = new FakeHandler((r, b) => (HttpStatusCode.Unauthorized, "{}"));
        using var http = new HttpClient(new BearerTokenHandler(new StubCache("t1")) { InnerHandler = inner });
        using var response = await http.GetAsync("https://api.test/x");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual(1, inner.Calls.Count);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task FileSecretStore_RoundTripsAndRejectsBadNames()
    {
        var folder = Path.Combine(Path.GetTempPath(), "oauth-" + Guid.NewGuid().ToString("N"));
        try
        {
            var store = new FileSecretStore(folder);
            Assert.IsNull(await store.ReadAsync("t"));
            await store.WriteAsync("t", "one");
            await store.WriteAsync("t", "two");
            Assert.AreEqual("two", await store.ReadAsync("t"));
            await store.DeleteAsync("t");
            await store.DeleteAsync("t");
            Assert.IsNull(await store.ReadAsync("t"));
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => store.ReadAsync("../x"));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }
}
