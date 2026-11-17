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
| `VectorStoreController` (index, query, list, re-embed), `VectorLabController`, `RagController`, `SearchController` (passages with a `RequiredRight` are hidden from callers without it), `ImagesController` | Vector store, vector experiments, retrieval, keyword, semantic and hybrid search, and image embeddings |
| `CgiProgramsController` | `GET api/CgiPrograms`: the CGI programs this host can start (Linux binaries are left out on Windows) |
| `MessageQueueingController` | Message queue sender, configured channels (no secrets) and the received-message monitor |
| `CommunicationsController` | Email through `ISendService` (channel `email`), directly or queued |
| `WebhooksController` | Anonymous `POST /webhooks/{channel}`: raw body to `IReceiveService.HandleWebhookAsync` (204, 400 or 404 problem details) |
| `DocumentController`, `TextToolsController` | Document conversion, the stored-documents list and text tools |
| `TextTemplateController` | Text templating: list, read and save the example template files, preview a draft, apply a named template |
| `UserController` | Current user and claims |
| `KernelPlugInsController` | Lists the Semantic Kernel plug-in functions and invokes one with text arguments (right `toolbox.use`) |
| `CacheController`, `OperationsController`, `TelemetryController`, `ToolboxController` | Caching, operations, telemetry readback and shared tools (the toolbox also signs and reads HS256 tokens through `OoBDev.Identity.Jws`, and converts text with the Morse, Roman numeral and Base32 codecs) |

## CGI example (Go)

`cgi-src/greet` is a small Go program served through `OoBDev.AspNetCore.Cgi`: `GET /cgi-bin/greet.exe` (`greet.cgi` off Windows) shows a form, and posting a `name` sends it back in a greeting (HTML-escaped). It also prints its own OpenAPI description (`greet -openapi`), which `OoBDev.AspNetCore.Cgi.OpenApi` lists in `/openapi/all.json`.

```powershell
./cgi-src/build.ps1     # go test, go build into cgi-bin/, writes cgi-bin/greet.openapi.json
dotnet run --project src/Examples/OoBDev.Example.WebApi
```

Needs Go on the path. The built files are not committed (`cgi-bin/.gitignore`). `AllowAnonymous` is on in `appsettings.json` for the example only.

On Windows call `greet.exe`. A `greet.cgi` left in `cgi-bin/` by a Linux or container build is an ELF binary, which Windows cannot start, so `/cgi-bin/greet.cgi` answers `500 The program could not start.` (the log line "CGI script greet.cgi could not start" names the script). Delete it or ignore it; `build.ps1` builds the file for the current system.

## Running

```bash
dotnet run --project src/Examples/OoBDev.Example.WebApi
```

Most controllers need a backing service. Start the Docker services from `containers/testing` (`./scripts/integration-up.sh --wait`) and configure the matching settings; see `CONFIGURATION_SETTINGS.md` and `TEST_VARIABLES.md`.

## Notes

- `RequireApplicationUserId` is set to `false` so the samples run without a signed-in user.
- The MailKit hosted receiver is disabled in `Program.cs`; enable it only with a reachable mailbox.
- This project is not packaged.

## Tests

`OoBDev.Example.WebApi.Tests` (Unit) calls the controllers directly with mocked services: `CommunicationsController` (send accepted, provider rejection to a 502 problem, enqueue) and `WebhooksController` (raw body, headers, query and clock reach `IReceiveService`; 204, 404 and 400 mapping).
