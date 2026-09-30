# TODO - OoBDev (dotex) Framework

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
| 11 | Write `05-industry-alternatives/` (22 topics, pros/cons, verdicts) | ✅ Done |
| 12 | Write `README.md` index with doc/code drift table | ✅ Done |
| 13 | Final review; validate-docs 56 files, 0 problems | ✅ Done (awaiting user review) |
| 14 | Write `06-design-document-standard.md` (how design docs are made; PlantUML/Salt rule; added to CLAUDE.md) | ✅ Done |

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

**Last Updated:** 2026-01-24

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
- [ ] Security beyond authentication: secrets and key management, input validation, transport security, CORS, rate limiting, audit logging, dependency and SBOM policy
- [ ] Worker and CLI blueprint recipe (a fifth recipe beside the existing four)
- [ ] AI, vector and RAG patterns (Qdrant, SBert, Ollama, Tika usage)
- [ ] Per-project catalog (one line per project: layer, purpose, owner-review flags)
- [ ] Observability beyond logging (metrics, tracing, health checks)
- [ ] Resilience (retry, timeout, circuit breaker, idempotency)

---

## 📌 Backlog: Naming Consistency

- [ ] **Rename `ServiceCollectionEx` to `ServiceCollectionExtensions` everywhere** (decision: `ServiceCollectionExtensions` is the standard; 44 projects already use it). Both names coexist in different namespaces, which is ambiguous for consumers importing many namespaces.
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

Docker-based integration testing with 15 services (Apache Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch, Qdrant, Azurite, LocalStack, Azure Service Bus Emulator, Keycloak, SBert, Ollama, Azurinsight). Infrastructure complete, 33 tests migrated and validated.

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

### 📝 [Documentation](./TODO-documentation.md)
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
├── TODO-migrations.md                   # All Incomming/ and BinaryDataDecoders migrations
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
- `Incomming/CHECKLIST.md` - Investigation tracking

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
