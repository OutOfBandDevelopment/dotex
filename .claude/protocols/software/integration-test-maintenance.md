# Integration Test Maintenance Protocol

**Version:** 1.1.0
**Last Updated:** 2026-01-24
**Category:** Software Development

---

## Purpose

This protocol defines the checklist for maintaining integration tests in the OoBDev project. When adding new Docker services, integration test parameters, or test files, follow this checklist to ensure all related files are updated consistently.

---

## When to Use This Protocol

Use this protocol when:

1. **Adding a new Docker service** to the integration test stack
2. **Adding new test parameters** (connection strings, credentials, URLs)
3. **Creating new integration tests** that require configuration
4. **Modifying existing service ports or credentials**
5. **Removing or deprecating services**

---

## Checklist: Adding a New Docker Service

### 1. Docker Compose Configuration

**File:** `/containers/testing/docker-compose.integration-tests.yml`

- [ ] Add service definition with appropriate image
- [ ] Configure port mappings (use non-conflicting ports)
- [ ] Add to `integration-test-net` network
- [ ] Configure health check with appropriate timing
- [ ] Add ephemeral volume if stateful (named `{service}-test-data`)
- [ ] Add volume to volumes section at bottom of file

**Example:**
```yaml
myservice:
  image: myservice/myservice:latest
  container_name: oobd-test-myservice
  ports:
    - "9999:9999"
  networks:
    - integration-test-net
  environment:
    - MY_CONFIG=value
  volumes:
    - myservice-test-data:/data
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:9999/health"]
    interval: 10s
    timeout: 5s
    retries: 5
    start_period: 10s

volumes:
  myservice-test-data:
    name: oobd-test-myservice-data
```

### 2. Startup Scripts

**Files:** `/containers/testing/scripts/integration-up.sh` and `/containers/testing/scripts/integration-up.bat`

- [ ] Add service to startup output display (shows all running services with URLs/ports)
- [ ] Update port list in script header comments
- [ ] Ensure service appears in correct alphabetical/logical order
- [ ] Include connection details (URL, port, or connection string)
- [ ] Include credentials if applicable (username/password)

**Example addition to integration-up.sh:**
```bash
echo "Services running:"
echo "  - Apache Tika:        http://localhost:9998"
# ... other services ...
echo "  - MyService:          http://localhost:9999 (admin/password)"
echo ""
```

**Example addition to integration-up.bat:**
```batch
echo Services running:
echo   - Apache Tika:        http://localhost:9998
REM ... other services ...
echo   - MyService:          http://localhost:9999 (admin/password)
echo.
```

**Port list update in script header:**
```bash
# Requirements:
#   - Docker and Docker Compose installed
#   - Ports available: 25, 1433, ..., 9999, ...
```

### 3. Health Check Scripts

**Files:** `/containers/testing/scripts/wait-for-services.sh` and `/containers/testing/scripts/wait-for-services.bat`

- [ ] Add container name to SERVICES array (must match `container_name` from docker-compose)
- [ ] Ensure container name follows pattern: `oobd-test-{servicename}`
- [ ] Place in logical order (typically alphabetical or by dependency)

**Example:**
```bash
SERVICES=(
    "oobd-test-tika"
    "oobd-test-smtp"
    # ... other services ...
    "oobd-test-myservice"
)
```

**Windows batch file:**
```batch
set SERVICES=oobd-test-tika oobd-test-smtp ... oobd-test-myservice
```

### 4. Nginx Reverse Proxy Configuration

**File:** `/containers/testing/nginx/nginx.conf`

- [ ] Add location block for web UI (if applicable)
- [ ] Add location block for API endpoint (if applicable)
- [ ] Use Docker DNS resolver pattern with `set $upstream_*` variable
- [ ] Configure appropriate proxy headers
- [ ] Add rewrite rules if service needs path prefix stripping

