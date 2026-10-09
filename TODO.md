# TODO - OoBDev (dotex) Framework

> **Owner questions awaiting answers:** [OPEN_QUESTIONS.md](./OPEN_QUESTIONS.md)

## 🔎 ACTIVE: Patterns Discovery (branch `dev/patterns-discovery`)

**Goal:** Document how this codebase is actually put together (architecture, design, patterns & practices) so future products/frameworks can be built the same way, then produce a pros/cons comparison against common industry alternatives.
**Output location:** `docs/patterns-discovery/`

| # | Task | Status |
|---|------|--------|
| 1 | Create branch, add tracking to TODO.md | ✅ Done |
| 2 | Review existing docs (`docs/architecture/*`, CLAUDE.md) | ✅ Done |
| 3 | Survey code: solution/layers, build props, csproj conventions | ✅ Done |
| 4 | Survey code: abstractions/provider/factory/DI patterns | ✅ Done |
| 5 | Survey code: options/config, hosting, ASP.NET, data, messaging | ✅ Done |
| 6 | Survey code: testing (categories, TestContext config, Docker) | ✅ Done |
| 7 | Write `01-architecture/` (solution shape, layers, dependency rules) | ✅ Done |
| 8 | Write `02-design-patterns/` (patterns with real code refs) | ✅ Done |
| 9 | Write `03-practices-and-conventions/` (naming, build, docs, testing, warts) | ✅ Done |
| 10 | Write `04-new-project-blueprint/` (4 recipes) | ✅ Done |
| 11 | Write `05-industry-alternatives/` (30 topics, pros/cons, verdicts, owner decisions applied) | ✅ Done |
| 12 | Write `README.md` index with doc/code drift table | ✅ Done |
| 13 | Final review; validate-docs 80 files, 0 problems | ✅ Done (awaiting user review) |
| 14 | Write `06-design-document-standard.md` (how design docs are made; PlantUML/Salt rule; added to CLAUDE.md) | ✅ Done |
| 15 | Add HTTP API and authorization practices, alternatives topics 25-26, apply architect answers | ✅ Done (commit 44fb0cd) |
| 16 | Build out remaining coverage gaps (Documentation Coverage Gaps backlog) | ✅ Done (awaiting user review). [Details](docs/changes/documentation-patterns-discovery-2026-09-30.md) |
| 17 | Review all TODO files and record what is actually outstanding (Outstanding Work Review) | ✅ Done (2026-09-30) |

**Notes / findings log:**
- Code survey: 123 csproj, ~1,061 .cs, 42 test projects, 120 on net10.0 (2 netstandard2.1, 1 net48 for SQL CLR).
- Core idiom: `I{Thing}` (Abstractions project) → `{Thing}` impl → `TryAdd{Thing}Services(IServiceCollection, IConfiguration, sectionName)`; providers registered twice (default + keyed); `ISelectedService<T>` picks one via config `OoBDev::ServiceKeys::{FullTypeName}`.
- Common layer is an *aggregator* (MSBuild glob ProjectReference include/remove), not "pure interfaces" as CLAUDE.md says.
- **Drift:** `[ContractConfig]` is declared + documented but never read at runtime (SelectedService hard-codes its own key).
- **Drift:** docs say .NET 9 / README.md build-enforced; code is net10.0, files are `ReadMe.{Project}.md`, missing readme is only warning OBDPK001 (error only if PackageReadmeFile set but absent).
- **Drift:** Framework has 4 empty placeholder dirs (Generations, DataLoader, ComplexEvents, SpatialServices); arch docs reference RabbitMQ.Abstractions which does not exist.
- `Directory.Packages.props` is empty, `ManagePackageVersionsCentrally=false` -> versions inline per csproj; analyzers block is commented out in Directory.Build.props.
- Unrequested-by-user side observation: no Polly/OpenTelemetry/MediatR/FluentValidation/HybridCache usage anywhere.

---

**Last Updated:** 2026-10-09

✅ **COMPLETED:** Build Warnings Resolution - Reduced from 95+ to 8 warnings (2026-01-22)
✅ **COMPLETED:** Test Category Cleanup - DevLocal tests converted to Integration/Unit/LiveIntegration (2026-01-22)
✅ **COMPLETED:** .runsettings Documentation - Comprehensive how-to guide created (2026-01-22)
✅ **COMPLETED:** Configuration Documentation - Comprehensive CONFIGURATION_SETTINGS.md created (157+ settings)
✅ **COMPLETED:** Ollama Integration Test Setup - Automated model pulling in integration-up scripts
✅ **COMPLETED:** Docker-based Integration Testing Infrastructure (Week 1 & 2) - 14 services + Ollama
✅ **COMPLETED:** Swashbuckle 10.1.0 & .NET 10.0 breaking changes - All fixes verified working
✅ **COMPLETED:** XML Documentation generation - Swagger Summary/Description now appear
✅ **COMPLETED:** Swagger generation tested and verified working
✅ **COMPLETED:** Local Docker testing validation - All Integration tests passing (2026-01-21)

> **New to this project?** Read `/CLAUDE.md` first for a complete development guide including architecture, patterns, and migration scope.

---

## 📋 Outstanding Work Review (2026-09-30)

Every TODO file and in-code marker was checked against the repository. The other TODO files carry "Last Updated" dates from 2026-01-20 to 2026-01-24 and several statuses in them are stale. This section is the corrected picture; the epic files keep their detail.

**Table — TODO files and what is really outstanding**

| File | Stated status | Actually outstanding | Action |
|------|---------------|----------------------|--------|
| `TODO-testing-local-integration.md` | Validated, ready for CI/CD | CI workflow `.github/workflows/integration-tests.yml` is still disabled (schedule and `workflow_dispatch` commented out, only `workflow_call`); 2 of 15 services not confirmed healthy (azurinsight, servicebus); Docker stack docs under `docs/architecture/testing/` do not exist (only `README.md` and `testing-guidelines.md`) | Keep open; see backlog below |
| `TODO-testing-live-integration.md` | Week 3 migration pending | Azure B2C tests are already `LiveIntegration` (3 tests in `OoBDev.Microsoft.Azure.B2C.Tests`, not `OoBDev.Microsoft.B2C.Tests` as the file says); Application Insights tests (10) are still `DevLocal`; Groq tests have no category at all; no `.env.liveintegration.template` or per-project README exists; no cloud docs | Keep open; fix paths |
| `TODO-migrations*.md`, `TODO-decisions.md` | Blocked on decisions | Unchanged: BinaryDataDecoders (14+ decisions), BotChat (archive, enhance or extract), ContractParser, Tools/BulkLlm. The files say `Incoming/`; the folder is `Incoming/` | Blocked on the owner; fix folder name |
| `Features/Caching/TODO-migrations-caching.md`, `docs/changes/migration-message-queues-2026-01-20.md` | Complete | Nothing outstanding | Archive candidates |
| `docs/todo.md` | Wish list | 4 open wishes (below) | Folded into this file |
| `src/Framework/OoBDev.DacFx.Tests/TODO.md` | Note | Test SQLCLR project covering all SQLCLR features | Backlog |
| `src/Framework/OoBDev.System.Abstractions/ComponentModel/Data/TODO.md` | Note | Example project for the data annotations | Backlog |
| `TODO.md` (this file) | Header dated 2026-01-24 | Links to `TODO-documentation.md` and `TODO-testing-infrastructure.md`, which do not exist | Remove links or create the targets |

