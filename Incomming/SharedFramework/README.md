# SharedFramework Migration - Quick Reference

**Source:** Application SharedFramework
**Date:** 2026-01-10
**Projects:** 52
**Status:** Ready for Integration

---

## What's Here

This directory contains 52 unique projects migrated from Application SharedFramework that provide new capabilities or significant enhancements to dotex.

### Quick Project Count by Category

- **Message Queueing:** 4 projects (Amazon SQS, Azure Service Bus)
- **Communications:** 10 projects (Twilio SendGrid, Twilio SMS, enhanced orchestration)
- **Spatial Services:** 7 projects (Census, Google Maps, Bing Maps, core abstractions)
- **Complex Events:** 7 projects (Event sourcing, CQRS, scheduling, Azure EventHub)
- **Data Loading:** 4 projects (ETL framework + CLI)
- **Code Generation:** 4 projects (Test data generation)
- **Document Management:** 3 projects (Enhanced document center)
- **Caching:** 7 projects (Distributed caching with Redis)
- **Identity/Session:** 3 projects (User session, claims enhancement)
- **Text Templating:** 3 projects (Template persistence)

---

## Top Priorities for Integration

### Immediate Value (Week 1-2)

1. **Amazon SQS** - AWS message queue support
2. **Azure Service Bus** - Enterprise messaging (topics, sessions, dead-letter)
3. **Twilio SendGrid** - Cloud email service
4. **Twilio SMS** - SMS messaging implementation

### High Impact (Week 3-4)

5. **Spatial Services** - Complete geocoding suite (Census, Google Maps)
6. **ComplexEvents** - Event sourcing, CQRS, cron-based scheduling
7. **DataLoader** - Database initialization and ETL

### Enhanced Capabilities (Month 2-3)

8. **Communications Orchestration** - Advanced multi-channel coordination
9. **Generations** - Attribute-driven test data generation
10. **Caching** - Distributed caching (Redis)

---

## Key New Capabilities

### ✅ Multi-Cloud Message Queues
- AWS via Amazon SQS
- Azure via Service Bus (topics, enterprise features)
- Existing: Azure Storage Queues, RabbitMQ, In-Process

### ✅ Complete Communication Suite
- Email: MailKit (SMTP/IMAP) + SendGrid (cloud)
- SMS: Twilio (first implementation!)
- Orchestration: Preferences, deferral, tracking, multi-channel

### ✅ Geocoding as a Service
- Provider pattern: Census (free), Google Maps (quality), Bing Maps (Microsoft)
- Unified `ILocationServices` interface
- Address validation and normalization

### ✅ Event-Driven Architecture
- Event sourcing and CQRS patterns
- Cron-based scheduled events (`[ScheduleAt("0 */45 * * * *")]`)
- Azure Event Hubs for big data streaming
- Event persistence and replay

### ✅ Data Loading Pipeline
- CSV/JSON to database ETL
- Seed data and reference data loading
- Alternative key lookup
- CLI tool for deployment automation

### ✅ Test Data Generation
- Attribute-driven: `[EmailAddress]`, `[Address]`, `[Phone]`
- Seeded randomization (reproducible)
- DI integration
- Alternative to Bogus/Faker

### ✅ Distributed Caching
- Provider pattern: Microsoft.Extensions.Caching, Redis
- Attribute-based: `[IsCacheable]`, `[FlushCache]`
- Testable abstraction layer

---

## Integration Steps

### 1. Namespace Renaming

Run MigrationHelper.Cli to rename `OoBDev.*` → `OoBDev.*`:

```bash
dotnet run --project src/Tools/OoBDev.MigrationHelper.Cli
```

Configure in Program.cs:
```csharp
var path = @"C:\repo\dotex\Incomming\SharedFramework";
var sourcePrefix = "OoBDev";
var targetPrefix = "OoBDev";
```

### 2. Add to Solution

Add projects to `OoBDev.sln` in appropriate categories:
- ExternalServices: Amazon.Sqs, Twilio.*, Census, GoogleMaps, etc.
- Framework: ComplexEvents, DataLoader, Generations, Caching, SpatialServices

### 3. Dependency Installation

