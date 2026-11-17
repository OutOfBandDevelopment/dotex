# Azure AI Search

[← Live integration testing](README.md)

Project: `src/ExternalServices/Microsoft/OoBDev.Microsoft.Azure.AISearch.Tests`. There is no emulator for Azure AI Search, so the vector store provider is checked against a real service.

## Contents

- [Setup](#setup)
- [Tests](#tests)
- [Cost and cleanup](#cost-and-cleanup)

## Setup

1. Create an Azure AI Search service (the free tier is enough) and copy its URL and an admin key.
2. Set `AZURE_SEARCH_ENDPOINT` and `AZURE_SEARCH_API_KEY`, as test properties or as environment variables. When either is empty the test ends `Inconclusive`.
3. Run `dotnet test src/ExternalServices/Microsoft/OoBDev.Microsoft.Azure.AISearch.Tests --filter "TestCategory=LiveIntegration"`.

The variables are listed in [TEST_VARIABLES.md](../../../../TEST_VARIABLES.md).

## Tests

**Table 1 — Azure AI Search live test**

| Test | Checks |
|------|--------|
| `StoreAndFindNeighbors_RoundTripsThroughARealIndex` | Two vectors are stored through `AzureAISearchVectorStoreProvider`; a query for the first finds it, with its metadata, within ten seconds (indexing is eventually consistent, so the test polls) |

## Cost and cleanup

Each run creates one index named `test-<guid>` and deletes it in a `finally` block, so a failed run leaves nothing behind unless the process is killed. Delete a stray `test-*` index in the portal. The free tier allows three indexes.

[← Live integration testing](README.md)