### Backlog: CI/CD Enablement and Docker Test Infrastructure
- [x] 2026-10-08: after a fresh start 14 services report healthy (azurinsight included); servicebus has no health check; nginx and opensearch-dashboards health checks fixed (nginx probed on 127.0.0.1, dashboards status call authenticated).
- [x] 2026-10-08 full Integration run against the Docker stack: 58 passed, 0 failed, 2 skipped (SBert tests marked `[Ignore]`). Fixed on the way: Tika 4 endpoints (`/detect`, `/tika/html|text|xml`), Ollama embedding tests now use `all-minilm`, Moto pinned to 4.4 (latest needs a licence token).
- [ ] Enable `integration-tests.yml`: uncomment `schedule` (daily 16:00 UTC) and `workflow_dispatch`, remove the temporary `workflow_call`, check the runner has Docker, trigger manually, watch the first run, confirm the `validated-v{version}` tag and the 30-minute limit.
- [ ] Azurinsight follow-ups: README service table, workflow variables, `.runsettings` Application Insights settings, nginx dashboard entry, PlantUML diagrams, stack doc.
- [ ] Docker documentation under `docs/architecture/testing/`: integration category pages (README, docker-setup, writing-tests, examples), one page per stack (SQL Server, MongoDB, RabbitMQ, OpenSearch, Qdrant, Tika, SMTP, Azurite, Moto, Keycloak, SBert, Ollama, azurinsight), a docker-infrastructure page, and network-topology and dependency-matrix diagrams (PlantUML, per the diagram rule).
- [ ] Decide whether the Aspire spike (see Architect Answers) changes any of this before the docs are written.

### Backlog: Live Integration (Cloud) Tests
- [ ] Azure B2C: add `.env.liveintegration.template` and a project README, and read settings from test properties (the category change is already done).
- [ ] Application Insights: library and tests moved to 3.x/OpenTelemetry (2026-10-07; 5 unit tests pass). Ran the 5 `DevLocal` integration tests against azurinsight on 2026-10-07: all fail because the 3.x exporter posts newline-delimited JSON envelopes to `/v2.1/track` and the emulator (image `oobdev/azurinsight`, fork `mwwhited-forks/Azurinsight`) parses one JSON document (`SyntaxError: Unexpected non-whitespace character after JSON`, body-parser). Fix the emulator to accept NDJSON (and gzip), then re-run and recategorize them (Integration against azurinsight, or LiveIntegration for the real service); add template and README.
- [ ] Groq (`OoBDev.GroqCloud.Tests`): categorize tests as `LiveIntegration`; add template and README.
- [x] Added `.env.liveintegration` to `.gitignore` (2026-10-09)
- [ ] Cloud docs: category README, cloud setup, credential and cost management, per-service pages, LiveIntegration vs Integration guide, PlantUML diagrams.
- [ ] Triage of `TestCategories.DevLocal` (partly done 2026-10-09; 4 tests moved to Unit: Bson serializer, `AsXElement`, XPath `max`, Example service registry). Kept as `DevLocal` on purpose: Ollama tests (hard-coded LAN host, model pull and delete), USB HID tests (hardware), `PathEx`, `MergedXPathNavigator` and `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Example blob tests (Azurite). Still to decide:
  - [ ] `Documents.Tests` `ServiceRegistryTests` (2): fail as Unit (missing logger and other services in the test setup; the keyed test asserts a service that is never registered). Fix the setup or the assertion first.
  - [ ] `System.Tests` `ReflectionElementNodeTest`: fails as Unit (`System.Type` cannot be serialized to JSON).
  - [ ] Not reviewed yet: Application Insights (5, against azurinsight, so Integration), Html `DeeperTest`, Markdown `TestMethod1` (reads `Design.md`), DacFx `BuildPackageTest`, `IDocumentConversionTests`.

### Backlog: Dependency Updates and Build Health (2026-10-07)
- [x] Owner package bumps committed (118 csproj, `58adc7e`); Application Insights 3.1.2 migrated to OpenTelemetry processors; AI tests rewritten (`1c2eab6`). Full solution builds with 0 errors.
- [x] Fork package bumps: moot, the fork was removed (2026-10-08).
- [x] Build warnings: the 542 figure was duplicate reporting; real distinct count was about 22 and is now about 14 (2026-10-07). Qdrant obsolete calls migrated to `QueryAsync`/`QueryGroupsAsync` with 2 integration tests (2026-10-07). Remaining: intentional `#warning` markers (4), pack NU5118 duplicates (2), MSTEST0032 constant assert, SqlProj SDK upgrade to 4.4.0 and rules package. CLAUDE.md count to refresh.
- [x] Unit/Simulate suites on the whole solution after the bumps: all pass (2026-10-07).
- [x] Central package management restored (2026-10-07): `src/Directory.Packages.props` holds 87 versions, project files carry none; 8 packages that differed between projects (test tooling, Moq, GitVersion, Microsoft.Extensions 10.0.2 vs 10.0.12) now use the highest version. Script: `scripts/packages/`.
- [ ] Run the Integration category against Docker after the bumps.
- [x] Pushed `dev/patterns-discovery` (2026-10-07).
- [x] Vectors NULL policy extended to `Parse` and matrix accessors, `.DB` dacpac build order (`EnsureClrDacpac`), `DB.Tests` project, AllMiniLm test model build order (2026-10-08, see `docs/changes/testing-vectors-sqs-moto-ci-2026-10-08.md`).
- [ ] Update `README`/`CLAUDE.md` text: warning count, Application Insights 3.x note.

### Backlog: Migration Decisions (blocked on the owner)
- [ ] BinaryDataDecoders: the 14+ decisions in `TODO-decisions.md` (endianness API, BinaryPrimitives naming, UI collections location, CodeAnalysis use case, archive formats, ExpressionCalculator audit, NMEA, drawing and geometry, barcode, hardware devices, CLI tools, ISO 9660 / Apple II / classic crypto, Windows Forms and UWP).
- [ ] BotChat: choose archive, enhance or extract.
- [ ] ContractParser: implement now, later, or keep as specification. Tools/BulkLlm: consolidate or archive.

