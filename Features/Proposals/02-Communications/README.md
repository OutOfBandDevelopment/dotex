# Epic 2: Communications Platform

**Priority:** CRITICAL
**Status:** 📋 Design Phase
**Complexity:** HIGH
**Impact:** ~2,500 LOC (replaces 16 LOC stub with production-ready implementation)

---

## Overview

The Communications Platform provides a **multi-channel communications orchestration system** that enables applications to send messages through various channels (Email, SMS, Push Notifications) with:

- **Channel Routing** - Automatic routing based on user preferences
- **Message Composition** - Template-based message generation per channel
- **Data Enhancement** - Enrich messages with user/system data before sending
- **Deferral Management** - Schedule messages for future delivery
- **Provider Abstraction** - Pluggable email/SMS providers (SendGrid, Twilio, custom)
- **Request Prioritization** - Normal, High, Critical priority levels
- **Correlation Tracking** - Track message chains across channels

---

## Business Problem

**Current State:** Main codebase has only a 16 LOC stub in `OoBDev.Communications` - essentially just dependency injection plumbing with no actual functionality.

**Desired State:** Production-ready multi-channel communications system that:
1. Sends emails via SendGrid (or custom SMTP)
2. Sends SMS via Twilio
3. Routes messages based on user channel preferences
4. Composes messages from templates with data injection
5. Enhances messages with user/system context
6. Defers messages for scheduled delivery
7. Tracks message correlation across channels
8. Supports extensibility for new channels (Push, WhatsApp, etc.)

---

## Architecture Components

### Layer 1: Abstractions (Framework)
**Project:** `OoBDev.Communications.Abstractions`
**LOC:** ~695 (vs 125 in main)
**Purpose:** Contracts and interfaces

**Key Interfaces:**
- `ICommunicationProvider` - Main entry point for sending messages
- `ICommunicationCentralProcessor` - Orchestrates multi-channel delivery
- `IMessageComposer` - Channel-specific message composition
- `ISendEmailProvider` - Email provider abstraction
- `ISendSmsProvider` - SMS provider abstraction
- `ITargetPreferenceManager` - User channel preference lookup
- `IDataEnhancementManager` - Message data enrichment
- `IDeferralManager` - Scheduled message delivery

**Key Models:**
- `IEmailMessage` - Email structure (To/Cc/Bcc, Subject, HTML/Text, Headers)
- `ISmsMessage` - SMS structure (To, From, Content)
- `ISendRequest` - Communication request (Target, MessageType, Data, Priority)
- `ITargetPreference` - User preferences (Channels[], Culture, Timezone)
- `NotificationCommunicationTypes` - Channel enum (Email, Sms, Push, etc.)
- `RequestPriorities` - Priority enum (Normal, High, Critical)

**Key Attributes:**
- `[Communication(MessageType, Priority)]` - Marks data enhancement providers

### Layer 2: Core Implementation (Framework)
**Project:** `OoBDev.Communications`
**LOC:** ~1,145 (vs 16 LOC stub in main)
**Purpose:** Orchestration engine

**Key Components:**

1. **Central Processor** (`CommunicationCentralProcessor`)
   - Receives send requests
   - Looks up user channel preferences
   - Enhances message data
   - Routes to appropriate channel composers
   - Handles deferrals for scheduled delivery
   - Manages correlation IDs

2. **Message Composers** (per channel)
   - `EmailMessageComposer` - Composes HTML/Text emails from templates
   - `SmsMessageComposer` - Composes SMS from templates
   - `NoMessageComposer` - Null object pattern for disabled channels

3. **Data Enhancement** (`DataEnhancementManager`)
   - Discovers providers via `[Communication]` attribute
   - Enriches message data with user/system context
   - Seeds correlation data (Headers, MessageType, CorrelationId)
   - Supports async data loading

4. **Target Preference Manager** (`TargetPreferenceManager`)
   - Looks up user communication preferences (DB/API/cache)
   - Determines enabled channels per user
   - Retrieves culture/timezone settings
   - Filters by priority level

5. **Deferral Manager** (`CommunicationDeferralProvider`)
   - Schedules messages for future delivery
   - Stores deferred requests (queue/DB)
   - Triggers re-processing at scheduled time
   - Supports time-based campaigns (e.g., "send at 9 AM local time")

6. **Template Provider** (`TemplateProvider`)
   - Loads message templates by MessageType and Culture
   - Supports template storage (DB, files, embedded resources)
   - Caches templates for performance

### Layer 3: Email Providers (ExternalServices)

