# OoBDev.Keycloak.Tests

Integration tests for Keycloak identity and access management operations.

## Overview

This test project demonstrates how to interact with Keycloak programmatically using the Keycloak Admin API. Tests cover:

- **User Management**: Create, update, delete, search users
- **Authentication**: Password grants, token validation
- **Role Management**: Assign and verify user roles
- **User States**: Enabled, disabled, email verified/unverified

## Prerequisites

Keycloak must be running with the `integration-test` realm configured:

```bash
cd /current/src/containers/testing
./scripts/integration-up.sh --wait
```

Verify Keycloak is ready:
```bash
curl http://localhost:8081/health/ready
```

## Test Categories

All tests are marked with `[TestCategory(TestCategories.DevLocal)]` as they require a running Keycloak instance.

## Running Tests

### Run All Keycloak Tests

```bash
dotnet test --filter "TestCategory=DevLocal&FullyQualifiedName~Keycloak"
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~CreateUser_WithValidData_CreatesSuccessfully"
```

## Test Coverage

### User Management Tests

#### 1. Create User with Valid Data
- Creates a new user with basic information
- Verifies user exists and has correct attributes
- Tests custom attributes (department, employeeId)

#### 2. Create User with Password and Authenticate
- Creates user
- Sets password
- Authenticates using Resource Owner Password Credentials grant
- Validates access token is returned

#### 3. Create User with Roles
- Creates user with assigned roles
- Verifies role mappings are correct
- Tests both default and custom roles

#### 4. Update User
- Creates user with initial data
- Updates user information
- Verifies updates were applied

#### 5. List and Search Users
- Retrieves user list with pagination
- Searches users by username
- Verifies pre-configured users exist

#### 6. Create Disabled User
- Creates disabled user account
- Sets password
- Verifies authentication fails (negative test)

## Configuration

Tests use `TestContext.GetProperty<T>()` for configuration:

| Property | Default | Description |
|----------|---------|-------------|
| `KEYCLOAK_URL` | `http://localhost:8081` | Keycloak base URL |
| `KEYCLOAK_REALM` | `integration-test` | Realm name |
| `KEYCLOAK_CLIENT_ID` | `integration-test-client` | Client ID for authentication tests |
| `KEYCLOAK_CLIENT_SECRET` | `test-client-secret-12345` | Client secret |

### Using .runsettings

```xml
<TestRunParameters>
  <Parameter name="KEYCLOAK_URL" value="http://localhost:8081" />
  <Parameter name="KEYCLOAK_REALM" value="integration-test" />
  <Parameter name="KEYCLOAK_CLIENT_ID" value="integration-test-client" />
  <Parameter name="KEYCLOAK_CLIENT_SECRET" value="test-client-secret-12345" />
</TestRunParameters>
```

## Test Cleanup

All tests use `[TestCleanup]` to automatically delete created users:

```csharp
[TestCleanup]
public async Task Cleanup()
{
    if (!string.IsNullOrEmpty(_createdUserId))
    {
        await _userClient.DeleteUser(_createdUserId);
    }
}
```

This ensures tests don't leave orphaned data in Keycloak.

## Dependencies

- **Keycloak.AuthServices.Sdk** (2.6.3) - Official Keycloak .NET SDK
- **System.IdentityModel.Tokens.Jwt** (8.3.1) - JWT token handling
- **OoBDev.TestUtilities** - Test helpers and categories

## Example: Programmatic User Creation

```csharp
// Create admin client
var adminClient = new KeycloakAdminApiClient(
    new HttpClient { BaseAddress = new Uri("http://localhost:8081") }
);

var userClient = adminClient.Users("integration-test");

// Create user
var newUser = new UserRepresentation
{
    Username = "john.doe",
    Email = "john.doe@example.com",
    FirstName = "John",
    LastName = "Doe",
    Enabled = true,
    EmailVerified = true,
    Attributes = new Dictionary<string, IEnumerable<string>>
    {
        { "department", new[] { "Engineering" } }
    }
};

var userId = await userClient.CreateUser(newUser);

// Set password
var credential = new CredentialRepresentation
{
    Type = "password",
    Value = "SecurePassword123!",
    Temporary = false
};

await userClient.ResetUserPassword(userId, credential);

// User can now authenticate!
```

## Admin API Authentication

The Keycloak Admin SDK automatically handles authentication using:
- Admin credentials (configured in Keycloak container)
- Service account tokens (for client credentials grant)

For production use, configure proper admin credentials and use service accounts.

## Troubleshooting

### Issue: "Realm not found"
Ensure you're using the correct realm name (`integration-test`):
```bash
curl http://localhost:8081/realms/integration-test/.well-known/openid-configuration
```

### Issue: "Unauthorized" when creating users
The SDK needs admin access. Verify Keycloak admin credentials in the container are correct.

### Issue: User creation succeeds but authentication fails
Wait a moment after setting password for it to propagate:
```csharp
await _userClient.ResetUserPassword(userId, credential);
await Task.Delay(1000); // Wait for password to propagate
```

### Issue: Tests leave orphaned users
Ensure `[TestCleanup]` is running. Check for exceptions in cleanup:
```bash
# List all users in realm
curl -u admin:admin http://localhost:8081/admin/realms/integration-test/users
```

## See Also

- [Keycloak Testing Guide](../../../containers/testing/KEYCLOAK-TESTING.md) - Complete testing documentation
- [Keycloak Realm Configuration](../../../containers/testing/keycloak-config/README.md) - Pre-configured test realm
- [Test Variables](../../../TEST_VARIABLES.md) - All test configuration variables
- [Keycloak Admin API](https://www.keycloak.org/docs-api/latest/rest-api/) - Official API documentation
