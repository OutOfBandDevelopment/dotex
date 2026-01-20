# TODO - Live Integration Testing (Cloud) Epic

**Last Updated:** 2026-01-19

Cloud-based integration testing for services that cannot be Dockerized or emulated.

> **Parent Document:** [TODO.md](./TODO.md)
> **Related:**
> - [TODO-testing-local-integration.md](./TODO-testing-local-integration.md) - Docker-based testing
> - [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Test property reference (3 cloud services)
> - [docs/architecture/testing-guidelines.md](./docs/architecture/testing-guidelines.md) - Testing best practices

---

## Overview

Integration testing for cloud services that require actual cloud infrastructure (Azure B2C, Application Insights, Groq Cloud, etc.).

**Goal:** Enable developers to run integration tests against real cloud services locally with proper credential management and documentation.

**Test Category:** `LiveIntegration` - Cloud services requiring live credentials, NOT run in CI/CD

**Key Principle:** LiveIntegration tests are **manual execution only** - they require cloud credentials and may incur costs, so they are NOT automated in CI/CD pipelines.

---

## Completed Work ✓

### Test Categories Enhancement (COMPLETED - 2026-01-19)

- [x] Added `LiveIntegration` category to `src/Framework/OoBDev.TestUtilities/TestCategories.cs`
- [x] Updated XML documentation clearly explaining:
  - LiveIntegration is for cloud services that cannot be emulated
  - Requires valid cloud credentials and active service subscriptions
  - Manual execution only, NOT run in CI/CD pipelines
  - Examples: Azure B2C, Application Insights, Groq Cloud
- [x] Clear distinction from Integration category (Docker-based, runs in CI/CD)

---

## Pending Work

### Week 3: Cloud Test Migration (PENDING)

**Migrate tests that require live cloud services to LiveIntegration category**

#### Azure B2C / Entra ID

**Purpose:** Microsoft identity platform for customer-facing applications

**Current Status:** Tests exist but marked as DevLocal

**Migration Tasks:**
- [ ] File: `src/ExternalServices/Microsoft/OoBDev.Microsoft.B2C.Tests/`
- [ ] Change `[TestCategory(TestCategories.DevLocal)]` → `[TestCategory(TestCategories.LiveIntegration)]`
- [ ] Create `.env.liveintegration.template` in test project root:
  ```bash
  # Azure B2C Configuration
  # See README.md for setup instructions

  AZURE_B2C_TENANT_ID=your-tenant-id-here
  AZURE_B2C_CLIENT_ID=your-client-id-here
  AZURE_B2C_CLIENT_SECRET=your-client-secret-here
  AZURE_B2C_DOMAIN=yourb2c.onmicrosoft.com
  AZURE_B2C_POLICY=B2C_1_signupsignin
  ```

- [ ] Create `README.md` in test project documenting:
  - **Purpose:** What these tests validate (authentication flows, token validation, etc.)
  - **Prerequisites:** Azure subscription, B2C tenant setup
  - **Azure Setup Steps:**
    1. Create Azure B2C tenant
    2. Register application
    3. Create user flows (sign-up/sign-in)
    4. Configure API permissions
    5. Generate client secret
  - **Obtaining Credentials:**
    - Where to find Tenant ID
    - How to register application
    - How to get Client ID and Secret
  - **Running Tests Locally:**
    1. Copy `.env.liveintegration.template` to `.env.liveintegration`
    2. Fill in actual credentials
    3. Run: `dotnet test --filter TestCategory=LiveIntegration`
  - **Cost Considerations:**
    - Azure B2C pricing (first 50K MAU free)
    - Expected API calls per test run
  - **Security:**
    - NEVER commit `.env.liveintegration` to source control
    - Add to `.gitignore`
    - Use Azure Key Vault for production
  - **NOTE:** Manual execution only, not in CI/CD

- [ ] Update tests to read from environment variables:
  ```csharp
  [TestInitialize]
  public void Setup()
  {
      var tenantId = Environment.GetEnvironmentVariable("AZURE_B2C_TENANT_ID")
          ?? throw new InvalidOperationException("AZURE_B2C_TENANT_ID not set");
      var clientId = Environment.GetEnvironmentVariable("AZURE_B2C_CLIENT_ID")
          ?? throw new InvalidOperationException("AZURE_B2C_CLIENT_ID not set");
      // ... configure B2C client
  }
  ```

- [ ] Add usage examples in README:
  - User authentication flow
  - Token validation
  - User management
  - Custom policies

#### Microsoft Application Insights

**Purpose:** Application performance monitoring and telemetry

**Current Status:** Tests exist but marked as DevLocal

**Migration Tasks:**
- [ ] File: `src/ExternalServices/Microsoft/OoBDev.Microsoft.ApplicationInsights.Tests/`
- [ ] Change `[TestCategory(TestCategories.DevLocal)]` → `[TestCategory(TestCategories.LiveIntegration)]`
- [ ] Create `.env.liveintegration.template`:
  ```bash
  # Application Insights Configuration
  # See README.md for setup instructions

  APPINSIGHTS_INSTRUMENTATION_KEY=your-instrumentation-key-here
  APPINSIGHTS_CONNECTION_STRING=InstrumentationKey=...;IngestionEndpoint=...
  ```

- [ ] Create `README.md` in test project:
  - **Purpose:** Validate telemetry sending, query API, alerts
  - **Prerequisites:** Azure subscription, Application Insights resource
  - **Azure Setup Steps:**
    1. Create Application Insights resource
    2. Get instrumentation key and connection string
    3. (Optional) Configure availability tests
  - **Obtaining Credentials:**
    - Navigate to Azure Portal → Application Insights → Properties
    - Copy Instrumentation Key
    - Copy Connection String
  - **Running Tests Locally:**
    1. Copy template to `.env.liveintegration`
    2. Fill in actual credentials
    3. Run tests
    4. Verify telemetry in Azure Portal (5-10 minute delay)
  - **Cost Considerations:**
    - First 5GB per month free
    - Each test run sends ~1-5 MB of telemetry
    - Data retention costs after 90 days
  - **Security:** Environment variable management, Key Vault
  - **NOTE:** Manual execution only

- [ ] Update tests for environment variables:
  ```csharp
  [TestInitialize]
  public void Setup()
  {
      var connectionString = Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTION_STRING")
          ?? throw new InvalidOperationException("APPINSIGHTS_CONNECTION_STRING not set");
      _telemetryClient = new TelemetryClient(new TelemetryConfiguration
      {
          ConnectionString = connectionString
      });
  }
  ```

- [ ] Add usage examples:
  - Send custom events
  - Send custom metrics
  - Query telemetry data
  - Verify dependency tracking

#### Groq Cloud

**Purpose:** High-performance LLM inference API

**Current Status:** Tests exist but marked as DevLocal

**Migration Tasks:**
- [ ] File: `src/ExternalServices/GroqCloud/OoBDev.Groq.Tests/`
- [ ] Change `[TestCategory(TestCategories.DevLocal)]` → `[TestCategory(TestCategories.LiveIntegration)]`
- [ ] Create `.env.liveintegration.template`:
  ```bash
  # Groq Cloud Configuration
  # See README.md for setup instructions

  GROQ_API_KEY=your-api-key-here
  GROQ_API_URL=https://api.groq.com/openai/v1
  GROQ_MODEL=llama3-8b-8192
  ```

- [ ] Create `README.md` in test project:
  - **Purpose:** Validate LLM inference, streaming, embeddings
  - **Prerequisites:** Groq Cloud account
  - **Setup Steps:**
    1. Sign up at https://console.groq.com
    2. Navigate to API Keys
    3. Create new API key
  - **Obtaining Credentials:**
    - Groq Console → API Keys → Create API Key
    - Copy key immediately (not shown again)
  - **Running Tests Locally:**
    1. Copy template to `.env.liveintegration`
    2. Paste API key
    3. Run tests
  - **Cost Considerations:**
    - Free tier: 14,400 requests per day
    - Each test run: ~10-20 requests
    - Monitor usage in Groq Console
  - **Rate Limits:**
    - Requests per minute: 30 (free tier)
    - Tokens per minute: 14,400
    - Tests include retry logic with backoff
  - **Security:** API key management
  - **NOTE:** Manual execution only

- [ ] Update tests for environment variables:
  ```csharp
  [TestInitialize]
  public void Setup()
  {
      var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
          ?? throw new InvalidOperationException("GROQ_API_KEY not set");
      var apiUrl = Environment.GetEnvironmentVariable("GROQ_API_URL")
          ?? "https://api.groq.com/openai/v1";

      _groqClient = new GroqClient(new GroqClientOptions
      {
          ApiKey = apiKey,
          BaseUrl = apiUrl
      });
  }
  ```

- [ ] Add usage examples:
  - Chat completions
  - Streaming responses
  - Function calling
  - Model selection

**Estimated Total:** 3 services migrated to LiveIntegration category

---

### Week 4 (Part 2): Cloud Documentation (PENDING)

#### LiveIntegration Category Documentation

- [ ] Create `docs/architecture/testing/categories/liveintegration/README.md`
  - LiveIntegration test standards
  - Cloud service requirements
  - Credential management best practices
  - Cost management (avoiding unexpected cloud bills)
  - Security considerations
  - Manual execution workflow
  - Code examples for each cloud service
  - Environment variable patterns
  - .gitignore requirements
  - Azure Key Vault integration for production

- [ ] Create `docs/architecture/testing/categories/liveintegration/cloud-setup.md`
  - Step-by-step setup for each cloud provider
  - Azure subscription setup
  - Groq account creation
  - Credential acquisition
  - Service quotas and limits
  - Cost estimation
  - Billing alerts setup

- [ ] Create `docs/architecture/testing/categories/liveintegration/credential-management.md`
  - Environment variable patterns
  - .env.liveintegration file structure
  - .gitignore configuration
  - Azure Key Vault integration
  - Developer machine security
  - Credential rotation
  - Team sharing (Azure AD integration)

- [ ] Create `docs/architecture/testing/categories/liveintegration/cost-management.md`
  - Cost breakdown per service
  - Free tier limits
  - Cost per test run estimation
  - Billing alerts setup
  - Resource cleanup automation
  - Budget recommendations

#### Stack Documentation (3 Cloud-based stacks)

**Identity:**
- [ ] `docs/architecture/testing/stacks/identity/azure-b2c.md`
  - Service: Azure B2C / Entra ID External
  - Pricing: First 50K MAU free
  - Setup: Tenant creation, app registration
  - Credentials: Tenant ID, Client ID, Client Secret
  - Test patterns: Authentication flows, token validation
  - Cost per test run: ~$0.001 (within free tier)
  - Code examples: User sign-up/sign-in, token validation

**Observability:**
- [ ] `docs/architecture/testing/stacks/observability/application-insights.md`
  - Service: Azure Application Insights
  - Pricing: First 5GB/month free
  - Setup: Resource creation, instrumentation key
  - Credentials: Instrumentation Key, Connection String
  - Test patterns: Telemetry sending, querying
  - Cost per test run: ~$0.01 (within free tier for normal use)
  - Code examples: Custom events, metrics, dependency tracking

**AI/ML:**
- [ ] `docs/architecture/testing/stacks/ai-ml/groq.md`
  - Service: Groq Cloud LLM Inference
  - Pricing: Free tier 14,400 requests/day
  - Setup: Account creation, API key
  - Credentials: API Key
  - Test patterns: Chat completions, streaming
  - Rate limits: 30 RPM, 14,400 TPM (free tier)
  - Cost per test run: $0 (within free tier)
  - Code examples: Chat, streaming, function calling

**Stack Doc Template** (for all 3 cloud services):
```markdown
# {Service Name} Testing (LiveIntegration)

## Overview
- Service provider and type
- Purpose in OoBDev framework
- Which projects use this service

## Cloud Setup
- Account/subscription requirements
- Step-by-step setup instructions
- Resource creation
- Configuration

## Credentials
- What credentials are needed
- How to obtain them
- Where to find them in cloud console
- How to store them securely

## Test Patterns
- Connection setup
- Authentication
- Test data management
- Common operations
- Code examples

## Cost Management
- Pricing model
- Free tier details
- Cost per test run
- Billing alerts
- Resource cleanup

## Security
- Credential storage
- .gitignore requirements
- Key Vault integration
- Rotation policies

## Troubleshooting
- Common issues
- Authentication failures
- Rate limiting
- Cost overruns
```

#### Top-Level Documentation Updates

- [ ] Create `docs/architecture/testing/liveintegration-vs-integration.md`
  - Clear comparison table
  - When to use Integration (Docker) vs LiveIntegration (Cloud)
  - Decision flowchart (PlantUML)
  - Migration guide (DevLocal → Integration or LiveIntegration)

- [ ] Update `docs/architecture/testing/test-categories.md`
  - Add LiveIntegration to category comparison matrix
  - Add LiveIntegration to decision tree
  - Add code examples for LiveIntegration pattern

- [ ] Update `docs/architecture/testing/environment-variables.md`
  - Add section on `.env.liveintegration` file
  - Security best practices for cloud credentials
  - Azure Key Vault integration patterns
  - Team credential sharing strategies

#### PlantUML Diagrams (Cloud-focused)

- [ ] Create `docs/architecture/testing/diagrams/cloud-credential-flow.puml`
  - How credentials flow from cloud console → developer machine → tests
  - .env.liveintegration file structure
  - Environment variable loading
  - Azure Key Vault integration (optional)

- [ ] Create `docs/architecture/testing/diagrams/liveintegration-decision-tree.puml`
  - Flowchart: Should this be Integration or LiveIntegration?
  - Questions to ask when categorizing tests
  - Examples for each decision path

---

## Success Criteria

### Week 3: Cloud Test Migration
- [ ] 3 services categorized as LiveIntegration (Azure B2C, App Insights, Groq)
- [ ] `.env.liveintegration.template` created for each service
- [ ] README.md in each test project explaining setup
- [ ] Tests updated to use environment variables
- [ ] Clear separation from Integration category (Docker-based)
- [ ] Security: .env.liveintegration in .gitignore
- [ ] Cost estimation documented for each service

### Week 4 (Part 2): Cloud Documentation
- [ ] LiveIntegration category fully documented
- [ ] All 3 cloud stacks documented with cost details
- [ ] Credential management guide created
- [ ] Cost management guide created
- [ ] PlantUML diagrams for cloud flows
- [ ] Clear decision criteria: Integration vs LiveIntegration

---

## Security Considerations

### Credential Storage

**NEVER commit cloud credentials to source control:**
- [ ] Add `.env.liveintegration` to `.gitignore`
- [ ] Provide `.env.liveintegration.template` with placeholder values
- [ ] Document in README: "Copy template, fill in actual values"

**Example .gitignore:**
```gitignore
# LiveIntegration test credentials
**/.env.liveintegration
**/appsettings.liveintegration.json

# Keep templates
!**/.env.liveintegration.template
!**/appsettings.liveintegration.template.json
```

### Credential Management Options

**Option 1: Environment Variables (Simplest)**
```bash
# .env.liveintegration (gitignored)
AZURE_B2C_CLIENT_SECRET=actual-secret-here
```

**Option 2: Azure Key Vault (Production)**
```csharp
var keyVaultUri = new Uri("https://myvault.vault.azure.net/");
var client = new SecretClient(keyVaultUri, new DefaultAzureCredential());
var secret = await client.GetSecretAsync("AzureB2CClientSecret");
```

**Option 3: User Secrets (Development)**
```bash
dotnet user-secrets set "AzureB2C:ClientSecret" "actual-secret-here"
```

### Team Sharing

**For team environments:**
- [ ] Use Azure AD for authentication (no secrets)
- [ ] Use managed identities where possible
- [ ] Share Key Vault access (RBAC)
- [ ] Document onboarding process in README

---

## Cost Management

### Cost Estimation

**Azure B2C:**
- Free tier: First 50,000 MAU
- Cost per test run: ~$0.001 (within free tier)
- Risk: Low (generous free tier)

**Application Insights:**
- Free tier: 5 GB data ingestion/month
- Cost per test run: ~$0.01 (typical telemetry)
- Risk: Medium (can accumulate over time)
- Mitigation: Set billing alerts at $10, $50

**Groq Cloud:**
- Free tier: 14,400 requests/day
- Cost per test run: $0 (within free tier)
- Risk: Low (generous free tier)
- Mitigation: Monitor usage in console

**Total monthly cost (assuming daily testing):**
- Optimistic: $0 (all within free tiers)
- Realistic: $5-10 (occasional overages)
- Worst case: $50 (if not monitored)

### Billing Alerts

**Set up billing alerts for each service:**

**Azure:**
```bash
# Create budget alert
az consumption budget create \
  --budget-name OoBDev-LiveIntegration-Tests \
  --amount 10 \
  --time-grain Monthly \
  --category Cost
```

**Groq:**
- Monitor in Groq Console → Usage
- Free tier has hard limits (no overage charges)

### Resource Cleanup

**Automated cleanup after tests:**
```csharp
[TestCleanup]
public async Task Cleanup()
{
    // Clean up test data to avoid storage costs
    await _b2cClient.DeleteUserAsync(_testUserId);
    await _insightsClient.PurgeAsync(_testComponentId, ...);
}
```

---

## Risk Mitigation

### Accidental Cost Overruns
**Risk:** Tests run too frequently, exceed free tier

**Mitigation:**
- Document expected cost per run
- Set billing alerts at low thresholds ($10, $50)
- Manual execution only (not automated)
- Test data cleanup in [TestCleanup]
- Monitor usage dashboards weekly

### Credential Leakage
**Risk:** Credentials committed to source control

**Mitigation:**
- Add `.env.liveintegration` to .gitignore
- Use git-secrets or similar tools
- Pre-commit hooks to scan for secrets
- Document: "NEVER commit .env.liveintegration"
- Use Key Vault for production

### Rate Limiting
**Risk:** Tests hit API rate limits

**Mitigation:**
- Document rate limits in README
- Add retry logic with exponential backoff
- Space out test execution
- Use mocks for rapid iteration (switch to LiveIntegration for final validation)

### Service Availability
**Risk:** Cloud service outage during testing

**Mitigation:**
- Tests gracefully handle service unavailability
- Clear error messages
- Retry logic with max attempts
- Document: "LiveIntegration tests may fail due to service outages"

---

## CI/CD Exclusion

**LiveIntegration tests are NEVER run in CI/CD pipelines:**

**Why:**
- Require cloud credentials (security risk in CI/CD)
- May incur costs (every PR would trigger costs)
- Service availability not guaranteed
- Rate limits could cause flaky tests

**How to exclude:**
```yaml
# .github/workflows/integration-tests.yml
- name: Run Integration Tests
  run: >
    dotnet test
    --filter "TestCategory=Integration"  # Excludes LiveIntegration
```

**Developer workflow:**
```bash
# Run all automated tests (Unit, Simulate, Integration)
dotnet test --filter "TestCategory!=LiveIntegration"

# Run ONLY LiveIntegration tests (manual, as needed)
dotnet test --filter "TestCategory=LiveIntegration"
```

---

## Documentation References

- `src/Framework/OoBDev.TestUtilities/TestCategories.cs` - LiveIntegration category definition
- `.gitignore` - Ensure .env.liveintegration files excluded
- Each test project's README.md - Service-specific setup instructions

---

## Notes

**Why LiveIntegration instead of just using Integration:**
- Some services (Azure B2C, Groq) have NO local emulator/Docker container
- OpenStack/Azurite can't emulate Entra ID authentication flows
- Real cloud services have different behavior than emulators
- LiveIntegration makes cost/credential requirements explicit

**Alternative approaches considered:**
- **Mock cloud services:** Doesn't validate actual cloud behavior
- **Shared test tenant:** Credential management nightmare, test isolation issues
- **CI/CD with secrets:** Cost overruns, rate limiting, security risks
- **LiveIntegration (chosen):** Clear separation, manual control, cost awareness

**Future enhancements:**
- Azure Managed Identity support for developer machines
- Groq usage tracking dashboard
- Automated cleanup scripts
- Cost optimization recommendations
