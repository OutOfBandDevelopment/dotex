using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Keycloak.Identity;
using OoBDev.TestUtilities;

namespace OoBDev.Keycloak.Tests.Identity;

/// <summary>The read-only admin reader against a scripted Keycloak.</summary>
[TestCategory(TestCategories.Unit)]
[TestClass]
public sealed class KeycloakDirectoryReaderTests
{
    private sealed class Script : HttpMessageHandler
    {
        public List<string> Calls { get; } = [];

        public int Tokens { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.PathAndQuery;
            Calls.Add($"{request.Method} {url}");
            string body;
            if (url.EndsWith("/protocol/openid-connect/token", StringComparison.Ordinal))
            {
                Tokens++;
                body = """{"access_token":"tok","expires_in":60}""";
            }
            else if (url.Contains("/role-mappings/realm", StringComparison.Ordinal))
            {
                body = """[{"name":"viewer"},{"name":"admin"}]""";
            }
            else if (url.Contains("/users?", StringComparison.Ordinal))
            {
                body = """[{"id":"u1","username":"ann","email":"ann@example.test","firstName":"Ann","lastName":"Lee","enabled":true}]""";
            }
            else if (url.EndsWith("/roles", StringComparison.Ordinal))
            {
                body = """[{"name":"viewer"},{"name":"admin"}]""";
            }
            else
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }

    private static (KeycloakDirectoryReader Reader, Script Script, FakeTimeProvider Time) Create()
    {
        var script = new Script();
        var time = new FakeTimeProvider();
        var options = Options.Create(new KeycloakIdentityOptions { BaseUrl = "http://kc.test/", Realm = "demo", ClientId = "svc", ClientSecret = "s" });
        return (new KeycloakDirectoryReader(new HttpClient(script), options, time), script, time);
    }

    [TestMethod]
    public async Task ListUsers_Search_ReturnsUsersWithSortedRoles()
    {
        // Stage
        var (reader, script, _) = Create();

        // Test
        var users = await reader.ListUsersAsync(search: "an e");

        // Assert
        Assert.HasCount(1, users);
        Assert.AreEqual("ann", users[0].UserName);
        CollectionAssert.AreEqual(new[] { "admin", "viewer" }, new List<string>(users[0].Roles));
        Assert.Contains("GET /admin/realms/demo/users?max=50&search=an%20e", script.Calls);
    }

    [TestMethod]
    public async Task ListUsers_Email_MatchesExactly()
    {
        // Stage
        var (reader, script, _) = Create();

        // Test
        await reader.ListUsersAsync(email: "ann@example.test");

        // Assert
        Assert.Contains("GET /admin/realms/demo/users?max=50&exact=true&email=ann%40example.test", script.Calls);
    }

    [TestMethod]
    public async Task ListUsers_RepeatedCalls_ReuseTheTokenUntilItExpires()
    {
        // Stage
        var (reader, script, time) = Create();

        // Test
        await reader.ListRolesAsync();
        await reader.ListRolesAsync();
        time.Advance(TimeSpan.FromSeconds(31));
        await reader.ListRolesAsync();

        // Assert
        Assert.AreEqual(2, script.Tokens);
    }

    [TestMethod]
    public async Task ListRoles_ReturnsSortedNames()
    {
        // Stage
        var (reader, _, _) = Create();

        // Test
        var roles = await reader.ListRolesAsync();

        // Assert
        CollectionAssert.AreEqual(new[] { "admin", "viewer" }, new List<string>(roles));
    }

    [TestMethod]
    public async Task GetIdentityUsersByEmail_NoMatch_ReturnsNull()
    {
        // Stage
        var script = new EmptyUsers();
        var options = Options.Create(new KeycloakIdentityOptions { BaseUrl = "http://kc.test", Realm = "demo" });
        var manager = new ManageKeycloakUser(new KeycloakDirectoryReader(new HttpClient(script), options, TimeProvider.System));

        // Test
        var users = await manager.GetIdentityUsersByEmail("none@example.test");

        // Assert
        Assert.IsNull(users);
    }

    private sealed class EmptyUsers : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(request.RequestUri!.AbsolutePath.EndsWith("token", StringComparison.Ordinal) ? """{"access_token":"t","expires_in":60}""" : "[]", Encoding.UTF8, "application/json"),
            });
    }
}