Install required NuGet packages:
- `AWSSDK.SQS` - Amazon SQS
- `Azure.Messaging.ServiceBus` - Service Bus
- `Azure.Messaging.EventHubs` - Event Hubs
- `SendGrid` - SendGrid email
- `Twilio` - Twilio SMS
- `NCrontab` - Cron expressions
- `CsvHelper` - CSV reading
- `YamlDotNet` - YAML/JSON reading
- `StackExchange.Redis` - Redis caching

### 4. Testing

Test each new provider independently:
- Amazon SQS message send/receive
- Azure Service Bus queue/topic operations
- SendGrid email delivery
- Twilio SMS delivery
- Geocoding via each provider
- ComplexEvents scheduling
- DataLoader CLI execution
- Test data generation
- Redis caching

### 5. Documentation

Update documentation:
- `FEATURE_INVENTORY.md` - Add 52 new projects
- `ConfigurationSettings.md` - Add new config sections
- Library documentation for each project
- Example applications

---

## Directory Structure

```
SharedFramework/
├── OoBDev.Accounting.Contracts/
├── OoBDev.Amazon.Sqs/
├── OoBDev.Amazon.Sqs.Tests/
├── OoBDev.Api.Census.Geocoding/
├── OoBDev.Api.Census.Geocoding.Tests/
├── OoBDev.Api.Google.Maps/
├── OoBDev.Api.Google.Maps.Tests/
├── OoBDev.Api.Microsoft.BingMaps/
├── OoBDev.Api.Microsoft.BingMaps.Tests/
├── OoBDev.Api.Microsoft.Caching/
├── OoBDev.Api.Microsoft.Caching.Tests/
├── OoBDev.Api.Redis.Caching/
├── OoBDev.Api.Redis.Caching.Tests/
├── OoBDev.Api.Twilio.SendGrid/
├── OoBDev.Api.Twilio.SendGrid.Tests/
├── OoBDev.Api.Twilio.SmsMessaging/
├── OoBDev.Api.Twilio.SmsMessaging.Tests/
├── OoBDev.Azure.EventHub/
├── OoBDev.Azure.EventHub.Tests/
├── OoBDev.Azure.ServiceBus/
├── OoBDev.Azure.ServiceBus.Tests/
├── OoBDev.Caching.Common/
├── OoBDev.Caching.Common.Tests/
├── OoBDev.Caching.Contracts/
├── OoBDev.Communications/
├── OoBDev.Communications.Contracts/
├── OoBDev.Communications.Tests/
├── OoBDev.ComplexEvents.Common/
├── OoBDev.ComplexEvents.Common.Tests/
├── OoBDev.ComplexEvents.Contracts/
├── OoBDev.ComplexEvents.DatabaseExtensions/
├── OoBDev.ComplexEvents.EntityFrameworkCore/
├── OoBDev.DataLoader/
├── OoBDev.DataLoader.Cli/
├── OoBDev.DataLoader.Contracts/
├── OoBDev.DataLoader.Tests/
├── OoBDev.DocumentCenter/
├── OoBDev.DocumentCenter.Contracts/
├── OoBDev.DocumentCenter.Tests/
├── OoBDev.Generations/
├── OoBDev.Generations.Contracts/
├── OoBDev.Generations.Extensions.DependencyInjection/
├── OoBDev.Generations.Tests/
├── OoBDev.IdentityModel.Contracts/
├── OoBDev.IdentityModel.Extensions/
├── OoBDev.IdentityModel.Tests/
├── OoBDev.SpatialServices.Common/
├── OoBDev.SpatialServices.Common.Tests/
├── OoBDev.SpatialServices.Contracts/
├── OoBDev.TextTemplating/
├── OoBDev.TextTemplating.Contracts/
└── OoBDev.TextTemplating.Tests/
```

---

## Related Documents

- **SHAREDFRAMEWORK_MIGRATION.md** - Complete migration guide with detailed analysis
- **../FEATURE_INVENTORY.md** - dotex feature inventory (to be updated)
- **../../src/Tools/OoBDev.MigrationHelper.Cli/README.md** - Namespace migration tool

---

**Next Steps:** Review SHAREDFRAMEWORK_MIGRATION.md for detailed integration roadmap and priority recommendations.