**Example:**
```nginx
# MyService Web UI
location /myservice/ {
    set $upstream_myservice myservice:9999;
    rewrite ^/myservice/(.*)$ /$1 break;
    proxy_pass http://$upstream_myservice;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;

    # Rewrite redirects to include /myservice prefix
    proxy_redirect / /myservice/;
    proxy_redirect ~^/(.*)$ /myservice/$1;
}

# MyService API
location /myservice-api/ {
    rewrite ^/myservice-api/(.*)$ /$1 break;
    set $upstream_myservice_api myservice:9999;
    proxy_pass http://$upstream_myservice_api;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

### 5. Dashboard HTML

**File:** `/containers/testing/nginx/html/index.html`

- [ ] Update statistics (total services count, web UIs count, API endpoints count)
- [ ] Add service card in appropriate section (Web Interfaces, Database Services, or Cloud Emulators)
- [ ] Include service name with emoji icon
- [ ] Include service description
- [ ] Include links (web UI and/or API endpoint)
- [ ] Include port information
- [ ] Include credentials if applicable (username/password)

**Example:**
```html
<!-- MyService -->
<div class="service-card">
    <div class="service-header">
        <span class="service-name">🎯 MyService</span>
        <span class="service-status status-running">Web UI</span>
    </div>
    <div class="service-description">
        Description of the service. User: <strong>admin</strong> / Pass: <strong>password</strong>
    </div>
    <div class="service-links">
        <a href="http://localhost:9999" class="service-link" target="_blank">Open Dashboard :9999</a>
    </div>
    <div class="port-info">
        <div class="port-item">HTTP: <span class="port-number">localhost:9999</span></div>
        <div class="port-item">Config: <span class="port-number">myconfig-value</span></div>
    </div>
</div>
```

### 6. Test Run Settings

**File:** `/src/.runsettings`

- [ ] Add all connection parameters in `<TestRunParameters>` section
- [ ] Include URL, HOST, PORT variations for flexibility
- [ ] Include connection string if applicable
- [ ] Include credentials (username, password)
- [ ] Add comments grouping related parameters

**Example:**
```xml
<!-- MyService -->
<Parameter name="MYSERVICE_URL" value="http://localhost:9999" />
<Parameter name="MYSERVICE_HOST" value="localhost" />
<Parameter name="MYSERVICE_PORT" value="9999" />
<Parameter name="MYSERVICE_USERNAME" value="admin" />
<Parameter name="MYSERVICE_PASSWORD" value="password" />
<Parameter name="MYSERVICE_CONNECTION_STRING" value="Host=localhost;Port=9999;Username=admin;Password=password" />
```

### 7. Environment Configuration

**File:** `/containers/testing/.env.integration`

- [ ] Add all service-specific environment variables
- [ ] Follow naming convention: `{SERVICENAME}_{PROPERTY}`
- [ ] Include URL, HOST, PORT, credentials, and connection strings
- [ ] Add comments grouping related variables

**Example:**
```bash
# MyService Configuration
MYSERVICE_URL=http://localhost:9999
MYSERVICE_HOST=localhost
MYSERVICE_PORT=9999
MYSERVICE_USERNAME=admin
MYSERVICE_PASSWORD=password
MYSERVICE_CONNECTION_STRING=Host=localhost;Port=9999;Username=admin;Password=password
```

### 8. Documentation Updates

**Files to update:**

- [ ] `/containers/testing/README.md` - Add service to tables and architecture diagrams
- [ ] `/containers/testing/NGINX-DASHBOARD.md` - Add service to proxy path table
- [ ] `/src/TEST_VARIABLES.md` - Document new test parameters
- [ ] Service-specific README (if complex configuration)

### 9. CI/CD Workflow

**File:** `/.github/workflows/integration-tests.yml`

- [ ] Add environment variables to the test run step
- [ ] Match variable names from .env.integration and .runsettings
- [ ] Include all connection details needed for tests

**Example:**
```yaml
env:
  # ... other services ...

  # MyService
  MYSERVICE_URL: http://localhost:9999
  MYSERVICE_HOST: localhost
  MYSERVICE_PORT: 9999
  MYSERVICE_USERNAME: admin
  MYSERVICE_PASSWORD: password
  MYSERVICE_CONNECTION_STRING: Host=localhost;Port=9999;Username=admin;Password=password