#### SendGrid Provider
**Project:** `OoBDev.Twilio.SendGrid`
**LOC:** ~267
**Purpose:** Cloud email via SendGrid API

**Capabilities:**
- Send transactional emails
- HTML and plain text content
- Attachments
- Template variables
- CC/BCC support
- Custom headers
- Tracking/analytics integration

**Configuration:**
- `SendGrid:ApiKey` - SendGrid API key
- `SendGrid:FromAddress` - Default sender
- `SendGrid:FromName` - Default sender name

#### Alternative: SMTP Provider (existing in main)
**Project:** `OoBDev.MailKit` (already in main)
**Purpose:** Standard SMTP email

### Layer 4: SMS Providers (ExternalServices)

#### Twilio SMS Provider
**Project:** `OoBDev.Twilio.SmsMessaging`
**LOC:** ~151
**Purpose:** SMS via Twilio API

**Capabilities:**
- Send SMS messages
- MMS (multimedia messages)
- International numbers
- Status callbacks
- Delivery tracking

**Configuration:**
- `Twilio:AccountSid` - Twilio account SID
- `Twilio:AuthToken` - Twilio auth token
- `Twilio:FromNumber` - Default sender phone number

---

## Feature Breakdown

### Feature 1: Core Orchestration
**Path:** `./Core/`
**Status:** 📋 Design Phase

**Description:** Central message routing and orchestration engine that coordinates multi-channel delivery.

**Documentation:**
- [Requirements](./Core/requirements.md)
- [Architecture](./Core/architecture.md)
- [API Design](./Core/api-design.md)
- [Business Rules](./Core/business-rules.md)
- [Configuration](./Core/configuration.md)
- [Testing Strategy](./Core/testing-strategy.md)

### Feature 2: SendGrid Email Provider
**Path:** `./SendGridProvider/`
**Status:** 📋 Design Phase

**Description:** Cloud email service via SendGrid API.

**Documentation:**
- [Requirements](./SendGridProvider/requirements.md)
- [Architecture](./SendGridProvider/architecture.md)
- [API Design](./SendGridProvider/api-design.md)
- [Configuration](./SendGridProvider/configuration.md)
- [Testing Strategy](./SendGridProvider/testing-strategy.md)

### Feature 3: Twilio SMS Provider
**Path:** `./TwilioSmsProvider/`
**Status:** 📋 Design Phase

**Description:** SMS messaging via Twilio API.

**Documentation:**
- [Requirements](./TwilioSmsProvider/requirements.md)
- [Architecture](./TwilioSmsProvider/architecture.md)
- [API Design](./TwilioSmsProvider/api-design.md)
- [Configuration](./TwilioSmsProvider/configuration.md)
- [Testing Strategy](./TwilioSmsProvider/testing-strategy.md)

### Feature 4: Communications Abstractions
**Path:** `./Abstractions/`
**Status:** 📋 Design Phase

**Description:** Core interfaces and contracts for all communication channels.

**Documentation:**
- [Requirements](./Abstractions/requirements.md)
- [API Design](./Abstractions/api-design.md)

---

## User Journeys

### Journey 1: Send Welcome Email
```
Actor: Application Service
Goal: Send welcome email to new user

GIVEN a new user registration
  AND user prefers Email channel
  AND user culture is en-US
WHEN application calls ICommunicationProvider.SendAsync()
  WITH MessageType="user.welcome"
  AND TargetPersonId=userId
  AND Data={ FirstName, AccountType, ActivationLink }
THEN system:
  1. Looks up user preferences → returns [Email], en-US culture
  2. Enhances data with user profile (email address, timezone)
  3. Loads email template for "user.welcome" + en-US
  4. Composes HTML email with injected data
  5. Sends via SendGrid provider
  6. Returns correlation ID for tracking
```

### Journey 2: Multi-Channel Notification
```
Actor: Application Service
Goal: Send password reset to user via their preferred channels

GIVEN existing user requested password reset
  AND user prefers [Email, SMS] channels
  AND request has High priority
WHEN application calls ICommunicationProvider.SendAsync()
  WITH MessageType="security.password-reset"
  AND Priority=High
  AND Data={ ResetCode, ExpiresAt }
THEN system:
  1. Looks up user preferences → returns [Email, SMS], en-US
  2. Enhances data with user contact info
  3. Composes BOTH email and SMS in parallel:
     a. Email: Full HTML with explanation + reset link
     b. SMS: Brief message with reset code
  4. Sends via SendGrid (email) and Twilio (SMS)
  5. Returns same correlation ID for both channels
```

