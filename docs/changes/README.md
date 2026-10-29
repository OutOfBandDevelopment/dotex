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

```
{epic}-{feature}-{date}.md
```

**Examples:**
- `bug-fixes-swashbuckle-dotnet10-2026-01-20.md` - Bug fixes epic, Swashbuckle feature, completed 2026-01-20
- `testing-docker-infrastructure-2026-01-19.md` - Testing epic, Docker infrastructure, completed 2026-01-19
- `migration-sharedframework-phase0-2026-01-15.md` - Migration epic, SharedFramework phase 0, completed 2026-01-15

---

## Document Structure

Each change document should include:

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

## Current Change Documents

### Migrations

**[migration-caching-framework-2026-01-20.md](migration-caching-framework-2026-01-20.md)**
- Complete Caching framework migration from SharedFramework (4 implementation + 3 test projects)
- Enhanced StringFormatter with property chain support (unlimited depth)
- Added Redis to Docker integration testing infrastructure
- Comprehensive documentation (5 architecture docs)
- Status: ✅ Complete

**[migration-message-queues-2026-01-20.md](migration-message-queues-2026-01-20.md)**
- AWS SQS + Azure Service Bus providers
- LocalStack + Azure emulator integration
- Context-based pattern (non-generic)
- Status: ✅ Complete

**[sharedframework-phase0-2026-01-20.md](sharedframework-phase0-2026-01-20.md)**
- Namespace cleanup (27 directories renamed)
- Api. prefix removals, Azure reorganization, Contracts → Abstractions
- Status: ✅ Complete

### Documentation

**[todo-completed-items-2026-10-09.md](todo-completed-items-2026-10-09.md)**
- Completed Patterns Discovery tasks, banners and backlog items moved out of `TODO.md`
- Status: ✅ Complete

**[documentation-configuration-settings-2026-01-21.md](documentation-configuration-settings-2026-01-21.md)**
- CONFIGURATION_SETTINGS.md created (157+ settings)
- 31 Options classes, 24 direct keys, 102 environment variables
- New protocol: configuration-documentation.md
- Status: ✅ Complete

**[documentation-patterns-discovery-2026-09-30.md](documentation-patterns-discovery-2026-09-30.md)**
- Security, observability, resilience and AI/RAG practices and alternatives pages
- Worker/CLI recipe, generated project catalog (81 projects)
- Microsoft.Extensions.AI spike (6 tests) with migration recommendation
- Status: ✅ Complete

**[documentation-allminilml6v2-design-2026-10-07.md](documentation-allminilml6v2-design-2026-10-07.md)**
- Design set for replacing the `AllMiniLML6v2Sharp` fork with first-party, thread-safe ONNX embeddings
- Status: ✅ Complete (design only)

### API Documentation

**[migration-openapi-scalar-2026-10-09.md](migration-openapi-scalar-2026-10-09.md)**
- Swashbuckle replaced by `Microsoft.AspNetCore.OpenApi` transformers and the Scalar API reference; OAuth2 authorization code with PKCE; AsyncAPI is phase 2
- Status: ✅ Complete (phase 1)

**[migration-asyncapi-2026-10-09.md](migration-asyncapi-2026-10-09.md)**
- AsyncAPI 3.0 document and viewer for the message queues (`OoBDev.AsyncApi`, adapter contributors, `MapAsyncApi()`)
- Status: ✅ Complete

**[examples-cgi-go-openapi-2026-10-28.md](examples-cgi-go-openapi-2026-10-28.md)**
- Go CGI example in the example web API, CGI programs listed in OpenAPI, feature request folder
- Status: ✅ Complete (Linux build not run)

**[testing-cgi-roundtrip-2026-10-22.md](testing-cgi-roundtrip-2026-10-22.md)**
- CGI echo, Location, no-blank-line, shell-metacharacter and path-split tests
- Status: ✅ Complete

**[quality-pack-warnings-2026-10-22.md](quality-pack-warnings-2026-10-22.md)**
- NU5118 and NU5128 pack warnings removed
- Status: ✅ Complete

**[quality-analyzer-promotion-2026-10-22.md](quality-analyzer-promotion-2026-10-22.md)**
- MA0070, MA0084, MA0132, MA0144, MA0151, MA0158, MA0169 now warnings; sites fixed
- Status: ✅ Complete