### Backlog: `docs/todo.md` Wishes (folded in)
- [ ] Find a way to turn MS Project into planning control.
- [ ] Example OAuth token exchange with multiple client ids (covered by the STS backlog).
- [ ] User analytics (screen usage, URLs, timeouts).
- [ ] Error handling middleware with correlation that returns the original error location (overlaps the `ProblemDetails` item in Architect Answers; design them together).

### Backlog: Housekeeping
- [ ] Refresh this file's header date, move the ✅ list to a change document, and fix or remove the links to the non-existent `TODO-documentation.md` and `TODO-testing-infrastructure.md`.
- [x] Renamed `Incomming` references to `Incoming` across TODO files and `CLAUDE.md` (2026-10-09).
- [x] Archived the two completed TODO files: caching was already a summary stub with a link; the message queue file became its change document `docs/changes/migration-message-queues-2026-01-20.md` (2026-10-09)
- [x] Fixed the wrong test project paths in `TODO-testing-live-integration.md` (2026-10-09)

### Backlog: In-Code TODO Markers (86 comments in 62 files, grouped)
None of these are tracked elsewhere. Triage each into fix, ticket or delete.

**Table — In-code TODOs by theme**

| Theme | Where (examples) | Note |
|-------|------------------|------|
| Hard-coded values to make configurable | `MessageReceiverHost`, `EmailMessageReceiverHost`, `EmbeddingSentenceTransformerQueueReaderHost`, `InProcessMessageProvider` (10 s and 1 s delays); `HtmlToPdfConversionHandler`; `HandlebarsTemplateProvider` (`NoEscape`); `PlantUmlRenderer` (remote URL); `QdrantVectorStoreProvider` (distance); `DocumentSummaryGenerationProvider` (model name); `HealthChecksDocumentFilter` (health endpoint); `ConfigureOAuthSwaggerGenOptions` (well-known discovery) | Options with defaults; ties to the options validation backlog |
| Unfinished providers and features | `AmazonSqsMessageProvider` (receiver), `SqlServiceBrokerQueueMessageProvider` ("finish this out"), `MimeMessageFactory` (attachments), `AzureBlobContainerProvider` (query provider), `OllamaMessageCompletion` (two `NotImplementedException`), `IOllamaModelMapper`, `OllamaApiClientExtensions`, `GroqCloudModelMapper`, `EmailMessageHandler` | Track as incomplete features |
| Health check and authorization behavior | `HealthCheckOptionsFactory` (tiered detail: anonymous gets status only, authenticated gets the list, a special claim gets descriptions and errors), `UserAuthorizationHandler`, `AspNetCoreExtensionBuilder` and `ServiceCollectionExtensions` (`RequireApplicationUserId`, configurable user id claim), `#if DEBUG` PII logging switch | Fold into the rights middleware backlog |
| Query middleware gaps | `SearchQueryOperationFilter` (form request type, filter support), `SearchModelMapper` (dictionary binding for forms), `ExpressionTreeBuilder` (8 markers: like, null safety, `IEnumerable`, type casting, unrolling) | Fold into the nested query and OData/GraphQL items |
| Caching proxy | `CachedProxy` (exceptions for void and non-generic tasks), `CacheableFactory` (`ILoggerFactory`) | Fold into the caching proxy review |
| Code quality and modernization | `Base32Codec`, `BcdEx` (Span/Memory), `XsltTransformer` and the Xsl extensions (async, injectable logger, safe returns), `ObjectConverter` ("should be tossed"), `DataConverter` (try parse), `StreamEx` (leave stream open), `BsonTypeInfoResolver` (injectable), `StartAndFixLengthSegmenter` (endian check), `StreamDevice`, `SerialPortDeviceAdapter` (dispose) | Owner review; some are deletions |
| Migration leftovers | `Windows/Forms/ValidTextBox`, `ValidationEventArgs` ("port this out") | Part of the Windows Forms decision |
| Tests | `IDocumentConversionTests` (Linux configuration), `HandlebarsTemplateProviderTests`, `HtmlToPdfConversionHandlerTests`, `EnumExtensionsTests` ("fix this better"), `MongoQueryFixes`, `QdrantGrpcClientTests`, `SentenceEmbeddingClientTests` ("do away with ISentenceEmbeddingClient"), `ExpressionParserTests` (`"1e"` should throw), `QueryableExtensionsTests` | Include in coverage work |
| Miscellaneous | `GenAiContextRequestModel` (`AssistantConfinment` spelling), `Example.WebApi/Program.cs` (run profile, MailKit disabled), `OpenSearchTests` (`HACK` certificate bypass, test only), `Tools/*` CLIs | Fix the spelling; leave the test-only bypass |

### Backlog: Patterns Discovery (Open Work)
- [ ] Owner review of all docs under `docs/patterns-discovery/`; changes welcome along the way.
- [ ] Build out the coverage gaps in the Documentation Coverage Gaps backlog (security beyond authentication first).
- [x] Updated the `docs/patterns-discovery/README.md` drift table for decisions made since it was written (2026-10-09)
- [ ] Update the `dotnet new` templates once decisions land (`AddValidatedOptions<T>()`, selection factory, SHA-512, `[LoggerMessage]`).

---

## 📌 Backlog: Review `ISelectedService` Rough Edges (intent unknown, needs owner review)

Source: [pattern 4](docs/patterns-discovery/02-design-patterns/04-selected-service.md). The original intent is not remembered, so decide what each item should be before changing code.

- [ ] **`[ContractConfig(AllowDefault, ConfigKey)]` is unused.** It is declared on provider interfaces (e.g. `ICachingProvider`, `OoBDev:CachingProvider:Type`) and documented in readmes, but nothing reads it; the runtime key is the hard-coded `OoBDev::ServiceKeys::{FullTypeName}` path. Decide: wire the attribute into `SelectedService<T>`, delete it, or keep it as documentation only. If deleted, fix the readmes and the capability template, which currently emits the attribute.
- [ ] **Selection is resolved in the constructor**, so it is fixed for the lifetime of the wrapper (a singleton means for the process). Decide whether that is intended or whether a reload (`IOptionsMonitor`) is wanted.
- [ ] **`IServiceProvider` injection** (service locator) is contained in the wrapper. Confirm this stays the accepted exception, or replace with keyed-service resolution.

---

## 📌 Backlog: Architect Answers

Owner decisions are recorded under each verdict in `docs/patterns-discovery/05-industry-alternatives/`. Work items they create:

