# Feature Proposals - SharedFramework Modernization

**Last Updated:** 2026-01-22
**Status:** 📋 Design & Requirements Phase
**Purpose:** Comprehensive design documentation for reimplementing SharedFramework capabilities using modern OoBDev patterns

---

## Overview

This directory contains **implementation-ready design documentation** for SharedFramework features. Instead of direct code migration, each feature has been analyzed and redesigned to align with OoBDev architectural standards:

- **Provider/Factory patterns** for extensibility
- **Dependency injection** throughout
- **Modern .NET 10.0** idioms
- **80%+ test coverage** requirements
- **Comprehensive documentation**

## Documentation Structure

Each feature proposal follows this structure:

```
{Epic}/
├── README.md                    # Epic overview and feature index
├── {Feature}/
│   ├── README.md               # Feature overview
│   ├── requirements.md         # Functional & non-functional requirements
│   ├── architecture.md         # C4 component diagrams
│   ├── user-journeys.md        # Use cases and scenarios
│   ├── api-design.md           # Contracts and interfaces
│   ├── business-rules.md       # Logic (pseudo code/sequence diagrams)
│   ├── configuration.md        # Settings and options
│   └── testing-strategy.md     # Test approach
```

### Diagram Standards

- **Components/Layers:** PlantUML C4 style
- **Sequence Diagrams:** PlantUML UML style
- **UI Mockups:** ASCII art
- **Logic:** Pseudo code or sequence diagrams (NOT actual source code)

---

## Epic Index

### Epic 1: Message Queueing Infrastructure
**Status:** 📋 Documentation Pending
**Priority:** HIGH
**Capabilities:** AWS SQS, Azure Service Bus providers with unified abstraction

**Features:**
- [AWS SQS Provider](./01-MessageQueueing/AwsSqsProvider/) - Amazon Simple Queue Service integration
- [Azure Service Bus Provider](./01-MessageQueueing/AzureServiceBusProvider/) - Topics, sessions, dead-letter queues
- [Message Queueing Abstractions](./01-MessageQueueing/Abstractions/) - Unified provider interface

**Source Analysis:** ~600 LOC, fills critical gap in cloud deployments

---

### Epic 2: Communications Platform
**Status:** 📋 Documentation Pending
**Priority:** CRITICAL
**Capabilities:** Multi-channel communications (email, SMS) with orchestration

**Features:**
- [Communications Core](./02-Communications/Core/) - Multi-channel orchestration engine (1,145 LOC)
- [SendGrid Provider](./02-Communications/SendGridProvider/) - Cloud email service
- [Twilio SMS Provider](./02-Communications/TwilioSmsProvider/) - SMS messaging
- [Communications Abstractions](./02-Communications/Abstractions/) - Email/SMS contracts (695 LOC)

**Source Analysis:** ~2,500 LOC, replaces 16 LOC stub with full implementation

---

### Epic 3: Spatial Services
**Status:** 📋 Documentation Pending
**Priority:** HIGH
**Capabilities:** Geocoding and location services with multiple providers

**Features:**
- [Spatial Services Abstractions](./03-SpatialServices/Abstractions/) - ILocationServices interface
- [Census Geocoding Provider](./03-SpatialServices/CensusGeocodingProvider/) - Free US government data
- [Google Maps Provider](./03-SpatialServices/GoogleMapsProvider/) - High-quality geocoding
- [Bing Maps Provider](./03-SpatialServices/BingMapsProvider/) - Alternative provider

**Source Analysis:** ~1,100 LOC, new capability (main has zero spatial services)

---

### Epic 4: Distributed Caching
**Status:** 📋 Documentation Pending
**Priority:** HIGH
**Capabilities:** Distributed caching with attribute-based API

**Features:**
- [Caching Abstractions](./04-DistributedCaching/Abstractions/) - `[IsCacheable]`, `[FlushCache]` attributes
- [Caching Core](./04-DistributedCaching/Core/) - Factories, managers, property chains
- [Redis Provider](./04-DistributedCaching/RedisProvider/) - Distributed cache
- [Microsoft Caching Provider](./04-DistributedCaching/MicrosoftCachingProvider/) - In-memory cache

**Source Analysis:** ~600 LOC, enables distributed application patterns

