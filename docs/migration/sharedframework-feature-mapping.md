# Incomming/SharedFramework Feature Mapping

**Version:** 1.0
**Last Updated:** 2026-01-13
**Source:** Incomming/SharedFramework
**Target:** Main OoBDev (dotex) Framework
**Status:** 🔍 INVESTIGATION COMPLETE - Ready for systematic migration

---

## Executive Summary

**SharedFramework** is a comprehensive library collection (52 projects, ~28,582 LOC) migrated from a production application that provides **significant new capabilities** to the OoBDev framework.

### Critical Finding

SharedFramework contains a mix of **completely new capabilities** (24 projects) and **enhanced versions** of existing framework components (10 projects):

- **24 projects (71%)** - NEW capabilities not in main codebase (~9,000+ LOC)
- **10 projects (29%)** - ENHANCED versions of existing frameworks (DIFFERS status)
- **0 projects (0%)** - IDENTICAL to main codebase

### Comparison Summary

| Aspect | SharedFramework | Main OoBDev | Status |
|--------|----------------|------------|--------|
| **Total Projects** | 52 (34 + 18 tests) | ~100+ | Additional capabilities |
| **Total LOC** | ~28,582 | N/A | Substantial addition |
| **Target Framework** | .NET 8.0 | .NET 9.0 | Needs upgrade |
| **Pattern** | Production-ready implementations | Mix of abstractions + implementations | SF completes abstractions |

**Key Insight:** SharedFramework is NOT a duplicate framework - it's a collection of **production-proven external service integrations** and **complete implementations** that fill gaps in the main OoBDev codebase.

---

## Project Categories

### 1. Message Queueing (4 projects, ~600 LOC)

**Status:** NEW - Main has abstractions only

#### OoBDev.Amazon.Sqs
- **LOC:** 183
- **Status:** NEW
- **Purpose:** AWS SQS message queue provider implementation
- **Main Equivalent:** None (only OoBDev.MessageQueueing.Abstractions exists)
- **Dependencies:** AWSSDK.SQS
- **Migration Priority:** HIGH - Fills critical gap for AWS deployments

#### OoBDev.Azure.ServiceBus
- **LOC:** 114
- **Status:** NEW
- **Purpose:** Azure Service Bus provider (topics, sessions, dead-letter)
- **Main Equivalent:** None (only abstractions exist)
- **Dependencies:** Azure.Messaging.ServiceBus
- **Migration Priority:** HIGH - Enterprise messaging capability

**Summary:** Main has message queueing abstractions (OoBDev.MessageQueueing.Abstractions, OoBDev.Communications.MessageQueueing) but NO provider implementations except RabbitMQ and in-process. SharedFramework adds critical cloud providers.

---

### 2. Communications (6 projects, ~2,500 LOC)

**Status:** MIXED - 2 NEW providers, 2 DIFFERS (enhanced implementations)

#### OoBDev.Api.Twilio.SendGrid
- **LOC:** 267
- **Status:** NEW
- **Purpose:** SendGrid cloud email provider
- **Main Equivalent:** None (MailKit SMTP/IMAP only)
- **Dependencies:** SendGrid
- **Migration Priority:** HIGH - Cloud email service

#### OoBDev.Api.Twilio.SmsMessaging
- **LOC:** 151
- **Status:** NEW
- **Purpose:** Twilio SMS messaging provider
- **Main Equivalent:** None - First SMS implementation!
- **Dependencies:** Twilio
- **Migration Priority:** HIGH - NEW communication channel

#### OoBDev.Communications
- **LOC:** 1,145
- **Status:** DIFFERS
- **Main:** /src/Framework/OoBDev.Communications (16 LOC)
- **Difference:** SF has full implementation with composers, handlers, channel routing, preference management. Main has only registrar stub.
- **Size Ratio:** SharedFramework is **71x larger**
- **Migration Priority:** CRITICAL - Main is incomplete

#### OoBDev.Communications.Contracts
- **LOC:** 695
- **Status:** DIFFERS
- **Main:** /src/Framework/OoBDev.Communications.Abstractions (125 LOC)
- **Difference:** SF has comprehensive email/SMS contracts, multi-channel support, attributes, tracking. Main has only basic models.
- **Size Ratio:** SharedFramework is **5.5x larger**
- **Migration Priority:** CRITICAL - Main is incomplete

