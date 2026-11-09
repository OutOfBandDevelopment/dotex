# Epic & Feature Breakdown - Review for Reorganization

**Date:** 2026-01-22
**Purpose:** Review proposed epic structure before continuing detailed documentation
**Status:** 🔍 AWAITING FEEDBACK

---

## Current Proposed Epics

### Epic 11: Data Enhancement Pipeline (NEW - FOUNDATIONAL)
**Priority:** HIGH
**LOC:** ~400
**Purpose:** Generic data enrichment framework

**Proposed Features:**
1. **Core Pipeline** - Main orchestration engine
   - `IDataEnhancementPipeline` - Main interface
   - `IMessageData` - Data container abstraction
   - Pipeline orchestration logic

2. **Provider Discovery** - Attribute-based discovery
   - `[EnhancementContext]` attribute
   - Assembly scanning
   - Provider registration
   - Order-based execution

**Questions for Review:**
- Is this split correct (Core vs Discovery)?
- Should `IMessageData` be its own feature/epic?
- Any other features missing?

---

### Epic 10: Text Templating
**Priority:** HIGH (moved earlier - needed by composition)
**LOC:** ~550
**Purpose:** Template loading and rendering engine

**Proposed Features:**
1. **Template Engine** - Core rendering
   - Load templates (files, DB, embedded resources)
   - Variable substitution
   - Template caching

2. **Template Storage** - Persistence abstraction
   - File-based storage
   - Database storage
   - Azure Blob storage (optional)

3. **Template Rendering** - Format-specific renderers
   - Text templates
   - HTML templates
   - Markdown templates

**Questions for Review:**
- Should storage be separate from engine?
- Should renderers be per-format or unified?
- Is there a "template composition" feature (combining multiple templates)?

---

### Epic 12: Message Composition Service (NEW)
**Priority:** HIGH
**LOC:** ~300
**Purpose:** Composes messages from templates + enhanced data

**Proposed Features:**
1. **Email Composition** - Compose IEmailMessage
   - Use Data Enhancement Pipeline
   - Use Template Engine
   - Produce pre-formatted IEmailMessage

2. **SMS Composition** - Compose ISmsMessage
   - Same as email, but for SMS

3. **Multi-Format Composition** - Compose multiple formats
   - Generate Email + SMS + Push simultaneously
   - Consistent correlation IDs

**Questions for Review:**
- Should this be ONE feature or split by channel?
- Should this be part of Epic 10 (Templating) instead of standalone?
- Is "composition" the right term, or should it be "message generation"?

---

### Epic 2: Communications Platform (SIMPLIFIED)
**Priority:** HIGH
**LOC:** ~800 (down from ~2,500)
**Purpose:** Channel routing and delivery ONLY

**Proposed Features:**
1. **Channel Routing** - Route to Email/SMS/Push
   - User preference lookup
   - Channel selection
   - Quiet hours management
   - Priority filtering

2. **SendGrid Email Provider** - Email delivery
   - SendGrid API integration
   - Implements ISendEmailProvider

3. **Twilio SMS Provider** - SMS delivery
   - Twilio API integration
   - Implements ISendSmsProvider

4. **Deferral Management** - Scheduled delivery
   - Queue-based deferral
   - Reprocessing logic
   - Scheduled execution

**Questions for Review:**
- Should providers be separate epics?
- Should deferral be separate feature or part of routing?
- Any other channel-specific features needed (Push, WhatsApp)?

---

### Epic 3: Spatial Services
**Priority:** HIGH
**LOC:** ~1,100
**Purpose:** Geocoding and location services

**Proposed Features:**
1. **Spatial Abstractions** - ILocationServices interface
   - Address models
   - Geocoding contracts

2. **Census Geocoding Provider** - Free US government data
   - US Census Bureau API

3. **Google Maps Provider** - High-quality geocoding
   - Google Maps API

4. **Bing Maps Provider** - Alternative provider
   - Bing Maps API

5. **Spatial Utilities** - Common utilities
   - Distance calculations
   - Coordinate conversions

**Questions for Review:**
- Should utilities be part of abstractions?
- Should all 3 providers be in one epic or separate?
- Any other spatial features needed (routing, geocoding reverse, etc.)?

---

