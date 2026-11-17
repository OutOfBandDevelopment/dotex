using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Keycloak.Tests.Identity;

/// <summary>
/// Checks the <c>oobdev-example</c> development accounts: the realm file defines them as documented, and the running
/// Keycloak holds the same users and roles. Keycloak imports a realm only when it does not exist yet, so a container
/// created before the file changed fails the Integration test until its volume is reset.
/// </summary>
[TestClass]
public class ExampleRealmTests
{
    private const string Realm = "oobdev-example";

    private static readonly string[] AllRoles = ["reader", "editor", "operator", "admin"];

    public required TestContext TestContext { get; set; }

    private static string RepositoryFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "containers", "example")))
            directory = directory.Parent;
        return Path.Combine([directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), .. parts]);
    }

    private static Dictionary<string, (string[] Roles, bool Enabled)> ExpectedUsers()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(RepositoryFile("containers", "example", "keycloak", "oobdev-example-realm.json")));
        return document.RootElement.GetProperty("users").EnumerateArray()
            .Where(u => !u.GetProperty("username").GetString()!.StartsWith("service-account-", StringComparison.Ordinal))
            .ToDictionary(
                u => u.GetProperty("username").GetString()!,
                u => (
                    u.TryGetProperty("realmRoles", out var roles) ? roles.EnumerateArray().Select(r => r.GetString()!).Order(StringComparer.Ordinal).ToArray() : [],
                    !u.TryGetProperty("enabled", out var enabled) || enabled.GetBoolean()));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void RealmFile_DefinesTheDocumentedUsers()
    {
        // Test
        var users = ExpectedUsers();

        // Assert
        CollectionAssert.AreEquivalent(new[] { "ada", "alan", "margaret", "grace" }, users.Keys.ToArray());
        CollectionAssert.AreEqual(new[] { "reader" }, users["ada"].Roles);
        CollectionAssert.AreEqual(new[] { "editor", "reader" }, users["alan"].Roles);
        CollectionAssert.AreEqual(new[] { "operator", "reader" }, users["margaret"].Roles);
        CollectionAssert.AreEqual(AllRoles.Order(StringComparer.Ordinal).ToArray(), users["grace"].Roles);
        Assert.IsTrue(users.Values.All(u => u.Enabled));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ExampleApi_MapsTheAdminRoleGraceHoldsToEveryRightItGuards()
    {
        // Stage
        using var settings = JsonDocument.Parse(File.ReadAllText(RepositoryFile("src", "Examples", "OoBDev.Example.WebApi", "appsettings.json")),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var mapped = settings.RootElement.GetProperty("OoBDev").GetProperty("Authorization").GetProperty("Roles").GetProperty("admin")
            .EnumerateArray().Select(r => r.GetString()!).ToHashSet(StringComparer.Ordinal);
        var guarded = Directory.EnumerateFiles(RepositoryFile("src", "Examples", "OoBDev.Example.WebApi", "Controllers"), "*.cs")
            .SelectMany(f => global::System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(f), @"ApplicationRight\(""([a-z.]+)""").Select(m => m.Groups[1].Value))
            .ToHashSet(StringComparer.Ordinal);

        // Assert
        Assert.IsTrue(ExpectedUsers()["grace"].Roles.Contains("admin"));
        Assert.IsTrue(guarded.IsSubsetOf(mapped), $"Rights guarded but not granted to admin: {string.Join(", ", guarded.Except(mapped))}");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RunningKeycloak_HoldsTheRealmFileUsersAndRoles()
    {
        // Stage
        var baseUrl = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("KEYCLOAK_URL")).TrimEnd('/');
        using var http = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
        var token = await http.PostAsync("realms/master/protocol/openid-connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = TestContext.GetRequiredProperty<string>("KEYCLOAK_ADMIN_USERNAME"),
            ["password"] = TestContext.GetRequiredProperty<string>("KEYCLOAK_ADMIN_PASSWORD"),
        }));
        token.EnsureSuccessStatusCode();
        using var tokenDocument = JsonDocument.Parse(await token.Content.ReadAsStringAsync());
        http.DefaultRequestHeaders.Authorization = new("Bearer", tokenDocument.RootElement.GetProperty("access_token").GetString());

        var realm = await http.GetAsync($"admin/realms/{Realm}");
        Assert.AreEqual(global::System.Net.HttpStatusCode.OK, realm.StatusCode, $"Realm {Realm} is missing: it is imported from the file only when the stack first starts.");

        // Test and Assert
        foreach (var (name, expected) in ExpectedUsers())
        {
            using var found = JsonDocument.Parse(await http.GetStringAsync($"admin/realms/{Realm}/users?username={name}&exact=true"));
            Assert.AreEqual(1, found.RootElement.GetArrayLength(), $"User {name} is missing from Keycloak. Reset the Keycloak volume so the realm file is imported again (integration-down.sh --clean).");
            var user = found.RootElement[0];
            Assert.AreEqual(expected.Enabled, user.GetProperty("enabled").GetBoolean(), $"{name}: enabled");
            using var mappings = JsonDocument.Parse(await http.GetStringAsync($"admin/realms/{Realm}/users/{user.GetProperty("id").GetString()}/role-mappings/realm"));
            var roles = mappings.RootElement.EnumerateArray().Select(r => r.GetProperty("name").GetString()!).Where(AllRoles.Contains).Order(StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(expected.Roles, roles, $"{name}: realm roles differ from the realm file. Reset the Keycloak volume.");
        }
    }
}