**Summary:** SharedFramework provides complete communications orchestration system that main codebase is missing.

---

### 3. Spatial Services (7 projects, ~1,100 LOC)

**Status:** NEW - Main has NO spatial/geocoding services

#### OoBDev.Api.Census.Geocoding
- **LOC:** 420
- **Status:** NEW
- **Purpose:** US Census Bureau geocoding API (free, government data)
- **Main Equivalent:** None
- **Migration Priority:** HIGH - Free geocoding option

#### OoBDev.Api.Google.Maps
- **LOC:** 269
- **Status:** NEW
- **Purpose:** Google Maps geocoding API
- **Main Equivalent:** None
- **Migration Priority:** HIGH - High-quality geocoding

#### OoBDev.Api.Microsoft.BingMaps
- **LOC:** 288
- **Status:** NEW
- **Purpose:** Bing Maps geocoding API
- **Main Equivalent:** None
- **Migration Priority:** MEDIUM - Alternative provider

#### OoBDev.SpatialServices.Common
- **LOC:** 21
- **Status:** NEW
- **Purpose:** Shared spatial services utilities
- **Main Equivalent:** None
- **Migration Priority:** HIGH - Foundation for providers

#### OoBDev.SpatialServices.Contracts
- **LOC:** 85
- **Status:** NEW
- **Purpose:** ILocationServices interface, address models
- **Main Equivalent:** None
- **Migration Priority:** HIGH - Abstraction layer

**Summary:** Complete geocoding suite with provider pattern. Main codebase has ZERO spatial/geocoding capability.

---

### 4. Complex Events (5 projects, ~1,400 LOC)

**Status:** NEW - Main has NO event sourcing/CQRS support

#### OoBDev.ComplexEvents.Common
- **LOC:** 680
- **Status:** NEW
- **Purpose:** Event sourcing, CQRS, cron-based scheduling, event entities
- **Main Equivalent:** None
- **Features:**
  - `[ScheduleAt("0 */45 * * * *")]` - Cron-based event scheduling
  - Event resolvers and subscribers
  - Scheduled event execution
- **Migration Priority:** MEDIUM - Advanced architecture pattern

#### OoBDev.ComplexEvents.Contracts
- **LOC:** 315
- **Status:** NEW
- **Purpose:** Complex events service contracts
- **Main Equivalent:** None
- **Migration Priority:** MEDIUM

#### OoBDev.ComplexEvents.DatabaseExtensions
- **LOC:** 0 (SQL scripts only)
- **Status:** NEW
- **Purpose:** T-SQL schema for event storage
- **Main Equivalent:** None
- **Migration Priority:** MEDIUM

#### OoBDev.ComplexEvents.EntityFrameworkCore
- **LOC:** 252
- **Status:** NEW
- **Purpose:** EF Core persistence for event sourcing
- **Main Equivalent:** None
- **Migration Priority:** MEDIUM

#### OoBDev.Azure.EventHub
- **LOC:** 141
- **Status:** NEW
- **Purpose:** Azure Event Hubs integration for big data streaming
- **Main Equivalent:** None
- **Dependencies:** Azure.Messaging.EventHubs
- **Migration Priority:** LOW - Specialized use case

**Summary:** Complete event-driven architecture support. Main has none.

---

### 5. Data Loading (4 projects, ~2,300 LOC)

**Status:** NEW - Main has NO ETL/data loading support

#### OoBDev.DataLoader
- **LOC:** 1,633
- **Status:** NEW
- **Purpose:** CSV/JSON to database ETL pipeline
- **Features:**
  - Seed data and reference data loading
  - Alternative key lookup (match by unique fields, not just PK)
  - Batch processing
  - Error handling and logging
- **Main Equivalent:** None
- **Migration Priority:** HIGH - Critical for deployment automation

#### OoBDev.DataLoader.Cli
- **LOC:** 228
- **Status:** NEW
- **Purpose:** CLI tool for data loading
- **Main Equivalent:** None
- **Dependencies:** CsvHelper, YamlDotNet
- **Migration Priority:** HIGH - Deployment tool

