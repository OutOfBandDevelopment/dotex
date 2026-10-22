# OoBDev.Example.WebApi

## Summary

A sample ASP.NET Core web API that composes the OoBDev framework the way a real application would (see [Recipe 3 — New Application](../../../docs/patterns-discovery/04-new-project-blueprint/03-new-application.md)). It is a working reference for the composition calls, middleware ordering and provider selection, not a product.

## What it shows

- Composition in `Program.cs`: `AddApplicationServices()` for example services, the framework `Try*` extensions, `UseAllCommonMiddleware(...)`, then authentication, authorization and `MapControllers()`.
- OpenAPI documents (`/openapi/all.json` and one per assembly) with the Scalar API reference at `/scalar/all` in development, HTTPS redirection, console logging.
- One controller per framework capability:

| Controller | Capability |
|------------|-----------|
| `OllamaController`, `GroqCloudController`, `ModelCompareController` | Language model providers and side-by-side comparison |
| `AllMiniLMController`, `SBERTController`, `SBERTDefaultController` | Embedding providers |
| `VectorStoreController`, `VectorLabController`, `RagController`, `ImagesController` | Vector store, vector experiments, retrieval and image embeddings |
| `MessageQueueingController` | Message queue sender |
| `CommunicationsController` | Email and other channels |
| `DocumentController`, `TextToolsController` | Document conversion and text tools |
| `TextTemplateController` | Text templating |
| `UserController` | Current user and claims |
| `CacheController`, `OperationsController`, `TelemetryController`, `ToolboxController` | Caching, operations, telemetry readback and shared tools |

## Running

```bash
dotnet run --project src/Examples/OoBDev.Example.WebApi
```

Most controllers need a backing service. Start the Docker services from `containers/testing` (`./scripts/integration-up.sh --wait`) and configure the matching settings; see `CONFIGURATION_SETTINGS.md` and `TEST_VARIABLES.md`.

## Notes

- `RequireApplicationUserId` is set to `false` so the samples run without a signed-in user.
- The MailKit hosted receiver is disabled in `Program.cs`; enable it only with a reachable mailbox.
- This project is not packaged.