**[testing-example-web-viewmodels-2026-10-22.md](testing-example-web-viewmodels-2026-10-22.md)**
- Vitest view-model tests for Communications, Current user, Queues and Templates
- Status: ✅ Complete

**[apis-rights-debug-logging-2026-10-22.md](apis-rights-debug-logging-2026-10-22.md)**
- Role-to-right transformation logs counts at Debug; `OoBDev:Authorization` documented
- Status: ✅ Complete

**[apis-cgi-right-checker-2026-10-22.md](apis-cgi-right-checker-2026-10-22.md)**
- CGI middleware decides 401/403 with the shared `IApplicationRightChecker`
- Status: ✅ Complete

**[documentation-example-ui-reconcile-2026-10-22.md](documentation-example-ui-reconcile-2026-10-22.md)**
- example-ui navigation design checked against the built pages
- Status: ✅ Complete

**[apis-openapi-scalar-reconcile-2026-10-22.md](apis-openapi-scalar-reconcile-2026-10-22.md)**
- OpenApiScalar OAuth2 sign-in and API info covered by Unit tests
- Status: ✅ Complete

**[tools-llmcodegen-provider-selection-2026-10-22.md](tools-llmcodegen-provider-selection-2026-10-22.md)**
- LlmCodeGen provider selection extracted and unit tested; provider names are case-insensitive
- Status: ✅ Complete

**[documentation-image-embeddings-reconcile-2026-10-22.md](documentation-image-embeddings-reconcile-2026-10-22.md)**
- ImageEmbeddings testing strategy matched to the real tests, with a requirement-coverage table
- Status: ✅ Complete

**[apis-asyncapi-send-messages-2026-10-22.md](apis-asyncapi-send-messages-2026-10-22.md)**
- AsyncAPI send operations with schemas for registered message types
- Status: ✅ Complete

**[migration-embedding-telemetry-2026-10-22.md](migration-embedding-telemetry-2026-10-22.md)**
- ActivitySource and Meter for the ONNX sentence embedding generator (span, duration, batch size, tokens)
- Status: ✅ Complete

**[documentation-cgi-design-reconcile-2026-10-22.md](documentation-cgi-design-reconcile-2026-10-22.md)**
- CgiMiddleware design compared with code; testing strategy now names real tests, gaps in BACKLOG
- Status: ✅ Complete (other designs still to compare)

**[apis-http-query-verb-2026-10-22.md](apis-http-query-verb-2026-10-22.md)**
- `UseHttpQueryVerb()`: HTTP `QUERY` (and `POST` + `X-HTTP-Method-Override`) routed as `GET` so one endpoint and one rights check serve all
- Status: ✅ Complete (first slice; OData and GraphQL wait on Q2)

**[apis-application-rights-2026-10-22.md](apis-application-rights-2026-10-22.md)**
- Roles mapped to `app__application_right` claims from configuration, one shared right checker, `[ApplicationRight]` returns 401 or 403
- Status: ✅ Complete (CGI move and Debug logging in BACKLOG)

**[apis-cgi-middleware-2026-10-22.md](apis-cgi-middleware-2026-10-22.md)**
- CGI/1.1 middleware for Kestrel (`OoBDev.AspNetCore.Cgi`): safe defaults, limits, built-in rights check
- Status: ✅ Complete (first cut)

**[code-quality-and-tests-2026-10-22.md](code-quality-and-tests-2026-10-22.md)**
- Analyzer cleanup, Azurite queue tests, search annotation example, local CI runner fix
- Status: ✅ Complete

### Observability

**[migration-opentelemetry-2026-10-09.md](migration-opentelemetry-2026-10-09.md)**
- Application Insights replaced by `OoBDev.OpenTelemetry` (OTLP), Grafana LGTM test container, DevLocal tests fixed
- Status: ✅ Complete

### AI

**[migration-allminilml6v2-embedder-2026-10-08.md](migration-allminilml6v2-embedder-2026-10-08.md)**
- First-party in-process all-MiniLM-L6-v2 embedder verified against Hugging Face; model downloaded on first use; fork and submodules removed
- Status: ✅ Complete (GitHub run not yet observed)