#### OoBDev.DataLoader.Contracts
- **LOC:** 435
- **Status:** NEW
- **Purpose:** Data loader interfaces and models
- **Main Equivalent:** None
- **Migration Priority:** HIGH

**Summary:** Enterprise-grade ETL system with CLI tool. Main has nothing.

---

### 6. Code Generation / Test Data (4 projects, ~1,800 LOC)

**Status:** NEW - Main has NO test data generation

#### OoBDev.Generations
- **LOC:** 1,256
- **Status:** NEW
- **Purpose:** Attribute-driven test data generation
- **Features:**
  - `[EmailAddress]`, `[Address]`, `[Phone]`, `[Ssn]`, `[CreditCard]`
  - Seeded randomization (reproducible tests)
  - Type converters and generators
  - DI integration
- **Main Equivalent:** None (potential Bogus/Faker alternative)
- **Migration Priority:** MEDIUM - Useful for testing

#### OoBDev.Generations.Contracts
- **LOC:** 488
- **Status:** NEW
- **Purpose:** Generation rules and contracts
- **Main Equivalent:** None
- **Migration Priority:** MEDIUM

#### OoBDev.Generations.Extensions.DependencyInjection
- **LOC:** 21
- **Status:** NEW
- **Purpose:** DI registration for generators
- **Main Equivalent:** None
- **Migration Priority:** MEDIUM

**Summary:** Complete test data generation framework. Alternative to external libraries like Bogus.

---

### 7. Document Management (3 projects, ~1,300 LOC)

**Status:** DIFFERS - Main has basic abstractions, SF has full implementation

#### OoBDev.DocumentCenter
- **LOC:** 911
- **Status:** DIFFERS
- **Main:** /src/Framework/OoBDev.Documents (531 LOC)
- **Difference:** SF has full document center with packaging, conversion, storage providers. Main has basic abstractions only.
- **Size Ratio:** SharedFramework is **71% larger**
- **Migration Priority:** HIGH - Complete main's partial implementation

#### OoBDev.DocumentCenter.Contracts
- **LOC:** 422
- **Status:** NEW
- **Purpose:** Document handlers, providers, storage interfaces
- **Main Equivalent:** None (main has only basic abstractions)
- **Migration Priority:** HIGH

**Summary:** SharedFramework completes the document management system that main started.

---

### 8. Distributed Caching (7 projects, ~600 LOC)

**Status:** NEW - Main has NO distributed caching support

#### OoBDev.Caching.Common
- **LOC:** 290
- **Status:** NEW
- **Purpose:** Common caching abstractions, factories, managers
- **Main Equivalent:** None
- **Migration Priority:** HIGH - Foundation

#### OoBDev.Caching.Contracts
- **LOC:** 83
- **Status:** NEW
- **Purpose:** Caching interfaces and attributes
- **Features:**
  - `[IsCacheable]` - Mark methods for caching
  - `[FlushCache]` - Invalidate cache
- **Main Equivalent:** None
- **Migration Priority:** HIGH

#### OoBDev.Api.Microsoft.Caching
- **LOC:** 70
- **Status:** NEW
- **Purpose:** Microsoft.Extensions.Caching provider
- **Main Equivalent:** None
- **Migration Priority:** HIGH

#### OoBDev.Api.Redis.Caching
- **LOC:** 119
- **Status:** NEW
- **Purpose:** Redis distributed cache provider
- **Main Equivalent:** None
- **Dependencies:** StackExchange.Redis
- **Migration Priority:** HIGH - Critical for distributed apps

**Summary:** Complete distributed caching system with attribute-based API and multiple providers.

---

### 9. Identity & Session Management (3 projects, ~500 LOC)

**Status:** DIFFERS - Main has basic contracts, SF has full implementation

#### OoBDev.IdentityModel.Contracts
- **LOC:** 291
- **Status:** DIFFERS
- **Main:** /src/Framework/OoBDev.Identity.Abstractions (125 LOC)
- **Difference:** SF has user sessions, rights management, claims, extended properties. Main has only basic identity contracts.
- **Size Ratio:** SharedFramework is **2.3x larger**
- **Migration Priority:** HIGH - Enhanced identity system

