# Change Documentation Archive

This directory contains detailed documentation of completed work, archived to reduce context overhead in active TODO files and CLAUDE.md.

---

## Purpose

**Why archive completed work?**
- **Reduce Context Overhead:** Keep TODO files and CLAUDE.md focused on current/pending work
- **Preserve Detail:** Maintain comprehensive records of all changes for future reference
- **Improve Navigation:** Easier to find specific completed work when needed
- **Historical Record:** Track evolution of the codebase with dates and details

---

## Document Naming Convention

One file per day: `{YYYY-MM-DD}.md` (owner, 2026-11-04). Each finished task of that day is a `##` section with the task's title; its own headings sit one level lower. A day with three or more tasks starts with a contents list. Link a task as `{YYYY-MM-DD}.md#{section-anchor}`.

New tasks are appended to the file of the day they finish (create it with `# Changes {date}` when it is the first one). `scripts/docs/merge-changes-by-day.ps1` merged the earlier one-file-per-task documents and rewrote their links.

---

## Document Structure

Each task section should include:

### Required Sections

1. **Header**
   - Date
   - Epic
   - Status
   - Impact summary

2. **Summary**
   - Brief overview (2-3 sentences)
   - Key results/outcomes

3. **Detailed Changes**
   - What was done
   - Files modified
   - Code examples (before/after)
   - Technical details

4. **Verification**
   - Build verification
   - Test verification
   - Any manual verification steps

5. **Related Documentation**
   - Links to TODO files
   - Links to other change documents
   - Links to architecture docs

### Optional Sections

- **Key Patterns** - Reusable patterns for future reference
- **Impact Summary** - Tables or statistics
- **References** - External documentation links
- **Known Issues** - Any remaining known issues

---

## By day

- [2026-11-16](2026-11-16.md): 64 task(s)
- [2026-11-12](2026-11-12.md): 51 task(s)
- [2026-11-11](2026-11-11.md): 124 task(s)
- [2026-11-10](2026-11-10.md): 49 task(s)
- [2026-11-09](2026-11-09.md): 24 task(s)
- [2026-11-06](2026-11-06.md): 48 task(s)
- [2026-11-05](2026-11-05.md): 39 task(s)
- [2026-11-04](2026-11-04.md): 39 task(s)
- [2026-11-03](2026-11-03.md): 9 task(s)
- [2026-10-30](2026-10-30.md): 34 task(s)
- [2026-10-29](2026-10-29.md): 99 task(s)
- [2026-10-28](2026-10-28.md): 24 task(s)
- [2026-10-22](2026-10-22.md): 29 task(s)
- [2026-10-09](2026-10-09.md): 9 task(s)
- [2026-10-08](2026-10-08.md): 9 task(s)
- [2026-10-07](2026-10-07.md): 3 task(s)
- [2026-09-30](2026-09-30.md): 3 task(s)
- [2026-01-21](2026-01-21.md): 9 task(s)
- [2026-01-20](2026-01-20.md): 9 task(s)
- [2026-01-19](2026-01-19.md): 3 task(s)
- [2026-01-15](2026-01-15.md): 3 task(s)

---

## Current Change Documents

### Migrations

