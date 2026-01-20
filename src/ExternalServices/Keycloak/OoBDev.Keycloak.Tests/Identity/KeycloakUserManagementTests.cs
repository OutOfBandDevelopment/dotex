using Keycloak.AuthServices.Sdk;
using Keycloak.AuthServices.Sdk.Admin;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Keycloak.AuthServices.Sdk.Admin.Requests.Users;
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
    private IKeycloakUserClient? _userClient;
    private string _realm = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        var baseUrl = TestContext.GetProperty<string>("KEYCLOAK_URL") ?? "http://localhost:8081";
        _realm = TestContext.GetProperty<string>("KEYCLOAK_REALM") ?? "integration-test";

        var adminClient = new KeycloakAdminApiClient(
            new HttpClient { BaseAddress = new Uri(baseUrl) }
        );

        _userClient = adminClient.Users(_realm);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Clean up any created users
        if (!string.IsNullOrEmpty(_createdUserId) && _userClient != null)
        {
            try
            {
                await _userClient.DeleteUser(_createdUserId);
                TestContext.WriteLine($"Cleaned up test user: {_createdUserId}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to clean up user: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Creates a new user in Keycloak and verifies it was created successfully.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task CreateUser_WithValidData_CreatesSuccessfully()
    {
        // Arrange
        var username = $"testuser_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        var newUser = new UserRepresentation
        {
            Username = username,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Enabled = true,
            EmailVerified = true,
            Attributes = new Dictionary<string, IEnumerable<string>>
            {
                { "department", new[] { "Engineering" } },
                { "employeeId", new[] { $"EMP{Random.Shared.Next(1000, 9999)}" } }
            }
        };

        // Act - Create user
        var userId = await _userClient!.CreateUser(newUser);
        _createdUserId = userId; // Store for cleanup

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(userId), "User ID should be returned");
        TestContext.WriteLine($"Created user with ID: {userId}");
        TestContext.WriteLine($"Username: {username}");
        TestContext.WriteLine($"Email: {email}");

        // Verify user was created by fetching it
        var createdUser = await _userClient.GetUser(userId);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(username, createdUser.Username);
        Assert.AreEqual(email, createdUser.Email);
        Assert.AreEqual("Test", createdUser.FirstName);
        Assert.AreEqual("User", createdUser.LastName);
        Assert.IsTrue(createdUser.Enabled);
        Assert.IsTrue(createdUser.EmailVerified);

        TestContext.WriteLine("User creation verified successfully");
    }

    /// <summary>
    /// Creates a user, sets a password, and tests authentication.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task CreateUserWithPassword_CanAuthenticate()
    {
        // Arrange
        var username = $"authtest_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        var password = "TestPassword123!";

        var newUser = new UserRepresentation
        {
            Username = username,
            Email = email,
            FirstName = "Auth",
            LastName = "Test",
            Enabled = true,
            EmailVerified = true
        };

        // Act - Create user
        var userId = await _userClient!.CreateUser(newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created user: {username} (ID: {userId})");

        // Set password
        var credential = new CredentialRepresentation
        {
            Type = "password",
            Value = password,
            Temporary = false
        };

        await _userClient.ResetUserPassword(userId, credential);
        TestContext.WriteLine("Password set successfully");

        // Wait a moment for password to propagate
        await Task.Delay(1000);

        // Attempt authentication
        var baseUrl = TestContext.GetProperty<string>("KEYCLOAK_URL") ?? "http://localhost:8081";
        var clientId = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_ID") ?? "integration-test-client";
        var clientSecret = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_SECRET") ?? "test-client-secret-12345";

        var httpClient = new HttpClient();
        var tokenEndpoint = $"{baseUrl}/realms/{_realm}/protocol/openid-connect/token";

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
    [TestCategory(TestCategories.DevLocal)]
    public async Task CreateUser_WithRoles_AssignsCorrectly()
    {
        // Arrange
        var username = $"roletest_{Guid.NewGuid():N}";
        var email = $"{username}@example.com";

        var newUser = new UserRepresentation
        {
            Username = username,
            Email = email,
            FirstName = "Role",
            LastName = "Test",
            Enabled = true,
            EmailVerified = true,
            RealmRoles = new List<string> { "user", "test-role" }
        };

        // Act - Create user
        var userId = await _userClient!.CreateUser(newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created user with roles: {username}");

        // Verify user has roles
        var createdUser = await _userClient.GetUser(userId);
        Assert.IsNotNull(createdUser);

        // Get user's role mappings
        var roleMappings = await _userClient.GetUserRealmRoles(userId);
        var roleNames = roleMappings.Select(r => r.Name).ToList();

        TestContext.WriteLine($"Assigned roles: {string.Join(", ", roleNames)}");

        Assert.IsTrue(roleNames.Contains("user"), "User should have 'user' role");
        Assert.IsTrue(roleNames.Contains("test-role"), "User should have 'test-role' role");
    }

    /// <summary>
    /// Creates a user, updates their information, and verifies the update.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task UpdateUser_ModifiesUserData()
    {
        // Arrange - Create initial user
        var username = $"updatetest_{Guid.NewGuid():N}";
        var originalEmail = $"{username}@example.com";

        var newUser = new UserRepresentation
        {
            Username = username,
            Email = originalEmail,
            FirstName = "Original",
            LastName = "Name",
            Enabled = true
        };

        var userId = await _userClient!.CreateUser(newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created user: {username}");

        // Act - Update user
        var updatedUser = new UserRepresentation
        {
            FirstName = "Updated",
            LastName = "Name",
            Attributes = new Dictionary<string, IEnumerable<string>>
            {
                { "department", new[] { "Sales" } },
                { "location", new[] { "New York" } }
            }
        };

        await _userClient.UpdateUser(userId, updatedUser);
        TestContext.WriteLine("User updated");

        // Assert - Verify updates
        var fetchedUser = await _userClient.GetUser(userId);

        Assert.AreEqual("Updated", fetchedUser.FirstName);
        Assert.AreEqual("Name", fetchedUser.LastName);
        Assert.IsTrue(fetchedUser.Attributes!.ContainsKey("department"));
        Assert.AreEqual("Sales", fetchedUser.Attributes["department"].First());

        TestContext.WriteLine($"Verified updates: {fetchedUser.FirstName} {fetchedUser.LastName}");
    }

    /// <summary>
    /// Lists all users in the realm and verifies pagination.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task GetUsers_ReturnsUserList()
    {
        // Act
        var parameters = new GetUsersRequestParameters
        {
            Max = 10,
            First = 0
        };

        var users = await _userClient!.GetUsers(parameters);

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
    [TestCategory(TestCategories.DevLocal)]
    public async Task SearchUsers_ByUsername_FindsMatches()
    {
        // Arrange - Create a user with distinctive username
        var uniquePrefix = $"search_{Guid.NewGuid():N[..8]}";
        var username = $"{uniquePrefix}_user";

        var newUser = new UserRepresentation
        {
            Username = username,
            Email = $"{username}@example.com",
            Enabled = true
        };

        var userId = await _userClient!.CreateUser(newUser);
        _createdUserId = userId;

        TestContext.WriteLine($"Created searchable user: {username}");

        // Wait for indexing
        await Task.Delay(500);

        // Act - Search for the user
        var parameters = new GetUsersRequestParameters
        {
            Search = uniquePrefix
        };

        var searchResults = await _userClient.GetUsers(parameters);

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
    [TestCategory(TestCategories.DevLocal)]
    public async Task CreateDisabledUser_CannotAuthenticate()
    {
        // Arrange
        var username = $"disabled_{Guid.NewGuid():N}";
        var password = "TestPassword123!";

        var newUser = new UserRepresentation
        {
            Username = username,
            Email = $"{username}@example.com",
            Enabled = false, // Create as disabled
            EmailVerified = true
        };

        // Act - Create user and set password
        var userId = await _userClient!.CreateUser(newUser);
        _createdUserId = userId;

        var credential = new CredentialRepresentation
        {
            Type = "password",
            Value = password,
            Temporary = false
        };

        await _userClient.ResetUserPassword(userId, credential);
        TestContext.WriteLine($"Created disabled user: {username}");

        await Task.Delay(1000);

        // Attempt authentication
        var baseUrl = TestContext.GetProperty<string>("KEYCLOAK_URL") ?? "http://localhost:8081";
        var clientId = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_ID") ?? "integration-test-client";
        var clientSecret = TestContext.GetProperty<string>("KEYCLOAK_CLIENT_SECRET") ?? "test-client-secret-12345";

        var httpClient = new HttpClient();
        var tokenEndpoint = $"{baseUrl}/realms/{_realm}/protocol/openid-connect/token";

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