- [ ] **Selection factory replaces `ISelectedService<T>`:** a factory registered in the container that takes a configuration path and picks a keyed service at registration/initialization (or run time); migrate existing `ISelectedService<T>` uses; spike first. Third-party DI and logging libraries are rejected.
- [ ] **`AddValidatedOptions<T>()`:** one extension wrapping `BindConfiguration` + validation + `ValidateOnStart` (ties to the options validation backlog).
- [ ] **Builder forwarding spike:** compare the `#if DEBUG` required parameters, an analyzer, and `Action<TBuilder>` for chained/nested registration; show each.
- [ ] **Mutable configuration option:** evaluate dropping the `record` for mutable configuration while keeping the options pattern over keyed values.
- [ ] **Restore central package management** (record why it was removed).
- [ ] **Zero warnings and build-time pattern enforcement** (see Roslyn analyzers backlog).
- [ ] **Source-generated caching proxy** replacing `DispatchProxy`.
- [ ] **`ProblemDetails` middleware:** automatic, published in OpenAPI, transparent; no general HTTP envelope.
- [ ] **OpenTelemetry** capability; **no Polly** (check whether `Microsoft.Extensions.Resilience` is acceptable given its Polly dependency).
- [ ] **`[LoggerMessage]` migration**; no third-party logging.
- [ ] **Default hash to SHA-512** (MD5 stays only where a legacy format needs it).
- [ ] **Scalar** for API docs (confirm what "async-ui" means: AsyncAPI viewer or Swagger UI).
- [ ] **Spikes:** other mocking frameworks (MSTest stays), Aspire vs the Docker compose stack, documentation tool chains, UI (Blazor, Knockout, Vue), UI component libraries, WPF proof of concept.
- [ ] **Nested query support** for the `IQueryable<T>` middleware and OData/GraphQL on every `IQueryable<T>` endpoint by header or URL convention.

---

## 📌 Backlog: Options Validation Modes (strict default, relaxed opt-out) — analysis before decision

Source: [pattern 7](docs/patterns-discovery/02-design-patterns/07-options-binding-by-section-name.md). Options are deliberately not validated today so that a misconfiguration cannot take the application down. Proposal to analyze: validation available, **strict by default**, with a **relaxed** mode that disables it. No decision has been made.

- [ ] **Analyze the strict/relaxed model.** Questions: where is the switch (per builder record, a global `OoBDev:Validation:Mode` setting, or both); what "strict" means (`ValidateOnStart` failing startup vs. logging only); whether `relaxed` keeps today's behavior exactly (lazy failure at first use).
- [ ] **Assess blast radius of a strict default.** Adapters are config-gated (pattern 8) and often have no configuration at all; validation must apply only to options of registrations that actually happened, or unconfigured adapters would fail startup. Count option types and which have required members.
- [ ] **Choose the validation mechanism.** Compare DataAnnotations, `IValidateOptions<T>`, and source-generated validators (`[OptionsValidator]`, AOT-friendly) with the existing `required … init` records.
- [ ] **Decide the failure surface.** Startup exception vs. health-check degradation vs. warning log, and how `ConfigurationMissingException` fits.
- [ ] **Compatibility.** A strict default is a behavior change for existing hosts; consider a release with `relaxed` as default and a warning, then flip.
- [ ] **Docs and templates.** After the decision update pattern 7, `07-known-warts.md`, the industry-alternatives verdict, the capability/adapter templates, and add an analyzer rule if useful (see the analyzer backlog).

---

## 📌 Backlog: Caching Proxy Review (pattern 10)

Source: [pattern 10](docs/patterns-discovery/02-design-patterns/10-attribute-dispatch-proxy.md).

- [ ] **Fix the `Retreive` → `Retrieve` spelling** (decision: fix it). About 85 occurrences in 23 files: `ICachingProvider`, `ICachingManager`, `CachingManager`, `CachedProxy`, the Redis and Microsoft memory providers, their tests, readmes, and `docs/architecture/caching/*`. It is a public API rename (`RetreiveAsync` → `RetrieveAsync`), so decide whether to keep an `[Obsolete]` forwarding member for one release (CLAUDE.md says no breaking changes to existing APIs). Do it with a deterministic script, then build and run the caching tests.
- [ ] **Review the remaining rough edges** (needs explanation from the owner later): blocking on async in the proxy, per-call reflection, interface-only proxying. Interface-only fits the "inject by interface" preference, so first establish which of these, if any, is a real concern.

---

## 📌 Backlog: Message Context Caller Info (pattern 12) — review before deciding

Source: [pattern 12](docs/patterns-discovery/02-design-patterns/12-message-context-object.md). Caller method/line/file is captured with `new StackFrame(5, true)`, which is brittle when async state-machine depth changes. The `[CallerMemberName]` alternative was avoided on purpose so signatures stay clean.

- [ ] **Reproduce and measure the brittleness.** Which call paths (sync, async, wrapped by the caching proxy or other decorators) return the wrong frame; add tests that pin the expected caller.
- [ ] **Compare options.** (a) keep the stack walk but locate the frame by skipping known framework/infrastructure frames instead of a fixed depth of 5; (b) `[CallerMemberName]`/`[CallerFilePath]`/`[CallerLineNumber]` with default values on the public factory/send methods, hidden from the common interface via an overload or extension so signatures stay clean; (c) an optional `CallerInfo` struct parameter; (d) drop caller info from the context or make it opt-in (it costs a stack walk per message; Release builds may lack line info).
- [ ] **Decide** whether a change is warranted, then update `IMessageContextFactory`, pattern 12, and the industry-alternatives notes if it changes.

---

## 📌 Backlog: Replace Hand-Built Providers With Platform Primitives (pattern 16)

Source: [pattern 16](docs/patterns-discovery/02-design-patterns/16-injectable-non-determinism.md). Many injectable-non-determinism interfaces were created because .NET had no equivalent. Replace them with the platform primitive where one now exists.

