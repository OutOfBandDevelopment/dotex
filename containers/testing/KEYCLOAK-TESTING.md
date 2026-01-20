# Keycloak Integration Testing Guide

This guide explains how to use Keycloak for integration testing with pre-configured test accounts.

## Quick Start

### 1. Start Integration Test Stack

```bash
cd /current/src/containers/testing
./scripts/integration-up.sh --wait
```

Keycloak will be available at `http://localhost:8081`

### 2. Verify Keycloak is Ready

```bash
# Check health
curl http://localhost:8081/health/ready

# Access admin console (credentials: admin/admin)
open http://localhost:8081
```

### 3. Test User Authentication

```bash
# Get access token for test user
curl -X POST http://localhost:8081/realms/integration-test/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=integration-test-client" \
  -d "client_secret=test-client-secret-12345" \
  -d "grant_type=password" \
  -d "username=testuser" \
  -d "password=testpassword" | jq
```

## Pre-configured Test Accounts

The `integration-test` realm comes with 4 pre-configured users:

| Username | Password | Roles | Email Verified | Enabled | Purpose |
|----------|----------|-------|----------------|---------|---------|
| `testuser` | `testpassword` | user | ✅ Yes | ✅ Yes | Standard user for happy path tests |
| `adminuser` | `adminpassword` | user, admin | ✅ Yes | ✅ Yes | Admin user for elevated privilege tests |
| `disableduser` | `disabledpassword` | user | ✅ Yes | ❌ No | Testing disabled account handling |
| `unverifieduser` | `unverifiedpassword` | user | ❌ No | ✅ Yes | Testing email verification flows |

## Pre-configured Clients

### Confidential Client (Server-side)
- **Client ID**: `integration-test-client`
- **Client Secret**: `test-client-secret-12345`
- **Grant Types**: Password, Client Credentials, Service Account
- **Use Case**: Backend API testing, server-to-server authentication

### Public Client (Browser-based)
- **Client ID**: `integration-test-public-client`
- **Client Secret**: None (public client)
- **Grant Types**: Authorization Code, Password
- **Use Case**: SPA applications, browser-based integration tests

## Integration Test Examples

### Example 1: Basic Authentication Test

```csharp
using System.Net.Http;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

[TestClass]
public class KeycloakAuthenticationTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task AuthenticateUser_WithValidCredentials_ReturnsAccessToken()
    {
        // Arrange
        var baseUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
        var realm = "integration-test";
        var tokenEndpoint = $"{baseUrl}/realms/{realm}/protocol/openid-connect/token";

        var client = new HttpClient();
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", "integration-test-client" },
            { "client_secret", "test-client-secret-12345" },
            { "grant_type", "password" },
            { "username", "testuser" },
            { "password", "testpassword" }
        });

        // Act
        var response = await client.PostAsync(tokenEndpoint, requestBody);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode, "Authentication should succeed");

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

        Assert.IsNotNull(tokenResponse?.AccessToken);
        Assert.IsNotNull(tokenResponse?.RefreshToken);
        Assert.AreEqual("Bearer", tokenResponse?.TokenType);

        TestContext.WriteLine($"Access Token: {tokenResponse?.AccessToken?[..50]}...");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task AuthenticateUser_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var baseUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
        var tokenEndpoint = $"{baseUrl}/realms/integration-test/protocol/openid-connect/token";

        var client = new HttpClient();
        var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", "integration-test-client" },
            { "client_secret", "test-client-secret-12345" },
            { "grant_type", "password" },
            { "username", "testuser" },
            { "password", "wrongpassword" }
        });

        // Act
        var response = await client.PostAsync(tokenEndpoint, requestBody);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
```

### Example 2: Testing Disabled User

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task AuthenticateUser_WithDisabledAccount_ReturnsUnauthorized()
{
    // Arrange
    var baseUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
    var tokenEndpoint = $"{baseUrl}/realms/integration-test/protocol/openid-connect/token";

    var client = new HttpClient();
    var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", "integration-test-client" },
        { "client_secret", "test-client-secret-12345" },
        { "grant_type", "password" },
        { "username", "disableduser" },
        { "password", "disabledpassword" }
    });

    // Act
    var response = await client.PostAsync(tokenEndpoint, requestBody);

    // Assert
    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

    var content = await response.Content.ReadAsStringAsync();
    Assert.IsTrue(content.Contains("Account is disabled") || content.Contains("invalid_grant"));
}
```

### Example 3: Validate Token Claims

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task ValidateToken_ReturnsCorrectClaims()
{
    // Arrange - Get access token
    var baseUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
    var tokenEndpoint = $"{baseUrl}/realms/integration-test/protocol/openid-connect/token";

    var client = new HttpClient();
    var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", "integration-test-client" },
        { "client_secret", "test-client-secret-12345" },
        { "grant_type", "password" },
        { "username", "adminuser" },
        { "password", "adminpassword" }
    });

    var response = await client.PostAsync(tokenEndpoint, requestBody);
    var content = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

    // Act - Decode JWT (basic parsing, not validation)
    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var token = handler.ReadJwtToken(tokenResponse!.AccessToken);

    // Assert
    Assert.AreEqual("adminuser", token.Claims.First(c => c.Type == "preferred_username").Value);
    Assert.AreEqual("admin@example.com", token.Claims.First(c => c.Type == "email").Value);

    var roles = token.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToList();
    CollectionAssert.Contains(roles, "user");
    CollectionAssert.Contains(roles, "admin");
}
```

