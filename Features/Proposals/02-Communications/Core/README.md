# Feature: Communications Core Orchestration

**Epic:** Communications Platform
**Priority:** CRITICAL
**Status:** 📋 Design Phase
**LOC Impact:** ~1,145 (replaces 16 LOC stub)

---

## Overview

The Communications Core provides the central orchestration engine that coordinates multi-channel message delivery. It receives send requests, enriches message data, routes to appropriate channel composers, and handles deferred delivery.

**Key Responsibilities:**
1. **Request Orchestration** - Central entry point for all communication requests
2. **Channel Routing** - Route messages to Email/SMS/Push based on user preferences
3. **Data Enhancement** - Enrich messages with user/system context via provider pattern
4. **Message Composition** - Generate channel-specific messages from templates
5. **Deferral Management** - Schedule messages for future delivery
6. **Correlation Tracking** - Track message chains across channels
7. **Priority Handling** - Route High/Critical messages differently than Normal
8. **Provider Integration** - Coordinate with email/SMS providers

---

## Documentation

- **[Requirements](./requirements.md)** - Functional and non-functional requirements
- **[Architecture](./architecture.md)** - Component design with C4 diagrams
- **[API Design](./api-design.md)** - Interfaces and contracts
- **[Business Rules](./business-rules.md)** - Processing logic and flows
- **[Configuration](./configuration.md)** - Settings and options
- **[Testing Strategy](./testing-strategy.md)** - Test approach and coverage

---

## Quick Reference

### Main Entry Point

```csharp
public interface ICommunicationProvider
{
    Task<Guid> SendAsync(ISendRequest request, IDictionary<string, object>? headers = null);
    Task DeferAsync(ISendRequest request, DateTimeOffset until, IDictionary<string, object>? headers = null);
}
```

### Send Request Model

```csharp
public interface ISendRequest
{
    Guid TargetPersonId { get; }      // Who to send to
    string MessageType { get; }        // e.g., "order.confirmation"
    JObject Data { get; }              // Message data for template injection
    RequestPriorities Priority { get; } // Normal, High, Critical
}
```

### Processing Flow

```
Send Request
    ↓
Lookup User Preferences (channels, culture, timezone)
    ↓
Check Quiet Hours / Priority Filters
    ├─ Defer? → Queue for later delivery
    └─ Send Now ↓
Enhance Data (domain providers inject context)
    ↓
Route to Channel Composers (parallel)
    ├─ Email Composer → SendGrid/SMTP
    ├─ SMS Composer → Twilio
    └─ Push Composer → (future)
    ↓
Return Correlation ID
```

---

## Components

### 1. Communication Provider
**Class:** `CommunicationProvider`
**Interface:** `ICommunicationProvider`
**Responsibility:** Main facade for sending/deferring messages

### 2. Central Processor
**Class:** `CommunicationCentralProcessor`
**Interface:** `ICommunicationCentralProcessor`
**Responsibility:** Orchestrates multi-channel delivery

### 3. Target Preference Manager
**Class:** `TargetPreferenceManager`
**Interface:** `ITargetPreferenceManager`
**Responsibility:** Looks up user channel preferences

### 4. Data Enhancement Manager
**Class:** `DataEnhancementManager`
**Interface:** `IDataEnhancementManager`
**Responsibility:** Enriches message data via providers

### 5. Message Composer Factory
**Class:** `MessageComposerFactory`
**Interface:** `IMessageComposerFactory`
**Responsibility:** Creates channel-specific composers

### 6. Message Composers
**Classes:** `EmailMessageComposer`, `SmsMessageComposer`, `NoMessageComposer`
**Interface:** `IMessageComposer`
**Responsibility:** Generate channel-specific messages from templates

### 7. Deferral Manager
**Class:** `CommunicationDeferralProvider`
**Interface:** `IDeferralManager`
**Responsibility:** Schedule messages for future delivery

### 8. Template Provider
**Class:** `TemplateProvider`
**Interface:** `ITemplateProvider`
**Responsibility:** Load message templates by type and culture

---

## Key Features

### Attribute-Based Data Enhancement

Domain services register enhancement providers:

```csharp
[Communication(MessageType = "order.confirmation")]
public class OrderEnhancementProvider : IDataEnhancementProvider
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

System discovers and invokes providers automatically.

### Multi-Channel Parallel Delivery

If user prefers `[Email, SMS]`, system sends to BOTH channels simultaneously using `Task.WhenAll`.

### Quiet Hours Support

```csharp
ITargetPreference {
    TimeOnly? QuietHoursStart { get; }  // e.g., 22:00 (10 PM)
    TimeOnly? QuietHoursEnd { get; }    // e.g., 08:00 (8 AM)
}

// If current time is in quiet hours → defer until QuietHoursEnd
```

### Priority-Based Filtering

```csharp
ITargetPreference {
    RequestPriorities MinimumPriority { get; }  // Only High/Critical
}

// Normal priority requests ignored if user wants High+ only
```

---

## Dependencies

### OoBDev Framework
- `IStringFormatter` - Template variable substitution
- `ISelectedService<T>` - Provider selection
- `IObjectSerializer` - Deferral request serialization

### External
- Newtonsoft.Json (JObject for enhancement) OR System.Text.Json
- Microsoft.Extensions.Logging
- Microsoft.Extensions.DependencyInjection

---

## Next Steps

1. Review [requirements.md](./requirements.md) for detailed functional requirements
2. Study [architecture.md](./architecture.md) for component design
3. Review [api-design.md](./api-design.md) for interface contracts
4. Understand [business-rules.md](./business-rules.md) for processing logic