```

### 10. Test Implementation

**Test file requirements:**

- [ ] Use `TestContext.GetRequiredProperty<T>()` for non-standard values (URLs, credentials, connection strings)
- [ ] Use `TestContext.GetPropertyOrDefault<T>(name, default)` for industry-standard values (port numbers)
- [ ] NEVER use `Environment.GetEnvironmentVariable()` - the extension methods check both .runsettings and environment
- [ ] Add `[TestCategory(TestCategories.Integration)]` attribute
- [ ] Create unique resource names per test run (e.g., `$"Test_{Guid.NewGuid():N}"`)
- [ ] Implement cleanup in `[TestCleanup]` method

**Example:**
```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task MyService_Operation_Succeeds()
{
    // Required values - no defaults, must be configured
    var url = TestContext.GetRequiredProperty<string>("MYSERVICE_URL");
    var username = TestContext.GetRequiredProperty<string>("MYSERVICE_USERNAME");
    var password = TestContext.GetRequiredProperty<string>("MYSERVICE_PASSWORD");

    // Industry-standard port - use default
    var port = TestContext.GetPropertyOrDefault("MYSERVICE_PORT", 9999);

    // Create unique resource name
    var resourceName = $"IntegrationTest_{Guid.NewGuid():N}";
    _createdResourceName = resourceName;

    // Test implementation...
}

[TestCleanup]
public async Task Cleanup()
{
    if (!string.IsNullOrEmpty(_createdResourceName))
    {
        // Clean up test resources
    }
}
```

**When to use each method:**

| Method | Use When | Examples |
|--------|----------|----------|
| `GetRequiredProperty<T>()` | Value must be explicitly configured | URLs, usernames, passwords, connection strings, realm names, client IDs |
| `GetPropertyOrDefault<T>()` | Industry-standard default exists | Port 5432 (PostgreSQL), Port 27017 (MongoDB), Port 6379 (Redis) |

---

## Checklist: Modifying Service Configuration

When changing ports, credentials, or URLs:

- [ ] Update `/containers/testing/docker-compose.integration-tests.yml`
- [ ] Update `/containers/testing/scripts/integration-up.sh` (display output and port list)
- [ ] Update `/containers/testing/scripts/integration-up.bat` (display output and port list)
- [ ] Update `/containers/testing/nginx/nginx.conf` (if port changed)
- [ ] Update `/containers/testing/nginx/html/index.html` (port info and credentials)
- [ ] Update `/containers/testing/.env.integration` with new values
- [ ] Update `/src/.runsettings` with new values
- [ ] Update `/.github/workflows/integration-tests.yml` environment variables
- [ ] Update `/containers/testing/README.md` tables
- [ ] Update `/src/TEST_VARIABLES.md` documentation
- [ ] Search for hardcoded values in test files and update
- [ ] Update any service-specific config files (e.g., `keycloak-config/`, `servicebus-config/`)

---

## Checklist: Removing a Service

- [ ] Remove service from `docker-compose.integration-tests.yml`
- [ ] Remove volume from volumes section
- [ ] Remove from `scripts/integration-up.sh` display output and port list
- [ ] Remove from `scripts/integration-up.bat` display output and port list
- [ ] Remove from `scripts/wait-for-services.sh` SERVICES array
- [ ] Remove from `scripts/wait-for-services.bat` SERVICES list
- [ ] Remove location blocks from `nginx/nginx.conf`
- [ ] Remove service card from `nginx/html/index.html`
- [ ] Update statistics in dashboard HTML
- [ ] Remove variables from `.env.integration`
- [ ] Remove parameters from `/src/.runsettings`
- [ ] Remove environment variables from `/.github/workflows/integration-tests.yml`
- [ ] Remove or update affected integration tests
- [ ] Update `/containers/testing/README.md` tables and diagrams
- [ ] Update `/src/TEST_VARIABLES.md` documentation

---

## File Reference Quick Links

| File | Purpose |
|------|---------|
| `/containers/testing/docker-compose.integration-tests.yml` | Docker service definitions |
| `/containers/testing/scripts/integration-up.sh` | Startup script (Linux/macOS) - displays all services |
| `/containers/testing/scripts/integration-up.bat` | Startup script (Windows) - displays all services |
| `/containers/testing/scripts/wait-for-services.sh` | Health check script (Linux/macOS) |
| `/containers/testing/scripts/wait-for-services.bat` | Health check script (Windows) |
| `/containers/testing/nginx/nginx.conf` | Reverse proxy routing |
| `/containers/testing/nginx/html/index.html` | Dashboard UI |
| `/containers/testing/.env.integration` | Environment variables for Docker services |
| `/src/.runsettings` | Test parameters for MSTest |
| `/.github/workflows/integration-tests.yml` | CI/CD workflow environment variables |
| `/containers/testing/README.md` | Infrastructure documentation |
| `/containers/testing/NGINX-DASHBOARD.md` | Dashboard documentation |
| `/src/TEST_VARIABLES.md` | Test parameter reference |

---

## Validation Steps

After making changes:

1. **Start the stack:**
   ```bash
   cd containers/testing
   ./scripts/integration-up.sh --wait
   ```

2. **Verify dashboard:**
   - Open http://localhost:8080
   - Verify new service appears
   - Verify links work

3. **Run tests:**
   ```bash
   cd src
   dotnet test --filter "TestCategory=Integration"
   ```

4. **Clean up:**
   ```bash
   cd containers/testing
   ./scripts/integration-down.sh --clean
   ```

---

## Common Patterns

### Authentication Pattern (Token-based)

For services where admin credentials exist in a different realm/context than the target resources:

```csharp
// Get token from admin realm
var adminToken = await GetAdminTokenAsync(adminUsername, adminPassword);