---

### Epic 5: Data Loading Pipeline
**Status:** 📋 Documentation Pending
**Priority:** HIGH
**Capabilities:** ETL pipeline for CSV/JSON to database with CLI tool

**Features:**
- [DataLoader Abstractions](./05-DataLoading/Abstractions/) - ETL interfaces (435 LOC)
- [DataLoader Core](./05-DataLoading/Core/) - CSV/JSON pipeline, alternative key lookup (1,633 LOC)
- [DataLoader CLI](./05-DataLoading/Cli/) - Command-line tool for deployments (228 LOC)

**Source Analysis:** ~2,300 LOC, critical for deployment automation

---

### Epic 6: Document Management
**Status:** 📋 Documentation Pending
**Priority:** HIGH
**Capabilities:** Enhanced document center with packaging and conversion

**Features:**
- [Document Abstractions](./06-DocumentManagement/Abstractions/) - Handlers, providers, storage (422 LOC)
- [Document Core](./06-DocumentManagement/Core/) - Enhanced implementation (911 LOC vs 531 LOC in main)

**Source Analysis:** ~1,300 LOC, completes main's partial implementation

---

### Epic 7: Identity & Session Management
**Status:** 📋 Documentation Pending
**Priority:** HIGH
**Capabilities:** Enhanced identity with sessions, rights, and claims

**Features:**
- [Identity Abstractions](./07-Identity/Abstractions/) - Sessions, rights, claims (291 LOC vs 125 LOC in main)
- [Identity Extensions](./07-Identity/Extensions/) - Authorization, globalization (204 LOC)

**Source Analysis:** ~500 LOC, 2.3x larger than main

---

### Epic 8: Complex Events
**Status:** 📋 Documentation Pending
**Priority:** MEDIUM
**Capabilities:** Event sourcing, CQRS, cron-based scheduling

**Features:**
- [Complex Events Abstractions](./08-ComplexEvents/Abstractions/) - Event sourcing contracts (315 LOC)
- [Complex Events Core](./08-ComplexEvents/Core/) - CQRS, `[ScheduleAt("0 */45 * * * *")]` (680 LOC)
- [Complex Events Database](./08-ComplexEvents/Database/) - T-SQL schema and EF Core (252 LOC)
- [Azure EventHub Provider](./08-ComplexEvents/AzureEventHubProvider/) - Big data streaming (141 LOC)

**Source Analysis:** ~1,400 LOC, advanced architecture patterns

---

### Epic 9: Test Data Generation
**Status:** 📋 Documentation Pending
**Priority:** MEDIUM
**Capabilities:** Attribute-driven test data generation

**Features:**
- [Generations Abstractions](./09-TestDataGeneration/Abstractions/) - Generation rules (488 LOC)
- [Generations Core](./09-TestDataGeneration/Core/) - `[EmailAddress]`, `[Address]`, `[Phone]` generators (1,256 LOC)

**Source Analysis:** ~1,800 LOC, alternative to Bogus with attribute-driven API

---

### Epic 10: Text Templating
**Status:** 📋 Documentation Pending
**Priority:** MEDIUM
**Capabilities:** Unified templating engine with persistence

**Features:**
- [Templating Abstractions](./10-TextTemplating/Abstractions/) - Template contracts (117 LOC)
- [Templating Core](./10-TextTemplating/Core/) - Complete engine (424 LOC vs scattered in main)

**Source Analysis:** ~550 LOC, unifies main's scattered implementation

---

## Priority Matrix

| Epic | Priority | Complexity | Dependencies | LOC Impact |
|------|----------|------------|--------------|------------|
| **Communications Platform** | CRITICAL | HIGH | None | ~2,500 |
| **Message Queueing** | HIGH | MEDIUM | None | ~600 |
| **Spatial Services** | HIGH | MEDIUM | None | ~1,100 |
| **Distributed Caching** | HIGH | MEDIUM | None | ~600 |
| **Data Loading** | HIGH | MEDIUM | None | ~2,300 |
| **Document Management** | HIGH | MEDIUM | None | ~1,300 |
| **Identity & Session** | HIGH | MEDIUM | None | ~500 |
| **Complex Events** | MEDIUM | HIGH | None | ~1,400 |
| **Test Data Generation** | MEDIUM | MEDIUM | None | ~1,800 |
| **Text Templating** | MEDIUM | MEDIUM | None | ~550 |

