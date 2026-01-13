# SharedFramework Migration Plan

**Version:** 1.0
**Last Updated:** 2026-01-13
**Source:** Incomming/SharedFramework (52 projects, ~28,582 LOC)
**Target:** Main OoBDev (dotex) Framework
**Status:** 🔍 INVESTIGATION COMPLETE - Ready for execution

---

## Overview

This document provides a detailed migration plan for integrating 52 SharedFramework projects into the OoBDev framework. The projects provide critical external service integrations and complete implementations for partially-completed main framework components.

**Reference:** See [sharedframework-feature-mapping.md](./sharedframework-feature-mapping.md) for comprehensive feature comparison.

---

## Migration Scope

**ALL 52 projects will be migrated** - phases indicate priority order.

- **24 projects** provide NEW capabilities
- **10 projects** are ENHANCED versions of existing frameworks
- **18 projects** are test suites (migrate with their implementations)

**Migration Approach:** Systematic integration organized by capability area, not bulk copy-paste.

---

## Migration Principles

All migration work MUST follow `/docs/architecture` standards:

1. **Layer Placement** - ExternalServices for third-party, Framework for core, Extensions for specialized
2. **Provider Pattern** - All external services use provider/factory pattern
3. **Dependency Injection** - TryAdd* extensions, options pattern
4. **Framework Upgrade** - .NET 8.0 → .NET 9.0
5. **Namespace Cleanup** - Remove "Api." prefix where appropriate
6. **Testing** - 80% coverage minimum, migrate test projects
7. **Documentation** - README for each project, XML docs on public APIs

---

## Phase 0: Framework Upgrades (FOUNDATION)

**Priority:** CRITICAL
**Dependencies:** None
**Estimated Effort:** SMALL

### Task 0.1: Upgrade All Projects to .NET 9.0

**Current State:** All 52 projects target .NET 8.0

**Steps:**
1. Update all `.csproj` files: `<TargetFramework>net8.0</TargetFramework>` → `<TargetFramework>net9.0</TargetFramework>`
2. Update NuGet package versions to latest .NET 9-compatible versions
3. Test compilation after upgrade
4. Fix any breaking API changes

**Validation:**
- [ ] All 52 projects compile on .NET 9.0
- [ ] All tests pass after upgrade
- [ ] No deprecated API warnings

---

### Task 0.2: Namespace Cleanup Planning

**Current:** `OoBDev.Api.Twilio.SendGrid`, `OoBDev.Api.Census.Geocoding`
**Proposed:** `OoBDev.Twilio.SendGrid`, `OoBDev.Census.Geocoding`

**Steps:**
1. Create namespace mapping matrix
2. Use find/replace across all files
3. Update project names and directories
4. Update using statements

**Validation:**
- [ ] All "Api." prefixes removed
- [ ] Namespaces consistent with OoBDev conventions
- [ ] No broken references

---

## Phase 1: Message Queueing Providers (HIGH PRIORITY)

**Priority:** HIGH
**Dependencies:** Phase 0
**Estimated Effort:** MEDIUM

### Task 1.1: Migrate Amazon SQS

**Source:** `Incomming/SharedFramework/OoBDev.Amazon.Sqs/`
**Target:** `src/ExternalServices/Amazon/OoBDev.Amazon.Sqs/`
**LOC:** 183

**Steps:**
1. Create target directory structure
2. Copy source files to target
3. Update namespace: Keep `OoBDev.Amazon.Sqs`
4. Add project reference to OoBDev.sln under ExternalServices/Amazon
5. Add `AWSSDK.SQS` NuGet package
6. Update to .NET 9.0
7. Verify provider implements IMessageQueue or similar interface
8. Add ServiceCollectionExtensions with TryAddAmazonSqs()
9. Migrate test project: OoBDev.Amazon.Sqs.Tests
10. Create README.md with usage examples

**Validation:**
- [ ] Project compiles
- [ ] Tests pass
- [ ] Integrates with main MessageQueueing abstractions
- [ ] README complete

---

### Task 1.2: Migrate Azure Service Bus

**Source:** `Incomming/SharedFramework/OoBDev.Azure.ServiceBus/`
**Target:** `src/ExternalServices/Azure/OoBDev.Azure.ServiceBus/`
**LOC:** 114