- [ ] **Inventory** every provider registered by `TryAddProviders()` (`src/Framework/OoBDev.System.Abstractions/Providers/`) and any other hand-built clock/GUID/random/file/identity abstraction, and map each to a platform equivalent or "no equivalent, keep".
- [ ] **`IDateTimeProvider` → `System.TimeProvider`.** Register `TimeProvider.System` with `TryAddSingleton`, migrate consumers (including the Handlebars date helper and any timers/delays that can use `TimeProvider.CreateTimer`/`Delay`), use `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) in tests. Keep `IDateTimeProvider` as an `[Obsolete]` adapter over `TimeProvider` for one release (no breaking changes to existing APIs).
- [ ] **`IGuidProvider`:** no built-in provider abstraction exists; check whether it stays as is (optionally backed by `Guid.CreateVersion7(TimeProvider)`-style time-ordered ids) or is dropped for `Guid.NewGuid()` where determinism is not needed.
- [ ] **Others** (`ITempFileFactory`, `ICurrentUserAccessor`, `IHttpPrepareRequest`, any random source): compare against `Random.Shared`/injected `Random`, `System.IO.Abstractions`, `IHttpContextAccessor`, `IHttpClientFactory` handlers; keep where no fit.
- [ ] **Update** pattern 16, the industry-alternatives verdict, the templates (if they use a clock), and add an analyzer rule (see the analyzer backlog) that flags new custom clock abstractions and direct `DateTime.UtcNow`.

---

## 📌 Backlog: STS Token Exchange (SSO token → application token)

Source: [authentication practices](docs/patterns-discovery/03-practices-and-conventions/09-authentication-practices.md) and [alternatives topic 24](docs/patterns-discovery/05-industry-alternatives/09-authentication.md). Standing preference: OAuth/OIDC/JWT wherever possible, and an STS with token exchange (RFC 8693) when an application needs its own token. Related older wish in `docs/todo.md`: an example OAuth token exchange handling multiple client ids in one service set.

- [ ] **Inventory what exists:** `OoBDev.AspNetCore.JwtAuthentication`, `OoBDev.Identity`, the Keycloak container, and the Epic 7 identity design (`Features/Proposals/07-Identity`); note what is validation only vs. exchange.
- [ ] **Design docs (4-document standard):** requirements, architecture, api-design (`ITokenExchangeClient`/STS interface, claims mapper, config-gated registration), testing-strategy. Cover multiple client ids and audiences, per-client policy, token lifetime and refresh.
- [ ] **Choose the STS** (Keycloak token exchange, Duende/OpenIddict, cloud provider, or in-house) after verifying current feature availability and licensing; record as an ADR.
- [ ] **Example and tests:** a working exchange example against the Keycloak test container, with integration tests (`Integration` category) and fakes for unit tests.
- [ ] **Windows and SAML inputs:** the STS accepts a Windows (Kerberos/NTLM) token or SAML assertion and exchanges it for a JWT, so applications only ever see JWT. Decide the mechanism (AD FS/Entra federation, Keycloak identity brokering, RFC 7522 SAML bearer grant, or in-house) and cover it in the design and example.
- [ ] **Credential conversion (services know only JWT):** define the STS or edge-adapter contract for converting API keys, HMAC-signed requests, basic auth and legacy bearer tokens to JWT (secret storage, replay protection, per-client policy), and confirm services need no per-scheme code.
- [ ] **Optional by configuration:** exchange off when the SSO token is acceptable as is; no behavior change for existing hosts.

---

## 📌 Backlog: HTTP API Querying and Rights Middleware

Source: [HTTP API practices](docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md), [authorization practices](docs/patterns-discovery/03-practices-and-conventions/11-authorization-practices.md), alternatives topics [25](docs/patterns-discovery/05-industry-alternatives/10-http-api.md) and [26](docs/patterns-discovery/05-industry-alternatives/11-authorization.md). Standing preferences: REST where possible; evaluate OData over the HTTP `QUERY` verb and GraphQL beside the custom search syntax; RBAC with application rights declared per endpoint, roles translated to rights in middleware (or by claims exchange if tokens stay small).

- [ ] **Verify `QUERY` verb support:** current spec status, ASP.NET Core routing, gateway/proxy pass-through, OpenAPI representation; design a `POST` fallback.
- [ ] **OData evaluation:** current .NET library, `IQueryable<T>` seam shared with `SearchQueryMiddleware`, limits (max page size, allowed properties, `$filter` cost), AOT/trimming; prototype next to the custom syntax (no breaking change).
- [ ] **GraphQL evaluation:** HotChocolate or alternatives, depth/cost limits, per-field rights, projection to the same query seam.
- [ ] **ADR:** choose the query surfaces and record it.
- [ ] **Role-to-right middleware:** design `IRoleRightMapper` (config/data-driven mapping, caching), register with `TryAdd*` and config gating; inventory how `[ApplicationRight]`, `UserAuthorizationHandler` and the permissions OpenAPI extension consume rights today.
- [ ] **Claims-exchange variant:** define STS-side role-to-right mapping and a token size budget; decide when to prefer it over middleware translation (see STS backlog).
- [ ] **Deny-by-default check:** analyzer or startup check that flags endpoints with no declared rights.
- [ ] **Design docs (4-document standard)** for the rights middleware and for the query surfaces.

---

## 📌 Backlog: Documentation Coverage Gaps (build out in this order)

Areas the patterns docs do not yet cover. Each becomes a practices page plus an alternatives topic, following the same 4-part pattern.

- [x] HTTP API conventions (REST, OData/`QUERY`, GraphQL) - practices page 10, alternatives topic 25
- [x] Authorization (RBAC, application rights, role-to-right translation) - practices page 11, alternatives topic 26
- [x] Security beyond authentication - practices page 12, alternatives topic 27
- [x] Worker and CLI blueprint recipe - `04-new-project-blueprint/05-new-worker-or-cli.md`
- [x] AI, vector and RAG patterns - practices page 15, alternatives topic 30
- [x] Per-project catalog - `07-project-catalog/` (generated by `scripts/docs/build-project-catalog.py`)
- [x] Observability beyond logging - practices page 13, alternatives topic 28
- [x] Resilience - practices page 14, alternatives topic 29
- [ ] Follow-ups from the new pages: implement shared CORS allow-list, rate limiter and audit log category; add `ActivitySource`/`Meter` instrumentation and OpenTelemetry host wiring; decide the resilience library (license check of `Microsoft.Extensions.Resilience`); consider splitting `ILanguageModelProvider`; add liveness/readiness split and tiered health output; triage the catalog flags (21 Framework projects without a tests project, projects without a readme).

---

## 📌 Backlog: Owner Directives (2026-09-30, later)

- [ ] **Microsoft.Extensions.AI migration:** spike done (`docs/patterns-discovery/08-spikes/01-extensions-ai.md`, code in `src/Spikes/OoBDev.Spike.ExtensionsAI`); owner will probably accept. Next: ADR, pin `Microsoft.Extensions.AI*` together, move consumers of `ILanguageModelProvider` / `IMessageCompletion` to `IChatClient`, verify Groq path.
- [ ] **Update all .NET libraries for .NET 10 as far as possible:** survey with `dotnet list package --outdated`, upgrade in groups (Microsoft.Extensions.*, test stack, vendor SDKs), build and test each group; do together with restoring central package management.
  - Survey done 2026-09-30 (`dotnet list src/OoBDev.sln package --outdated`): 72 packages behind. Groups: (1) Microsoft.Extensions.*, AspNetCore JwtBearer, System.* 10.0.2 to 10.0.12 (patch, low risk); (2) test stack MSTest 4.0.2 to 4.4.1, Test.Sdk, coverlet 6.0.4 to 10.1.0, Moq; (3) vendor minor bumps (Azure.*, MailKit, MongoDB, Qdrant, OllamaSharp, Swashbuckle, Semantic Kernel); (4) major bumps needing code review: StackExchange.Redis 3, Microsoft.Graph 6, Microsoft.Data.SqlClient 7, ApplicationInsights 3, OpenSearch.Client 2, Markdig 1, YamlDotNet 18, ReverseMarkdown 6, AngleSharp. `src/Directory.Packages.props` is currently an empty `<Project />`.
- [ ] **Missing readmes are created:** `OoBDev.System.Text.Html` and `OoBDev.Example.WebApi` readmes written; the `AllMiniLmL6V2Sharp` fork is gone.
- [ ] **Abstractions need no tests** when they hold only interfaces and models; an abstractions project with testable implementation gets its own test library. The project catalog no longer flags `*.Abstractions` projects; review any that contain implementation.

- [x] **Image embeddings (2026-10-08):** runner, Skia decoder, DINOv2-small, ViT-base and CLIP presets compared with Python. [Details](docs/changes/migration-image-embeddings-2026-10-08.md). Remaining: runner unit coverage toward 80%, real-photo sanity check, GitHub CI run.
- [x] **AllMiniLmL6V2 replaced the fork (2026-10-08):** `OoBDev.Onnx.SentenceEmbeddings` and `OoBDev.SBert.AllMiniLmL6V2` verified against the Hugging Face model, model downloaded on first use, fork and submodules deleted. [Details](docs/changes/migration-allminilml6v2-embedder-2026-10-08.md). Presets `AllMpnetBaseV2` and `NomicEmbedTextV1_5` added and compared with Hugging Face ([Details](docs/changes/migration-embedding-presets-2026-10-08.md)). MiniLM now uses the official `sentence-transformers` repo files and its model-loading tests are Integration (2026-10-09). Linux accent stripping verified in a container (2026-10-09). Remaining: example app run.
- [ ] **Make embeddings as native as possible** (the fork is gone; the first-party tokenizer and runner cover all-MiniLM-L6-v2; the steps below still apply to other models) (owner directive; building the tooling is in scope). Today it runs the model through ONNX Runtime with a hand-written tokenizer, and the SBert container covers the same ground. Steps, in order:
  - (Done for BERT/WordPiece with a first-party tokenizer; `Microsoft.ML.Tokenizers` did not match the reference.) Replace the hand-written tokenizer with `Microsoft.ML.Tokenizers` (WordPiece/`BertTokenizer`), checked against golden ids produced by the Python `tokenizers` library.
  - Build a Hugging Face tokenizer loader (`tokenizer.json` and config folder to a `Tokenizer`, like `AutoTokenizer`): WordPiece first, byte-level BPE next, SentencePiece via `tokenizer.model`; gaps fail with a clear error. First check whether newer `Microsoft.ML.Tokenizers` releases already load `tokenizer.json`.
  - Build an `AutoModel`-style loader for embedding models: model folder to `IEmbeddingGenerator<string, Embedding<float>>` (tokenize, ONNX Runtime, pooling, normalization, dimension), registered as a keyed provider.
  - Add a deterministic export script (`scripts/models/export-onnx.py` with README and pinned requirements; Optimum for encoders, the ONNX Runtime GenAI builder for decoder LLMs) that writes a `model-card.md` (source id, revision, tool versions, cosine-similarity validation). Converted models stay out of git. Owner option: publish the converted ONNX models to Hugging Face (an owner-controlled account or organization, each repo with its model card and the source licence respected) while the framework tooling lives here; the export script then also uploads, and the loader can resolve a model id from the Hub into a local cache (an `AutoModel.from_pretrained` equivalent) as well as from a local folder. Decide hosting (Hugging Face vs artifact feed) before the first model is converted.
  - Golden tests: exact token-id equality and embedding cosine similarity above 0.999 against the Python reference.
  - Then retire the SBert container and its health check and integration tests if parity holds, and update the AI practices page and alternatives verdict.
  - Run as a spike first under `src/Spikes/`, recorded in `docs/patterns-discovery/08-spikes/`; record completed steps in `docs/changes/`.

---

## 📌 Backlog: Naming Consistency

- [x] **Renamed `ServiceCollectionEx` to `ServiceCollectionExtensions` everywhere (2026-10-09, all 6 classes and files; no forwarders, because duplicate extension methods in two classes would be ambiguous and no static callers existed)** (decision: `ServiceCollectionExtensions` is the standard; 44 projects already use it). Both names coexist in different namespaces, which is ambiguous for consumers importing many namespaces.
  - Classes to rename (file and class): `OoBDev.Amazon.Sqs`, `OoBDev.Microsoft.Azure.ServiceBus`, `OoBDev.Microsoft.Caching`, `OoBDev.RabbitMQ`, `OoBDev.Redis.Caching`, `OoBDev.Caching` (find with `grep -rl "class ServiceCollectionEx" src`).
  - Update `<see cref>` references, tests, readmes and docs that mention the old name.
  - Public API rename: check for external consumers; extension-method call sites are unaffected because only the class name changes.
  - New code and the `templates/` already use `ServiceCollectionExtensions`.
- [ ] **Normalize provider keys to kebab-case** (`"Redis"` → `"redis"`, `"OLLAMA"` → `"ollama"`, `"HTTP"` → `"http"`, `"Environment"` → `"environment"`, and the upper-cased enum forwarding for `IHash` etc.). Message-queue adapters already comply. Keep keys next to each adapter (`{Vendor}Globals.ProviderKey`), never in a global registry, so adapters keep referencing only their Abstractions project; make sure every adapter exposes a constant rather than an inline string. Keys are used in configuration (`OoBDev::ServiceKeys::...`), so update `appsettings*`, `.runsettings`, `CONFIGURATION_SETTINGS.md`, tests and docs together, and consider accepting the old key as an alias for one release. Add an analyzer rule (see the analyzer backlog) that flags non-kebab-case key constants.

---

## 📌 Backlog: Roslyn Analyzers That Enforce the Documented Patterns

Goal: turn the rules in `docs/patterns-discovery/` into build-time diagnostics (an `OoBDev.Analyzers` package referenced by `src/Directory.Build.props`), so the patterns are enforced instead of remembered. Start with warnings, promote to errors once the baseline is clean. Each rule gets a diagnostic ID (`OOB0xx`), a code fix where mechanical, tests, and a doc page.

| Candidate rule | Source document | Severity | Code fix |
|----------------|-----------------|----------|----------|
| Registration methods use `TryAdd*` (collections may use `Add*`) | [pattern 2](docs/patterns-discovery/02-design-patterns/02-tryadd-everywhere.md) | Warning | Yes |
| Public `Try*` registration methods forward every child builder parameter they receive (companion to the `#if DEBUG` required-parameter pattern) | [pattern 9](docs/patterns-discovery/02-design-patterns/09-if-debug-explicit-arguments.md) | Warning | No |
| Entry-point builder parameter uses the `#if DEBUG` required / Release optional shape | [pattern 9](docs/patterns-discovery/02-design-patterns/09-if-debug-explicit-arguments.md) | Info | Yes |
| Registration class is named `ServiceCollectionExtensions` (retire `ServiceCollectionEx`) | [pattern 1](docs/patterns-discovery/02-design-patterns/01-abstractions-implementation-registration.md) | Warning | Yes |
| Abstractions projects do not reference implementation projects or vendor SDK packages; adapters reference Abstractions only | [layers](docs/patterns-discovery/01-architecture/02-five-source-layers.md) | Error | No |
| Builder records expose `OptionsSection` defaulting to `nameof(Options)` | [pattern 6](docs/patterns-discovery/02-design-patterns/06-builder-records.md) | Info | Yes |
| No `DateTime.Now/UtcNow`, `Guid.NewGuid()`, `Random` in framework code; inject the abstraction | [pattern 16](docs/patterns-discovery/02-design-patterns/16-injectable-non-determinism.md) | Warning | Yes |
| Only infrastructure types may inject `IServiceProvider` | [alternatives 5](docs/patterns-discovery/05-industry-alternatives/01-di-and-composition.md) | Warning | No |
| Public API has XML documentation; nullable on; implicit usings off | [practices](docs/patterns-discovery/03-practices-and-conventions/02-build-and-csproj.md) | Warning | No |
| Test classes carry a `TestCategory`; unit tests avoid non-strict mocks | [testing practices](docs/patterns-discovery/03-practices-and-conventions/04-testing-practices.md) | Warning | No |
| Test configuration read through `TestContext`, not `Environment.GetEnvironmentVariable` | [testing practices](docs/patterns-discovery/03-practices-and-conventions/04-testing-practices.md) | Warning | Yes |
| Provider key constants (`*ProviderKey`, keyed registrations) are kebab-case | [pattern 3](docs/patterns-discovery/02-design-patterns/03-provider-factory-keyed.md) | Warning | Yes |
| Hosted services catch and log and do not exit on a single failure | [pattern 13](docs/patterns-discovery/02-design-patterns/13-supervised-hosted-service.md) | Info | No |

