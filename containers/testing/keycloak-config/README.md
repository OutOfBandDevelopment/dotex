# Keycloak Test Configuration

This directory contains Keycloak realm configuration for integration testing.

## Test Realm: `integration-test`

### Pre-configured Test Users

| Username | Password | Email | Roles | Status | Use Case |
|----------|----------|-------|-------|--------|----------|
| `testuser` | `testpassword` | testuser@example.com | user | ✅ Enabled, Verified | Standard authenticated user |
| `adminuser` | `adminpassword` | admin@example.com | user, admin | ✅ Enabled, Verified | Administrator/elevated privileges |
| `disableduser` | `disabledpassword` | disabled@example.com | user | ❌ Disabled | Testing disabled account handling |
| `unverifieduser` | `unverifiedpassword` | unverified@example.com | user | ⚠️ Email not verified | Testing email verification flows |

### Pre-configured Clients

#### Confidential Client (for server-side apps)
- **Client ID**: `integration-test-client`
- **Client Secret**: `test-client-secret-12345`
- **Grant Types**: Direct Access Grants, Service Account
- **Use**: Backend services, API testing

#### Public Client (for frontend apps)
- **Client ID**: `integration-test-public-client`
- **Client Secret**: N/A (public client)
- **Grant Types**: Authorization Code, Direct Access Grants
- **Use**: SPA applications, browser-based testing

### Roles

- `user` - Default role for standard users
- `admin` - Administrator privileges
- `test-role` - Custom role for testing RBAC

### Groups

- `test-group` - Test group with `test-role` assigned

## Usage in Tests

### Get Access Token (Resource Owner Password Credentials)

```bash
# Using confidential client
curl -X POST http://localhost:8081/realms/integration-test/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=integration-test-client" \
  -d "client_secret=test-client-secret-12345" \
  -d "grant_type=password" \
  -d "username=testuser" \
  -d "password=testpassword"
```

### C# Example

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task AuthenticateUser_WithKeycloak_ReturnsAccessToken()
{
    var baseUrl = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
    var realm = TestContext.GetRequiredProperty<string>("KEYCLOAK_REALM");
    var clientId = TestContext.GetRequiredProperty<string>("KEYCLOAK_CLIENT_ID");
    var clientSecret = TestContext.GetRequiredProperty<string>("KEYCLOAK_CLIENT_SECRET");

    var username = TestContext.GetRequiredProperty<string>("KEYCLOAK_TEST_USERNAME");
    var password = TestContext.GetRequiredProperty<string>("KEYCLOAK_TEST_PASSWORD");

    var client = new HttpClient();
    var tokenEndpoint = $"{baseUrl}/realms/{realm}/protocol/openid-connect/token";

    var requestBody = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", clientId },
        { "client_secret", clientSecret },
        { "grant_type", "password" },
        { "username", username },
        { "password", password }
    });

    var response = await client.PostAsync(tokenEndpoint, requestBody);
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);

    Assert.IsNotNull(tokenResponse);
    Assert.IsFalse(string.IsNullOrEmpty(tokenResponse.AccessToken));
}
```

## Modifying Test Realm

To add new users, roles, or clients:

1. Edit `integration-test-realm.json`
2. Restart Keycloak container:
   ```bash
   cd /current/src/containers/testing
   ./scripts/integration-down.sh
   ./scripts/integration-up.sh
   ```

The realm will be re-imported with your changes.

## Environment Variables for Tests

Add these to your `.runsettings` or test configuration:

```xml
<TestRunParameters>
  <Parameter name="KEYCLOAK_URL" value="http://localhost:8081" />
  <Parameter name="KEYCLOAK_REALM" value="integration-test" />
  <Parameter name="KEYCLOAK_CLIENT_ID" value="integration-test-client" />
  <Parameter name="KEYCLOAK_CLIENT_SECRET" value="test-client-secret-12345" />
  <Parameter name="KEYCLOAK_TEST_USERNAME" value="testuser" />
  <Parameter name="KEYCLOAK_TEST_PASSWORD" value="testpassword" />
  <Parameter name="KEYCLOAK_ADMIN_USERNAME" value="adminuser" />
  <Parameter name="KEYCLOAK_ADMIN_PASSWORD" value="adminpassword" />
