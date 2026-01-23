# Channel Abstraction - Requirements

**Epic:** 2 - Communications Platform
**Feature:** Channel Abstraction
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~200

---

## Overview

Channel abstraction provides a unified interface for communication channels (Email, SMS, Push Notifications, Slack, Teams, Live Chat) with protocol + provider + name pattern. This enables extensible, pluggable communication providers.

---

## Business Requirements

### BR-1: Multi-Channel Support
**As a** system administrator
**I want** to configure multiple communication channels
**So that** the system can send messages via Email, SMS, Push, Slack, Teams, and other channels

**Acceptance Criteria:**
- Support Email channel (SMTP, SendGrid, MailKit)
- Support SMS channel (Twilio, AWS SNS)
- Support Push Notification channel (Firebase, APNs)
- Support Slack channel (Slack API)
- Support Microsoft Teams channel (Teams API)
- Support extensible channel providers
- Each channel identified by name, protocol, and provider

---

### BR-2: Channel Abstraction Pattern
**As a** developer
**I want** channels to follow a consistent abstraction pattern
**So that** I can work with any channel type using the same interface

**Acceptance Criteria:**
- Channel has **Name** (e.g., "sales-team-slack", "support-email")
- Channel has **Protocol** (e.g., "email", "sms", "slack")
- Channel has **Provider** (e.g., "sendgrid", "twilio", "slack-api")
- Channel has **Configuration** (provider-specific settings)
- All channels implement `IChannel` interface

**Example:**
```csharp
// Email channel via SendGrid
var channel = new Channel
{
    Name = "support-email",
    Protocol = "email",
    Provider = "sendgrid",
    Configuration = new Dictionary<string, object>
    {
        ["ApiKey"] = "SG.xxx",
        ["FromEmail"] = "support@company.com"
    }
};

// Slack channel
var slackChannel = new Channel
{
    Name = "sales-team-slack",
    Protocol = "slack",
    Provider = "slack-api",
    Configuration = new Dictionary<string, object>
    {
        ["WebhookUrl"] = "https://hooks.slack.com/services/xxx",
        ["Channel"] = "#sales"
    }
};
```

---

### BR-3: Channel Provider Registry
**As a** system
**I want** a registry of available channel providers
**So that** I can dynamically discover and instantiate providers at runtime

**Acceptance Criteria:**
- Registry discovers all `IChannelProvider` implementations
- Registry matches providers by protocol (e.g., "email" → SendGridProvider)
- Registry supports multiple providers per protocol
- Registry supports provider selection criteria (cost, reliability, region)

---

### BR-4: Channel Discovery
**As a** developer
**I want** to discover available channels by protocol
**So that** I can list all email channels, all SMS channels, etc.

**Acceptance Criteria:**
- List all channels
- Filter channels by protocol (e.g., "email")
- Filter channels by provider (e.g., "sendgrid")
- Get channel by name

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IChannel
{
    string Name { get; }      // Unique channel name
    string Protocol { get; }  // Protocol type: "email", "sms", "slack", etc.
    string Provider { get; }  // Provider name: "sendgrid", "twilio", etc.
    IDictionary<string, object> Configuration { get; }
    bool IsEnabled { get; }
    DateTimeOffset? CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
}

public interface IChannelProvider
{
    string ProviderName { get; }  // e.g., "sendgrid", "twilio"
    string[] SupportedProtocols { get; }  // e.g., ["email"], ["sms"]

    Task<bool> CanSendAsync(IChannel channel, IMessage message);
    Task<SendResult> SendAsync(IChannel channel, IMessage message);
    Task<IMessage?> ReceiveAsync(IChannel channel);
    Task RegisterWebhookAsync(IChannel channel, string webhookUrl);
    Task UnregisterWebhookAsync(IChannel channel);
}

public interface IChannelRegistry
{
    void RegisterProvider(IChannelProvider provider);
    IChannelProvider? GetProvider(string protocol, string providerName);
    IEnumerable<IChannelProvider> GetProvidersByProtocol(string protocol);
    IEnumerable<string> GetSupportedProtocols();
}