**[migration-embedding-presets-2026-10-08.md](migration-embedding-presets-2026-10-08.md)**
- MPNet and Nomic presets on the ONNX runner, compared with Hugging Face; shared hub cache; version policy
- Status: ✅ Complete (GitHub run not yet observed)

**[migration-image-embeddings-2026-10-08.md](migration-image-embeddings-2026-10-08.md)**
- In-process image embeddings and classification: ONNX runner, Skia decoder, DINOv2-small, ViT-base and CLIP presets compared with the Python models
- Status: ✅ Complete (GitHub run not yet observed)

### Testing

**[testing-live-integration-completed-2026-10-09.md](testing-live-integration-completed-2026-10-09.md)**
- Archived completed section of the live integration TODO (LiveIntegration category)
- Status: ✅ Complete

**[testing-local-integration-completed-2026-10-09.md](testing-local-integration-completed-2026-10-09.md)**
- Archived completed sections of the local integration TODO (Weeks 1 and 2, validation, script and health check fixes)
- Status: ✅ Complete

**[testing-vectors-sqs-moto-ci-2026-10-08.md](testing-vectors-sqs-moto-ci-2026-10-08.md)**
- CI restore fix, vector `Angle` correction and NULL-safe functions with SQL Server Integration tests
- Moto replaces LocalStack; SQS client and test fixes
- Status: ✅ Complete (GitHub run not yet observed)

**[testing-ollama-integration-2026-01-21.md](testing-ollama-integration-2026-01-21.md)**
- Ollama LLM inference (14th Docker service)
- 4 tests migrated, automated phi3 model setup
- Fixed Windows batch container detection
- Status: ✅ Complete

**[testing-docker-infrastructure-2026-01-19.md](testing-docker-infrastructure-2026-01-19.md)**
- Docker integration testing infrastructure (14 services)
- 23 tests migrated from DevLocal to Integration
- CI/CD pipeline, cross-platform scripts
- Status: ✅ Complete (Week 1 & 2) - ⏳ Awaiting local validation

### Bug Fixes & Technical Debt

**[bug-fixes-antlr-keycloak-2026-01-20.md](bug-fixes-antlr-keycloak-2026-01-20.md)**
- ANTLR cross-platform build fix (Windows absolute paths in generated files)
- Keycloak script feature configuration (--features=scripts, NOT upload-scripts)
- Nginx DNS resolver with variable-based proxy_pass (Docker 127.0.0.11)
- RabbitMQ port conflict resolution (5672→5673, removed extends)
- Documentation updates for troubleshooting all four issues
- Status: ✅ Complete and verified

**[bug-fixes-swashbuckle-dotnet10-2026-01-20.md](bug-fixes-swashbuckle-dotnet10-2026-01-20.md)**
- Swashbuckle 10.1.0 breaking changes (5 files)
- .NET 10.0 breaking changes (ClaimsPrincipal DI, IActionContextAccessor deprecation)
- XML documentation generation enabled globally (3 files)
- Status: ✅ Complete and verified

**[bug-fixes-mstest-expected-exception-2026-01-15.md](bug-fixes-mstest-expected-exception-2026-01-15.md)**
- Converted 40 test instances from `[ExpectedException]` to `Assert.ThrowsException<T>()`
- 24 files across Framework, Binary Decoders, and SharedFramework
- Status: ✅ Complete

**[bug-fixes-phase0-critical-2026-01-15.md](bug-fixes-phase0-critical-2026-01-15.md)**
- Fixed 6 critical bugs (syntax errors, stubs, precision issues)
- Created NumericAsserts utility
- Status: ✅ Complete

---

## How to Use

### When Archiving Completed Work

1. **Create change document** in this directory with detailed information
2. **Update TODO files** - Remove detailed information, add link to change document
3. **Update CLAUDE.md** - Move from "Current Work" to "Recently Completed Work" with link
4. **Update this README** - Add entry to "Current Change Documents" section

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

**[documentation-docs-validation-tests-2026-10-22.md](documentation-docs-validation-tests-2026-10-22.md)**
- `validate-docs.py` replaced by MSTest project `OoBDev.Docs.Tests` (PlantUML, links, captions)
- Status: ✅ Complete