**Steps:**
1. Create target directory structure
2. Copy source files
3. Add to OoBDev.sln under ExternalServices/Azure
4. Add `Azure.Messaging.ServiceBus` NuGet package
5. Update to .NET 9.0
6. Verify topics, sessions, dead-letter queue support
7. Add ServiceCollectionExtensions
8. Migrate test project
9. Create README.md

**Validation:**
- [ ] Project compiles
- [ ] Tests pass
- [ ] Supports queues, topics, sessions
- [ ] README complete

---

## Phase 2: Communications Complete Implementation (CRITICAL)

**Priority:** CRITICAL
**Dependencies:** Phase 1
**Estimated Effort:** LARGE

### Task 2.1: Merge OoBDev.Communications.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.Communications.Contracts/` (695 LOC)
**Target:** `src/Framework/OoBDev.Communications.Abstractions/` (125 LOC - REPLACE)
**Strategy:** REPLACE main with SharedFramework version

**Current Main Issues:**
- Only 125 LOC (basic models)
- Missing email/SMS contracts
- Missing multi-channel support
- Missing tracking/preferences

**SharedFramework Advantages:**
- 695 LOC (5.5x larger)
- Complete email and SMS contracts
- Channel management
- Tracking and preferences
- Attributes for configuration

**Steps:**
1. **Backup main version:** Create `OoBDev.Communications.Abstractions.backup/`
2. **Compare interfaces:** Ensure no breaking changes to existing main interfaces
3. **Replace implementation:**
   - Delete main's partial implementation
   - Copy SharedFramework contracts
   - Update namespace to `OoBDev.Communications` (match main)
4. **Merge unique main features** (if any exist)
5. **Update dependent projects** to use new contracts
6. **Migrate tests**
7. **Create migration guide** for users

**Validation:**
- [ ] No breaking changes to public APIs
- [ ] All existing main usages still compile
- [ ] SharedFramework contracts integrated
- [ ] Tests pass
- [ ] Migration guide created

---

### Task 2.2: Merge OoBDev.Communications

**Source:** `Incomming/SharedFramework/OoBDev.Communications/` (1,145 LOC)
**Target:** `src/Framework/OoBDev.Communications/` (16 LOC - REPLACE)
**Strategy:** REPLACE main stub with SharedFramework implementation

**Current Main:** Only ServiceCollectionExtensions registrar stub (16 LOC)

**SharedFramework:** Complete implementation (1,145 LOC)
- Composers (multi-channel orchestration)
- Handlers (email/SMS processing)
- Providers (SendGrid, Twilio integration points)
- Channel routing
- Preference management

**Steps:**
1. **Backup main stub**
2. **Replace with SharedFramework implementation**
3. **Update namespace** to match main
4. **Verify** all providers hook into abstraction layer correctly
5. **Migrate tests**
6. **Update ServiceCollectionExtensions** to register all new services
7. **Create comprehensive README**

**Validation:**
- [ ] Complete communications orchestration system
- [ ] Email and SMS channels working
- [ ] Tests pass
- [ ] README with examples

---

### Task 2.3: Migrate Twilio SendGrid Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Twilio.SendGrid/`
**Target:** `src/ExternalServices/Twilio/OoBDev.Twilio.SendGrid/`
**LOC:** 267

**Steps:**
1. Create target directory
2. Copy files, remove "Api." from namespace: `OoBDev.Twilio.SendGrid`
3. Add `SendGrid` NuGet package
4. Update to .NET 9.0
5. Integrate with OoBDev.Communications abstractions
6. Add ServiceCollectionExtensions
7. Migrate tests
8. Create README with SendGrid API key setup

**Validation:**
- [ ] Implements IEmailProvider or similar
- [ ] SendGrid email delivery works
- [ ] Tests pass
- [ ] README complete

---

### Task 2.4: Migrate Twilio SMS Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Twilio.SmsMessaging/`
**Target:** `src/ExternalServices/Twilio/OoBDev.Twilio.SmsMessaging/`
**LOC:** 151

**Steps:**
1. Create target directory
2. Copy files, update namespace
3. Add `Twilio` NuGet package
4. Update to .NET 9.0
5. Integrate with OoBDev.Communications abstractions
6. Add ServiceCollectionExtensions
7. Migrate tests
8. Create README

**Validation:**
- [ ] Implements ISmsProvider or similar
- [ ] Twilio SMS works
- [ ] Tests pass
- [ ] README complete

