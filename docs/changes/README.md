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

**[documentation-configuration-settings-2026-01-21.md](documentation-configuration-settings-2026-01-21.md)**
- CONFIGURATION_SETTINGS.md created (157+ settings)
- 31 Options classes, 24 direct keys, 102 environment variables
- New protocol: configuration-documentation.md
- Status: ✅ Complete

### Testing

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

- Link from TODO files: `[Details](docs/changes/{document}.md)`
- Link from CLAUDE.md: `**Details:** [docs/changes/{document}.md](docs/changes/{document}.md)`
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
- [TODO-bug-fixes.md](../../TODO-bug-fixes.md) - Active bug tracking
- [CLAUDE.md](../../CLAUDE.md) - Development guide
- [docs/architecture/](../architecture/) - Architecture documentation

---

**Last Updated:** 2026-01-21