### Epic 4: Distributed Caching
**Priority:** HIGH
**LOC:** ~600
**Purpose:** Distributed caching with attribute-based API

**Status:** ✅ ALREADY MIGRATED (2026-01-20)

**Features (Complete):**
1. Caching Abstractions - `[IsCacheable]`, `[FlushCache]`
2. Caching Core - Proxy pattern, manager, factory
3. Redis Provider - Distributed cache
4. Microsoft Caching Provider - In-memory cache

**Questions for Review:**
- Are there additional caching features needed?
- Should cache warming/preloading be added?

---

### Epic 5: Data Loading Pipeline (ETL)
**Priority:** HIGH
**LOC:** ~2,300
**Purpose:** ETL pipeline for CSV/JSON to database

**Proposed Features:**
1. **DataLoader Abstractions** - ETL interfaces
   - Loader contracts
   - Transformation contracts
   - Mapping contracts

2. **DataLoader Core** - ETL engine
   - CSV to database
   - JSON to database
   - Alternative key lookup
   - Batch processing

3. **DataLoader CLI** - Command-line tool
   - CLI for deployments
   - Configuration file support
   - Logging and error reporting

**Questions for Review:**
- Should CSV and JSON be separate features?
- Should transformations be a separate feature?
- Any other data sources needed (XML, Excel, Parquet)?

---

### Epic 6: Document Management (SPLIT)
**Priority:** HIGH
**LOC:** ~900
**Purpose:** Document lifecycle management

**Proposed Features:**
1. **Persistence & Retrieval** - Storage abstraction
   - IDocumentRepository
   - IDocumentStore (DB, file system, S3, Azure Blob)
   - Query/search by metadata
   - Version control

2. **Conversion Pipelines** - Format transformations
   - IDocumentConverter
   - IConversionPipeline
   - PDF ↔ Word, HTML → PDF
   - Text extraction
   - OCR processing

3. **Pack/Unpack** - Document packaging
   - IDocumentPacker
   - IPackageManager
   - ZIP/TAR support
   - Package metadata

**Questions for Review:**
- Is this 3-feature split correct?
- Should OCR be a separate feature?
- Should document search be a separate feature (beyond basic metadata)?
- Any other document operations needed (merge, split, watermark)?

---

### Epic 7: Identity & Session Management
**Priority:** HIGH
**LOC:** ~500
**Purpose:** Enhanced identity with sessions and rights

**Proposed Features:**
1. **Identity Abstractions** - Enhanced contracts
   - User sessions
   - Rights management
   - Claims enhancements

2. **Identity Extensions** - Implementation
   - Authorization services
   - Globalization support
   - Claims enhancement services

3. **Session Management** - User session tracking
   - Session creation/validation
   - Session persistence
   - Session expiration

**Questions for Review:**
- Should sessions be separate from identity?
- Should rights/permissions be a separate epic?
- Any authentication providers needed (Azure B2C, OAuth, SAML)?

---

### Epic 8: Complex Events (Event Sourcing)
**Priority:** MEDIUM
**LOC:** ~1,400
**Purpose:** Event sourcing, CQRS, scheduling

**Proposed Features:**
1. **Event Sourcing Abstractions** - CQRS contracts
   - Event contracts
   - Command/Query separation

2. **Event Sourcing Core** - Implementation
   - Event resolvers
   - Event subscribers
   - Event replay

3. **Event Scheduling** - Cron-based scheduling
   - `[ScheduleAt("0 */45 * * * *")]` attribute
   - Cron expression parsing
   - Scheduled execution

4. **Event Persistence** - Storage
   - EF Core provider
   - Database schema

5. **Azure EventHub Provider** - Big data streaming
   - Azure EventHub integration

**Questions for Review:**
- Is this too many features in one epic?
- Should CQRS be separate from Event Sourcing?
- Should scheduling be its own epic (reusable beyond events)?
- Should Azure EventHub be in a separate "Cloud Integrations" epic?

---

### Epic 9: Test Data Generation
**Priority:** MEDIUM
**LOC:** ~1,800
**Purpose:** Attribute-driven test data generation

**Proposed Features:**
1. **Generation Abstractions** - Generation contracts
   - `[EmailAddress]`, `[Address]`, `[Phone]` attributes
   - Generation rules

