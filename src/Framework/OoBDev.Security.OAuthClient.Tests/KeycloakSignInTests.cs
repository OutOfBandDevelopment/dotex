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
        using var browser = new HttpClient(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });
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

    [TestCategory("Integration")]
    [IntegrationTestMethod("oobd-test-keycloak")]
    public async Task SignInWithDeviceCode_UserApprovesCode_ReturnsTokens()
    {
        // Stage
        var baseUrl = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("KEYCLOAK_URL"));
        var realm = TestContext.GetRequiredProperty<string>("KEYCLOAK_REALM");
        var user = TestContext.GetRequiredProperty<string>("KEYCLOAK_TEST_USERNAME");
        var password = TestContext.GetRequiredProperty<string>("KEYCLOAK_TEST_PASSWORD");
        var options = new OAuthClientOptions { Issuer = new Uri($"{baseUrl.TrimEnd('/')}/realms/{realm}/"), ClientId = "integration-test-desktop-client", Scopes = ["openid"] };
        using var http = new HttpClient();
        var client = new PublicOAuthClient(http, Microsoft.Extensions.Options.Options.Create(options), new MemoryStore());
        using var browser = new HttpClient(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });
        Task? approval = null;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationTokenSource.Token);
        timeout.CancelAfter(TimeSpan.FromSeconds(60));

        // Test
        var tokens = await client.SignInWithDeviceCodeAsync(prompt =>
        {
            TestContext.WriteLine($"Device code {prompt.UserCode}, open {prompt.VerificationUriComplete ?? prompt.VerificationUri}");
            approval = LoginAsync(browser, prompt.VerificationUriComplete ?? prompt.VerificationUri, user, password);
            _ = approval.ContinueWith(task => { TestContext.WriteLine($"Approval failed: {task.Exception}"); timeout.Cancel(); }, TaskContinuationOptions.OnlyOnFaulted);
        }, timeout.Token);
        await approval!;

        // Assert
        TestContext.WriteLine($"Access token {tokens.AccessToken.Length} chars");
        Assert.IsFalse(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.IsNotNull(await client.LoadAsync(TestContext.CancellationTokenSource.Token));
    }

    private static async Task LoginAsync(HttpClient browser, Uri authorizeUrl, string user, string password)
    {
        // Redirects and cookies are handled by hand: Keycloak marks cookies Secure and the cookie container would not send them over plain http.
        var cookies = new Dictionary<string, string>();
        var (page, at) = await SendAsync(browser, HttpMethod.Get, authorizeUrl, cookies, null);
        (page, at) = await SendAsync(browser, HttpMethod.Post, FormAction(page, at), cookies, new Dictionary<string, string> { ["username"] = user, ["password"] = password });
        if (page.Contains("name=\"accept\"", StringComparison.Ordinal))
            await SendAsync(browser, HttpMethod.Post, FormAction(page, at), cookies, new Dictionary<string, string> { ["accept"] = "Yes" });
    }

    private static Uri FormAction(string page, Uri at)
    {
        var match = Regex.Match(page, "action=\"([^\"]+)\"");
        Assert.IsTrue(match.Success, $"No form on the page at {at}: {page[..Math.Min(page.Length, 300)]}");
        return new Uri(at, match.Groups[1].Value.Replace("&amp;", "&", StringComparison.Ordinal));
    }

    private static async Task<(string Page, Uri At)> SendAsync(HttpClient browser, HttpMethod method, Uri url, Dictionary<string, string> cookies, Dictionary<string, string>? form)
    {
        for (var hop = 0; hop < 10; hop++)
        {
            using var request = new HttpRequestMessage(method, url) { Content = form is null ? null : new FormUrlEncodedContent(form) };
            if (cookies.Count > 0)
                request.Headers.Add("Cookie", string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}")));
            using var response = await browser.SendAsync(request);
            if (response.Headers.TryGetValues("Set-Cookie", out var set))
                foreach (var cookie in set.Select(c => c.Split(';')[0]))
                {
                    var i = cookie.IndexOf('=', StringComparison.Ordinal);
                    cookies[cookie[..i]] = cookie[(i + 1)..];
                }

            if (response.Headers.Location is not { } next || (int)response.StatusCode is < 300 or > 399)
                return (await response.Content.ReadAsStringAsync(), url);
            url = next.IsAbsoluteUri ? next : new Uri(url, next);
            method = HttpMethod.Get;
            form = null;
        }

        throw new InvalidOperationException("Too many redirects.");
    }

    private sealed class MemoryStore : ISecretStore
    {
        private readonly Dictionary<string, string> _values = [];
        public Task<string?> ReadAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(_values.GetValueOrDefault(name));
        public Task WriteAsync(string name, string value, CancellationToken cancellationToken = default) { _values[name] = value; return Task.CompletedTask; }
        public Task DeleteAsync(string name, CancellationToken cancellationToken = default) { _values.Remove(name); return Task.CompletedTask; }
    }
}
