# Groq Cloud

[← Live integration testing](README.md)

Project: `src/ExternalServices/GroqCloud/OoBDev.GroqCloud.Tests`. Its readme is `README.GroqCloud.Tests.md`.

## Contents

- [Setup](#setup)
- [Tests](#tests)
- [Model availability](#model-availability)

## Setup

1. Create an account and an API key in the Groq console.
2. Set `GROQ_API_KEY`; optionally set `GROQ_MODEL`. The library default is `openai/gpt-oss-20b`.
3. Run `dotnet test src/ExternalServices/GroqCloud/OoBDev.GroqCloud.Tests --filter "TestCategory=LiveIntegration"`.

## Tests

**Table 1 — Groq live tests**

| Test | Checks |
|------|--------|
| `GetCompletionAsync_ShortPrompt_ReturnsText` | `IMessageCompletion` returns text for a one-line prompt |
| `HealthCheck_WithValidKey_IsHealthy` | `GroqCloudHealthCheck` reports Healthy |

Each run makes two small chat completions. Both passed against the live API on 2026-10-09.

## Model availability

Groq retires models. The first live run found the previous default, `llama3-8b-8192`, decommissioned (HTTP 400), and `llama-3.1-8b-instant` not available to the account (404). List what an account can use with:

```bash
curl -s https://api.groq.com/openai/v1/models -H "Authorization: Bearer $GROQ_API_KEY"
```

Set `GROQ_MODEL` to an id from that list when the default stops working.

[← Live integration testing](README.md)
