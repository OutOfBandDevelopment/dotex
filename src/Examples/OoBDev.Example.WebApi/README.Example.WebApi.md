# OoBDev.Example.WebApi

## Summary

A sample ASP.NET Core web API that composes the OoBDev framework the way a real application would (see [Recipe 3 — New Application](../../../docs/patterns-discovery/04-new-project-blueprint/03-new-application.md)). It is a working reference for the composition calls, middleware ordering and provider selection, not a product.

## What it shows

- Composition in `Program.cs`: `AddApplicationServices()` for example services, the framework `Try*` extensions, `UseAllCommonMiddleware(...)`, then authentication, authorization and `MapControllers()`.
- Swagger UI in development, HTTPS redirection, console logging.
- One controller per framework capability:

| Controller | Capability |
|------------|-----------|
| `AIController`, `OllamaController`, `GroqCloudController` | Language model providers selected by key (`OPENAI`, `OLLAMA`) |
| `AllMiniLMController`, `SBERTController`, `SBERTDefaultController` | Embedding providers |
| `SearchController` | Search and vector store |
| `MessageQueueingController` | Message queue sender |
| `CommunicationsController` | Email and other channels |
| `DocumentController` | Document conversion |
| `TextTemplateController` | Text templating |
| `UserController` | Current user and claims |

## Running

```bash
dotnet run --project src/Examples/OoBDev.Example.WebApi
```

Most controllers need a backing service. Start the Docker services from `containers/testing` (`./scripts/integration-up.sh --wait`) and configure the matching settings; see `CONFIGURATION_SETTINGS.md` and `TEST_VARIABLES.md`.

## Notes

- `RequireApplicationUserId` is set to `false` so the samples run without a signed-in user.
- The MailKit hosted receiver is disabled in `Program.cs`; enable it only with a reachable mailbox.
- This project is not packaged.
