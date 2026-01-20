using Keycloak.ApiClient.Net;
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
/// Integration tests for Keycloak user management operations using Keycloak.ApiClient.Net.
/// These tests require Keycloak running (Docker container or local instance).
/// </summary>
/// <remarks>
/// Required test properties in .runsettings:
/// - KEYCLOAK_URL: Base URL of Keycloak server (e.g., http://localhost:8081)
/// - KEYCLOAK_ADMIN_USERNAME: Admin username (e.g., admin) - must exist in master realm
/// - KEYCLOAK_ADMIN_PASSWORD: Admin password (e.g., admin)
/// - KEYCLOAK_REALM: Realm to use for testing (e.g., integration-test)
/// - KEYCLOAK_CLIENT_ID: Client ID for authentication tests (e.g., integration-test-client)
/// - KEYCLOAK_CLIENT_SECRET: Client secret for authentication tests
/// </remarks>
[TestClass]
public class KeycloakUserManagementTests
{
    private const string MasterRealm = "master";

    public required TestContext TestContext { get; set; }

    private string _createdUserId = string.Empty;
    private KeycloakClient? _keycloakClient;
    private string _realm = string.Empty;
    private string _baseUrl = string.Empty;
    private string? _adminToken;

    [TestInitialize]
    public async Task Setup()
    {
        _baseUrl = TestContext.GetProperty<string>("KEYCLOAK_URL")
            ?? throw new ApplicationException("Missing KEYCLOAK_URL");
        var adminUsername = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_USERNAME")
            ?? throw new ApplicationException("Missing KEYCLOAK_ADMIN_USERNAME");
        var adminPassword = TestContext.GetProperty<string>("KEYCLOAK_ADMIN_PASSWORD")
            ?? throw new ApplicationException("Missing KEYCLOAK_ADMIN_PASSWORD");
        _realm = TestContext.GetProperty<string>("KEYCLOAK_REALM")
            ?? throw new ApplicationException("Missing KEYCLOAK_REALM");

        // Get admin token from MASTER realm (where admin user exists)
        _adminToken = await GetAdminTokenAsync(adminUsername, adminPassword);

        // Create KeycloakClient with token provider that returns the master realm admin token
        _keycloakClient = new KeycloakClient(_baseUrl, () => _adminToken);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (!string.IsNullOrEmpty(_createdUserId) && _keycloakClient != null)
        {
            try
            {
                await _keycloakClient.DeleteUserAsync(_realm, _createdUserId);
                TestContext.WriteLine($"Cleaned up test user: {_createdUserId}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to clean up user: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets an admin token from the master realm.
    /// </summary>
    private async Task<string> GetAdminTokenAsync(string username, string password)
    {
        using var httpClient = new HttpClient();
        var tokenEndpoint = $"{_baseUrl}/realms/{MasterRealm}/protocol/openid-connect/token";

        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", "admin-cli" },
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        });

        var response = await httpClient.PostAsync(tokenEndpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ApplicationException($"Failed to get admin token from master realm: {response.StatusCode} - {error}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
        return tokenResponse.GetProperty("access_token").GetString()
            ?? throw new ApplicationException("Admin token was null");
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
                { "department", ["Engineering"] },
                { "employeeId", [$"EMP{Random.Shared.Next(1000, 9999)}"] }
            }
        };

        // Act
        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);

        // Assert
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");

        _createdUserId = createdUser.Id ?? string.Empty;
        Assert.IsFalse(string.IsNullOrEmpty(_createdUserId), "User ID should be retrieved");

        Assert.AreEqual(username, createdUser.Username);
        Assert.AreEqual(email, createdUser.Email);
        Assert.AreEqual("Test", createdUser.FirstName);
        Assert.AreEqual("User", createdUser.LastName);

        TestContext.WriteLine($"Created user: {username} (ID: {_createdUserId})");
    }

    /// <summary>
    /// Creates a user with credentials inline and verifies the user can authenticate.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateUserWithCredentials_CanAuthenticate()
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
            Enabled = true,
            EmailVerified = true,
            Credentials =
            [
                new Credentials
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            ]
        };