**[documentation-cgi-design-reconcile-2026-10-22.md](documentation-cgi-design-reconcile-2026-10-22.md)**
- CGI design documents reconciled with the code; `PATH_TRANSLATED` now set
- Status: ✅ Complete

**[documentation-asyncapi-design-reconcile-2026-10-22.md](documentation-asyncapi-design-reconcile-2026-10-22.md)**
- AsyncAPI design documents reconciled with the code; unit tests added to the backlog
- Status: ✅ Complete

**[documentation-openapi-scalar-design-reconcile-2026-10-22.md](documentation-openapi-scalar-design-reconcile-2026-10-22.md)**
- OpenAPI and Scalar design documents reconciled with the code
- Status: ✅ Complete

**[documentation-image-embeddings-design-reconcile-2026-10-22.md](documentation-image-embeddings-design-reconcile-2026-10-22.md)**
- Image embeddings design documents reconciled with the code
- Status: ✅ Complete

**[documentation-allminilm-design-reconcile-2026-10-22.md](documentation-allminilm-design-reconcile-2026-10-22.md)**
- AllMiniLmL6V2 design documents reconciled with the code; three planned items in the backlog
- Status: ✅ Complete

**[documentation-llmcodegen-design-reconcile-2026-10-22.md](documentation-llmcodegen-design-reconcile-2026-10-22.md)**
- LlmCodeGen design documents reconciled with the code; missing tests in the backlog
- Status: ✅ Complete

**[testing-asyncapi-resolver-tests-2026-10-22.md](testing-asyncapi-resolver-tests-2026-10-22.md)**
- AsyncAPI builder, contributor and message resolver unit tests; resolver flat-configuration bug fixed
- Status: ✅ Complete

**[build-slnx-solution-2026-10-22.md](build-slnx-solution-2026-10-22.md)**
- Solution converted from `.sln` to `.slnx`; docs rendering reuses the existing PlantUML container
- Status: ✅ Complete

**[testing-aspire-sqlserver-vectors-2026-10-22.md](testing-aspire-sqlserver-vectors-2026-10-22.md)**
- Aspire example host runs SQL Server 2019 with the vector types deployed from the dacpac
- Status: ✅ Complete (host start not run)

**[documentation-example-ui-design-reconcile-2026-10-22.md](documentation-example-ui-design-reconcile-2026-10-22.md)**
- Example UI design documents reconciled with the code
- Status: ✅ Complete

**[data-vectors-aggregate-merge-2026-10-28.md](data-vectors-aggregate-merge-2026-10-28.md)**
- SQLCLR aggregate Merge and double-matrix Read defects fixed; aggregate, matrix and serialization tests
- Status: ✅ Complete

**[tools-llmcodegen-ollama-live-run-2026-10-28.md](tools-llmcodegen-ollama-live-run-2026-10-28.md)**
- LlmCodeGen folder runner Integration test against the stack Ollama (phi3)
- Status: ✅ Complete

**[testing-instance-guard-2026-10-29.md](testing-instance-guard-2026-10-29.md)**
- TestInstanceGuard refuses non-local connection strings in Integration tests
- Status: ✅ Complete

**[apis-cgi-latency-gate-2026-10-29.md](apis-cgi-latency-gate-2026-10-29.md)**
- CGI middleware latency gate (log 500 ms, fail 750 ms) on Windows and Linux
- Status: ✅ Complete

**[framework-platform-primitives-audit-2026-10-29.md](framework-platform-primitives-audit-2026-10-29.md)**
- Audit of hand-built providers: only IGuidProvider remains and it stays
- Status: ✅ Complete

**[apis-asyncapi-azure-storage-queue-2026-10-29.md](apis-asyncapi-azure-storage-queue-2026-10-29.md)**
- AsyncAPI contributor for Azure Storage queues
- Status: ✅ Complete

**[apis-search-latency-gate-2026-10-29.md](apis-search-latency-gate-2026-10-29.md)**
- Latency gate for the search syntax (500 ms log, 750 ms fail)
- Status: ✅ Complete

**[process-security-audit-operations-check-2026-10-29.md](process-security-audit-operations-check-2026-10-29.md)**
- Operations check added to the security-audit skill
- Status: ✅ Complete