Also enforce at the MSBuild level (no analyzer needed): README present and packed, project name matches folder, central package versions (see [alternatives 9](docs/patterns-discovery/05-industry-alternatives/02-project-structure-and-build.md)).

Plan: (1) design document set for the analyzer package following the design document standard; (2) `OoBDev.Analyzers` and `OoBDev.Analyzers.Tests` projects (use the `oobdev-*` template conventions); (3) implement rules in the order above; (4) measure baseline warnings and fix or suppress; (5) enable as errors in CI.

---

## Quick Navigation

This document is organized into **epic-based files** for better navigation and maintenance:

### 🧪 [Local Integration Testing (Docker)](./TODO-testing-local-integration.md)
**Status:** ✅ VALIDATED & COMPLETE - Ready for CI/CD Enablement

Docker-based integration testing with 15 services (Apache Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch, Qdrant, Azurite, Moto, Azure Service Bus Emulator, Keycloak, SBert, Ollama, Azurinsight). Infrastructure complete, 33 tests migrated and validated.

**Key Tasks:**
- ✅ Docker infrastructure (15 services, compose files, scripts, README with PlantUML)
- ✅ Test category: Integration (Docker-based, runs in CI/CD)
- ✅ Local validation complete - All Integration tests passing (2026-01-21)
- ✅ Week 2: Migrated 23 tests from DevLocal to Integration category (Apache Tika, SMTP/MailKit, MongoDB, RabbitMQ, OpenSearch, SBert, Ollama)
- ✅ Ollama integration: Auto-pull phi3 model in integration-up scripts
- ✅ Azurinsight integration complete - 10 Application Insights tests (2026-01-24)
- ✅ **NEW:** Scripts enhanced with --build flag, fixed Windows path handling, universal health checks (2026-01-24)
- ⏳ **IN PROGRESS:** Health check fixes (13/15 services healthy, 2 remaining)
- ⏳ **NEXT:** Enable CI/CD pipeline for daily Integration test runs
- ⏳ Week 4: Docker documentation (15 stacks)

