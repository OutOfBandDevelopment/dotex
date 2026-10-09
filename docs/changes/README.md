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