**[documentation-externalservices-readme-check-2026-10-29.md](documentation-externalservices-readme-check-2026-10-29.md)**
- ExternalServices readmes identifier-checked against code
- Status: ✅ Complete

**[migration-embedding-keyed-generator-2026-10-29.md](migration-embedding-keyed-generator-2026-10-29.md)**
- AllMiniLmL6V2 embedding generator also registered under its key
- Status: ✅ Complete

**[apis-odata-query-2026-10-29.md](apis-odata-query-2026-10-29.md)**
- OData query options on IQueryable actions, with limits, QUERY form bodies and tests
- Status: ✅ Complete

**[apis-cgi-symlink-test-2026-10-29.md](apis-cgi-symlink-test-2026-10-29.md)**
- CGI symlink-out-of-root test (REQ-007)
- Status: ✅ Complete

**[build-warnings-clean-2026-10-29.md](build-warnings-clean-2026-10-29.md)**
- Last build warnings resolved or suppressed with reasons
- Status: ✅ Complete

**[examples-aspire-vectordb-wiring-2026-10-28.md](examples-aspire-vectordb-wiring-2026-10-28.md)**
- Example Aspire host gives the API the SQL vector store connection string
- Status: Built, not run end to end

**[examples-cgi-greet-linux-test-2026-10-28.md](examples-cgi-greet-linux-test-2026-10-28.md)**
- CGI example built on Linux; automated form test through the middleware
- Status: ✅ Complete

**[apis-cgi-tests-bad-header-kill-2026-10-28.md](apis-cgi-tests-bad-header-kill-2026-10-28.md)**
- CGI tests for control characters in headers and child kill on timeout
- Status: ✅ Complete (Linux container)

**[http-accepts-query-attribute-2026-10-28.md](http-accepts-query-attribute-2026-10-28.md)**
- `[AcceptsQuery]` result filter adds the Accept-Query header
- Status: ✅ Complete

**[hosting-mailkit-all-builds-2026-10-28.md](hosting-mailkit-all-builds-2026-10-28.md)**
- MailKit hosting registered in every build (was DEBUG only)
- Status: ✅ Complete

**[testing-aspire-test-stack-2026-10-28.md](testing-aspire-test-stack-2026-10-28.md)**
- Aspire host for the integration test stack; wkhtmltopdf test moved to DevLocal; Tika URL from TestContext
- Status: ✅ Verified against the running stack (MongoDB tests fail on a code defect)