### ☁️ [Live Integration Testing (Cloud)](./TODO-testing-live-integration.md)
**Status:** ✅ Category Added - ⏳ Week 3 Migration Pending

Cloud-based integration testing for services requiring live credentials (Azure B2C, Application Insights, Groq Cloud). Manual execution only, NOT run in CI/CD.

**Key Tasks:**
- ✅ Test category: LiveIntegration (cloud services, manual only)
- ⏳ Week 3: Migrate 3 cloud services (Azure B2C, App Insights, Groq)
- ⏳ Week 4: Cloud documentation (credential management, cost management)

### 📦 [Migrations](./TODO-migrations.md)
**Status:** ⏸️ Awaiting Decisions

**📋 Individual Migration TODOs:** [TODO-migrations-index.md](./TODO-migrations-index.md)

**Pending Migration:**
- ⏸️ **BinaryDataDecoders** - 5-phase migration (Foundation → Specialized Features)
  - Blocked: Awaiting 14+ critical decisions
  - Priority: HIGH
  - Scope: ~50,000 LOC (binary processing, protocols, hardware)

### 📝 [Documentation](./docs/architecture/README.md)
**Status:** 📋 Ongoing Maintenance

**Active Work:**
- ⏳ Maintain architecture documentation
- ⏳ Keep testing guidelines current
- ⏳ Update configuration references as needed

### ❓ [Decisions Required](./TODO-decisions.md)
**Status:** ⏸️ Multiple Migrations Blocked - Awaiting Strategic Decisions

Critical decisions blocking migration work:
- **BinaryDataDecoders** - Endianness API design, CodeAnalysis use cases, archive formats, hardware devices
- **BotChat** - Choose: Archive, Enhance, or Extract patterns (RECOMMEND: Archive)
- ✅ **Oobtainium** - RESOLVED: Moved to proving-grounds repository

**Key Decisions:**
- ⏸️ BotChat: Sample application (Option 1 ARCHIVE recommended)
- ⏸️ BinaryDataDecoders: 14+ questions across 4 priority levels
- ✅ Oobtainium: RESOLVED - Moved to separate code playground repository

---

## Epic Files Structure

```
TODO.md                                  # This file - Index and navigation
├── TODO-testing-local-integration.md    # Docker-based integration testing (11 services)
├── TODO-testing-live-integration.md     # Cloud-based integration testing (3 services)
├── TODO-migrations.md                   # All Incoming/ and BinaryDataDecoders migrations
├── TODO-decisions.md                    # Pending strategic decisions
└── TEST_VARIABLES.md                    # All test properties and configuration variables
```

---

## Migration Scope Philosophy

**ALL features from BinaryDataDecoders will be migrated** - phases indicate priority order, not feature selection.

### What Gets Migrated
✅ Core features (obviously)
✅ Highly specialized features (ISO 9660, hardware devices)
✅ Niche features (Apple II, retro computing, fencing equipment)
✅ Educational features (classic cryptography with warnings)
✅ Incomplete features (migrate as-is, track TODOs)
✅ UI components (Windows Forms validation controls)