2. **Generation Core** - Generators
   - Email generator
   - Address generator
   - Phone generator
   - SSN, Credit Card, etc.
   - Seeded randomization

3. **Generation Extensions** - DI integration
   - Service registration
   - Type converters

**Questions for Review:**
- Should this be 3 features or combined into 2?
- Should generators be grouped by domain (Person, Financial, Location)?
- Any other data types needed (Company, Product, Order)?

---

## Cross-Cutting Concerns to Consider

### Logging & Observability
**Question:** Should there be an epic for logging/telemetry infrastructure?
- Structured logging helpers
- Correlation ID tracking
- OpenTelemetry integration
- Log aggregation patterns

### Configuration Management
**Question:** Should there be an epic for configuration patterns?
- IOptions pattern helpers
- Configuration validation
- Environment variable loading
- Secret management (Azure Key Vault, AWS Secrets Manager)

### Health Checks
**Question:** Should there be an epic for health check infrastructure?
- Provider health checks
- Dependency health checks
- Composite health check patterns

### Background Processing
**Question:** Should there be an epic for background jobs?
- Hangfire/Quartz integration
- Background job patterns
- Recurring job scheduling
- Job monitoring

---

## Potential New Epics (Not in SharedFramework)

### Epic: Workflow Engine
**Question:** Should we add a workflow/orchestration engine?
- State machine workflows
- Long-running processes
- Compensating transactions
- Saga pattern

### Epic: API Client Framework
**Question:** Should we add API client generation/helpers?
- Typed HTTP clients
- Retry policies (Polly)
- Circuit breakers
- Rate limiting

### Epic: Validation Framework
**Question:** Should we add validation infrastructure?
- FluentValidation integration
- Composite validators
- Validation pipelines
- Error formatting

---

## Questions for Reorganization

### 1. Epic Grouping
Should some epics be combined or split further?
- Combine Message Composition + Communications?
- Split Complex Events into multiple epics?
- Combine all Cloud Providers (SendGrid, Twilio, Azure EventHub) into one epic?

### 2. Feature Granularity
Are features at the right level of granularity?
- Too many features per epic?
- Features too large (should be split)?
- Features too small (should be combined)?

### 3. Priority Order
Should implementation order be different?
- Are foundation epics first (Data Enhancement, Templating)?
- Should provider epics come later?
- Should utilities come earlier?

### 4. Missing Features
What features are missing from current breakdown?
- Document search/indexing (beyond basic metadata)?
- OCR as standalone feature?
- Background job scheduling (beyond event scheduling)?
- Workflow/orchestration?

### 5. Epic Naming
Are epic names clear and consistent?
- "Pipeline" vs "Engine" vs "Framework" vs "Service"?
- "Enhancement" vs "Enrichment"?
- "Composition" vs "Generation"?

---

## Proposed Alternatives (Examples)

### Alternative 1: Group by Domain
Instead of technical features, group by business domain:
- **Notifications Epic** (Communications + Message Composition + Templates)
- **Documents Epic** (Persistence + Conversion + Packaging)
- **Data Epic** (Enhancement + Loading + Test Generation)
- **Events Epic** (Complex Events + Scheduling)

### Alternative 2: Group by Layer
Group by architectural layer:
- **Core Abstractions Epic** (IMessageData, Enhancement, Template contracts)
- **Infrastructure Epic** (Caching, Logging, Health Checks)
- **Domain Services Epic** (Communications, Documents, Spatial)
- **External Integrations Epic** (SendGrid, Twilio, Google Maps, etc.)

### Alternative 3: Keep Current (Feature-Based)
Keep current breakdown with minor adjustments

---

## Next Steps

**Please provide feedback on:**
1. **Epic structure** - Should any be combined/split?
2. **Feature breakdown** - Are features at right granularity?
3. **Missing pieces** - What features/epics are missing?
4. **Naming** - Any naming improvements?
5. **Priority** - Should implementation order change?

After your feedback, I'll adjust the structure and continue with detailed documentation.

---

**Current Status:**
- ✅ Master proposals index created
- ✅ Epic 11: Data Enhancement Pipeline (overview created)
- ✅ Epic 2: Communications (revised overview created)
- ✅ Architectural improvements documented
- ⏸️ Awaiting feedback before continuing