### Journey 3: Scheduled Campaign
```
Actor: Marketing Service
Goal: Schedule promotional email for 9 AM local time

GIVEN marketing campaign targeting 10,000 users
  AND users across multiple timezones
  AND messages should arrive at 9 AM local time
WHEN service calls ICommunicationProvider.SendAsync()
  WITH MessageType="campaign.spring-sale"
  AND DeferUntil=user.Timezone.ConvertToUtc(9:00 AM)
  FOR EACH user
THEN system:
  1. For each user:
     a. Calculates 9 AM in user's timezone
     b. Stores deferred request with HoldUntil timestamp
  2. Deferral processor triggers at scheduled times
  3. Reprocesses requests through normal flow
  4. Sends emails as they reach scheduled time
```

### Journey 4: Data Enhancement
```
Actor: Order Service
Goal: Send order confirmation with full order details

GIVEN order placed successfully
  AND OrderConfirmation data enhancement provider registered
  AND Provider enriches with: Order line items, Shipping address, Total
WHEN service calls ICommunicationProvider.SendAsync()
  WITH MessageType="order.confirmation"
  AND Data={ OrderId }  // Minimal data
THEN system:
  1. Discovers OrderConfirmationEnhancementProvider via [Communication] attribute
  2. Calls provider.EnhanceAsync(OrderId, Data)
  3. Provider loads full order from database
  4. Returns enriched data: { OrderId, LineItems[], ShippingAddress, Total, EstimatedDelivery }
  5. Composes email template with all order details
  6. Sends confirmation email
```

---

## Key Design Decisions

### 1. Provider Pattern for Channels
**Decision:** Use provider/factory pattern for all channel implementations (Email, SMS, Push)

**Rationale:**
- Allows swapping providers (SendGrid ↔ SMTP, Twilio ↔ Plivo)
- Supports testing with mock providers
- Enables multi-provider scenarios (primary/fallback)

**Pattern:**
```csharp
services.AddCommunications()
    .AddSendGridProvider(options => { /* ... */ })
    .AddTwilioSmsProvider(options => { /* ... */ });

// System uses ISelectedService<ISendEmailProvider> to pick provider
// Configured via: OoBDev:ServiceKeys:OoBDev.Communications.ISendEmailProvider = "SendGrid"
```

### 2. Attribute-Based Data Enhancement
**Decision:** Use `[Communication(MessageType)]` attribute to discover data enhancement providers

**Rationale:**
- Decouples message sending from data loading
- Allows domain services to enrich their own message types
- Supports dependency injection in enhancement providers
- Enables async data loading without blocking sender

**Pattern:**
```csharp
[Communication(MessageType = "order.confirmation", Priority = RequestPriorities.High)]
public class OrderConfirmationEnhancement : IDataEnhancementProvider
{
    public async Task<JObject> EnhanceAsync(Guid targetPersonId, string messageType, JObject data)
    {
        var orderId = data["OrderId"].Value<int>();
        var order = await _orderRepository.GetByIdAsync(orderId);

        data["LineItems"] = JArray.FromObject(order.LineItems);
        data["Total"] = order.Total;
        return data;
    }
}
```

### 3. Template-Based Message Composition
**Decision:** Use template engine for message content, NOT hardcoded strings

**Rationale:**
- Supports localization (multiple cultures per MessageType)
- Allows non-developers to edit templates
- Enables A/B testing by swapping templates
- Separates content from code

**Pattern:**
```
Templates/
  └── email/
      └── order.confirmation/
          ├── en-US.html
          ├── en-US.txt
          ├── es-MX.html
          └── es-MX.txt

Template content uses variable substitution:
"Hi {{FirstName}}, your order #{{OrderId}} totaling {{Total}} has shipped!"
```

### 4. Deferral for Scheduled Delivery
**Decision:** Support deferred message delivery via separate deferral manager

**Rationale:**
- Enables time-based campaigns ("send at 9 AM local time")
- Supports retry logic (defer on provider failure)
- Allows message aggregation ("daily digest at 6 PM")
- Decouples scheduling from sending

**Pattern:**
```csharp
// Option 1: Explicit deferral
await _communicationProvider.DeferAsync(
    targetPersonId, messageType, data,
    until: user.Timezone.ConvertToUtc(new TimeOnly(9, 0)));

// Option 2: Auto-deferral based on preference
// User preference: "No emails between 10 PM - 8 AM"
// System automatically defers to 8 AM if sent at 11 PM
```

### 5. Channel Preference Management
**Decision:** User preferences control which channels receive messages

