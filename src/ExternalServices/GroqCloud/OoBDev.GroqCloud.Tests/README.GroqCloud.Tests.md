# OoBDev.GroqCloud.Tests

Live tests for the Groq Cloud adapter. They call the real API, so they are category `LiveIntegration`: manual runs only, never in CI, and they may incur cost.

## Setup

1. Create a Groq account and an API key.
2. Provide `GROQ_API_KEY` (and optionally `GROQ_MODEL`) either as a `.runsettings` parameter or as an environment variable; the tests read the `TestContext` property first, then the environment variable. `.env.liveintegration.template` lists the names. The copy named `.env.liveintegration` is git-ignored.
3. Run:

   ```bash
   dotnet test src/ExternalServices/GroqCloud/OoBDev.GroqCloud.Tests --filter "TestCategory=LiveIntegration"
   ```

Without `GROQ_API_KEY` the tests report Inconclusive instead of failing.

## Tests

**Table 1 — Live tests**

| Test | Checks |
|------|--------|
| `GetCompletionAsync_ShortPrompt_ReturnsText` | `IMessageCompletion` returns non-empty text for a one-line prompt |
| `HealthCheck_WithValidKey_IsHealthy` | `GroqCloudHealthCheck` reports Healthy against the live API |

Each run makes two small chat completions. See [TEST_VARIABLES.md](../../../../TEST_VARIABLES.md) for the variable reference.