// Use token to manage resources in target realm/context
var client = new ServiceClient(baseUrl, () => adminToken);
await client.CreateResourceAsync(targetRealm, resource);
```

### TestContext Property Patterns

Use the appropriate extension method based on whether a sensible default exists:

```csharp
// REQUIRED VALUES - URLs, credentials, connection strings (no sensible default)
var url = TestContext.GetRequiredProperty<string>("MYSERVICE_URL");
var username = TestContext.GetRequiredProperty<string>("MYSERVICE_USERNAME");
var password = TestContext.GetRequiredProperty<string>("MYSERVICE_PASSWORD");
var connectionString = TestContext.GetRequiredProperty<string>("MYSERVICE_CONNECTION_STRING");

// OPTIONAL VALUES WITH INDUSTRY DEFAULTS - port numbers, timeouts
var port = TestContext.GetPropertyOrDefault("MONGODB_PORT", 27017);
var redisPort = TestContext.GetPropertyOrDefault("REDIS_PORT", 6379);
var timeout = TestContext.GetPropertyOrDefault("REQUEST_TIMEOUT_MS", 30000);

// INCORRECT - Never use environment variables directly
var value = Environment.GetEnvironmentVariable("PARAMETER_NAME"); // DON'T DO THIS
```

**Note:** Both `GetRequiredProperty` and `GetPropertyOrDefault` check .runsettings first, then fall back to environment variables automatically.

### Unique Resource Naming

Prevent test collisions with unique names:

```csharp
var uniqueName = $"IntegrationTest_{Guid.NewGuid():N}";
var uniqueDb = $"TestDb_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
```

---

## Related Documentation

- [Testing Guidelines](../../../docs/architecture/testing/testing-guidelines.md) - Overall testing strategy
- [Testing README](../../../docs/architecture/testing/README.md) - Testing documentation index
- [Test Variables Reference](../../../TEST_VARIABLES.md) - Complete test property reference
- [Docker Infrastructure](../../../containers/testing/README.md) - Container management

## Related Protocols

- [Change Documentation Archival](../documentation/change-documentation-archival.md) - Documenting completed work

---

**Protocol Owner:** DevOps / Testing Team
**Review Cycle:** Quarterly or when adding new services
