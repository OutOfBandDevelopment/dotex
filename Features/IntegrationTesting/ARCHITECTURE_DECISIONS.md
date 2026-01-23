# Architecture & Implementation Decisions

**Last Updated:** 2026-01-16
**Status:** Ready for Implementation

## Executive Summary

This document records key architectural decisions for OoBDev integration testing implementation. These decisions clarify ambiguities in planning documents and provide definitive guidance for developers during implementation phases. All decisions balance simplicity, performance, and maintainability.

## Table of Contents

- [Schema Migrations](#schema-migrations)
- [Flaky Test Handling](#flaky-test-handling)
- [Test Parallelization](#test-parallelization)
- [Local Development](#local-development)
- [Secret Management](#secret-management)
- [Test Data](#test-data)
- [Backwards Compatibility](#backwards-compatibility)
- [Performance Monitoring](#performance-monitoring)
- [Release Publishing](#release-publishing)
- [Service Startup](#service-startup)

---

## Schema Migrations

**Decision:** Use baseline database with idempotent migrations applied during test startup. Leverage SSDT/DacFx where possible for deterministic schema management.

### Approach

1. **Baseline Database:**
   - Start with empty SQL Server container
   - Apply baseline schema via SSDT/DacFx DACPAC file (deterministic)
   - OR use init.sql script for simpler projects

2. **Idempotent Migrations:**
   - All migrations must be idempotent (safe to run multiple times)
   - Use `IF NOT EXISTS` pattern for schema creation
   - Track applied versions to avoid re-applying
   - **Note:** Idempotency should be applied across ALL architectural and design documents

3. **Final State Always:**
   - Tests expect "latest schema" state
   - No downgrade/upgrade scenarios tested (unless backwards compatibility required)
   - Roll-forward only (no rollback testing)

### Implementation

```csharp
// Test fixture applies migrations
public class DatabaseFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Deploy DACPAC (idempotent)
        var dacpacPath = "path/to/database.dacpac";
        await DeployDacpacAsync(connectionString, dacpacPath);

        // OR run migration scripts
        await RunMigrationsAsync(connectionString, "migrations/");
    }
}
```

---

## Flaky Test Handling

**Decision:** Quarantine known flaky tests - tag them to skip in CI, run manually/locally only.

### Approach

1. **Identify Flaky Tests:**
   - After initial test runs, identify tests that fail intermittently
   - Document reason (network timeout, timing, etc.)

2. **Quarantine (Don't Retry):**
   - Use `[TestCategory("Quarantine")]` attribute
   - CI filters: `--filter "TestCategory!=Quarantine"`
   - Developers can run locally: `--filter "TestCategory=Quarantine"` for debugging

3. **No Automatic Retries:**
   - Don't mask flakiness with retries
   - Investigate and fix root cause
   - Move test to Quarantine while investigating

### Example

```csharp
[TestClass]
[TestCategory("Integration")]
[TestCategory("Quarantine")]  // ← Skip in CI
public class FlakeyNetworkTests
{
    [TestMethod]
    public async Task ExternalApiCall_Timeout()
    {
        // This test sometimes fails due to network latency
        // Fix: Mock network or increase timeout
        // Then remove Quarantine tag
    }
}
```

---

## Test Parallelization

**Decision:** Run integration tests in parallel with **data isolation** (not container isolation). Tests share containers but have isolated datasets.

### Approach

1. **Shared Containers:**
   - Single SQL Server instance for all tests (not per-test containers)
   - Single MongoDB instance for all tests
   - Reduces startup time and resource usage

2. **Data Isolation:**
   - Each test class gets isolated database/schema
   - Use unique test IDs: `test_user_<TestClassName>_<MethodName>`
   - Clean up after each test class (not after each method)

3. **Parallel Execution:**
   - MSTest runs test classes in parallel by default
   - Use `[AssemblyInitialize]` for shared setup
   - Use `[ClassInitialize]` for per-class isolation

### Example

```csharp
[TestClass]
[TestCategory("Integration")]
public class UserRepositoryTests : IAsyncLifetime
{
    // Unique test prefix per class
    private const string TestPrefix = "test_users_UserRepositoryTests";
    private SqlConnection _connection;

    public async Task InitializeAsync()
    {
        _connection = new SqlConnection(connectionString);

        // Create isolated database for this test class
        await CreateTestDatabaseAsync($"{TestPrefix}_db");
        await CreateTestSchemaAsync($"{TestPrefix}_db");
    }

    public async Task DisposeAsync()
    {
        // Drop isolated database
        await DropTestDatabaseAsync($"{TestPrefix}_db");
        _connection?.Close();
    }

    [TestMethod]
    public async Task SaveUser_Persists_Successfully()
    {
        // Tests use isolated {TestPrefix}_db
    }
}
```

---

## Local Development

**Decision:** Developers can run integration tests locally using docker-compose.

### Setup

1. **Prerequisites:**
   - Docker Desktop installed
   - `Features/Integration/Workflows/docker-compose.yml` in place
   - .NET SDK 10.0

2. **Local Execution:**
   ```bash
   # Terminal 1: Start services
   cd Features/Integration/Workflows
   docker-compose up -d

   # Terminal 2: Run integration tests
   dotnet test src/ --filter "TestCategory=Integration"

   # Cleanup
   docker-compose down
   ```

3. **docker-compose Override (Optional):**
   - Create `docker-compose.override.yml` for local tweaks
   - Gitignore this file (local customization only)
   - Example: Slower timeouts for debugging, verbose logging

---

## Secret Management

**Decision:** Use default safe credentials for isolated Docker container tests. External/cloud tests require GitHub Secrets and .env.local files.

### For Isolated Container Tests (Current)

**Default credentials are acceptable:**
- Test containers are ephemeral (discarded after test run)
- No real data/services to protect
- Credentials work everywhere (no environment-specific config)
- Simplifies setup for contributors

**Credentials:**
```yaml
SQL Server:
  User: sa
  Password: L0c@lD3v

RabbitMQ:
  User: guest
  Password: guest

MongoDB:
  User: root
  Password: root
```

**Implementation:**
```csharp
// Use constants in test fixture (safe for isolated containers)
private const string SqlConnectionString =
    "Server=localhost,1433;User Id=sa;Password=L0c@lD3v;TrustServerCertificate=True";

// OR environment variables (same for CI/local)
var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer") ??
    "Server=localhost,1433;User Id=sa;Password=L0c@lD3v;TrustServerCertificate=True";
```

### For External/Public Cloud Tests (Future)

**If integration tests ever include Azure, AWS, Google Cloud, or other external services:**

1. **GitHub Secrets (CI):**
   - Store real credentials as GitHub organization/repo secrets
   - Pass to CI workflows via `secrets.AZURE_CREDENTIAL` etc.
   - Never logged or exposed in workflow output

2. **Local .env.local (Development):**
   ```bash
   # .env.local (GITIGNORED - never commit)
   AZURE_TENANT_ID=xxxx
   AZURE_CLIENT_ID=yyyy
   AZURE_CLIENT_SECRET=zzzz
   AWS_ACCESS_KEY_ID=aaaa
   AWS_SECRET_ACCESS_KEY=bbbb
   ```

3. **Loading in Tests:**
   ```csharp
   // Load from environment (populated by GitHub Secrets or local .env.local)
   var azureCredential = Environment.GetEnvironmentVariable("AZURE_CREDENTIAL")
       ?? throw new InvalidOperationException("AZURE_CREDENTIAL not set");

   var client = new AzureClient(azureCredential);
   ```

4. **Gitignore Entry:**
   ```
   # .gitignore
   .env.local
   .env.*.local
   ```

### Migration Strategy

**When adding external/cloud tests:**
1. Create `.env.example` with placeholder values (no real secrets)
2. Add `.env.local` to `.gitignore`
3. Update CI workflow to inject secrets as environment variables
4. Document in contribution guide how to set up `.env.local` locally

**Example .env.example:**
```bash
# Azure (required for cloud tests)
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret

# Local development: Create .env.local with real values
# CI: Populate from GitHub Secrets
```

---

## Test Data

**Decision:** Keep test data small (<10MB total). Generate via code when possible.

### Approach

1. **Preferred: Generate Programmatically**
   ```csharp
   // Instead of importing CSV
   for (int i = 0; i < 100; i++)
   {
       await userRepository.CreateAsync(new User
       {
           Id = i,
           Name = $"TestUser{i}",
           Email = $"user{i}@test.local"
       });
   }
   ```

2. **Acceptable: Small Seed Files**
   - Maximum 10MB total for all test data files
   - Checked into repository
   - Mounted read-only: `./test-data:/test-data:ro`

3. **Avoid: Large Data Files**
   - No 100MB+ CSV/JSON imports
   - No realistic production-sized datasets
   - Use representative samples instead

### Benefits

- Faster test startup
- Easier version control
- Reduced CI artifact storage
- Faster test data cleanup

---

## Backwards Compatibility

**Decision:** Test only latest versions. Support policy documented separately.

### Approach

1. **Test Against:**
   - SQL Server 2019+ (latest stable)
   - PostgreSQL 15+ (latest stable)
   - MongoDB 7.0+ (latest stable)
   - Other services: latest available

2. **Support Policy:**
   - Document supported versions in README.md
   - Example: "Supports SQL Server 2019, 2022"
   - Drop old versions when they go EOL

3. **Breaking Changes:**
   - Allow breaking changes to unsupported versions
   - Document migration path in CHANGELOG.md

---

## Performance Monitoring

**Decision:** Collect performance metrics informational (log but don't block tests).

### Approach

1. **Metrics to Track:**
   - Test execution time
   - Database query times
   - Message publish/consume latency
   - HTTP API response times

2. **Implementation:**
   ```csharp
   var sw = Stopwatch.StartNew();

   // Test code
   var result = await repository.GetUserAsync(userId);

   sw.Stop();
   TestContext.WriteLine($"Query time: {sw.ElapsedMilliseconds}ms");

   // Log but don't assert
   Assert.IsTrue(result != null);  // Correctness matters
   // Don't: Assert.IsTrue(sw.ElapsedMilliseconds < 100);
   ```

3. **Reporting:**
   - Performance metrics included in test logs
   - Available in GitHub Actions artifacts
   - Use for trend analysis (not blocking)

4. **Future Optimization:**
   - Review logs for slowdowns
   - Optimize bottlenecks
   - Add strict SLAs if needed after baseline established

---

## Release Publishing & Tagging Strategy

**Decision:** Automatic NuGet publish (no approval gate) after integration tests pass. Three-tier tagging on every build and release.

### Three-Tier Tagging

**Tier 1: Build Tag (Every Build)**
```
dotnet.yml completes
  └─ Create vX.Y.Z (main branch)
     OR vX.Y.Z-{branch-name} (dev/* branches)
```

- Created on every successful build
- Main branch: `v2.1.0`, `v2.1.1`, etc.
- Dev branches: `v2.1.0-feature-xyz`, `v2.1.0-hotfix-abc`, etc.
- Always pushed to git history

**Tier 2: Validation Tag (Integration Pass)**
```
integration-tests.yml ✅ PASS
  └─ Create validated-vX.Y.Z (main branch only)
```

- Only created if all integration tests pass
- Only on main branch (proof of validation)
- Additional tag, doesn't replace vX.Y.Z

**Tier 3: Release Tag (NuGet Published)**
```
scheduled-release.yml ✅ NuGet publish succeeds
  └─ Create release-vX.Y.Z (main branch only)
```

- Only created after successful NuGet publish
- Only on main branch (proof of release)
- Additional tag, doesn't replace previous tags

### Full Flow

```
Every Push:
├─ dotnet.yml
│  └─ Tag: vX.Y.Z (main) or vX.Y.Z-branch (dev/*)

Daily on Main (if changes detected):
├─ integration-tests.yml
│  └─ IF TESTS PASS: Tag: validated-vX.Y.Z
│  └─ IF TESTS FAIL: No tag, release blocked
│
└─ scheduled-release.yml (only if integration tests passed)
   ├─ Create GitHub Release
   └─ IF NuGet publish succeeds: Tag: release-vX.Y.Z
```

### Example Commit History

```
Commit ABC123: "Add feature X"
├─ v2.1.0-feature-x (build tag, dev branch)
├─ validated-v2.1.0 (integration tests pass, main)
└─ release-v2.1.0 (published to NuGet, main)

Commit DEF456: "Add feature Y"
├─ v2.1.1-feature-y (build tag, dev branch)
├─ validated-v2.1.1 (integration tests pass, main)
└─ release-v2.1.1 (published to NuGet, main)
```

### Implementation

**In dotnet.yml (every build):**
```yaml
- name: Create build tag
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    BRANCH=$(echo ${{ github.ref }} | sed 's/refs\/heads\///')

    if [ "$BRANCH" = "main" ]; then
      TAG="v${VERSION}"
    else
      TAG="v${VERSION}-${BRANCH}"
    fi

    git config user.name "github-actions[bot]"
    git config user.email "github-actions[bot]@users.noreply.github.com"
    git tag -a "${TAG}" -m "Build from branch ${BRANCH}"
    git push origin "${TAG}"
```

**In integration-tests.yml (on main, if tests pass):**
```yaml
- name: Create validated tag
  if: success() && github.ref == 'refs/heads/main'
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    git tag -a "validated-v${VERSION}" -m "Integration tests passed"
    git push origin "validated-v${VERSION}"
```

**In scheduled-release.yml (on main, after NuGet publish):**
```yaml
- name: Create release tag
  if: success() && github.ref == 'refs/heads/main'
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    git tag -a "release-v${VERSION}" -m "Released to NuGet"
    git push origin "release-v${VERSION}"
```

### Tag Conditions Summary

| Tag | Branch | When Created | Meaning |
|-----|--------|--------------|---------|
| `vX.Y.Z` | main | Every build ✅ | Build artifact created |
| `vX.Y.Z-{branch}` | dev/* | Every build ✅ | Build from feature branch |
| `validated-vX.Y.Z` | main | Integration tests ✅ | Code validated for release |
| ❌ | main | Integration tests ❌ | Release blocked (no tag) |
| `release-vX.Y.Z` | main | NuGet publish ✅ | Published to NuGet |

### Safety

- Every build is tagged (full history)
- Integration tests act as approval gate for release
- If tests fail → no `validated-` or `release-` tag, release blocked
- If tests pass → safe to release automatically

### Manual Override

```bash
# If needed to skip automated publish
gh workflow run scheduled-release.yml \
  -f publish-nuget=false \
  -f packages-artifact=build-12345
```

---

## Service Startup

**Decision:** Progressive backoff with timeout for service health checks.

### Approach

1. **Initial Wait:**
   - Wait 30 seconds for service to be ready (Docker `start_period`)

2. **Health Checks:**
   - Check every 2 seconds
   - Exponential backoff: 2s → 4s → 8s (up to max)
   - Maximum wait: 120 seconds total

3. **Failure Handling:**
   - If service not healthy after 120s → fail entire test run
   - Provide detailed error: which service, what health check failed
   - Include service logs in failure output

### Implementation

```csharp
public async Task WaitForServicesAsync(TimeSpan maxWait)
{
    var sw = Stopwatch.StartNew();
    var delayMs = 2000;
    const int maxDelayMs = 30000;
    const int maxTotalMs = 120000;

    while (sw.ElapsedMilliseconds < maxTotalMs)
    {
        try
        {
            // Check each service health
            await CheckSqlServerHealthAsync();
            await CheckMongoDbHealthAsync();
            await CheckRabbitMqHealthAsync();
            // All healthy, return
            return;
        }
        catch
        {
            await Task.Delay(delayMs);
            delayMs = Math.Min(delayMs * 2, maxDelayMs);
        }
    }

    throw new TimeoutException(
        $"Services not healthy after {maxTotalMs}ms. " +
        "Check Docker logs for details.");
}
```

---

## Summary Table

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Schema | Baseline + idempotent migrations | Deterministic, repeatable |
| Flaky Tests | Quarantine | Fix root cause, don't mask |
| Parallelization | Shared containers, data isolation | Performance + correctness |
| Local Dev | Yes, docker-compose | Easy for contributors |
| Secrets | Default credentials | Simpler, safe for test containers |
| Test Data | Small (<10MB), generated | Fast, version-controllable |
| Compatibility | Latest only | Simpler, clear support policy |
| Performance | Informational only | Don't block on performance yet |
| Publishing | Automatic (tests = gate) | Safe due to integration test validation |
| Startup | Progressive backoff | Handles slow container startup |

---

## When to Revisit These Decisions

- After 50+ integration tests written (evaluate parallelization effectiveness)
- After first performance regressions appear (evaluate performance SLAs)
- When supporting multiple database versions becomes requirement (revisit compatibility)
- If test data size grows beyond 10MB (revisit data strategy)

