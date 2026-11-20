using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Security.OAuthClient.Tests;

/// <summary>The browser sign-in (authorization code with PKCE on a loopback redirect) against the Keycloak test container, driving the login form over HTTP instead of a real browser.</summary>
[TestClass]
public sealed class KeycloakSignInTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory("Integration")]
    [IntegrationTestMethod("oobd-test-keycloak")]
    public async Task SignInWithBrowser_LoginFormPosted_ReturnsTokensThatRefresh()
    {
        // Stage
        var baseUrl = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("KEYCLOAK_URL"));
        var realm = TestContext.GetRequiredProperty<string>("KEYCLOAK_REALM");
        var user = TestContext.GetRequiredProperty<string>("KEYCLOAK_TEST_USERNAME");
        var password = TestContext.GetRequiredProperty<string>("KEYCLOAK_TEST_PASSWORD");
        var options = new OAuthClientOptions { Issuer = new Uri($"{baseUrl.TrimEnd('/')}/realms/{realm}/"), ClientId = "integration-test-desktop-client", Scopes = ["openid"], LoopbackTimeout = TimeSpan.FromSeconds(30) };
        var store = new MemoryStore();
        using var http = new HttpClient();
        var client = new PublicOAuthClient(http, Microsoft.Extensions.Options.Options.Create(options), store);
        using var browser = new HttpClient(new HttpClientHandler { UseCookies = false });
        Task? login = null;
        TestContext.WriteLine($"Issuer {options.Issuer}, client {options.ClientId}, user {user}");

        // Test
        var tokens = await client.SignInWithBrowserAsync(url => login = LoginAsync(browser, url, user, password), TestContext.CancellationTokenSource.Token);
        await login!;
        var refreshed = await client.RefreshAsync(tokens.RefreshToken!, TestContext.CancellationTokenSource.Token);

        // Assert
        TestContext.WriteLine($"Access token {tokens.AccessToken.Length} chars, refresh token present {tokens.RefreshToken is not null}, expires {tokens.ExpiresAt:u}");
        Assert.IsFalse(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.IsNotNull(tokens.RefreshToken);
        Assert.IsFalse(string.IsNullOrEmpty(refreshed.AccessToken));
        Assert.IsNotNull(await client.LoadAsync(TestContext.CancellationTokenSource.Token));
    }

    private static async Task LoginAsync(HttpClient browser, Uri authorizeUrl, string user, string password)
    {
        // Cookies are replayed by hand: Keycloak marks them Secure and the cookie container would not send them over plain http.
        using var first = await browser.GetAsync(authorizeUrl);
        var cookies = string.Join("; ", first.Headers.GetValues("Set-Cookie").Select(c => c.Split(';')[0]));
        var page = await first.Content.ReadAsStringAsync();
        var action = Regex.Match(page, "action=\"([^\"]+)\"").Groups[1].Value.Replace("&amp;", "&", StringComparison.Ordinal);
        using var request = new HttpRequestMessage(HttpMethod.Post, action)
        {
            Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("username", user), new KeyValuePair<string, string>("password", password) }),
        };
        request.Headers.Add("Cookie", cookies);
        using var response = await browser.SendAsync(request);
    }

    private sealed class MemoryStore : ISecretStore
    {
        private readonly Dictionary<string, string> _values = [];
        public Task<string?> ReadAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(_values.GetValueOrDefault(name));
        public Task WriteAsync(string name, string value, CancellationToken cancellationToken = default) { _values[name] = value; return Task.CompletedTask; }
        public Task DeleteAsync(string name, CancellationToken cancellationToken = default) { _values.Remove(name); return Task.CompletedTask; }
    }
}