---

## Phase 3: Spatial Services Suite (HIGH PRIORITY)

**Priority:** HIGH
**Dependencies:** Phase 0
**Estimated Effort:** LARGE

### Task 3.1: Migrate Spatial Services Contracts

**Source:** `Incomming/SharedFramework/OoBDev.SpatialServices.Contracts/`
**Target:** `src/Framework/OoBDev.SpatialServices.Abstractions/`
**LOC:** 85

**Steps:**
1. Create new framework project
2. Copy contracts: ILocationServices, address models
3. Update namespace to `OoBDev.SpatialServices`
4. Add to OoBDev.sln under Framework
5. Create README

**Validation:**
- [ ] Contracts compile
- [ ] Clear abstraction for geocoding providers
- [ ] README explains ILocationServices

---

### Task 3.2: Migrate Spatial Services Common

**Source:** `Incomming/SharedFramework/OoBDev.SpatialServices.Common/`
**Target:** `src/Framework/OoBDev.SpatialServices/`
**LOC:** 21

**Steps:**
1. Create framework implementation project
2. Copy common utilities
3. Add reference to Abstractions
4. Add ServiceCollectionExtensions
5. Add to solution

**Validation:**
- [ ] Common utilities available
- [ ] DI registration working

---

### Task 3.3: Migrate Census Geocoding Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Census.Geocoding/`
**Target:** `src/ExternalServices/Census/OoBDev.Census.Geocoding/`
**LOC:** 420

**Steps:**
1. Create ExternalServices/Census directory
2. Copy provider files
3. Remove "Api." from namespace: `OoBDev.Census.Geocoding`
4. Update to .NET 9.0
5. Implement ILocationServices interface
6. Add ServiceCollectionExtensions with TryAddCensusGeocoding()
7. Migrate tests
8. Create README (note: US Census API is free, no key required)

**Validation:**
- [ ] Implements ILocationServices
- [ ] Geocoding queries work
- [ ] Tests pass
- [ ] README complete

---

### Task 3.4: Migrate Google Maps Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Google.Maps/`
**Target:** `src/ExternalServices/Google/OoBDev.Google.Maps/`
**LOC:** 269

**Steps:**
1. Create ExternalServices/Google directory
2. Copy provider files
3. Update namespace
4. Update to .NET 9.0
5. Implement ILocationServices
6. Add ServiceCollectionExtensions
7. Migrate tests
8. Create README with Google API key setup

**Validation:**
- [ ] Implements ILocationServices
- [ ] Geocoding works with API key
- [ ] Tests pass
- [ ] README complete

---

### Task 3.5: Migrate Bing Maps Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Microsoft.BingMaps/`
**Target:** `src/ExternalServices/Microsoft/OoBDev.Microsoft.BingMaps/`
**LOC:** 288

**Steps:**
1. Create ExternalServices/Microsoft directory (if not exists)
2. Copy provider files
3. Update namespace
4. Update to .NET 9.0
5. Implement ILocationServices
6. Add ServiceCollectionExtensions
7. Migrate tests
8. Create README

**Validation:**
- [ ] Implements ILocationServices
- [ ] Geocoding works
- [ ] Tests pass
- [ ] README complete

---

## Phase 4: Distributed Caching (HIGH PRIORITY)

**Priority:** HIGH
**Dependencies:** Phase 0
**Estimated Effort:** MEDIUM

### Task 4.1: Migrate Caching Contracts

**Source:** `Incomming/SharedFramework/OoBDev.Caching.Contracts/`
**Target:** `src/Framework/OoBDev.Caching.Abstractions/`
**LOC:** 83

**Steps:**
1. Create new framework project
2. Copy caching interfaces
3. Copy attributes: `[IsCacheable]`, `[FlushCache]`
4. Update namespace
5. Add to solution
6. Create README

**Validation:**
- [ ] Caching abstraction clear
- [ ] Attributes documented
- [ ] README complete

---

### Task 4.2: Migrate Caching Common

**Source:** `Incomming/SharedFramework/OoBDev.Caching.Common/`
**Target:** `src/Framework/OoBDev.Caching/`
**LOC:** 290

**Steps:**
1. Create framework implementation project
2. Copy factories, managers, utilities
3. Add reference to Abstractions
4. Add ServiceCollectionExtensions
5. Migrate tests
6. Create README