</TestRunParameters>
```

## Security Notes

⚠️ **These credentials are for LOCAL TESTING ONLY**

- Never use these credentials in production
- Never commit real credentials to version control
- The realm file is designed for ephemeral test environments
- All containers and data are destroyed after tests (`docker-compose down -v`)

## Realm Import Process

The realm is automatically imported when Keycloak starts via:

```yaml
keycloak:
  environment:
    - KEYCLOAK_ADMIN=admin
    - KEYCLOAK_ADMIN_PASSWORD=admin
    - KC_HEALTH_ENABLED=true
  volumes:
    - ./keycloak-config/integration-test-realm.json:/opt/keycloak/data/import/integration-test-realm.json:ro
  command: start-dev --import-realm --features=scripts
```

**Configuration Notes:**
- The `--import-realm` flag tells Keycloak to import all JSON files from `/opt/keycloak/data/import/` on startup
- `--features=scripts` enables JavaScript-based authentication flows in development mode
- Script features are preview/optional in Keycloak for security reasons
- Safe for ephemeral test environments, carefully evaluate for production

## Troubleshooting

### Realm not imported
- Check container logs: `docker logs oobd-test-keycloak`
- Verify JSON syntax: `jq . integration-test-realm.json`
- Ensure volume mount is correct in docker-compose

### Cannot authenticate
- Verify realm name is `integration-test`
- Check username/password match realm configuration
- Ensure user is enabled and email is verified (if required)
- Check client ID and secret are correct

### Health check failing
- Keycloak takes 30-40 seconds to start in dev mode
- Check logs for startup errors
- Verify port 8081 is accessible: `curl http://localhost:8081/health/ready`

### "Script upload is disabled" error
- This occurs when the realm contains JavaScript authenticators but script features are disabled
- **Solution**: Use `--features=scripts` in the start command (already configured in docker-compose)
- Note: `scripts` is a preview feature in Keycloak and must be explicitly enabled
- Safe for ephemeral test environments, carefully evaluate for production

### "Unrecognized feature" error
- If you see errors about unrecognized features, check the valid features list in the error message
- Common mistake: `upload-scripts` is NOT a valid feature (use `scripts` instead)
- Valid feature format: `--features=feature1,feature2` (comma-separated, no spaces)

## Reverse Proxy Configuration

Keycloak is accessible through the nginx reverse proxy at `http://localhost:8080/keycloak/` and directly at `http://localhost:8081/`.

### Nginx Configuration

The nginx reverse proxy is configured to handle Keycloak's redirects correctly:

```nginx
location /keycloak/ {
    set $upstream_keycloak keycloak:8080;
    rewrite ^/keycloak/(.*)$ /$1 break;
    proxy_pass http://$upstream_keycloak;

    # Forward original request information
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Forwarded-Host $host;
    proxy_set_header X-Forwarded-Port $server_port;

    # Rewrite redirects to include /keycloak prefix
    proxy_redirect http://$host/ http://$host/keycloak/;
    proxy_redirect https://$host/ https://$host/keycloak/;
}
```

### Keycloak Proxy Settings

Keycloak is configured to work behind a reverse proxy:

```yaml
environment:
  - KC_PROXY=edge           # Trust X-Forwarded-* headers from edge proxy
  - KC_HOSTNAME_STRICT=false  # Allow flexible hostname handling
```

**Key Configuration:**
- `KC_PROXY=edge` - Tells Keycloak to trust X-Forwarded headers from the reverse proxy
- `KC_HOSTNAME_STRICT=false` - Allows Keycloak to work with different hostnames (localhost, docker service names)
- `proxy_redirect` in nginx - Rewrites redirect Location headers to include `/keycloak` prefix

### Access URLs

- **Through Nginx (Recommended for integration tests)**: `http://localhost:8080/keycloak/`
- **Direct Access**: `http://localhost:8081/`
- **Admin Console (via nginx)**: `http://localhost:8080/keycloak/admin/`
- **Admin Console (direct)**: `http://localhost:8081/admin/`

**Note:** When using nginx proxy, all URLs must include the `/keycloak` prefix. The proxy automatically handles redirects to maintain this prefix.

## References

- [Keycloak Server Administration Guide](https://www.keycloak.org/docs/latest/server_admin/)
- [Realm Import/Export](https://www.keycloak.org/server/importExport)
- [Admin REST API](https://www.keycloak.org/docs-api/latest/rest-api/)