**[examples-apphost-startup-fixes-2026-10-28.md](examples-apphost-startup-fixes-2026-10-28.md)**
- Example AppHost: dacpac re-publish skipped, API binds the Aspire port, vector reader uses the Aspire database
- Status: ✅ Complete (full AppHost start not rerun)
- [Z-Wave core](devices-zwave-core-2026-10-28.md) - first device package, verified on a Z-Stick (2026-10-28)
- [DE-5000 core](devices-de5000-core-2026-10-28.md) - DE-5000 packet decoding, 13 unit tests, no hardware test yet (2026-10-28)
- [Busylight core](devices-busylight-core-2026-10-28.md) - Busylight commands, program frame and poll verified on the light; device packages moved to src/HardwareDevices (2026-10-28)
- [K8055 core](devices-k8055-core-2026-10-28.md) - K8055 reports, commands and named input bits; Hardware tests ported, not yet run on a board (2026-10-28)
- [Radex One core](devices-radexone-core-2026-10-28.md) - Radex One packets, replies and requests; Hardware tests passed on a real device (2026-10-28)
- [Zoom H4n remote](devices-zoomh4n-core-2026-10-28.md) - H4n status flags, button codes and handshake constants; Hardware test ported, not yet run on the adapter (2026-10-28)
- [NMEA 0183](devices-nmea-core-2026-10-28.md) - sentence segmenter, decoder, typed GGA/RMC/GSA/GSV/VTG; BT-20 Hardware test ported, not yet run on the receiver (2026-10-28)
- [SCPI instrument control](devices-scpi-core-2026-10-28.md) - profile model and catalog (nine bundled profiles), command builder, reply segmenter and codecs; no instrument tested yet (2026-10-28)
- [Z-Wave id tables](devices-zwave-id-tables-2026-10-28.md) - embedded manufacturer, product, command class and device class names (2026-10-28)
- [Method length rule](quality-method-length-2026-10-29.md) - MA0051 is a warning; 26 long methods split, one sample suppressed (2026-10-29)
- [Vector search latency gate](testing-vector-search-latency-gate-2026-10-29.md) - SQL vector nearest-neighbour query over 1000 x 384 stays under 750 ms (2026-10-29)
- [Service Bus emulator coverage](testing-servicebus-emulator-2026-10-29.md) - connection string fixed, topic subscription and session queue round trips (2026-10-29)
- [Production-copy rehearsal](testing-production-copy-rehearsal-2026-10-29.md) - backup, restore with a new broker id and query a throwaway copy; redeploy defect found (2026-10-29)
- [Local runner, Release build](testing-local-runner-release-build-2026-10-29.md) - runner verified for Unit, Simulate and Integration; Release-only MailKit using fixed (2026-10-29)
- [Service Bus readiness probe](testing-servicebus-health-probe-2026-10-29.md) - wait-for-services probes the AMQP port because the emulator has no shell (2026-10-29)
- [SCPI block replies](devices-scpi-block-replies-2026-10-29.md) - definite-length block framing and ScpiReply.Block (2026-10-29)
- [Z-Wave ACK responder](devices-zwave-ack-responder-2026-10-29.md) - ZWaveAckResponder picks ACK or NAK per received frame (2026-10-29)
- [Vectors DB redeploy](data-vectors-db-redeploy-2026-10-29.md) - second deploy of the dacpac changes nothing (2026-10-29)
- [LlmCodeGen response metadata](tools-llmcodegen-response-metadata-2026-10-29.md) - response.json beside each response (2026-10-29)
- [DacFx build test path](testing-dacfx-buildpackage-path-2026-10-29.md) - BuildPackageTest finds the CLR build itself and asserts the dacpac (2026-10-29)
- [AsyncAPI viewer browser check](apis-asyncapi-viewer-browser-check-2026-10-29.md) - viewer renders send and receive operations in Chromium (2026-10-29)
- [Z-Wave product configs](devices-zwave-product-configs-2026-10-29.md) - ZWaveNames.ConfigParameters from the embedded product table (2026-10-29)
- [Device bench notes](docs-device-bench-notes-2026-10-29.md) - dev-term known-configuration, .ksy and instrument manuals moved beside the packages (2026-10-29)
- [JWS reference](identity-jws-reference-2026-10-29.md) - OoBDev.Identity.Jws: HS256 and RS256 sign and verify from RFC 7515 (2026-10-29)
- [SQL Server Service Broker](sqlserver-service-broker-2026-10-29.md) - OoBDev.Microsoft.SqlServer.ServiceBroker: queue provider on Service Broker (2026-10-29)
- [BinaryDataDecoders Drawing (2026-10-29)](migration-drawing-2026-10-29.md) - Code 39, JPEG segments, PNG packing, Z-order curve and tile pyramid in `OoBDev.Extensions.Drawing`
- [BinaryDataDecoders Fencing score machines (2026-10-29)](migration-fencing-2026-10-29.md) - Favero and Saint George frame decoders in `OoBDev.Fencing.ScoreMachines`
- [BinaryDataDecoders Code analysis (2026-10-29)](migration-code-analysis-2026-10-29.md) - Roslyn C#/VB, DacPac and MSBuild binlog XPath navigators in `OoBDev.Extensions.CodeAnalysis*`
- [BinaryDataDecoders NMEA helpers (2026-10-29)](migration-nmea-helpers-2026-10-29.md) - bearing, compass point and coordinate formatting in `OoBDev.Nmea`
- [BinaryDataDecoders Numerics (2026-10-29)](migration-numerics-2026-10-29.md) - byte, engineering and scientific notation numbers in `OoBDev.System`
- [BinaryDataDecoders re-review against current main (2026-10-29)](docs-binarydatadecoders-rereview-2026-10-29.md) - refreshed comparison, gaps and order
- [Plugin sandbox threat model](docs-plugin-sandbox-threat-model-2026-10-29.md) - phase 0: threats, PoC escape matrix review and gaps (2026-10-29)