#### OoBDev.IdentityModel.Extensions
- **LOC:** 204
- **Status:** NEW
- **Purpose:** Authorization services, globalization, claims enhancement
- **Main Equivalent:** None
- **Migration Priority:** HIGH - Identity features

**Summary:** Enhanced identity system with session management and authorization.

---

### 10. Text Templating (3 projects, ~550 LOC)

**Status:** DIFFERS - Main has framework-only, SF has complete engine

#### OoBDev.TextTemplating
- **LOC:** 424
- **Status:** DIFFERS
- **Main:** /src/Framework/OoBDev.System (partial abstractions) + /src/Tools/OoBDev.TemplateEngine.Cli
- **Difference:** SF has complete templating engine with persistence. Main has framework abstractions and CLI tool separately.
- **Size Ratio:** SharedFramework is **4.2x larger** than framework portion
- **Migration Priority:** MEDIUM - Complete templating system

#### OoBDev.TextTemplating.Contracts
- **LOC:** 117
- **Status:** NEW
- **Purpose:** Template contracts and models
- **Main Equivalent:** None (abstractions scattered)
- **Migration Priority:** MEDIUM

**Summary:** Unified templating system vs main's scattered implementation.

---

### 11. Accounting (1 project, ~170 LOC)

**Status:** NEW - Domain-specific

#### OoBDev.Accounting.Contracts
- **LOC:** 166
- **Status:** NEW
- **Purpose:** Accounting domain models (invoices, payments, line items)
- **Main Equivalent:** None
- **Migration Priority:** LOW - Domain-specific, may not be generally useful

**Summary:** Domain-specific accounting contracts. Assess if generally useful or application-specific.

---

## Migration Decision Matrix

| Category | Projects | Status | Total LOC | Priority | Complexity | Recommendation |
|----------|----------|--------|-----------|----------|------------|----------------|
| **Message Queueing** | 4 | NEW | ~600 | HIGH | MEDIUM | MIGRATE - Fill critical gaps |
| **Communications (providers)** | 2 | NEW | ~420 | HIGH | MEDIUM | MIGRATE - SendGrid + Twilio |
| **Communications (core)** | 2 | DIFFERS | ~1,840 | CRITICAL | HIGH | MERGE - 71x larger than main |
| **Spatial Services** | 7 | NEW | ~1,100 | HIGH | MEDIUM | MIGRATE - New capability |
| **Complex Events** | 5 | NEW | ~1,400 | MEDIUM | HIGH | MIGRATE - Event sourcing |
| **Data Loading** | 4 | NEW | ~2,300 | HIGH | MEDIUM | MIGRATE - Critical ETL tool |
| **Code Generation** | 4 | NEW | ~1,800 | MEDIUM | MEDIUM | MIGRATE - Testing utility |
| **Document Management** | 3 | DIFFERS | ~1,300 | HIGH | MEDIUM | MERGE - Complete main's work |
| **Caching** | 7 | NEW | ~600 | HIGH | MEDIUM | MIGRATE - Distributed caching |
| **Identity** | 3 | DIFFERS | ~500 | HIGH | MEDIUM | MERGE - Enhanced identity |
| **Text Templating** | 3 | DIFFERS | ~550 | MEDIUM | MEDIUM | MERGE - Unify implementations |
| **Accounting** | 1 | NEW | ~170 | LOW | LOW | REVIEW - Domain-specific? |

---

## Architectural Mapping

### Layer Placement Strategy