**[Migration - Caching Framework](2026-01-20.md#migration---caching-framework)**
- Complete Caching framework migration from SharedFramework (4 implementation + 3 test projects)
- Enhanced StringFormatter with property chain support (unlimited depth)
- Added Redis to Docker integration testing infrastructure
- Comprehensive documentation (5 architecture docs)
- Status: ✅ Complete

**[Migration: Message Queue Providers (Amazon SQS, Azure Service Bus)](2026-01-20.md#migration-message-queue-providers-amazon-sqs-azure-service-bus)**
- AWS SQS + Azure Service Bus providers
- LocalStack + Azure emulator integration
- Context-based pattern (non-generic)
- Status: ✅ Complete

**[Migration - SharedFramework Phase 0 Namespace Cleanup](2026-01-20.md#migration---sharedframework-phase-0-namespace-cleanup)**
- Namespace cleanup (27 directories renamed)
- Api. prefix removals, Azure reorganization, Contracts → Abstractions
- Status: ✅ Complete

### Documentation

**[TODO.md - Completed Items (archived)](2026-10-09.md#todomd---completed-items-archived)**
- Completed Patterns Discovery tasks, banners and backlog items moved out of `TODO.md`
- Status: ✅ Complete

**[Documentation - Configuration Settings Reference (CONFIGURATION_SETTINGS.md)](2026-01-21.md#documentation---configuration-settings-reference-configuration_settingsmd)**
- CONFIGURATION_SETTINGS.md created (157+ settings)
- 31 Options classes, 24 direct keys, 102 environment variables
- New protocol: configuration-documentation.md
- Status: ✅ Complete

**[Documentation - Patterns Discovery Coverage and Extensions.AI Spike](2026-09-30.md#documentation---patterns-discovery-coverage-and-extensionsai-spike)**
- Security, observability, resilience and AI/RAG practices and alternatives pages
- Worker/CLI recipe, generated project catalog (81 projects)
- Microsoft.Extensions.AI spike (6 tests) with migration recommendation
- Status: ✅ Complete

**[Documentation - AllMiniLmL6V2 Replacement Design](2026-10-07.md#documentation---allminilml6v2-replacement-design)**
- Design set for replacing the `AllMiniLML6v2Sharp` fork with first-party, thread-safe ONNX embeddings
- Status: ✅ Complete (design only)

### API Documentation

**[Swashbuckle replaced by OpenAPI transformers and Scalar](2026-10-09.md#swashbuckle-replaced-by-openapi-transformers-and-scalar)**
- Swashbuckle replaced by `Microsoft.AspNetCore.OpenApi` transformers and the Scalar API reference; OAuth2 authorization code with PKCE; AsyncAPI is phase 2
- Status: ✅ Complete (phase 1)

**[AsyncAPI document and viewer for the message queues](2026-10-09.md#asyncapi-document-and-viewer-for-the-message-queues)**
- AsyncAPI 3.0 document and viewer for the message queues (`OoBDev.AsyncApi`, adapter contributors, `MapAsyncApi()`)
- Status: ✅ Complete

**[CGI example in Go and the OpenAPI listing](2026-10-28.md#cgi-example-in-go-and-the-openapi-listing)**
- Go CGI example in the example web API, CGI programs listed in OpenAPI, feature request folder
- Status: ✅ Complete (Linux build not run)

**[CGI middleware: round-trip tests](2026-10-22.md#cgi-middleware-round-trip-tests)**
- CGI echo, Location, no-blank-line, shell-metacharacter and path-split tests
- Status: ✅ Complete

**[Pack warnings NU5118 and NU5128 removed](2026-10-22.md#pack-warnings-nu5118-and-nu5128-removed)**
- NU5118 and NU5128 pack warnings removed
- Status: ✅ Complete

**[Analyzer rules promoted to warning](2026-10-22.md#analyzer-rules-promoted-to-warning)**
- MA0070, MA0084, MA0132, MA0144, MA0151, MA0158, MA0169 now warnings; sites fixed
- Status: ✅ Complete

**[Example web: view-model tests for the remaining pages](2026-10-22.md#example-web-view-model-tests-for-the-remaining-pages)**
- Vitest view-model tests for Communications, Current user, Queues and Templates
- Status: ✅ Complete

**[Rights: Debug logging and configuration docs](2026-10-22.md#rights-debug-logging-and-configuration-docs)**
- Role-to-right transformation logs counts at Debug; `OoBDev:Authorization` documented
- Status: ✅ Complete

**[CGI middleware uses the shared right checker](2026-10-22.md#cgi-middleware-uses-the-shared-right-checker)**
- CGI middleware decides 401/403 with the shared `IApplicationRightChecker`
- Status: ✅ Complete

**[example-ui design reconciled with the code](2026-10-22.md#example-ui-design-reconciled-with-the-code-1)**
- example-ui navigation design checked against the built pages
- Status: ✅ Complete

**[OpenApiScalar design reconciled with the code](2026-10-22.md#openapiscalar-design-reconciled-with-the-code)**
- OpenApiScalar OAuth2 sign-in and API info covered by Unit tests
- Status: ✅ Complete

**[LlmCodeGen provider selection](2026-10-22.md#llmcodegen-provider-selection)**
- LlmCodeGen provider selection extracted and unit tested; provider names are case-insensitive
- Status: ✅ Complete

**[ImageEmbeddings design reconciled with the code](2026-10-22.md#imageembeddings-design-reconciled-with-the-code)**
- ImageEmbeddings testing strategy matched to the real tests, with a requirement-coverage table
- Status: ✅ Complete

**[AsyncAPI send messages by type](2026-10-22.md#asyncapi-send-messages-by-type)**
- AsyncAPI send operations with schemas for registered message types
- Status: ✅ Complete

**[Embedding generator telemetry](2026-10-22.md#embedding-generator-telemetry)**
- ActivitySource and Meter for the ONNX sentence embedding generator (span, duration, batch size, tokens)
- Status: ✅ Complete

**[CgiMiddleware design reconciled with code](2026-10-22.md#cgimiddleware-design-reconciled-with-code)**
- CgiMiddleware design compared with code; testing strategy now names real tests, gaps in BACKLOG
- Status: ✅ Complete (other designs still to compare)

**[HTTP QUERY verb](2026-10-22.md#http-query-verb)**
- `UseHttpQueryVerb()`: HTTP `QUERY` (and `POST` + `X-HTTP-Method-Override`) routed as `GET` so one endpoint and one rights check serve all
- Status: ✅ Complete (first slice; OData and GraphQL wait on Q2)

**[Application rights: roles to rights, one shared check](2026-10-22.md#application-rights-roles-to-rights-one-shared-check)**
- Roles mapped to `app__application_right` claims from configuration, one shared right checker, `[ApplicationRight]` returns 401 or 403
- Status: ✅ Complete (CGI move and Debug logging in BACKLOG)

**[CGI middleware for Kestrel (first cut)](2026-10-22.md#cgi-middleware-for-kestrel-first-cut)**
- CGI/1.1 middleware for Kestrel (`OoBDev.AspNetCore.Cgi`): safe defaults, limits, built-in rights check
- Status: ✅ Complete (first cut)

**[Analyzer cleanup, Azurite queue tests, search example and local CI runner](2026-10-22.md#analyzer-cleanup-azurite-queue-tests-search-example-and-local-ci-runner)**
- Analyzer cleanup, Azurite queue tests, search annotation example, local CI runner fix
- Status: ✅ Complete

### Observability

**[Application Insights replaced by OpenTelemetry, broken DevLocal tests fixed](2026-10-09.md#application-insights-replaced-by-opentelemetry-broken-devlocal-tests-fixed)**
- Application Insights replaced by `OoBDev.OpenTelemetry` (OTLP), Grafana LGTM test container, DevLocal tests fixed
- Status: ✅ Complete

### AI

**[In-process all-MiniLM-L6-v2 embedder replaces the fork](2026-10-08.md#in-process-all-minilm-l6-v2-embedder-replaces-the-fork)**
- First-party in-process all-MiniLM-L6-v2 embedder verified against Hugging Face; model downloaded on first use; fork and submodules removed
- Status: ✅ Complete (GitHub run not yet observed)

**[Embedding presets for MPNet and Nomic, shared Hugging Face cache, version policy](2026-10-08.md#embedding-presets-for-mpnet-and-nomic-shared-hugging-face-cache-version-policy)**
- MPNet and Nomic presets on the ONNX runner, compared with Hugging Face; shared hub cache; version policy
- Status: ✅ Complete (GitHub run not yet observed)

**[In-process image embeddings and classification (DINOv2, ViT, CLIP)](2026-10-08.md#in-process-image-embeddings-and-classification-dinov2-vit-clip)**
- In-process image embeddings and classification: ONNX runner, Skia decoder, DINOv2-small, ViT-base and CLIP presets compared with the Python models
- Status: ✅ Complete (GitHub run not yet observed)

### Testing

**[Testing - Live Integration Testing (completed work)](2026-10-09.md#testing---live-integration-testing-completed-work)**
- Archived completed section of the live integration TODO (LiveIntegration category)
- Status: ✅ Complete

**[Testing - Local Integration Testing (completed work)](2026-10-09.md#testing---local-integration-testing-completed-work)**
- Archived completed sections of the local integration TODO (Weeks 1 and 2, validation, script and health check fixes)
- Status: ✅ Complete

**[CI restore, vector functions and Moto AWS emulator](2026-10-08.md#ci-restore-vector-functions-and-moto-aws-emulator)**
- CI restore fix, vector `Angle` correction and NULL-safe functions with SQL Server Integration tests
- Moto replaces LocalStack; SQS client and test fixes
- Status: ✅ Complete (GitHub run not yet observed)

**[Testing - Ollama Integration & Automated Model Setup](2026-01-21.md#testing---ollama-integration--automated-model-setup)**
- Ollama LLM inference (14th Docker service)
- 4 tests migrated, automated phi3 model setup
- Fixed Windows batch container detection
- Status: ✅ Complete

**[Testing - Docker-Based Integration Testing Infrastructure](2026-01-19.md#testing---docker-based-integration-testing-infrastructure)**
- Docker integration testing infrastructure (14 services)
- 23 tests migrated from DevLocal to Integration
- CI/CD pipeline, cross-platform scripts
- Status: ✅ Complete (Week 1 & 2) - ⏳ Awaiting local validation

### Bug Fixes & Technical Debt

**[Bug Fixes: ANTLR Cross-Platform Build & Integration Testing Infrastructure](2026-01-20.md#bug-fixes-antlr-cross-platform-build--integration-testing-infrastructure)**
- ANTLR cross-platform build fix (Windows absolute paths in generated files)
- Keycloak script feature configuration (--features=scripts, NOT upload-scripts)
- Nginx DNS resolver with variable-based proxy_pass (Docker 127.0.0.11)
- RabbitMQ port conflict resolution (5672→5673, removed extends)
- Documentation updates for troubleshooting all four issues
- Status: ✅ Complete and verified

**[Bug Fixes - Swashbuckle 10.1.0 & .NET 10.0 Breaking Changes](2026-01-20.md#bug-fixes---swashbuckle-1010--net-100-breaking-changes)**
- Swashbuckle 10.1.0 breaking changes (5 files)
- .NET 10.0 breaking changes (ClaimsPrincipal DI, IActionContextAccessor deprecation)
- XML documentation generation enabled globally (3 files)
- Status: ✅ Complete and verified

**[Bug Fixes - MSTest ExpectedExceptionAttribute Conversion](2026-01-15.md#bug-fixes---mstest-expectedexceptionattribute-conversion)**
- Converted 40 test instances from `[ExpectedException]` to `Assert.ThrowsException<T>()`
- 24 files across Framework, Binary Decoders, and SharedFramework
- Status: ✅ Complete

**[Bug Fixes - Phase 0 Critical Bugs](2026-01-15.md#bug-fixes---phase-0-critical-bugs)**
- Fixed 6 critical bugs (syntax errors, stubs, precision issues)
- Created NumericAsserts utility
- Status: ✅ Complete

---

## How to Use

### When Archiving Completed Work

1. **Add a section** to today's daily file in this directory with detailed information
2. **Update TODO files** - Remove detailed information, add link to change document
3. **Update CLAUDE.md** - Move from "Current Work" to "Recently Completed Work" with link
4. **Update this README** - Add the day to "By day" if new, and an entry to "Current Change Documents"

### When Referencing Archived Work

- Link from TODO files: a "Details" link whose target is the change document path under `docs/changes/`
- Link from CLAUDE.md: a "**Details:**" link with the same path as target and text
- Direct reference: Check this README for list of available documents

---

## Retention Policy

**Keep Indefinitely:**
- All change documents are permanent records
- No automatic deletion
- May be consolidated into annual summaries if directory becomes too large

**Cleanup Criteria (Future):**
- After 1 year, consider consolidating into yearly summaries
- Never delete - only consolidate
- Maintain links from TODO files even after consolidation

---

## Related Documentation

- [TODO.md](../../TODO.md) - Main project tracking
- [CLAUDE.md](../../CLAUDE.md) - Development guide
- [docs/architecture/](../architecture/) - Architecture documentation

---

**Last Updated:** 2026-01-21

**[Docs validation as MSTest tests](2026-10-22.md#docs-validation-as-mstest-tests)**
- `validate-docs.py` replaced by MSTest project `OoBDev.Docs.Tests` (PlantUML, links, captions)
- Status: ✅ Complete

**[CgiMiddleware design reconciled with code](2026-10-22.md#cgimiddleware-design-reconciled-with-code)**
- CGI design documents reconciled with the code; `PATH_TRANSLATED` now set
- Status: ✅ Complete

**[AsyncAPI design reconciled with the code](2026-10-22.md#asyncapi-design-reconciled-with-the-code)**
- AsyncAPI design documents reconciled with the code; unit tests added to the backlog
- Status: ✅ Complete

**[OpenAPI and Scalar design reconciled with the code](2026-10-22.md#openapi-and-scalar-design-reconciled-with-the-code)**
- OpenAPI and Scalar design documents reconciled with the code
- Status: ✅ Complete

**[Image embeddings design reconciled with the code](2026-10-22.md#image-embeddings-design-reconciled-with-the-code)**
- Image embeddings design documents reconciled with the code
- Status: ✅ Complete

**[AllMiniLmL6V2 design reconciled with the code](2026-10-22.md#allminilml6v2-design-reconciled-with-the-code)**
- AllMiniLmL6V2 design documents reconciled with the code; three planned items in the backlog
- Status: ✅ Complete

**[LlmCodeGen design reconciled with the code](2026-10-22.md#llmcodegen-design-reconciled-with-the-code)**
- LlmCodeGen design documents reconciled with the code; missing tests in the backlog
- Status: ✅ Complete

**[AsyncAPI and message resolver unit tests](2026-10-22.md#asyncapi-and-message-resolver-unit-tests)**
- AsyncAPI builder, contributor and message resolver unit tests; resolver flat-configuration bug fixed
- Status: ✅ Complete

**[Solution converted to .slnx](2026-10-22.md#solution-converted-to-slnx)**
- Solution converted from `.sln` to `.slnx`; docs rendering reuses the existing PlantUML container
- Status: ✅ Complete

**[Aspire host: SQL Server 2019 with the vector types](2026-10-22.md#aspire-host-sql-server-2019-with-the-vector-types)**
- Aspire example host runs SQL Server 2019 with the vector types deployed from the dacpac
- Status: ✅ Complete (host start not run)

**[Example UI design reconciled with the code](2026-10-22.md#example-ui-design-reconciled-with-the-code)**
- Example UI design documents reconciled with the code
- Status: ✅ Complete

**[SQLCLR aggregates: Merge fixed and covered by tests](2026-10-28.md#sqlclr-aggregates-merge-fixed-and-covered-by-tests)**
- SQLCLR aggregate Merge and double-matrix Read defects fixed; aggregate, matrix and serialization tests
- Status: ✅ Complete

**[LlmCodeGen Ollama live run (2026-10-28)](2026-10-28.md#llmcodegen-ollama-live-run-2026-10-28)**
- LlmCodeGen folder runner Integration test against the stack Ollama (phi3)
- Status: ✅ Complete

**[Integration test guard (2026-10-29)](2026-10-29.md#integration-test-guard-2026-10-29)**
- TestInstanceGuard refuses non-local connection strings in Integration tests
- Status: ✅ Complete

**[CGI latency gate (2026-10-29)](2026-10-29.md#cgi-latency-gate-2026-10-29)**
- CGI middleware latency gate (log 500 ms, fail 750 ms) on Windows and Linux
- Status: ✅ Complete

**[Platform primitives audit (2026-10-29)](2026-10-29.md#platform-primitives-audit-2026-10-29)**
- Audit of hand-built providers: only IGuidProvider remains and it stays
- Status: ✅ Complete

**[AsyncAPI contributor for Azure Storage queues (2026-10-29)](2026-10-29.md#asyncapi-contributor-for-azure-storage-queues-2026-10-29)**
- AsyncAPI contributor for Azure Storage queues
- Status: ✅ Complete

**[Search latency gate (2026-10-29)](2026-10-29.md#search-latency-gate-2026-10-29)**
- Latency gate for the search syntax (500 ms log, 750 ms fail)
- Status: ✅ Complete

**[Operations check in the security-audit skill (2026-10-29)](2026-10-29.md#operations-check-in-the-security-audit-skill-2026-10-29)**
- Operations check added to the security-audit skill
- Status: ✅ Complete

**[ExternalServices readme identifier check (2026-10-29)](2026-10-29.md#externalservices-readme-identifier-check-2026-10-29)**
- ExternalServices readmes identifier-checked against code
- Status: ✅ Complete

**[Keyed embedding generator (2026-10-29)](2026-10-29.md#keyed-embedding-generator-2026-10-29)**
- AllMiniLmL6V2 embedding generator also registered under its key
- Status: ✅ Complete

**[OData query surface (2026-10-29)](2026-10-29.md#odata-query-surface-2026-10-29)**
- OData query options on IQueryable actions, with limits, QUERY form bodies and tests
- Status: ✅ Complete

**[CGI symlink test (2026-10-29)](2026-10-29.md#cgi-symlink-test-2026-10-29)**
- CGI symlink-out-of-root test (REQ-007)
- Status: ✅ Complete

**[Build warnings clean (2026-10-29)](2026-10-29.md#build-warnings-clean-2026-10-29)**
- Last build warnings resolved or suppressed with reasons
- Status: ✅ Complete

**[Example Aspire host: the API uses the SQL vector store](2026-10-28.md#example-aspire-host-the-api-uses-the-sql-vector-store)**
- Example Aspire host gives the API the SQL vector store connection string
- Status: Built, not run end to end

**[CGI example: Linux build and an automated form test](2026-10-28.md#cgi-example-linux-build-and-an-automated-form-test)**
- CGI example built on Linux; automated form test through the middleware
- Status: ✅ Complete

**[CGI tests: control characters in headers and child kill on timeout](2026-10-28.md#cgi-tests-control-characters-in-headers-and-child-kill-on-timeout)**
- CGI tests for control characters in headers and child kill on timeout
- Status: ✅ Complete (Linux container)

**[`[AcceptsQuery]` attribute (Accept-Query header)](2026-10-28.md#acceptsquery-attribute-accept-query-header)**
- `[AcceptsQuery]` result filter adds the Accept-Query header
- Status: ✅ Complete

**[MailKit hosting registered in every build](2026-10-28.md#mailkit-hosting-registered-in-every-build)**
- MailKit hosting registered in every build (was DEBUG only)
- Status: ✅ Complete

**[Aspire host for the integration test stack, and Unit test re-categorisation](2026-10-28.md#aspire-host-for-the-integration-test-stack-and-unit-test-re-categorisation)**
- Aspire host for the integration test stack; wkhtmltopdf test moved to DevLocal; Tika URL from TestContext
- Status: ✅ Verified against the running stack (MongoDB tests fail on a code defect)

**[Example AppHost startup fixes](2026-10-28.md#example-apphost-startup-fixes)**
- Example AppHost: dacpac re-publish skipped, API binds the Aspire port, vector reader uses the Aspire database
- Status: ✅ Complete (full AppHost start not rerun)
- [Z-Wave core](2026-10-28.md#z-wave-core-oobdevzwave---2026-10-28) - first device package, verified on a Z-Stick (2026-10-28)
- [DE-5000 core](2026-10-28.md#de-5000-lcr-meter-decoding) - DE-5000 packet decoding, 13 unit tests, no hardware test yet (2026-10-28)
- [Busylight core](2026-10-28.md#busylight-status-light-and-the-hardwaredevices-folder) - Busylight commands, program frame and poll verified on the light; device packages moved to src/HardwareDevices (2026-10-28)
- [K8055 core](2026-10-28.md#k8055-experiment-board) - K8055 reports, commands and named input bits; Hardware tests ported, not yet run on a board (2026-10-28)
- [Radex One core](2026-10-28.md#radex-one-dosimeter) - Radex One packets, replies and requests; Hardware tests passed on a real device (2026-10-28)
- [Zoom H4n remote](2026-10-28.md#zoom-h4n-remote) - H4n status flags, button codes and handshake constants; Hardware test ported, not yet run on the adapter (2026-10-28)
- [NMEA 0183](2026-10-28.md#nmea-0183-decoding) - sentence segmenter, decoder, typed GGA/RMC/GSA/GSV/VTG; BT-20 Hardware test ported, not yet run on the receiver (2026-10-28)
- [SCPI instrument control](2026-10-28.md#scpi-instrument-control) - profile model and catalog (nine bundled profiles), command builder, reply segmenter and codecs; no instrument tested yet (2026-10-28)
- [Z-Wave id tables](2026-10-28.md#z-wave-id-tables) - embedded manufacturer, product, command class and device class names (2026-10-28)
- [Method length rule](2026-10-29.md#method-length-rule-ma0051-promoted-2026-10-29) - MA0051 is a warning; 26 long methods split, one sample suppressed (2026-10-29)
- [Vector search latency gate](2026-10-29.md#vector-search-latency-gate-2026-10-29) - SQL vector nearest-neighbour query over 1000 x 384 stays under 750 ms (2026-10-29)
- [Service Bus emulator coverage](2026-10-29.md#service-bus-emulator-coverage-2026-10-29) - connection string fixed, topic subscription and session queue round trips (2026-10-29)
- [Production-copy rehearsal](2026-10-29.md#production-copy-rehearsal-first-slice-2026-10-29) - backup, restore with a new broker id and query a throwaway copy; redeploy defect found (2026-10-29)
- [Local runner, Release build](2026-10-29.md#local-runner-verified-for-unit-and-simulate-2026-10-29) - runner verified for Unit, Simulate and Integration; Release-only MailKit using fixed (2026-10-29)
- [Service Bus readiness probe](2026-10-29.md#service-bus-emulator-readiness-probe-2026-10-29) - wait-for-services probes the AMQP port because the emulator has no shell (2026-10-29)
- [SCPI block replies](2026-10-29.md#scpi-definite-length-block-replies-2026-10-29) - definite-length block framing and ScpiReply.Block (2026-10-29)
- [Z-Wave ACK responder](2026-10-29.md#z-wave-ack-responder-2026-10-29) - ZWaveAckResponder picks ACK or NAK per received frame (2026-10-29)
- [Z-Wave session](2026-10-29.md#z-wave-session-2026-10-29) - ZWaveSession owns a stream device, acknowledges frames and keeps the node table (2026-10-29)
- [StreamDevice idle transmitter](2026-10-29.md#streamdevice-idle-transmitter-2026-10-29) - transmit queue is a Channel, so an idle device no longer spins a core (2026-10-29)
- [SCPI reply tracker](2026-10-29.md#scpi-reply-tracker-and-auto-detect-2026-10-29) - ScpiReplyTracker pairs queries with replies; ScpiAutoDetect identifies by *IDN? (2026-10-29)
- [Docs validators clean](2026-10-29.md#documentation-validators-clean-outside-generated-output-2026-10-29) - links, PlantUML blocks and table captions fixed in hand-written docs; sbom and generated remain (2026-10-29)
- [SCPI generated panel](2026-10-29.md#scpi-generated-control-panel-2026-10-29) - OoBDev.Scpi.Panels builds a form, model and binding from an instrument profile (2026-10-29)
- [TCP device transport](2026-10-29.md#tcp-device-transport-2026-10-29) - OoBDev.System.IO.Tcp: TcpDeviceAdapter, first phase 9 transport (2026-10-29)
- [Vectors DB redeploy](2026-10-29.md#vectors-database-redeploy-is-idempotent-2026-10-29) - second deploy of the dacpac changes nothing (2026-10-29)
- [LlmCodeGen response metadata](2026-10-29.md#llmcodegen-response-metadata) - response.json beside each response (2026-10-29)
- [DacFx build test path](2026-10-29.md#dacfx-buildpackagetest-path) - BuildPackageTest finds the CLR build itself and asserts the dacpac (2026-10-29)
- [AsyncAPI viewer browser check](2026-10-29.md#asyncapi-viewer-browser-check) - viewer renders send and receive operations in Chromium (2026-10-29)
- [Z-Wave product configs](2026-10-29.md#z-wave-product-configuration-table) - ZWaveNames.ConfigParameters from the embedded product table (2026-10-29)
- [UiDefinitions](2026-10-29.md#device-decoding-uidefinitions-2026-10-29) - OoBDev.UiDefinitions: toolkit-neutral control model, serializer, validator, expressions and forms from dev-term (2026-10-29)
- [UiDefinitions view models](2026-10-29.md#uidefinitions-view-models-2026-10-29) - OoBDev.UiDefinitions.ViewModels: toolkit-neutral view models and ICommand types over forms (2026-10-29)
- [UiDefinitions WPF view](2026-10-29.md#uidefinitions-wpf-view-2026-10-29) - OoBDev.UiDefinitions.Wpf: XAML FormView over the view models (2026-10-29)
- [UiDefinitions contracts](2026-10-29.md#uidefinitions-contracts-move-to-systemabstractions-2026-10-29) - UiDefinition model and control-surface contracts moved to System.Abstractions; WPF views namespace UiDefinitionsWpf (2026-10-29)
- [DevTerm WPF shell](2026-10-29.md#devterm-wpf-shell-skeleton-2026-10-29) - OoBDev.DevTerm.Wpf: shell skeleton with panel factories and a demo panel (2026-10-29)
- [DevTerm WPF captures and layout review (2026-11-03)](2026-11-03.md#devterm-wpf-captures-and-layout-review-2026-11-03) - every WPF window captured for the guide and checked in both themes; three theme and layout defects fixed
- [Drawing on SkiaSharp (2026-11-03)](2026-11-03.md#drawing-on-skiasharp-2026-11-03) - Code39, MultiScaleTile and PngPack on SkiaSharp; no System.Drawing.Common, tests run on any platform
- [Applications layout (2026-11-04)](2026-11-04.md#applications-layout-2026-11-04) - the 13 DevTerm projects moved under src/Applications/DevTerm/
- [Tui guide determinism (2026-11-04)](2026-11-04.md#tui-guide-determinism-2026-11-04) - Tui tests no longer rewrite the manifest-editor and plugin-approval guide images
- [OoBDev.Documents.ExternalTools library (2026-11-04)](2026-11-04.md#oobdevdocumentsexternaltools-library) - external tool conversion handler
- [HP-GL conversion moved to OoBDev.Documents.Hpgl](2026-11-04.md#hp-gl-conversion-moved-to-oobdevdocumentshpgl) - Framework library with a document conversion handler
- [Flaky tests fixed](2026-11-04.md#flaky-tests-fixed) - StreamDevice cancellation race and the CGI latency gate
- [Bench Hardware tests (2026-11-04)](2026-11-04.md#bench-hardware-tests-2026-11-04) - Korad supply tests and the DevTerm CLI identify tests on the shared bench parameters
- [Coding-standards tests (2026-11-04)](2026-11-04.md#coding-standards-tests-2026-11-04) - repository-wide test category check; two untagged or double-tagged test classes fixed
- [DevTerm containers wiring (2026-11-04)](2026-11-04.md#devterm-containers-wired-into-the-integration-stack-2026-11-04) - mosquitto, ser2net, loopback-device and RabbitMQ STOMP/TLS in the integration stack
- [DevTerm docs: transports (2026-11-04)](2026-11-04.md#devterm-docs-transports-folded-into-a-reusable-design-2026-11-04) - four transport designs moved to `docs/design/Transports/`
- [DevTerm docs: reusable designs (2026-11-04)](2026-11-04.md#devterm-docs-reusable-designs-folded-2026-11-04) - UI definitions, presenters, session logging and theming moved to `docs/design/`
- [DevTerm CLI loopback-device test (2026-11-04)](2026-11-04.md#devterm-cli-test-against-the-loopback-device-container-2026-11-04) - TCP CLI path tested against the stack container
- [DevTerm manifest-panel layout test (2026-11-04)](2026-11-04.md#devterm-tui-manifest-panel-layout-test-2026-11-04) - Tui layout check for the manifest device panel
- [DevTerm docs: plugin model, connection profiles, multi-session UI (2026-11-04)](2026-11-04.md#devterm-docs-plugin-model-connection-profiles-multi-session-ui-2026-11-04) - three more designs folded into docs/design
- [DevTerm docs: proposals (2026-11-04)](2026-11-04.md#devterm-docs-proposals-folded-into-designs-and-protocols-2026-11-04) - proposals folded into docs/design and docs/protocols
- [DevTerm docs: features and rejected (2026-11-04)](2026-11-04.md#devterm-docs-features-and-rejected-folded-2026-11-04) - feature designs folded into docs/design, rejected protocols into docs/protocols/rejected
- [Web guide image zones (2026-11-04)](2026-11-04.md#web-guide-images-stop-churning) - volatile regions masked so reruns leave no image churn
- [Temp directories from ITempFileFactory (2026-11-04)](2026-11-04.md#temp-directories-from-itempfilefactory) - `GetTempDirectory()` with auto-cleanup; caller migration backlogged
- [DevTerm web host API guide (2026-11-04)](2026-11-04.md#devterm-web-host-api-guide) - REST and streaming guides generated from the OpenAPI and AsyncAPI documents
- [Device binary-format guide (2026-11-04)](2026-11-04.md#device-binary-format-guide) - field tables generated from the device `.ksy` files
- [Web flow re-check (2026-11-04)](2026-11-04.md#web-playwright-flow-re-checked) - one-off failure not reproduced in three runs
- [LXI discovery scan test (2026-11-04)](2026-11-04.md#lxi-discovery-scan-test) - scan covered against loopback fakes
- [Whole-solution restore (2026-11-04)](2026-11-04.md#whole-solution-restore-works-again) - aggregator no longer references the Windows-only Bluetooth project
- [DevTerm device notes with the device libraries](2026-11-04.md#devterm-device-notes-live-with-the-device-libraries) - copies deleted, stale pointers repointed
- [BinaryDataDecoders surroundings review (2026-11-04)](2026-11-04.md#binarydatadecoders-surroundings-review-2026-11-04) - bug reports kept as history, pipeline and GitHub items rejected with reasons
- [WPF Notes section state (2026-11-04)](2026-11-04.md#wpf-notes-section-remembers-its-state) - the Notes expander remembers collapsed, shared with the Tui
- [mDNS Ser2NetWin bridges (2026-11-04)](2026-11-04.md#mdns-discovery-lists-ser2netwin-bridges) - the mDNS probe finds _ser2net._tcp and _zwave._tcp bridges
- [WPF panel layout review (2026-11-04)](2026-11-04.md#wpf-layout-review-covers-a-code-built-control-panel) - main window with a generated SCPI panel checked in both themes
- [Shared FakeTransport (2026-11-03)](2026-11-03.md#shared-faketransport-2026-11-03) - four FakeTransport copies merged into OoBDev.DevTerm.TestUtilities
- [Docs check for imported material (2026-11-03)](2026-11-03.md#docs-check-for-imported-material-2026-11-03) - orphaned files under `docs/applications` fail the docs tests; default scope widened
- [DevTerm Web shell](2026-10-29.md#devterm-web-shell-and-connection-editor-2026-10-29) - OoBDev.DevTerm.Web (Blazor), connection editor view model, transport selection in AddDevTermFrontEnd (2026-10-29)
- [DevTerm Tui shell](2026-10-29.md#devterm-tui-shell-2026-10-29) - OoBDev.DevTerm.Tui (Terminal.Gui) and its tests, LxiDeviceScanner (2026-10-29)
- [Device bench notes](2026-10-29.md#device-bench-notes-carried-from-dev-term) - dev-term known-configuration, .ksy and instrument manuals moved beside the packages (2026-10-29)
- [JWS reference](2026-10-29.md#jws-reference-implementation) - OoBDev.Identity.Jws: HS256 and RS256 sign and verify from RFC 7515 (2026-10-29)
- [SQL Server Service Broker](2026-10-29.md#sql-server-service-broker-queue-provider) - OoBDev.Microsoft.SqlServer.ServiceBroker: queue provider on Service Broker (2026-10-29)
- [BinaryDataDecoders Drawing (2026-10-29)](2026-10-29.md#binarydatadecoders-drawing) - Code 39, JPEG segments, PNG packing, Z-order curve and tile pyramid in `OoBDev.Extensions.Drawing`
- [BinaryDataDecoders LanC (2026-10-29)](2026-10-29.md#binarydatadecoders-lanc-command-tables-2026-10-29) - Sony LANC command and status tables in `OoBDev.LanC`
- [BinaryDataDecoders Xslt CLI (2026-10-29)](2026-10-29.md#binarydatadecoders-xslt-cli-2026-10-29) - `oobdev-xslt` tool with `SandboxedTransformer` over XML, HTML, JSON, YAML, C#, VB, binlog, DacPac and path inputs
- [BinaryDataDecoders PackMan CLI (2026-10-29)](2026-10-29.md#binarydatadecoders-packman-cli-2026-10-29) - `oobdev-packman` tool that updates Directory.Packages.props and project package versions
- [BinaryDataDecoders Fencing score machines (2026-10-29)](2026-10-29.md#binarydatadecoders-fencing-score-machines-2026-10-29) - Favero and Saint George frame decoders in `OoBDev.Fencing.ScoreMachines`
- [BinaryDataDecoders Code analysis (2026-10-29)](2026-10-29.md#binarydatadecoders-code-analysis) - Roslyn C#/VB, DacPac and MSBuild binlog XPath navigators in `OoBDev.Extensions.CodeAnalysis*`
- [BinaryDataDecoders NMEA helpers (2026-10-29)](2026-10-29.md#binarydatadecoders-nmea-helpers) - bearing, compass point and coordinate formatting in `OoBDev.Nmea`
- [BinaryDataDecoders Numerics (2026-10-29)](2026-10-29.md#binarydatadecoders-numerics) - byte, engineering and scientific notation numbers in `OoBDev.System`
- [BinaryDataDecoders re-review against current main (2026-10-29)](2026-10-29.md#binarydatadecoders-re-review) - refreshed comparison, gaps and order
- [Plugin sandbox threat model](2026-10-29.md#plugin-sandbox-threat-model) - phase 0: threats, PoC escape matrix review and gaps (2026-10-29)
- [Documentation style guide](2026-10-29.md#documentation-style-guide-2026-10-29) - numbering and naming rules, readme renames, names check in Docs.Tests (2026-10-29)
- [Migration notes inventory (2026-10-29)](2026-10-29.md#migration-notes-inventory-2026-10-29) - README as an inventory; BotChat and MailKit notes removed
- [Manual CI and packaging on demand (2026-10-29)](2026-10-29.md#manual-ci-and-packaging-on-demand-2026-10-29) - pack only on `dotnet pack`, output `G:\nuget`, workflows removed
- [Example AppHost SQL password and queue wiring (2026-10-29)](2026-10-29.md#example-apphost-fixed-sql-password-and-queuemail-wiring-2026-10-29) - fixed sa password, new volume, SMTP and RabbitMQ config from the host
- [DevTerm engine contracts (2026-10-29)](2026-10-29.md#devterm-engine-contracts) - `OoBDev.DevTerm.Core`: `ITransport`, `IPresenter`, pump and `StreamTransport` over `IDeviceAdapter`
- [DevTerm loopback transport (2026-10-29)](2026-10-29.md#devterm-loopback-transport) - `OoBDev.DevTerm.Transports.Loopback`: scripted in-process `ITransport` for hardware-free sessions and shells
- [DevTerm text presenters (2026-10-29)](2026-10-29.md#devterm-text-presenters) - `OoBDev.DevTerm.Presenters.Text`: ascii, utf8, hex, decimal, octal, binary
- [DevTerm session engine core (2026-10-29)](2026-10-29.md#devterm-session-engine-core) - `Session`, `Pipeline`, limits, observers and telemetry in `OoBDev.DevTerm.Core`
- [DevTerm message router (2026-10-29)](2026-10-29.md#devterm-message-router) - `MessageRouter`, routing rules and sink in `OoBDev.DevTerm.Core`
- [DevTerm stream content detection (2026-10-29)](2026-10-29.md#devterm-stream-content-detection) - watcher, sniffer, end finder and HP-GL to SVG in `OoBDev.DevTerm.Core`
- [DevTerm reply presenter and home folder (2026-10-29)](2026-10-29.md#devterm-reply-presenter-and-home-folder) - `LineReplyPresenter`, `IReplyTracker`, `DevTermHome`
- [DevTerm plugins and presenter catalog (2026-10-29)](2026-10-29.md#devterm-plugins-and-presenter-catalog) - plugin loader, trust, out-of-process presenters, sample plugin
- [DevTerm control contracts and transport helpers (2026-10-29)](2026-10-29.md#devterm-control-contracts-and-transport-helpers) - control surface, panel, config editor, write delay
- [DevTerm session servers and clients (2026-10-29)](2026-10-29.md#devterm-session-servers-and-clients) - pipe, control pipe, TCP share, HTTP control
- [DevTerm KeyValue plugin (2026-10-29)](2026-10-29.md#devterm-keyvalue-plugin) - structured-message example plugin
- [DevTerm observability (2026-10-29)](2026-10-29.md#devterm-observability) - OTLP exporter
- [DevTerm device manifests (2026-10-29)](2026-10-29.md#devterm-device-manifests) - manifests, frames, .ksy import, presenters
- [DevTerm configuration (2026-10-29)](2026-10-29.md#devterm-configuration) - options, profiles, routing, discovery, session builder
- [DevTerm schemas (2026-10-29)](2026-10-29.md#devterm-schemas) - JSON Schema generator and drift test
- [DevTerm TCP and serial transports (2026-10-29)](2026-10-29.md#devterm-tcp-and-serial-transports) - pipe transports for sockets and COM ports
- [DevTerm RFC 2217 and VXI-11 transports (2026-10-29)](2026-10-29.md#devterm-rfc-2217-and-vxi-11-transports) - network serial and ONC-RPC instrument transports
- [Telemetry library and user accessor (2026-10-29)](2026-10-29.md#telemetry-library-and-user-accessor-2026-10-29) - OoBDev.Telemetry in Framework, ICurrentUserAccessor.User
- [Pipe-based device adapter (2026-10-29)](2026-10-29.md#pipe-based-device-adapter-2026-10-29) - IDeviceAdapter exposes PipeReader/PipeWriter, state and events; StreamDeviceAdapter base
- [Transports become device adapters (2026-10-29)](2026-10-29.md#transports-become-device-adapters-2026-10-29) - ITransport deleted; transports implement IDeviceAdapter with Output via DeviceAdapterOutput
- [K8055 and Busylight panels with their protocols (2026-10-29)](2026-10-29.md#k8055-and-busylight-panels-with-their-protocols-2026-10-29) - IDevicePanel contracts, panels in the protocol libraries, TryAddDevicePanel and the OoBDev.DevTerm.Devices bundle
- [SCPI presenter and instrument panel in OoBDev.DevTerm.Devices (2026-10-29)](2026-10-29.md#scpi-presenter-and-instrument-panel-in-oobdevdevtermdevices-2026-10-29) - scpi presenter, auto-detect, control surface and provider registered by AddDevTermDevicePanels; 73 Unit tests
- [Windows BLE backend copied next to the Tui and Web apps (2026-10-29)](2026-10-29.md#windows-ble-backend-copied-next-to-the-tui-and-web-apps-2026-10-29) - CopyWindowsBleBackend target in both csproj files
- [Tui tests for the device panels restored (2026-10-29)](2026-10-29.md#tui-tests-for-the-device-panels-restored-2026-10-29) - built-in panels in the test runner, SCPI profile CLI test, plugin panel menu test, panel layout tests
- [DeviceConfigViewModel tests (2026-10-29)](2026-10-29.md#deviceconfigviewmodel-tests-2026-10-29) - 5 Unit tests for the shared Configure device flow
- [LxiDeviceOption tests (2026-10-29)](2026-10-29.md#lxideviceoption-tests-2026-10-29) - 6 Unit tests for the discovery-hit to picker-option mapping
- [DevTerm one type per file (2026-10-29)](2026-10-29.md#devterm-one-top-level-type-per-file-2026-10-29) - MA0048 splits across the DevTerm Core, Web, Tui and test projects
- [Tui doc images go to artifacts (2026-10-29)](2026-10-29.md#tui-screenshot-tests-no-longer-write-into-the-docs-2026-10-29) - screenshot tests stop writing into docs/user-guide/images
- [Bench instrument identify tests (2026-10-30)](2026-10-30.md#bench-instrument-identify-tests-2026-10-30) - Hardware tests for the HP 34401A, TDS2024 and 2230 over their serial bridges (CR/LF and per-byte delay settings)
- [DevTerm parity design documents (2026-10-29)](2026-10-29.md#devterm-parity-design-documents-2026-10-29) - requirements, api-design and testing-strategy drafts
- [DevTerm Core device panel gating (2026-10-29)](2026-10-29.md#devterm-core-drops-the-k8055-and-busylight-gating-2026-10-29) - K8055 and Busylight ids leave Core; the panels own them
- [RFC 2217 adapter open-path tests (2026-10-29)](2026-10-29.md#rfc-2217-adapter-open-path-tests-2026-10-29) - loopback tests for open, fault and close
- [DevTerm schema generator entry point (2026-10-29)](2026-10-29.md#devterm-schema-generator-entry-point-2026-10-29) - WriteTo and the Tui --write-schemas flag
- [DevTerm BLE platform loader test (2026-10-29)](2026-10-29.md#devterm-ble-platform-loader-test-2026-10-29) - loader degrades gracefully; stale backlog line removed
- [USBTMC hardware tests (2026-10-29)](2026-10-29.md#usbtmc-hardware-tests-2026-10-29) - two Hardware tests, Inconclusive without a device
- [DevTerm routing link factory (2026-10-29)](2026-10-29.md#devterm-routing-link-factory-2026-10-29) - default IRoutingLinkFactory for MQTT, AMQP and STOMP
- [DevTerm WPF DI composition (2026-10-29)](2026-10-29.md#devterm-wpf-di-composition-2026-10-29) - the WPF shell composes core, device panels and shell through DI
- [DevTerm WPF session view model (2026-10-29)](2026-10-29.md#devterm-wpf-session-view-model-2026-10-29) - connect, disconnect, send and output over a Session
- [DevTerm WPF profile sessions (2026-10-29)](2026-10-29.md#devterm-wpf-profile-sessions-2026-10-29) - the shell opens sessions from saved connection profiles
- [DevTerm WPF connection editor window (2026-10-29)](2026-10-29.md#devterm-wpf-connection-editor-window-2026-10-29) - New connection dialog over ConnectionEditorViewModel
- [DevTerm WPF routing window (2026-10-29)](2026-10-29.md#devterm-wpf-routing-window-2026-10-29) - Routing... opens a window over RoutingViewModel for the selected session
- [DevTerm WPF stream monitor window (2026-10-29)](2026-10-29.md#devterm-wpf-stream-monitor-window-2026-10-29) - Stream monitor... tracks the selected session and lists captures
- [DevTerm WPF merged log window (2026-10-29)](2026-10-29.md#devterm-wpf-merged-log-window-2026-10-29) - Merged log... lists every opened session in time order
- [DevTerm WPF playback window (2026-10-29)](2026-10-29.md#devterm-wpf-playback-window-2026-10-29) - Open log for playback... replays a session log with transport commands
- [DevTerm WPF device panels (2026-10-30)](2026-10-30.md#devterm-wpf-device-panels-2026-10-30) - plugin device panels open from the shell through a generated model and the control surface
- [DevTerm WPF session output controls (2026-10-30)](2026-10-30.md#devterm-wpf-session-output-controls-2026-10-30) - echo sent commands and clear output per session
- [DevTerm WPF serial fields (2026-10-30)](2026-10-30.md#devterm-wpf-serial-fields-in-the-connection-editor-2026-10-30) - baud, data bits, parity, stop bits and flow control in the connection editor
- [DevTerm WPF command previews (2026-10-30)](2026-10-30.md#devterm-wpf-device-panel-command-previews-2026-10-30) - device panel buttons show what they send in the tooltip
- [DevTerm WPF session logging (2026-10-30)](2026-10-30.md#devterm-wpf-session-logging-2026-10-30) - Log to file toggle per session
- [DevTerm WPF theme (2026-10-30)](2026-10-30.md#devterm-wpf-theme-2026-10-30) - WpfTheme, dark control templates and a theme chooser in the shell
- [DevTerm WPF converter tools window (2026-10-30)](2026-10-30.md#devterm-wpf-converter-tools-window-2026-10-30) - edit, reorder and save the stream monitor converter tools from the shell
- [DevTerm WPF device manifest picker (2026-10-30)](2026-10-30.md#devterm-wpf-device-manifest-picker-2026-10-30) - pick or browse a manifest and open its panel on the session
- [DevTerm WPF theme builder (2026-10-30)](2026-10-30.md#devterm-wpf-theme-builder-2026-10-30) - seed picker, colour role editor with live preview, save as a user theme
- [DevTerm WPF Configure device window (2026-10-30)](2026-10-30.md#devterm-wpf-configure-device-window-2026-10-30) - editor, host and port, read, edit, review changes, write
- [DevTerm WPF instrument picker (2026-10-30)](2026-10-30.md#devterm-wpf-instrument-picker-2026-10-30) - provider buttons, choice picker, auto-detect, panel
- [DevTerm WPF manifest editor (2026-10-30)](2026-10-30.md#devterm-wpf-manifest-editor-2026-10-30) - outline, forms, live preview, file seams
- [DevTerm WPF expression picker (2026-10-30)](2026-10-30.md#devterm-wpf-expression-picker-2026-10-30) - picker window, per-field picker buttons in the manifest editor
- [DevTerm WPF SVG preview (2026-10-30)](2026-10-30.md#devterm-wpf-svg-preview-2026-10-30) - SvgPreview renderer for the SVG subset
- [DevTerm WPF stream monitor preview and export (2026-10-30)](2026-10-30.md#devterm-wpf-stream-monitor-preview-and-export-2026-10-30) - capture preview, details and Export as
- [DevTerm WPF profile management (2026-10-30)](2026-10-30.md#devterm-wpf-profile-management-2026-10-30) - multi-select, export, import, replace all, confirmations
- [One USB HID adapter (2026-10-29)](2026-10-29.md#one-usb-hid-adapter-2026-10-29) - UsbHidFactory returns the endpoint adapter; the thin one is gone
- [DevTerm WPF log path choice and notes (2026-10-30)](2026-10-30.md#devterm-wpf-log-path-choice-and-log-notes-2026-10-30) - save dialog for the log file, Add note command while logging
- [DevTerm test plugin rename (2026-10-30)](2026-10-30.md#devterm-sample-plugin-renamed-to-a-test-fixture-2026-10-30) - Sample plugin is now OoBDev.DevTerm.Core.TestPlugin
- [DevTerm shared test base (2026-10-30)](2026-10-30.md#devterm-shared-user-flow-test-base-2026-10-30) - OoBDev.DevTerm.TestUtilities holds UserFlowTestsBase and IFrontEndDriver
- [DevTerm WPF menu bar and Theme menu (2026-10-30)](2026-10-30.md#devterm-wpf-menu-bar-and-theme-menu-2026-10-30) - File, Device and View menus; checkable Theme submenu
- [DevTerm WPF profile zip Browse (2026-10-30)](2026-10-30.md#devterm-wpf-browse-and-save-as-for-the-profile-zip-2026-10-30) - Browse and Save as for the import and export path
- [DevTerm WPF routing rule editing (2026-10-30)](2026-10-30.md#devterm-wpf-routing-rule-editing-and-history-2026-10-30) - rule list editing, sample test and history in the routing window
- [Z-Wave Hardware tests open the adapter (2026-10-30)](2026-10-30.md#z-wave-hardware-tests-open-the-adapter-2026-10-30) - the Z-Stick tests pass against real hardware
- [DevTerm surroundings and documentation layout (2026-10-30)](2026-10-30.md#devterm-surroundings-and-documentation-layout-2026-10-30) - application docs under docs/applications, protocols moved, surroundings review recorded
- [image-mask tool (2026-10-30)](2026-10-30.md#image-mask-tool-2026-10-30) - dotnet tool that masks zones from a JSON file, diffs and syncs guide images
- [WPF theme start-up (2026-10-30)](2026-10-30.md#wpf-theme-start-up-2026-10-30) - `--theme`, saved preference, persisted choices and system-theme refresh in the WPF app
- [WPF software flow control toggle (2026-10-30)](2026-10-30.md#wpf-software-flow-control-toggle-2026-10-30) - View menu XON/XOFF toggle on TCP sessions
- [WPF connection editor device pickers (2026-10-30)](2026-10-30.md#wpf-connection-editor-device-pickers-2026-10-30) - per-transport fields, detected ports, LAN, HID, USBTMC and BLE pickers with scan commands
- [Demo device plugin and Web flows (2026-10-30)](2026-10-30.md#demo-device-plugin-and-web-flows-2026-10-30) - `OoBDev.DevTerm.Devices.Demo` migrated; the Web panel flow passes again
- [Guide screenshots through image-mask sync (2026-10-30)](2026-10-30.md#guide-screenshots-through-image-mask-sync-2026-10-30) - Tui and Web guide captures only replace committed images when they differ once masked
- [WPF screenshot capture (2026-10-30)](2026-10-30.md#wpf-screenshot-capture-2026-10-30) - `WpfScreenshot` through `GuideImage`; connection editor captured
- [WPF field-following previews (2026-10-30)](2026-10-30.md#wpf-field-following-previews-2026-10-30) - a button tooltip's Sends line recomputes as its parameter fields change
- [Guide renderer moved to OoBDev.Documents.Guides (2026-11-04)](2026-11-04.md#guide-renderer-moved-to-oobdevdocumentsguides) - `ApiGuideRenderer` is now a Framework library with its own tests
- [Command-line tool guides generated (2026-11-04)](2026-11-04.md#command-line-tool-guides-generated) - xslt, packman and image-mask guides in docs/applications/tools
