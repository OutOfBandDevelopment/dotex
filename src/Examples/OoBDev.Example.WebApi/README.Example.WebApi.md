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

## CGI example (Go)

`cgi-src/greet` is a small Go program served through `OoBDev.AspNetCore.Cgi`: `GET /cgi-bin/greet.exe` (`greet.cgi` off Windows) shows a form, and posting a `name` sends it back in a greeting (HTML-escaped). It also prints its own OpenAPI description (`greet -openapi`), which `OoBDev.AspNetCore.Cgi.OpenApi` lists in `/openapi/all.json`.

```powershell
./cgi-src/build.ps1     # go test, go build into cgi-bin/, writes cgi-bin/greet.openapi.json
dotnet run --project src/Examples/OoBDev.Example.WebApi
```

Needs Go on the path. The built files are not committed (`cgi-bin/.gitignore`). `AllowAnonymous` is on in `appsettings.json` for the example only.

## Running

```bash
dotnet run --project src/Examples/OoBDev.Example.WebApi
```

Most controllers need a backing service. Start the Docker services from `containers/testing` (`./scripts/integration-up.sh --wait`) and configure the matching settings; see `CONFIGURATION_SETTINGS.md` and `TEST_VARIABLES.md`.

## Notes

- `RequireApplicationUserId` is set to `false` so the samples run without a signed-in user.
- The MailKit hosted receiver is disabled in `Program.cs`; enable it only with a reachable mailbox.
- This project is not packaged.
