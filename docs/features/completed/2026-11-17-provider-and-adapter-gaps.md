# Feature request: provider and adapter gaps

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#provider-and-adapter-gaps))
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** external-service adapters, tools and the vector database project

## Need

Several adapters and tools carry unfinished or placeholder behavior. Each is small; they are batched so triage can build, merge or decline them one by one.

## Proposed behavior

Close or decline each item:

1. Azure blob container provider: a query provider over blobs.
2. SQL Service Broker queue message provider: finish it (design `docs/design/SqlServiceBroker`).
3. Ollama: `CopyModelAsync("llama2", ...)` is a stand-in; copy the real source model.
4. Groq model mapper: map `Context` generically instead of an empty array.
5. DacPac merge compiler: carry `TSqlObjectOptions` forward; template factory: detect the serializer from the media type.
6. OAuth OpenAPI transformer: read the authority from `.well-known/openid-configuration` when not set.
7. Vector stored procedure `EmbeddingSentenceTransformerResponseSend`: optionally route an event back to the application.
8. MongoDB dispatch proxy: share the name logic with the data loader command factory.
9. File RAG engine: choose the handler from more than the file extension; DacPac compiler CLI service registration "fix this up".
10. SBert tests: decide whether to drop `ISentenceEmbeddingClient`.

## Acceptance criteria

- Given each item, when triaged, then it is built with a test, merged into a design document, or declined with a reason.

## Existing code checked

`docs/design/SqlServiceBroker`, `MessageQueuing`, `DesiredStateDatabase`, `EmbeddingClientAndPgVector`, `docs/harvest-candidates.md`. They cover the areas, not these items.

Source comments (replaced by a reference to this request):

- `src/ExternalServices/Microsoft/OoBDev.Microsoft.Azure.StorageAccount/BlobStorage/AzureBlobContainerProvider.cs`: "build a query provider!".
- `src/ExternalServices/Microsoft/OoBDev.Microsoft.SqlServer.Server/MessageQueueing/SqlServiceBrokerQueueMessageProvider.cs`: "finish this out".
- `src/ExternalServices/Ollama/OoBDev.Ollama/OllamaApiClientExtensions.cs`: "fix this" (copy of `llama2`).
- `src/ExternalServices/GroqCloud/OoBDev.GroqCloud/GroqCloudModelMapper.cs`: "I need a way to map this more generic".
- `src/ExternalServices/Microsoft/OoBDev.Microsoft.SqlServer.DacFx/DacPacMergeCompiler.cs`: "figure out how to carry TSqlObjectOptions forward".
- `src/ExternalServices/Microsoft/OoBDev.Microsoft.SqlServer.DacFx/TemplateFactoryBase.cs`: "add a way to detect serializer by passing around media type".
- `src/Framework/OoBDev.AspNetCore.JwtAuthentication/OpenApi/OAuthDocumentTransformer.cs`: "consider getting from oauth ./.well-known/openid-configuration if not set".
- `src/Extensions/OoBDev.Data.Vectors.DB/Programmability/Stored Procedures/EmbeddingSentenceTransformerResponseSend.sql`: "Add support to optionally route an event back to the application".
- `src/ExternalServices/MongoDb/OoBDev.MongoDB/Extensions/MongoDispatchProxy.cs`: "need to simplify this name logic with that on the DataloaderCommandFactory".
- `src/Tools/OoBDev.FileRagEngine.Cli/FileRagEngineService.cs`: "do something smarter" (extension based choice).
- `src/Tools/OoBDev.DacPacCompiler.Cli/ServiceCollectionExtensions.cs`: "fix this up".
- `src/ExternalServices/SBert/OoBDev.SBert.Tests/SentenceEmbeddingClientTests.cs`: "I think I want to do away with ISentenceEmbeddingClient".

## Security and operations

Item 6 changes how the authority is discovered (a network call at startup): cache it and time it out. None otherwise.

## Priority

- **Impact:** each item is a known rough edge.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Which to build first? Recommendation: 2 (finish Service Broker) and 6 (discovery).

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments).

## Outcome

Built:

- Item 1: `AzureBlobContainerProvider.QueryContent` already listed blobs; it did not request metadata, so `MetaData` was empty. It now lists with `BlobTraits.Metadata` (Azurite integration test filters by metadata). Filters run in memory over the page-by-page listing; the service has no server-side filter beyond a prefix.
- Item 2: the finished provider is `OoBDev.Microsoft.SqlServer.ServiceBroker`; the `NotSupportedException` stub in `OoBDev.Microsoft.SqlServer.Server` was dead and is removed with its project reference.
- Item 3: `EnsureModelExistsAsync` copies from a named `sourceModel` or the first local model, and copies only when no model starts with the name (the old test was inverted).
- Item 6: `OAuth2OpenApiOptions.Authority` and `OpenIdConfigurationDiscovery` (5 second timeout, success cached, failure retried); configured URLs win.
- Item 9: the file RAG engine also skips content that looks binary (`TextContent` in `OoBDev.System.IO`); the unused `AddRequiredFramework` in the DacPac compiler CLI is deleted.

Declined, comments removed:

- Item 4: Groq chat completions are stateless and return no context tokens; the empty array is correct.
- Item 5: the dacpac script text carries no `TSqlObjectOptions`, and `IObjectConverter` already reads the template content, so there is no serializer to pick by media type (the YAML lines were commented out; YAML has no serializer in the framework).
- Item 7: nothing in the application consumes a routed event; revisit when the Hosting project needs a notification.
- Item 8: there is no data loader command factory yet; the name logic stays in `MongoDispatchProxy` until the data loader request is built.
- Item 10: `ISentenceEmbeddingClient` is the seam the provider and health check use and mock; kept.

Unit tests: 13 added (AspNetCore 52 passed, Ollama 12, System 6 for TextContent) plus one Azurite integration test (7 passed).