| SharedFramework Project | Target Layer | Target Path |
|------------------------|--------------|-------------|
| **Amazon.Sqs** | ExternalServices | /src/ExternalServices/Amazon/OoBDev.Amazon.Sqs |
| **Azure.ServiceBus** | ExternalServices | /src/ExternalServices/Azure/OoBDev.Azure.ServiceBus |
| **Azure.EventHub** | ExternalServices | /src/ExternalServices/Azure/OoBDev.Azure.EventHub |
| **Api.Twilio.*** | ExternalServices | /src/ExternalServices/Twilio/OoBDev.Twilio.* |
| **Api.Census.Geocoding** | ExternalServices | /src/ExternalServices/Census/OoBDev.Census.Geocoding |
| **Api.Google.Maps** | ExternalServices | /src/ExternalServices/Google/OoBDev.Google.Maps |
| **Api.Microsoft.BingMaps** | ExternalServices | /src/ExternalServices/Microsoft/OoBDev.Microsoft.BingMaps |
| **Api.Microsoft.Caching** | ExternalServices | /src/ExternalServices/Microsoft/OoBDev.Microsoft.Caching |
| **Api.Redis.Caching** | ExternalServices | /src/ExternalServices/Redis/OoBDev.Redis.Caching |
| **SpatialServices*** | Framework | /src/Framework/OoBDev.SpatialServices* |
| **Caching.*** | Framework | /src/Framework/OoBDev.Caching* |
| **ComplexEvents.*** | Framework | /src/Framework/OoBDev.ComplexEvents* |
| **Communications*** | Framework | /src/Framework/OoBDev.Communications* (MERGE) |
| **DocumentCenter*** | Framework | /src/Framework/OoBDev.Documents* (MERGE) |
| **IdentityModel*** | Framework | /src/Framework/OoBDev.Identity* (MERGE) |
| **TextTemplating*** | Framework | /src/Framework/OoBDev.TextTemplating* |
| **DataLoader*** | Framework | /src/Framework/OoBDev.DataLoader* |
| **Generations*** | Extensions | /src/Extensions/OoBDev.Extensions.TestData* |
| **Accounting.Contracts** | TBD | Review if generally useful |

---

## Dependencies Analysis

### External NuGet Packages Required

**AWS:**
- AWSSDK.SQS

**Azure:**
- Azure.Messaging.ServiceBus
- Azure.Messaging.EventHubs

**Communications:**
- SendGrid
- Twilio

**Geocoding:**
- (HTTP clients only - no special packages)

**Caching:**
- StackExchange.Redis
- Microsoft.Extensions.Caching.Memory

**Scheduling:**
- NCrontab

**Data Loading:**
- CsvHelper
- YamlDotNet

**Entity Framework:**
- Microsoft.EntityFrameworkCore (already in main)

### Framework Version

**Current:** .NET 8.0
**Target:** .NET 9.0
**Action:** Upgrade all projects during migration

---

## Questions Requiring Answers

### 1. Communications Merge Strategy

**Question:** How to merge SharedFramework Communications with main?

**Options:**
- **A.** Replace main stub with SharedFramework implementation (RECOMMENDED)
- **B.** Keep both and mark main as deprecated
- **C.** Manual merge preserving main's interfaces

**Recommendation:** **Option A** - Main's 16 LOC stub should be replaced with SF's 1,145 LOC implementation

---

### 2. Accounting Domain Specificity

**Question:** Is OoBDev.Accounting.Contracts generally useful or application-specific?

**Assessment Needed:**
- Review invoice/payment models
- Determine if applicable to multiple domains
- Decide: Migrate, archive as reference, or delete

---

### 3. Test Data Generation vs Bogus

**Question:** Should we migrate OoBDev.Generations or recommend external library (Bogus)?

**Options:**
- **A.** Migrate Generations (1,256 LOC, attribute-driven)
- **B.** Use Bogus library and delete Generations
- **C.** Offer both as options

**Recommendation:** **Option A** - Attribute-driven approach is unique value

---

### 4. Namespace Strategy

**Question:** Keep "OoBDev." prefix or adjust namespaces?

**Current SF:** OoBDev.Amazon.Sqs, OoBDev.Api.Twilio.SendGrid
**Proposed:** OoBDev.Amazon.Sqs, OoBDev.Twilio.SendGrid (remove "Api.")

**Recommendation:** Remove "Api." prefix for cleaner namespaces

---

## Related Documents

- [SharedFramework Migration Plan](./sharedframework-migration-plan.md) - Detailed execution steps
- [Incomming Checklist](../../Incomming/CHECKLIST.md) - Overall tracking

---

## Change Log

- 2026-01-13 v1.0: Initial SharedFramework feature mapping created