        // Act - Create user with credentials
        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");
        _createdUserId = createdUser.Id ?? string.Empty;

        TestContext.WriteLine($"Created user: {username} (ID: {_createdUserId})");

        // Assert - Authenticate with the created user
        var tokenResponse = await AuthenticateUserAsync(username, password);
        Assert.IsNotNull(tokenResponse?.AccessToken, "Should receive access token");
        Assert.IsFalse(string.IsNullOrEmpty(tokenResponse.AccessToken), "Access token should not be empty");

        TestContext.WriteLine("Authentication successful!");
    }

    /// <summary>
    /// Creates a user, sets password using SetUserPasswordAsync, and verifies authentication.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SetUserPassword_CanAuthenticate()
    {
        // Arrange
        var username = $"pwdtest_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        var password = "TestPassword123!";

        var newUser = new User
        {
            Username = username,
            Email = email,
            FirstName = "Password",
            LastName = "Test",
            Enabled = true,
            EmailVerified = true
        };

        // Act - Create user without password
        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");
        _createdUserId = createdUser.Id ?? string.Empty;

        // Set password using library method
        await _keycloakClient.SetUserPasswordAsync(_realm, _createdUserId, password);
        TestContext.WriteLine($"Password set for user: {username}");

        // Assert - Authenticate
        var tokenResponse = await AuthenticateUserAsync(username, password);
        Assert.IsNotNull(tokenResponse?.AccessToken, "Should receive access token");

        TestContext.WriteLine("Authentication after SetUserPasswordAsync successful!");
    }

    /// <summary>
    /// Creates a user and assigns realm roles using the library.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateUser_AssignRealmRoles_RolesAssignedCorrectly()
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
            Enabled = true,
            EmailVerified = true
        };

        // Act - Create user
        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");
        _createdUserId = createdUser.Id ?? string.Empty;

        // Get available roles and filter for the ones we want
        var availableRoles = await _keycloakClient.GetAvailableRealmRoleMappingsForUserAsync(_realm, _createdUserId);
        var rolesToAssign = availableRoles
            .Where(r => r.Name == "user" || r.Name == "test-role")
            .ToList();

        TestContext.WriteLine($"Available roles: {string.Join(", ", availableRoles.Select(r => r.Name))}");

        // Assign roles
        if (rolesToAssign.Count != 0)
        {
            await _keycloakClient.AddRealmRoleMappingsToUserAsync(_realm, _createdUserId, rolesToAssign);
            TestContext.WriteLine($"Assigned {rolesToAssign.Count} role(s)");
        }

        // Assert - Verify roles were assigned
        var assignedRoles = await _keycloakClient.GetRealmRoleMappingsForUserAsync(_realm, _createdUserId);
        var roleNames = assignedRoles.Select(r => r.Name).ToList();

        TestContext.WriteLine($"User's assigned roles: {string.Join(", ", roleNames)}");

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
        // Arrange
        var username = $"updatetest_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        var newUser = new User
        {
            Username = username,
            Email = email,
            FirstName = "Original",
            LastName = "Name",
            Enabled = true
        };

        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");
        _createdUserId = createdUser.Id ?? string.Empty;

        // Act - Update user
        var updatedUser = new User
        {
            FirstName = "Updated",
            LastName = "Name",
            Attributes = new Dictionary<string, List<string>>
            {
                { "department", ["Sales"] },
                { "location", ["New York"] }
            }
        };

        await _keycloakClient.UpdateUserAsync(_realm, _createdUserId, updatedUser);

        // Assert
        var fetchedUser = await _keycloakClient.GetUserAsync(_realm, _createdUserId);

        Assert.AreEqual("Updated", fetchedUser.FirstName);
        Assert.AreEqual("Name", fetchedUser.LastName);
        Assert.IsNotNull(fetchedUser.Attributes);
        Assert.IsTrue(fetchedUser.Attributes.ContainsKey("department"));
        Assert.AreEqual("Sales", fetchedUser.Attributes["department"].First());

        TestContext.WriteLine($"User updated: {fetchedUser.FirstName} {fetchedUser.LastName}");
    }

    /// <summary>
    /// Lists users in the realm with pagination.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task GetUsers_ReturnsPaginatedList()
    {
        // Act
        var users = await _keycloakClient!.GetUsersAsync(_realm, max: 10, first: 0);

        // Assert
        Assert.IsNotNull(users);
        Assert.IsTrue(users.Any(), "Should have at least one user in the realm");

        TestContext.WriteLine($"Found {users.Count()} users (first 10):");
        foreach (var user in users.Take(5))
        {
            TestContext.WriteLine($"  - {user.Username} ({user.Email})");
        }
    }

    /// <summary>
    /// Searches for users by username pattern.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SearchUsers_FindsByUsername()
    {
        // Arrange
        var uniquePrefix = $"search{Guid.NewGuid():N}"[..16];
        var username = $"{uniquePrefix}_user";

        var newUser = new User
        {
            Username = username,
            Email = $"{username}@example.com",
            Enabled = true
        };

        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");
        _createdUserId = createdUser.Id ?? string.Empty;

        // Act
        var searchResults = await _keycloakClient.GetUsersAsync(_realm, search: uniquePrefix);

        // Assert
        Assert.IsNotNull(searchResults);
        Assert.IsTrue(searchResults.Any(), "Should find the created user");

        var foundUser = searchResults.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(foundUser, "Should find user by search prefix");

        TestContext.WriteLine($"Search found {searchResults.Count()} user(s) matching '{uniquePrefix}'");
    }

    /// <summary>
    /// Creates a disabled user and verifies authentication fails.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task DisabledUser_CannotAuthenticate()
    {
        // Arrange
        var username = $"disabled_{Guid.NewGuid():N}";
        var password = "TestPassword123!";

        var newUser = new User
        {
            Username = username,
            Email = $"{username}@example.com",
            Enabled = false,
            EmailVerified = true,
            Credentials =
            [
                new Credentials
                {
                    Type = "password",
                    Value = password,
                    Temporary = false
                }
            ]
        };

        // Act
        var created = await _keycloakClient!.CreateUserAsync(_realm, newUser);
        Assert.IsTrue(created, "User creation should succeed");

        var users = await _keycloakClient.GetUsersAsync(_realm, search: username);
        var createdUser = users.FirstOrDefault(u => u.Username == username);
        Assert.IsNotNull(createdUser, "Created user should be found");
        _createdUserId = createdUser.Id ?? string.Empty;

        TestContext.WriteLine($"Created disabled user: {username}");

        // Assert - Authentication should fail
        var tokenResponse = await AuthenticateUserAsync(username, password, expectFailure: true);
        Assert.IsNull(tokenResponse, "Disabled user should not receive tokens");

        TestContext.WriteLine("Verified: Disabled user cannot authenticate");
    }

    /// <summary>
    /// Gets the total count of users in the realm.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task GetUsersCount_ReturnsCount()
    {
        // Act
        var count = await _keycloakClient!.GetUsersCountAsync(_realm);

        // Assert
        Assert.IsTrue(count >= 0, "User count should be non-negative");
        TestContext.WriteLine($"Total users in realm '{_realm}': {count}");
    }

    #region Helper Methods

    /// <summary>
    /// Authenticates a user against the test realm and returns the token response.
    /// </summary>
    private async Task<TokenResponse?> AuthenticateUserAsync(string username, string password, bool expectFailure = false)
    {
        var clientId = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_ID")
            ?? throw new ApplicationException("Missing KEYCLOAK_CLIENT_ID");
        var clientSecret = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_SECRET")
            ?? throw new ApplicationException("Missing KEYCLOAK_CLIENT_SECRET");

        using var httpClient = new HttpClient();
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

        if (expectFailure)
        {
            if (response.IsSuccessStatusCode)
            {
                Assert.Fail("Expected authentication to fail but it succeeded");
            }
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Authentication failed: {response.StatusCode} - {error}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TokenResponse>(content);
    }

    #endregion

    #region Models

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

    #endregion
}