**Validation:**
- [ ] Caching infrastructure works
- [ ] Tests pass
- [ ] README complete

---

### Task 4.3: Migrate Microsoft Caching Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Microsoft.Caching/`
**Target:** `src/ExternalServices/Microsoft/OoBDev.Microsoft.Caching/`
**LOC:** 70

**Steps:**
1. Create provider project
2. Copy files
3. Add `Microsoft.Extensions.Caching.Memory` package
4. Implement caching provider interface
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Create README

**Validation:**
- [ ] In-memory caching works
- [ ] Tests pass
- [ ] README complete

---

### Task 4.4: Migrate Redis Caching Provider

**Source:** `Incomming/SharedFramework/OoBDev.Api.Redis.Caching/`
**Target:** `src/ExternalServices/Redis/OoBDev.Redis.Caching/`
**LOC:** 119

**Steps:**
1. Create ExternalServices/Redis directory
2. Copy provider files
3. Add `StackExchange.Redis` NuGet package
4. Implement caching provider interface
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Create README with Redis connection setup

**Validation:**
- [ ] Redis caching works
- [ ] Tests pass
- [ ] README complete

---

## Phase 5: Data Loading Pipeline (HIGH PRIORITY)

**Priority:** HIGH
**Dependencies:** Phase 0
**Estimated Effort:** LARGE

### Task 5.1: Migrate DataLoader Contracts

**Source:** `Incomming/SharedFramework/OoBDev.DataLoader.Contracts/`
**Target:** `src/Framework/OoBDev.DataLoader.Abstractions/`
**LOC:** 435

**Steps:**
1. Create framework project
2. Copy interfaces and models
3. Update namespace
4. Add to solution
5. Create README

**Validation:**
- [ ] Data loader abstractions clear
- [ ] Models documented
- [ ] README complete

---

### Task 5.2: Migrate DataLoader Implementation

**Source:** `Incomming/SharedFramework/OoBDev.DataLoader/`
**Target:** `src/Framework/OoBDev.DataLoader/`
**LOC:** 1,633

**Steps:**
1. Create framework implementation project
2. Copy all ETL pipeline code
3. Add `CsvHelper`, `YamlDotNet` NuGet packages
4. Add reference to Abstractions
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Create comprehensive README with CSV/JSON examples

**Validation:**
- [ ] CSV to database works
- [ ] JSON to database works
- [ ] Alternative key lookup works
- [ ] Tests pass
- [ ] README with examples

---

### Task 5.3: Migrate DataLoader CLI Tool

**Source:** `Incomming/SharedFramework/OoBDev.DataLoader.Cli/`
**Target:** `src/Tools/OoBDev.DataLoader.Cli/`
**LOC:** 228

**Steps:**
1. Create Tools project
2. Copy CLI application
3. Add reference to OoBDev.DataLoader
4. Update to .NET 9.0
5. Test CLI execution
6. Create README with command-line usage
7. Add to solution under Tools

**Validation:**
- [ ] CLI runs successfully
- [ ] Can load data from command line
- [ ] README with usage examples
- [ ] Help text clear

---

## Phase 6: Document Management Enhancement (HIGH PRIORITY)

**Priority:** HIGH
**Dependencies:** Phase 0
**Estimated Effort:** LARGE

### Task 6.1: Merge DocumentCenter.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.DocumentCenter.Contracts/`
**Target:** `src/Framework/OoBDev.Documents.Abstractions/` (NEW)
**LOC:** 422