### Example 4: Client Credentials Grant

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task GetServiceAccountToken_WithClientCredentials_Succeeds()
{
    // Arrange
    var baseUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
    var tokenEndpoint = $"{baseUrl}/realms/integration-test/protocol/openid-connect/token";

    var client = new HttpClient();
    var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", "integration-test-client" },
        { "client_secret", "test-client-secret-12345" },
        { "grant_type", "client_credentials" }
    });

    // Act
    var response = await client.PostAsync(tokenEndpoint, requestBody);

    // Assert
    Assert.IsTrue(response.IsSuccessStatusCode);

    var content = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

    Assert.IsNotNull(tokenResponse?.AccessToken);
    TestContext.WriteLine("Service account token obtained successfully");
}
```

## Test Configuration

### Using .runsettings

Add to your `.runsettings` file:

```xml
<TestRunParameters>
  <!-- Keycloak Connection -->
  <Parameter name="KEYCLOAK_URL" value="http://localhost:8081" />
  <Parameter name="KEYCLOAK_REALM" value="integration-test" />

  <!-- Confidential Client -->
  <Parameter name="KEYCLOAK_CLIENT_ID" value="integration-test-client" />
  <Parameter name="KEYCLOAK_CLIENT_SECRET" value="test-client-secret-12345" />

  <!-- Test Users -->
  <Parameter name="KEYCLOAK_TEST_USERNAME" value="testuser" />
  <Parameter name="KEYCLOAK_TEST_PASSWORD" value="testpassword" />
  <Parameter name="KEYCLOAK_ADMIN_USERNAME" value="adminuser" />
  <Parameter name="KEYCLOAK_ADMIN_PASSWORD" value="adminpassword" />
</TestRunParameters>
```

### Using TestContext

```csharp
var keycloakUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
var realm = TestContext.GetRequiredProperty<string>("KEYCLOAK_REALM");
var clientId = TestContext.GetRequiredProperty<string>("KEYCLOAK_CLIENT_ID");
```

## Admin Console Access

- **URL**: `http://localhost:8081`
- **Username**: `admin`
- **Password**: `admin`
- **Realm**: Select `integration-test` from dropdown

Use the admin console to:
- View test users
- Inspect tokens
- Check role assignments
- Debug authentication issues

## Customizing Test Realm

To add new users, roles, or clients:

1. Edit `/current/src/containers/testing/keycloak-config/integration-test-realm.json`
2. Restart containers:
   ```bash
   ./scripts/integration-down.sh
   ./scripts/integration-up.sh --wait
   ```

The realm will be re-imported with your changes.

## Troubleshooting

### Issue: "Realm not found"
**Solution**: Ensure you're using `integration-test` as the realm name, not `master`

```bash
# Correct
http://localhost:8081/realms/integration-test/protocol/openid-connect/token

# Wrong
http://localhost:8081/realms/master/protocol/openid-connect/token
```

### Issue: "Invalid client credentials"
**Solution**: Verify client ID and secret match the realm configuration

```bash
# Check realm configuration
curl http://localhost:8081/realms/integration-test/.well-known/openid-configuration | jq
```

### Issue: Container not starting
**Solution**: Check logs and ensure port 8081 is available

```bash
docker logs oobd-test-keycloak
lsof -i :8081  # Check if port is in use
```

### Issue: User authentication fails
**Solution**: Verify user exists and is enabled

1. Open admin console: `http://localhost:8081`
2. Login as `admin/admin`
3. Select `integration-test` realm
4. Go to Users → View all users
5. Check user status and credentials

## Security Warnings

⚠️ **TESTING ONLY - DO NOT USE IN PRODUCTION**

- All passwords are hardcoded for testing convenience
- Client secrets are visible in configuration files
- No SSL/TLS encryption (HTTP only)
- Simplified security settings for development mode
- All data is ephemeral (destroyed with `docker-compose down -v`)

## See Also

- [Keycloak Configuration](./keycloak-config/README.md) - Detailed realm configuration
- [Integration Test Infrastructure](./README.md) - Overall testing setup
- [Test Variables](../../TEST_VARIABLES.md) - All test configuration variables
- [Keycloak Documentation](https://www.keycloak.org/documentation)