**Rationale:**
- Supports user opt-in/opt-out per channel
- Enables channel prioritization (SMS only for Critical)
- Respects quiet hours per channel
- GDPR/compliance friendly

**Pattern:**
```csharp
ITargetPreference {
    Guid TargetPersonId { get; }
    string[] Channels { get; }  // ["Email", "SMS"]
    CultureInfo Culture { get; }
    TimeZoneInfo Timezone { get; }
    RequestPriorities MinimumPriority { get; }  // Only High/Critical
    TimeOnly? QuietHoursStart { get; }  // No messages 10 PM - 8 AM
    TimeOnly? QuietHoursEnd { get; }
}
```

---

## Success Metrics

### Functional Requirements
- ✅ Send emails via SendGrid OR SMTP
- ✅ Send SMS via Twilio
- ✅ Route messages to multiple channels simultaneously
- ✅ Compose messages from templates (HTML/Text/SMS)
- ✅ Inject data variables into templates
- ✅ Enhance message data from domain providers
- ✅ Defer messages for scheduled delivery
- ✅ Respect user channel preferences
- ✅ Support multiple cultures/languages
- ✅ Track correlation across channels

### Non-Functional Requirements
- ✅ 80%+ test coverage
- ✅ Support 100+ messages/second throughput
- ✅ < 500ms processing time (excluding provider API calls)
- ✅ Graceful degradation on provider failures
- ✅ Async/await throughout (no blocking calls)
- ✅ Structured logging with correlation IDs
- ✅ Configuration-based provider selection

---

## Dependencies

### NuGet Packages
- **SendGrid** - SendGrid cloud email API
- **Twilio** - Twilio SMS/MMS API
- **Newtonsoft.Json** - JSON data handling (JObject for enhancement)
- **Microsoft.Extensions.DependencyInjection** - DI framework
- **Microsoft.Extensions.Logging** - Logging abstractions

### OoBDev Framework
- **OoBDev.System** - `IStringFormatter` (template variable substitution)
- **OoBDev.System** - `ISelectedService<T>` (provider selection)
- **OoBDev.System** - `IObjectSerializer` (deferral request serialization)
- **OoBDev.MailKit** - Alternative SMTP email provider (already in main)

### External Services (Runtime)
- **SendGrid Account** - Cloud email service (optional, SMTP works too)
- **Twilio Account** - SMS service
- **Template Storage** - Database or file system for templates
- **Preference Storage** - User channel preferences (database/API)

---

## Migration from SharedFramework

**Source:**
- `Incoming/SharedFramework/OoBDev.Communications` (1,145 LOC)
- `Incoming/SharedFramework/OoBDev.Communications.Abstractions` (695 LOC)
- `Incoming/SharedFramework/OoBDev.Twilio.SendGrid` (267 LOC)
- `Incoming/SharedFramework/OoBDev.Twilio.SmsMessaging` (151 LOC)

**Target:**
- `src/Framework/OoBDev.Communications.Abstractions/`
- `src/Framework/OoBDev.Communications/`
- `src/ExternalServices/Twilio/OoBDev.Twilio.SendGrid/`
- `src/ExternalServices/Twilio/OoBDev.Twilio.SmsMessaging/`

**Changes from Source:**
- Upgrade .NET 8.0 → .NET 10.0
- Replace `Newtonsoft.Json.JObject` with `System.Text.Json` (or keep for compatibility)
- Update to latest SendGrid/Twilio SDK versions
- Add nullable reference types
- Modernize async/await patterns
- Enhance error handling
- Add comprehensive logging
- Follow OoBDev dependency injection patterns (`TryAdd*`)

---

## Related Documentation

- [SharedFramework Feature Mapping](../../../docs/migration/sharedframework-feature-mapping.md#2-communications-6-projects-2500-loc)
- [SharedFramework Migration Plan - Phase 2](../../../docs/migration/sharedframework-migration-plan.md#phase-2-communications-complete-implementation-critical)

---

## Next Steps

1. **Complete feature documentation** for all 4 components
2. **Review with stakeholders** for requirements validation
3. **Create implementation plan** with task breakdown
4. **Implement in priority order:**
   - Phase 1: Abstractions + Core (replaces 16 LOC stub)
   - Phase 2: SendGrid Provider (cloud email)
   - Phase 3: Twilio SMS Provider (enables SMS channel)
5. **Comprehensive testing** (Unit, Integration with Docker, LiveIntegration with real services)
6. **Documentation** (README per project, usage examples, migration guide)