### What Gets Deleted (Very Rare)
❌ Stub projects with zero implementation (e.g., Rigol)
❌ Silverlight-only or obsolete platform code
❌ Features with no .NET 10.0 equivalent

### Migration Principles
1. Phases indicate **priority order**, not feature selection
2. Even incomplete features are migrated and tracked
3. Specialized features packaged separately for target audiences
4. Security warnings added where appropriate (e.g., broken ciphers)
5. All code must follow architectural standards

---

---

## Active Priorities

### Immediate (This Week)
1. **Local Docker Testing** ⏳ CRITICAL
   - Follow `/containers/testing/TESTING-CHECKLIST.md`
   - Verify all 11 services become healthy
   - Test cleanup and restart
   - Enable CI/CD workflow after validation

2. **Strategic Decisions** ⏸️ BLOCKING
   - Decide on BotChat migration (RECOMMEND: Archive)
   - Review BinaryDataDecoders decision points

### Short Term (Next 2 Weeks)
1. **Live Integration Test Migration (Week 3)** ⏳ PENDING
   - Azure B2C tests - Move from DevLocal to LiveIntegration
   - Application Insights tests - Move to LiveIntegration
   - Groq Cloud tests - Move to LiveIntegration
   - Create .env.liveintegration.template files
   - Document cloud credential setup

2. **Testing Documentation (Week 4)** ⏳ PENDING
   - Create 11 stack-specific documentation files
   - Document each Docker service (MongoDB, RabbitMQ, etc.)
   - Add PlantUML diagrams for service architecture
   - Create LiveIntegration setup guides
   - Update SemanticKernel integration

### Medium Term (Next Month)
1. **Feature Implementation Decisions** ⏸️ PENDING
   - ContractParser - Decide: Implement now, later, or keep as specification
   - Tools (BulkLlm) - Decide: Consolidate into unified LlmCodeGen tool

2. **BinaryDataDecoders Phase 1** ⏸️ BLOCKED (Awaiting decisions)
   - Endianness support enhancements
   - Utility enhancements
   - BinaryPrimitives expansion

---

## Reference Documentation

### Architecture
- `docs/architecture/README.md` - Overview
- `docs/architecture/architectural-guidelines.md` - 14 principles
- `docs/architecture/architectural-standards.md` - Enforceable standards
- `docs/architecture/architectural-patterns.md` - Pattern catalog
- `docs/architecture/layering-architecture.md` - Layer details

### Migration
- `docs/migration/README.md` - Migration overview
- `docs/migration/binarydatadecoders-feature-mapping.md` - Feature comparison
- `docs/migration/binarydatadecoders-migration-plan.md` - Detailed plan
- `docs/migration/binarydatadecoders-critical-questions.md` - Decision matrix
- `docs/migration/framework-feature-mapping.md` - Framework analysis
- `docs/migration/vector-comparison.md` - Vector library comparison
- `docs/migration/sharedframework-feature-mapping.md` - 52 projects analyzed
- `docs/migration/sharedframework-migration-plan.md` - 12-phase plan
- `docs/migration/sharedframework-phase0-coverage-analysis.md` - Overlap analysis with main codebase
- `docs/migration/sharedframework-differs-detailed-comparison.md` - **NEW:** Detailed DIFFERS project comparison
- `docs/migration/oobtainium-feature-mapping.md` - Mocking framework analysis
- `docs/migration/oobtainium-migration-plan.md` - 4 migration options
- `docs/migration/botchat-feature-mapping.md` - Sample app analysis
- `docs/migration/botchat-migration-plan.md` - 3 migration options

### Testing
- `TEST_VARIABLES.md` - All test properties reference (30+ variables)
- `docs/architecture/testing-guidelines.md` - Comprehensive testing best practices
- `containers/testing/README.md` - Infrastructure guide with PlantUML
- `containers/testing/TESTING-CHECKLIST.md` - Local validation procedure
- `containers/testing/STATUS.md` - Implementation progress tracker
- `Incoming/CHECKLIST.md` - Investigation tracking

### Change History
- `docs/changes/README.md` - Archived completed work (reduces context overhead)
- `docs/changes/documentation-configuration-settings-2026-01-21.md` - Configuration documentation (157+ settings)
- `docs/changes/testing-ollama-integration-2026-01-21.md` - Ollama integration (4 tests, auto-setup)
- `docs/changes/sharedframework-phase0-2026-01-20.md` - Namespace cleanup (27 directories)
- `docs/changes/migration-message-queues-2026-01-20.md` - AWS SQS + Azure Service Bus
- `docs/changes/testing-docker-infrastructure-2026-01-19.md` - Docker testing (14 services, 23 tests)

### Protocols
- `.claude/protocols/software/incoming-codebase-comparison.md` - v1.1
- `.claude/protocols/software/architectural-analysis.md` - Architecture review
- `.claude/protocols/software/security-audit.md` - Security protocol

---

## Notes

- All new code must follow `docs/architecture/architectural-standards.md`
- All new projects must have README.md (build-enforced)
- Framework layer requires 80%+ test coverage
- Use provider/factory pattern for extensible components
- Follow existing dependency injection patterns
- Maintain .NET 10.0 target framework
- Keep nullable reference types enabled
- Keep implicit usings disabled

---

## Quick Commands

```bash
# Build entire solution
dotnet build src/

# Run all tests
dotnet test src/

# Run specific test categories
dotnet test src/ --filter "TestCategory=Unit"
dotnet test src/ --filter "TestCategory=Simulate"
dotnet test src/ --filter "TestCategory=Integration"

# Run Integration tests with custom settings
dotnet test src/ --settings integration.runsettings --filter "TestCategory=Integration"

# Start integration test services
cd containers/testing
./scripts/integration-up.sh --wait

# Stop integration test services
./scripts/integration-down.sh --clean

# Run integration tests (after services started)
dotnet test src/ --filter TestCategory=Integration
```

---

**For detailed information on each epic, see the respective TODO-{epic}.md files above.**

- [x] Fixed 2026-10-07: `TestContextExtensions.GetPropertyOrDefault<T>`/`GetRequiredProperty<T>` returned `default(T)` instead of the supplied default for missing value-type properties; they now use a new `TryGetProperty<T>` (7 unit tests).

- [x] Package updates (2026-10-08): the major bumps listed earlier (Redis 3, SqlClient 7, OpenSearch 2, Markdig 1, YamlDotNet 18, ReverseMarkdown 6, Graph 6) were already in place; applied the remaining minor/patch updates (MSTest 4.5.1, Swashbuckle 10.3.0, Azure.Storage, AWSSDK.SQS, AngleSharp, Graph 6.8.0, SemanticKernel.Core 1.81.0). `Microsoft.SemanticKernel.Connectors.Ollama` stays on its alpha.