**Total Impact:** ~13,700+ LOC of production-proven capabilities

---

## Implementation Sequence

### Phase 1: Foundation (Weeks 1-2)
1. **Message Queueing** - Critical cloud deployment capability
2. **Distributed Caching** - Enables distributed patterns
3. **Spatial Services** - Clean abstractions, multiple providers

### Phase 2: Core Services (Weeks 3-5)
4. **Communications Platform** - Most critical (replaces 16 LOC stub)
5. **Data Loading** - Deployment automation
6. **Document Management** - Complete existing work

### Phase 3: Advanced Features (Weeks 6-8)
7. **Identity & Session** - Enhanced authentication
8. **Complex Events** - Event sourcing and CQRS
9. **Test Data Generation** - Testing utilities

### Phase 4: Remaining Features (Week 9)
10. **Text Templating** - Unify implementations

---

## Design Principles

All feature designs MUST adhere to:

### 1. Architectural Standards
- Follow [OoBDev Architectural Standards](../../docs/architecture/architectural-standards.md)
- Layer placement: ExternalServices for third-party, Framework for core, Extensions for specialized
- Provider/Factory pattern for all external integrations
- Dependency injection with TryAdd* extensions

### 2. Modern .NET 10.0
- Nullable reference types enabled
- Implicit usings disabled
- Latest language features
- Performance optimizations

### 3. Testing Requirements
- 80%+ coverage for Framework layer
- Test categories: Unit, Simulate, Integration, LiveIntegration
- MSTest framework
- Comprehensive test scenarios

### 4. Documentation Requirements
- README.md for every project (build-enforced)
- XML documentation on public APIs
- Usage examples in README
- Configuration guide

### 5. Configuration Pattern
- IOptions<T> pattern
- Environment variable support
- .runsettings integration for tests
- Clear validation rules

---

## Source Material

**Original Source:** `Incomming/SharedFramework/` (52 projects, ~28,582 LOC)
**Target Framework:** .NET 10.0 (upgrade from .NET 8.0)
**Migration Strategy:** Design-first, modern reimplementation

**Reference Documents:**
- [SharedFramework Feature Mapping](../../docs/migration/sharedframework-feature-mapping.md)
- [SharedFramework Migration Plan](../../docs/migration/sharedframework-migration-plan.md)
- [OoBDev Architecture](../../docs/architecture/)

---

## Usage

### For Implementers

1. **Read Epic README** - Understand the capability area
2. **Read Feature README** - Get feature overview
3. **Review requirements.md** - Understand what needs to be built
4. **Study architecture.md** - Understand component design
5. **Review api-design.md** - Understand contracts
6. **Follow business-rules.md** - Understand logic flow
7. **Check configuration.md** - Understand settings
8. **Review testing-strategy.md** - Plan test implementation

### For Reviewers

1. **Validate requirements** - Are they complete and clear?
2. **Review architecture** - Does it follow OoBDev patterns?
3. **Check API design** - Is it intuitive and well-designed?
4. **Verify business rules** - Are they comprehensive?
5. **Validate configuration** - Is it flexible and clear?
6. **Review testing strategy** - Is coverage adequate?

---

## Progress Tracking

**Overall Status:** 0 of 10 epics documented (0%)

| Epic | Requirements | Architecture | API Design | Business Rules | Config | Testing | Status |
|------|-------------|--------------|------------|----------------|--------|---------|--------|
| Message Queueing | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Communications | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Spatial Services | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Distributed Caching | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Data Loading | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Document Management | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Identity & Session | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Complex Events | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Test Data Generation | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |
| Text Templating | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | 📋 Pending |

---

## Next Steps

1. **Start with Message Queueing** - Foundation infrastructure (Epic 1)
2. **Document requirements** - Functional and non-functional
3. **Create architecture diagrams** - C4 component views
4. **Design API contracts** - Modern OoBDev patterns
5. **Define business rules** - Sequence diagrams and pseudo code
6. **Plan testing strategy** - Unit, Integration, LiveIntegration

**Ready to begin documentation!**
