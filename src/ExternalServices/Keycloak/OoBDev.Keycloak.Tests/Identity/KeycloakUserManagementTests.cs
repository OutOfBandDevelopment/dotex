using Keycloak.ApiClient.Net;
using Keycloak.ApiClient.Net.Models;
using Keycloak.ApiClient.Net.Models.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.Keycloak.Tests.Identity;

/// <summary>
/// Integration tests for Keycloak user management operations.
/// These tests require Keycloak running (Docker container or local instance).
/// </summary>
[TestClass]
public class KeycloakUserManagementTests
{
    public required TestContext TestContext { get; set; }

    private string _createdUserId = string.Empty;
    private KeycloakClient? _keycloakClient;
    private string _realm = string.Empty;
    private string _baseUrl = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _baseUrl = TestContext.GetProperty<string>("KEYCLOAK_URL") ?? "http://localhost:8081";
        var adminUsername = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_USERNAME") ?? "admin";
        var adminPassword = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_PASSWORD") ?? "admin";
        _realm = TestContext.GetProperty<string>("KEYCLOAK_REALM") ?? "integration-test";

        _keycloakClient = new KeycloakClient(_baseUrl, adminUsername, adminPassword);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Clean up any created users
        if (!string.IsNullOrEmpty(_createdUserId) && _keycloakClient != null)
        {
            try
            {
                await DeleteUserViaHttpAsync(_createdUserId);
                TestContext.WriteLine($"Cleaned up test user: {_createdUserId}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to clean up user: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Helper method to delete a user via direct HTTP call to Keycloak Admin API.
    /// </summary>
    private async Task DeleteUserViaHttpAsync(string userId)
    {
        var client = new HttpClient();
        var adminUsername = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_USERNAME") ?? "admin";
        var adminPassword = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_PASSWORD") ?? "admin";

        // Get admin token
        var tokenResponse = await GetAdminTokenAsync(client, adminUsername, adminPassword);
        var token = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();

        // Delete user
        var deleteUrl = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}";
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
        deleteRequest.Headers.Add("Authorization", $"Bearer {token}");

        var deleteResponse = await client.SendAsync(deleteRequest);
        deleteResponse.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Helper method to get admin token for direct API calls.
    /// </summary>
    private async Task<string> GetAdminTokenAsync(HttpClient client, string username, string password)
    {
        var tokenEndpoint = $"{_baseUrl}/realms/master/protocol/openid-connect/token";
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", "admin-cli" },
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        });

        var response = await client.PostAsync(tokenEndpoint, requestBody);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Helper method to set a user password via direct HTTP call to Keycloak Admin API.
    /// </summary>
    private async Task SetUserPasswordViaHttpAsync(string userId, string password, bool temporary = false)
    {
        var client = new HttpClient();
        var adminUsername = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_USERNAME") ?? "admin";
        var adminPassword = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_PASSWORD") ?? "admin";

        // Get admin token
        var tokenResponse = await GetAdminTokenAsync(client, adminUsername, adminPassword);
        var token = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();

        // Set password
        var setPasswordUrl = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/reset-password";
        var passwordBody = new StringContent(
            JsonSerializer.Serialize(new { type = "password", value = password, temporary }),
            global::System.Text.Encoding.UTF8,
            "application/json"
        );

        var setPasswordRequest = new HttpRequestMessage(HttpMethod.Post, setPasswordUrl)
        {
            Content = passwordBody
        };
        setPasswordRequest.Headers.Add("Authorization", $"Bearer {token}");

        var setPasswordResponse = await client.SendAsync(setPasswordRequest);
        setPasswordResponse.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Helper method to assign realm roles to a user via direct HTTP call.
    /// </summary>
    private async Task AddUserRealmRolesViaHttpAsync(string userId, List<string> roleNames)
    {
        var client = new HttpClient();
        var adminUsername = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_USERNAME") ?? "admin";
        var adminPassword = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_PASSWORD") ?? "admin";

        // Get admin token
        var tokenResponse = await GetAdminTokenAsync(client, adminUsername, adminPassword);
        var token = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();

        // Get role IDs
        var getRolesUrl = $"{_baseUrl}/admin/realms/{_realm}/roles";
        var getRolesRequest = new HttpRequestMessage(HttpMethod.Get, getRolesUrl);
        getRolesRequest.Headers.Add("Authorization", $"Bearer {token}");

        var getRolesResponse = await client.SendAsync(getRolesRequest);
        getRolesResponse.EnsureSuccessStatusCode();
        var rolesContent = await getRolesResponse.Content.ReadAsStringAsync();
        var rolesDoc = JsonDocument.Parse(rolesContent);

        var rolesToAssign = new List<object>();
        foreach (var roleJson in rolesDoc.RootElement.EnumerateArray())
        {
            var roleName = roleJson.GetProperty("name").GetString();
            if (roleNames.Contains(roleName))
            {
                var roleId = roleJson.GetProperty("id").GetString();
                rolesToAssign.Add(new
                {
                    id = roleId,
                    name = roleName,
                    composite = roleJson.GetProperty("composite").GetBoolean(),
                    clientRole = roleJson.GetProperty("clientRole").GetBoolean(),
                    containerId = roleJson.GetProperty("containerId").GetString()
                });
            }
        }

        // Assign roles
        var assignRolesUrl = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";
        var assignRolesBody = new StringContent(
            JsonSerializer.Serialize(rolesToAssign),
            global::System.Text.Encoding.UTF8,
            "application/json"
        );

        var assignRolesRequest = new HttpRequestMessage(HttpMethod.Post, assignRolesUrl)
        {
            Content = assignRolesBody
        };
        assignRolesRequest.Headers.Add("Authorization", $"Bearer {token}");

        var assignRolesResponse = await client.SendAsync(assignRolesRequest);
        assignRolesResponse.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Helper method to get user realm roles via direct HTTP call.
    /// </summary>
    private async Task<List<(string id, string name)>> GetUserRealmRolesViaHttpAsync(string userId)
    {
        var client = new HttpClient();
        var adminUsername = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_USERNAME") ?? "admin";
        var adminPassword = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_PASSWORD") ?? "admin";

        // Get admin token
        var tokenResponse = await GetAdminTokenAsync(client, adminUsername, adminPassword);
        var token = JsonDocument.Parse(tokenResponse).RootElement.GetProperty("access_token").GetString();

        // Get user roles
        var getUserRolesUrl = $"{_baseUrl}/admin/realms/{_realm}/users/{userId}/role-mappings/realm";
        var getUserRolesRequest = new HttpRequestMessage(HttpMethod.Get, getUserRolesUrl);
        getUserRolesRequest.Headers.Add("Authorization", $"Bearer {token}");

        var getUserRolesResponse = await client.SendAsync(getUserRolesRequest);
        getUserRolesResponse.EnsureSuccessStatusCode();
        var rolesContent = await getUserRolesResponse.Content.ReadAsStringAsync();
        var rolesDoc = JsonDocument.Parse(rolesContent);

        var roles = new List<(string id, string name)>();
        foreach (var roleJson in rolesDoc.RootElement.EnumerateArray())
        {
            var id = roleJson.GetProperty("id").GetString() ?? string.Empty;
            var name = roleJson.GetProperty("name").GetString() ?? string.Empty;
            roles.Add((id, name));
        }

        return roles;
    }

    /// <summary>
    /// Creates a new user in Keycloak and verifies it was created successfully.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateUser_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        var newUser = new User
        {
            Username = username,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Enabled = true,
            EmailVerified = true,
            Attributes = new Dictionary<string, List<string>>
            {
                { "department", new List<string> { "Engineering" } },
                { "employeeId", new List<string> { $"EMP{Random.Shared.Next(1000, 9999)}" } }
            }
        };

        // Act - Create user
        var userId = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        _createdUserId = userId;

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(userId), "User ID should be returned");
        TestContext.WriteLine($"Created user with ID: {userId}");
        TestContext.WriteLine($"Username: {username}");
        TestContext.WriteLine($"Email: {email}");

        // Verify user was created by fetching it
        var createdUser = await _keycloakClient.GetUserAsync(_realm, userId);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(username, createdUser.Username);
        Assert.AreEqual(email, createdUser.Email);
        Assert.AreEqual("Test", createdUser.FirstName);
        Assert.AreEqual("User", createdUser.LastName);

        TestContext.WriteLine("User creation verified successfully");
    }

    /// <summary>
    /// Creates a user, sets a password, and tests authentication.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateUserWithPassword_CanAuthenticate()
    {
        // Arrange
        var username = $"authtest_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        var password = "TestPassword123!";

        var newUser = new User
        {
            Username = username,
            Email = email,
            FirstName = "Auth",
            LastName = "Test",
            Enabled = "true",
            EmailVerified = "true"
        };

        // Act - Create user
        var userId = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created user: {username} (ID: {userId})");

        // Set password
        await SetUserPasswordViaHttpAsync(userId, password, temporary: false);
        TestContext.WriteLine("Password set successfully");

        // Wait a moment for password to propagate
        await Task.Delay(1000);

        // Attempt authentication
        var clientId = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_ID") ?? "integration-test-client";
        var clientSecret = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_SECRET") ?? "test-client-secret-12345";

        var httpClient = new HttpClient();
        var tokenEndpoint = $"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        });

        var response = await httpClient.PostAsync(tokenEndpoint, requestBody);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode, $"Authentication failed: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

        Assert.IsNotNull(tokenResponse?.AccessToken);
        Assert.IsFalse(string.IsNullOrEmpty(tokenResponse.AccessToken));

        TestContext.WriteLine($"Authentication successful!");
        TestContext.WriteLine($"Access token (first 50 chars): {tokenResponse.AccessToken[..Math.Min(50, tokenResponse.AccessToken.Length)]}...");
    }

    /// <summary>
    /// Creates a user with roles assigned.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateUser_WithRoles_AssignsCorrectly()
    {
        // Arrange
        var username = $"roletest_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        var newUser = new User
        {
            Username = username,
            Email = email,
            FirstName = "Role",
            LastName = "Test",
            Enabled = "true",
            EmailVerified = "true"
        };

        // Act - Create user
        var userId = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created user with roles: {username}");

        // Assign roles to the user
        var rolesToAssign = new List<string> { "user", "test-role" };
        await AddUserRealmRolesViaHttpAsync(userId, rolesToAssign);

        // Verify user has roles
        var createdUser = await _keycloakClient.GetUserAsync(_realm, userId);
        Assert.IsNotNull(createdUser);

        // Get user's role mappings
        var userRoles = await GetUserRealmRolesViaHttpAsync(userId);
        var roleNames = userRoles.Select(r => r.name).ToList();

        TestContext.WriteLine($"Assigned roles: {string.Join(", ", roleNames)}");

        Assert.IsTrue(roleNames.Contains("user"), "User should have 'user' role");
        Assert.IsTrue(roleNames.Contains("test-role"), "User should have 'test-role' role");
    }

    /// <summary>
    /// Creates a user, updates their information, and verifies the update.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task UpdateUser_ModifiesUserData()
    {
        // Arrange - Create initial user
        var username = $"updatetest_{Guid.NewGuid():N}";
        var originalEmail = $"{username}@example.com";

        var newUser = new User
        {
            Username = username,
            Email = originalEmail,
            FirstName = "Original",
            LastName = "Name",
            Enabled = "true"
        };

        var userId = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created user: {username}");

        // Act - Update user
        var updatedUser = new User
        {
            FirstName = "Updated",
            LastName = "Name",
            Attributes = new Dictionary<string, List<string>>
            {
                { "department", new List<string> { "Sales" } },
                { "location", new List<string> { "New York" } }
            }
        };

        await _keycloakClient.UpdateUserAsync(_realm, userId, updatedUser);
        TestContext.WriteLine("User updated");

        // Assert - Verify updates
        var fetchedUser = await _keycloakClient.GetUserAsync(_realm, userId);

        Assert.AreEqual("Updated", fetchedUser.FirstName);
        Assert.AreEqual("Name", fetchedUser.LastName);
        Assert.IsNotNull(fetchedUser.Attributes);
        Assert.IsTrue(fetchedUser.Attributes.ContainsKey("department"));
        Assert.AreEqual("Sales", fetchedUser.Attributes["department"].First());

        TestContext.WriteLine($"Verified updates: {fetchedUser.FirstName} {fetchedUser.LastName}");
    }

    /// <summary>
    /// Lists all users in the realm and verifies pagination.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task GetUsers_ReturnsUserList()
    {
        // Act
        var users = await _keycloakClient!.GetUsersAsync(_realm, max: 10, first: 0);

        // Assert
        Assert.IsNotNull(users);
        Assert.IsTrue(users.Any(), "Should have at least the pre-configured test users");

        TestContext.WriteLine($"Found {users.Count()} users (first 10):");
        foreach (var user in users.Take(5))
        {
            TestContext.WriteLine($"  - {user.Username} ({user.Email}) - Enabled: {user.Enabled}");
        }

        // Verify pre-configured users exist
        var usernames = users.Select(u => u.Username).ToList();
        Assert.IsTrue(usernames.Contains("testuser"), "Should find pre-configured 'testuser'");
        Assert.IsTrue(usernames.Contains("adminuser"), "Should find pre-configured 'adminuser'");
    }

    /// <summary>
    /// Searches for users by username.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SearchUsers_ByUsername_FindsMatches()
    {
        // Arrange - Create a user with distinctive username
        var uniquePrefix = $"search_{Guid.NewGuid():N[..8]}";
        var username = $"{uniquePrefix}_user";

        var newUser = new User
        {
            Username = username,
            Email = $"{username}@example.com",
            Enabled = "true"
        };

        var userId = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created searchable user: {username}");

        // Wait for indexing
        await Task.Delay(500);

        // Act - Search for the user
        var searchResults = await _keycloakClient.GetUsersAsync(_realm, search: uniquePrefix);

        // Assert
        Assert.IsNotNull(searchResults);
        Assert.IsTrue(searchResults.Any(), "Should find the created user");

        var foundUser = searchResults.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(foundUser, "Should find user by exact username");

        TestContext.WriteLine($"Search successful: Found {searchResults.Count()} user(s)");
    }

    /// <summary>
    /// Creates a disabled user and verifies authentication fails.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateDisabledUser_CannotAuthenticate()
    {
        // Arrange
        var username = $"disabled_{Guid.NewGuid():N}";
        var password = "TestPassword123!";

        var newUser = new User
        {
            Username = username,
            Email = $"{username}@example.com",
            Enabled = "false",
            EmailVerified = "true"
        };

        // Act - Create user and set password
        var userId = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        _createdUserId = userId;

        await SetUserPasswordViaHttpAsync(userId, password, temporary: false);
        TestContext.WriteLine($"Created disabled user: {username}");

        await Task.Delay(1000);

        // Attempt authentication
        var clientId = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_ID") ?? "integration-test-client";
        var clientSecret = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_SECRET") ?? "test-client-secret-12345";

        var httpClient = new HttpClient();
        var tokenEndpoint = $"{_baseUrl}/realms/{_realm}/protocol/openid-connect/token";

        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        });

        var response = await httpClient.PostAsync(tokenEndpoint, requestBody);

        // Assert - Authentication should fail
        Assert.IsFalse(response.IsSuccessStatusCode, "Disabled user should not be able to authenticate");
        Assert.AreEqual(global::System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        TestContext.WriteLine("Verified: Disabled user cannot authenticate");
    }

    private class TokenResponse
    {
        [global::System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [global::System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [global::System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [global::System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