**Steps:**
1. Create new Abstractions project (main doesn't have separate one)
2. Copy document handlers, providers, storage interfaces
3. Update namespace to `OoBDev.Documents`
4. Add to solution
5. Create README

**Validation:**
- [ ] Document abstractions complete
- [ ] Interfaces clear
- [ ] README complete

---

### Task 6.2: Merge DocumentCenter Implementation

**Source:** `Incomming/SharedFramework/OoBDev.DocumentCenter/` (911 LOC)
**Target:** `src/Framework/OoBDev.Documents/` (531 LOC - MERGE)
**Strategy:** MERGE, taking best of both

**Current Main:** Basic document abstractions (531 LOC)
**SharedFramework:** Full document center with packaging, conversion (911 LOC, 71% larger)

**Steps:**
1. **Backup main version**
2. **Compare implementations:** Identify unique features in each
3. **Merge strategy:**
   - Keep main's abstractions if compatible
   - Add SF's packaging features
   - Add SF's conversion features
   - Add SF's storage providers
4. **Update namespace** to match main
5. **Migrate tests** from both
6. **Create comprehensive README**

**Validation:**
- [ ] No breaking changes to main's public API
- [ ] SF features integrated
- [ ] Tests from both pass
- [ ] README complete

---

## Phase 7: Identity & Session Enhancement (HIGH PRIORITY)

**Priority:** HIGH
**Dependencies:** Phase 0
**Estimated Effort:** MEDIUM

### Task 7.1: Merge IdentityModel.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.IdentityModel.Contracts/` (291 LOC)
**Target:** `src/Framework/OoBDev.Identity.Abstractions/` (125 LOC - MERGE)
**Strategy:** MERGE, SF is 2.3x larger

**Current Main:** Basic identity contracts (125 LOC)
**SharedFramework:** User sessions, rights, claims, extended properties (291 LOC)

**Steps:**
1. **Backup main version**
2. **Compare contracts**
3. **Merge:**
   - Keep main's basic contracts
   - Add SF's user session contracts
   - Add SF's rights management
   - Add SF's claims enhancements
4. **Update namespace**
5. **Migrate tests**
6. **Create README**

**Validation:**
- [ ] No breaking changes
- [ ] SF enhancements integrated
- [ ] Tests pass
- [ ] README complete

---

### Task 7.2: Migrate IdentityModel.Extensions

**Source:** `Incomming/SharedFramework/OoBDev.IdentityModel.Extensions/`
**Target:** `src/Framework/OoBDev.Identity/` (new files)
**LOC:** 204

**Steps:**
1. Copy authorization services
2. Copy globalization support
3. Copy claims enhancement services
4. Add to existing OoBDev.Identity project
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Update README

**Validation:**
- [ ] Authorization services work
- [ ] Globalization support works
- [ ] Claims enhancement works
- [ ] Tests pass
- [ ] README updated

---

## Phase 8: Complex Events & Event Sourcing (MEDIUM PRIORITY)

**Priority:** MEDIUM
**Dependencies:** Phase 0
**Estimated Effort:** LARGE

### Task 8.1: Migrate ComplexEvents.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.ComplexEvents.Contracts/`
**Target:** `src/Framework/OoBDev.ComplexEvents.Abstractions/`
**LOC:** 315

**Steps:**
1. Create new framework project
2. Copy event sourcing contracts
3. Copy CQRS interfaces
4. Copy scheduling contracts
5. Update namespace
6. Add to solution
7. Create README

**Validation:**
- [ ] Event sourcing abstractions clear
- [ ] CQRS pattern documented
- [ ] README complete

---

### Task 8.2: Migrate ComplexEvents.Common

**Source:** `Incomming/SharedFramework/OoBDev.ComplexEvents.Common/`
**Target:** `src/Framework/OoBDev.ComplexEvents/`
**LOC:** 680

**Steps:**
1. Create framework implementation project
2. Copy event resolvers, subscribers, schedulers
3. Add `NCrontab` NuGet package for cron expressions
4. Add reference to Abstractions
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Create comprehensive README with cron examples

**Validation:**
- [ ] Event sourcing works
- [ ] Cron scheduling works (e.g., `[ScheduleAt("0 */45 * * * *")]`)
- [ ] Tests pass
- [ ] README with examples

---

### Task 8.3: Migrate ComplexEvents.DatabaseExtensions

**Source:** `Incomming/SharedFramework/OoBDev.ComplexEvents.DatabaseExtensions/`
**Target:** `src/Framework/OoBDev.ComplexEvents.Database/`
**LOC:** 0 (SQL scripts only)

**Steps:**
1. Create database project
2. Copy T-SQL schema scripts
3. Create migration scripts
4. Document schema in README
5. Add to solution

**Validation:**
- [ ] SQL schema scripts work
- [ ] Event storage tables created
- [ ] README documents schema

---

### Task 8.4: Migrate ComplexEvents.EntityFrameworkCore

**Source:** `Incomming/SharedFramework/OoBDev.ComplexEvents.EntityFrameworkCore/`
**Target:** `src/Framework/OoBDev.ComplexEvents.EntityFrameworkCore/`
**LOC:** 252

**Steps:**
1. Create EF Core project
2. Copy DbContext and entity configurations
3. Add `Microsoft.EntityFrameworkCore` package
4. Add reference to ComplexEvents
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Create README

**Validation:**
- [ ] EF Core event persistence works
- [ ] Migrations work
- [ ] Tests pass
- [ ] README complete

---

### Task 8.5: Migrate Azure EventHub

**Source:** `Incomming/SharedFramework/OoBDev.Azure.EventHub/`
**Target:** `src/ExternalServices/Azure/OoBDev.Azure.EventHub/`
**LOC:** 141

**Steps:**
1. Create ExternalServices/Azure project
2. Copy EventHub integration code
3. Add `Azure.Messaging.EventHubs` NuGet package
4. Integrate with ComplexEvents abstractions
5. Add ServiceCollectionExtensions
6. Migrate tests
7. Create README

**Validation:**
- [ ] EventHub integration works
- [ ] Tests pass
- [ ] README complete

---

## Phase 9: Test Data Generation (MEDIUM PRIORITY)

**Priority:** MEDIUM
**Dependencies:** Phase 0
**Estimated Effort:** MEDIUM

### Task 9.1: Migrate Generations.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.Generations.Contracts/`
**Target:** `src/Extensions/OoBDev.Extensions.TestData.Abstractions/`
**LOC:** 488

**Steps:**
1. Create Extensions project
2. Copy generation rules and contracts
3. Update namespace to `OoBDev.Extensions.TestData`
4. Add to solution
5. Create README

**Validation:**
- [ ] Generation contracts clear
- [ ] Attributes documented
- [ ] README complete

---

### Task 9.2: Migrate Generations Implementation

**Source:** `Incomming/SharedFramework/OoBDev.Generations/`
**Target:** `src/Extensions/OoBDev.Extensions.TestData/`
**LOC:** 1,256

**Steps:**
1. Create Extensions implementation project
2. Copy all generators: `[EmailAddress]`, `[Address]`, `[Phone]`, etc.
3. Copy type converters
4. Add reference to Abstractions
5. Migrate tests
6. Create comprehensive README with attribute examples

**Validation:**
- [ ] Attribute-driven generation works
- [ ] Seeded randomization works
- [ ] Tests pass
- [ ] README with examples

---

### Task 9.3: Migrate Generations DI Extensions

**Source:** `Incomming/SharedFramework/OoBDev.Generations.Extensions.DependencyInjection/`
**Target:** Merge into `OoBDev.Extensions.TestData/`
**LOC:** 21

**Steps:**
1. Copy ServiceCollectionExtensions
2. Merge into main TestData project
3. Test DI registration

**Validation:**
- [ ] DI registration works
- [ ] Generators injectable

---

## Phase 10: Text Templating Unification (MEDIUM PRIORITY)

**Priority:** MEDIUM
**Dependencies:** Phase 0
**Estimated Effort:** MEDIUM

### Task 10.1: Migrate TextTemplating.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.TextTemplating.Contracts/`
**Target:** `src/Framework/OoBDev.TextTemplating.Abstractions/`
**LOC:** 117

**Steps:**
1. Create framework Abstractions project
2. Copy template contracts and models
3. Update namespace
4. Add to solution
5. Create README

**Validation:**
- [ ] Template abstractions clear
- [ ] Models documented
- [ ] README complete

---

### Task 10.2: Merge TextTemplating Implementation

**Source:** `Incomming/SharedFramework/OoBDev.TextTemplating/` (424 LOC)
**Target:** `src/Framework/OoBDev.TextTemplating/` (NEW)
**Related Main:** `src/Framework/OoBDev.System/` (partial), `src/Tools/OoBDev.TemplateEngine.Cli`

**Current Main:** Scattered - framework abstractions in OoBDev.System, CLI tool separate
**SharedFramework:** Unified templating engine with persistence (424 LOC)

**Strategy:** Create unified OoBDev.TextTemplating project

**Steps:**
1. Create new OoBDev.TextTemplating framework project
2. Copy SF's complete templating engine
3. Integrate with main's existing abstractions
4. Keep OoBDev.TemplateEngine.Cli as separate tool
5. Add reference to Abstractions
6. Migrate tests
7. Create README

**Validation:**
- [ ] Template engine works
- [ ] Persistence works
- [ ] CLI tool still functional
- [ ] Tests pass
- [ ] README complete

---

## Phase 11: Domain-Specific Review (LOW PRIORITY)

**Priority:** LOW
**Dependencies:** Phase 0
**Estimated Effort:** SMALL

### Task 11.1: Review Accounting.Contracts

**Source:** `Incomming/SharedFramework/OoBDev.Accounting.Contracts/`
**LOC:** 166

**Assessment Required:**
1. Review invoice/payment models
2. Determine if generally useful or application-specific
3. Decide: Migrate, Archive, or Delete

**Options:**
- **A.** Migrate to `src/Extensions/OoBDev.Extensions.Accounting/` if generally useful
- **B.** Archive in Incomming/ as reference with README
- **C.** Delete if too application-specific

**Recommendation:** **Review with stakeholders** before decision

---

## Phase 12: Final Integration & Testing (FINAL)

**Priority:** CRITICAL
**Dependencies:** All phases complete
**Estimated Effort:** LARGE

### Task 12.1: Solution-Wide Integration

**Steps:**
1. Verify all 52 projects added to OoBDev.sln
2. Verify all projects compile together
3. Resolve any circular dependencies
4. Verify NuGet package restore works
5. Build entire solution

**Validation:**
- [ ] Solution builds successfully
- [ ] No compilation errors
- [ ] No warnings (or acceptable warnings documented)

---

### Task 12.2: Test Suite Execution

**Steps:**
1. Run all migrated test projects
2. Verify 80%+ coverage on new projects
3. Fix any failing tests
4. Document known test limitations

**Validation:**
- [ ] All tests pass (or failures documented)
- [ ] Coverage targets met
- [ ] Test report generated

---

### Task 12.3: Documentation Updates

**Steps:**
1. Update main FEATURE_INVENTORY.md with all 52 projects
2. Update main README.md
3. Create ConfigurationSettings.md updates
4. Create migration guide for users
5. Update architectural documentation

**Validation:**
- [ ] FEATURE_INVENTORY.md complete
- [ ] README updated
- [ ] Configuration docs complete
- [ ] Migration guide published

---

### Task 12.4: Cleanup & Archive

**Steps:**
1. Delete or archive `Incomming/SharedFramework/` after successful migration
2. Update Incomming/CHECKLIST.md
3. Update TODO.md
4. Create final migration report

**Validation:**
- [ ] Incomming/SharedFramework archived or deleted
- [ ] All tracking updated
- [ ] Migration report published

---

## Success Metrics

Migration is complete when:

1. ✅ All 52 projects migrated to appropriate layers
2. ✅ All projects upgraded to .NET 9.0
3. ✅ All namespaces cleaned ("Api." prefix removed)
4. ✅ All tests pass with 80%+ coverage
5. ✅ Solution builds without errors
6. ✅ Documentation complete
7. ✅ No breaking changes to existing main APIs
8. ✅ All merged projects preserve main's public contracts
9. ✅ Configuration guide updated
10. ✅ Migration guide published

---

## Risk Mitigation

### Risk: Breaking Changes in Merged Projects

**Mitigation:**
- Backup main versions before merge
- Compare public APIs carefully
- Create deprecation notices if needed
- Provide migration guide

### Risk: Namespace Conflicts

**Mitigation:**
- Map namespaces carefully in Phase 0
- Use find/replace across entire codebase
- Test compilation after each namespace change

### Risk: Dependency Version Conflicts

**Mitigation:**
- Upgrade to latest compatible versions
- Document any version constraints
- Test thoroughly after upgrades

### Risk: Test Failures After Migration

**Mitigation:**
- Run tests incrementally (per phase)
- Fix failures immediately
- Document known issues

---

## Rollback Plan

If migration causes critical issues:

1. **Stop migration** at current phase
2. **Restore from backup:** Use git to revert changes
3. **Analyze failure:** Identify root cause
4. **Document issue:** Add to migration notes
5. **Adjust plan:** Update migration strategy
6. **Resume:** Continue when issue resolved

---

## Related Documents

- [Feature Mapping](./sharedframework-feature-mapping.md) - Comprehensive comparison
- [Incomming Checklist](../../Incomming/CHECKLIST.md) - Overall tracking
- [Architectural Guidelines](../architecture/architectural-guidelines.md)

---

## Change Log

- 2026-01-13 v1.0: Initial SharedFramework migration plan created