public interface IChannelRepository
{
    Task<IChannel?> GetByNameAsync(string name);
    Task<IEnumerable<IChannel>> GetByProtocolAsync(string protocol);
    Task<IEnumerable<IChannel>> GetAllAsync();
    Task<IChannel> CreateAsync(IChannel channel);
    Task UpdateAsync(IChannel channel);
    Task DeleteAsync(string name);
}
```

---

### TR-2: Protocol Types
**Standard Protocols:**
- `"email"` - Email (SMTP, API-based)
- `"sms"` - SMS/Text messaging
- `"push"` - Push notifications (iOS, Android, Web)
- `"slack"` - Slack messaging
- `"teams"` - Microsoft Teams
- `"webhook"` - Generic webhook
- `"chatbot"` - Chatbot integrations

**Extensibility:**
- Custom protocols supported
- Protocol names lowercase, alphanumeric only
- Protocol validation in channel creation

---

### TR-3: Provider Naming Convention
**Format:** `{vendor}-{service}` (lowercase, hyphen-separated)

**Examples:**
- `sendgrid` - SendGrid email
- `smtp` - Standard SMTP
- `mailkit` - MailKit email
- `twilio` - Twilio SMS
- `aws-sns` - AWS Simple Notification Service
- `firebase` - Firebase Cloud Messaging
- `slack-api` - Slack API
- `microsoft-teams` - Microsoft Teams

---

### TR-4: Channel Configuration
**Configuration storage:**
- Dictionary<string, object> for flexibility
- Provider-specific configuration keys
- Sensitive data (API keys) encrypted at rest
- Configuration validation before channel creation

**Common Configuration Keys:**
```csharp
// Email provider configuration
{
    "ApiKey": "SG.xxx",
    "FromEmail": "noreply@company.com",
    "FromName": "Company Name",
    "ReplyToEmail": "support@company.com"
}

// SMS provider configuration
{
    "AccountSid": "AC123",
    "AuthToken": "xxx",
    "FromPhoneNumber": "+15551234567"
}

// Slack provider configuration
{
    "WebhookUrl": "https://hooks.slack.com/services/xxx",
    "Channel": "#general",
    "BotToken": "xoxb-xxx"
}
```

---

### TR-5: Channel Lifecycle
**States:**
- **Enabled** - Channel active, can send/receive
- **Disabled** - Channel inactive, no operations allowed
- **Archived** - Soft-deleted, retained for audit

**Operations:**
- Create channel → Validates configuration, stores in repository
- Update channel → Updates configuration, validates settings
- Enable/Disable → Changes `IsEnabled` flag
- Delete channel → Soft-delete (archive) or hard-delete

---

### TR-6: Provider Capabilities
**Required Capabilities:**
- `SendAsync()` - All providers MUST support sending

**Optional Capabilities:**
- `ReceiveAsync()` - Polling-based receive (optional)
- `RegisterWebhookAsync()` - Webhook-based receive (optional)
- `CanSendAsync()` - Pre-flight check (recommended)

**Capability Detection:**
```csharp
public interface IChannelProvider
{
    bool SupportsSending => true;  // All providers support sending
    bool SupportsReceiving { get; }  // Optional
    bool SupportsWebhooks { get; }   // Optional
}
```

---

### TR-7: Performance Requirements
- **Channel lookup:** < 10ms (cached in registry)
- **Provider discovery:** < 50ms (registry scans at startup)
- **Channel validation:** < 100ms (validates configuration)
- **Registry caching:** In-memory cache for channel instances

---

### TR-8: Thread Safety
- Channel registry is thread-safe for concurrent access
- Channel repository uses database transactions
- Provider instances are stateless and thread-safe

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Supports async/await patterns
- Compatible with dependency injection

### NFR-2: Extensibility
- Custom protocols can be added
- Custom providers can be registered
- Configuration schema extensible

### NFR-3: Testability
- Mock providers for unit testing
- In-memory repository for integration tests
- Deterministic provider behavior

---

## Constraints

### C-1: Channel Name Constraints
- Must be unique across all channels
- Alphanumeric + hyphens + underscores only
- Max length: 100 characters
- Case-insensitive

### C-2: Protocol Constraints
- Lowercase alphanumeric only
- Max length: 50 characters
- Reserved protocols: "email", "sms", "push", "slack", "teams"

### C-3: Provider Constraints
- Providers must be stateless
- Providers must be thread-safe
- Provider exceptions wrapped in `ChannelException`

### C-4: Configuration Constraints
- Configuration keys limited to 1000 entries
- Configuration values max size: 64KB per value
- Sensitive data encrypted before storage

---

## Success Criteria

- ✅ Support Email, SMS, Push, Slack, Teams channels
- ✅ Channel abstraction: Name + Protocol + Provider
- ✅ Registry discovers all providers
- ✅ Multiple providers per protocol
- ✅ Thread-safe concurrent access
- ✅ 80%+ test coverage
- ✅ < 10ms channel lookup (cached)

---

## Out of Scope

- ❌ Message composition (handled by Epic 10, Epic 11)
- ❌ User preferences (separate feature in Epic 2)
- ❌ Message queuing (separate feature)
- ❌ Delivery tracking (separate feature)

---

## Dependencies

### Internal
- None (foundation component)

### External
- .NET 10.0 BCL
- System.Collections.Generic
- Database for channel storage (SQL Server, PostgreSQL, MongoDB)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 2 Overview](../README-REVISED.md)
