# Send & Receive - Requirements

**Epic:** 2 - Communications Platform
**Feature:** Send & Receive
**Priority:** HIGH (Core Functionality)
**Complexity:** MEDIUM
**Estimated LOC:** ~250

---

## Overview

Send & Receive provides the core messaging functionality for the communications platform, enabling applications to send messages via channels and receive inbound messages through webhooks or polling.

---

## Business Requirements

### BR-1: Send Messages
**As a** developer
**I want** to send messages via configured channels
**So that** users receive notifications, alerts, and communications

**Acceptance Criteria:**
- Send email messages via Email channels
- Send SMS messages via SMS channels
- Send messages to Slack, Teams, and other channels
- Support synchronous and asynchronous sending
- Return send result with message ID
- Track delivery status

**Example:**
```csharp
// Send email
var emailMessage = new EmailMessage
{
    To = new[] { "customer@example.com" },
    Subject = "Order Confirmation",
    HtmlContent = "<h1>Thank you!</h1>",
    TextContent = "Thank you for your order!"
};

var result = await _sendService.SendAsync("support-email", emailMessage);

if (result.Success)
{
    Console.WriteLine($"Message sent: {result.MessageId}");
}
```

---

### BR-2: Receive Messages (Webhooks)
**As a** system
**I want** to receive inbound messages via webhooks
**So that** I can process replies, callbacks, and delivery notifications

**Acceptance Criteria:**
- Register webhooks with channel providers
- Handle webhook callbacks (POST requests)
- Validate webhook signatures
- Parse provider-specific webhook payloads
- Emit events for received messages
- Support delivery status updates (delivered, failed, bounced)

**Example:**
```csharp
// Handle webhook from SendGrid
[HttpPost("/webhooks/sendgrid")]
public async Task<IActionResult> HandleSendGridWebhook([FromBody] SendGridWebhookPayload payload)
{
    var result = await _receiveService.HandleWebhookAsync("support-email", payload);

    if (result.Success)
    {
        return Ok();
    }

    return BadRequest(result.ErrorMessage);
}
```

---

### BR-3: Receive Messages (Polling)
**As a** system
**I want** to poll channels for inbound messages
**So that** I can receive messages from providers that don't support webhooks

**Acceptance Criteria:**
- Poll channels at configurable intervals
- Retrieve unread messages
- Mark messages as read after processing
- Support batch retrieval
- Handle polling failures gracefully

---

### BR-4: Delivery Tracking
**As a** developer
**I want** to track message delivery status
**So that** I can verify messages were delivered successfully

**Acceptance Criteria:**
- Track message states: Queued, Sent, Delivered, Failed, Bounced
- Store delivery timestamps
- Store provider-specific status codes
- Query delivery status by message ID
- Webhook updates for delivery events

**States:**
- **Queued** - Message queued for sending
- **Sent** - Message sent to provider
- **Delivered** - Message delivered to recipient
- **Failed** - Delivery failed (temporary or permanent)
- **Bounced** - Message bounced (invalid address)
- **Opened** - Recipient opened message (email tracking)
- **Clicked** - Recipient clicked link (email tracking)

---

### BR-5: Message Correlation
**As a** developer
**I want** to correlate messages across channels
**So that** I can track multi-channel notifications

**Acceptance Criteria:**
- Assign correlation ID to related messages
- Query messages by correlation ID
- Track message sequence (email → SMS → push)
- Associate replies with original messages

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface ISendService
{
    /// <summary>
    /// Sends message via channel.
    /// </summary>
    /// <param name="channelName">Channel name</param>
    /// <param name="message">Message to send</param>
    /// <param name="options">Optional send options (correlation ID, metadata)</param>
    Task<SendResult> SendAsync(
        string channelName,
        IMessage message,
        SendOptions? options = null);

    /// <summary>
    /// Sends message to user's preferred channel.
    /// </summary>
    Task<SendResult> SendToUserAsync(
        Guid userId,
        IMessage message,
        SendOptions? options = null);

    /// <summary>
    /// Sends message to multiple channels in parallel.
    /// </summary>
    Task<SendResult[]> SendToMultipleChannelsAsync(
        string[] channelNames,
        IMessage message,
        SendOptions? options = null);
}

public interface IReceiveService
{
    /// <summary>
    /// Handles webhook callback from provider.
    /// </summary>
    /// <param name="channelName">Channel name</param>
    /// <param name="webhookPayload">Webhook payload from provider</param>
    Task<WebhookResult> HandleWebhookAsync(
        string channelName,
        object webhookPayload);

    /// <summary>
    /// Polls channel for inbound messages.
    /// </summary>
    /// <param name="channelName">Channel name</param>
    /// <param name="maxMessages">Maximum messages to retrieve</param>
    Task<IEnumerable<IMessage>> PollAsync(
        string channelName,
        int maxMessages = 10);

    /// <summary>
    /// Registers event handler for received messages.
    /// </summary>
    void OnMessageReceived(Func<IMessage, Task> handler);

    /// <summary>
    /// Registers event handler for delivery status updates.
    /// </summary>
    void OnDeliveryStatusUpdated(Func<DeliveryStatusUpdate, Task> handler);
}

public interface IDeliveryTracker
{
    /// <summary>
    /// Records message sent event.
    /// </summary>
    Task RecordSentAsync(string messageId, string channelName, SendResult result);

    /// <summary>
    /// Updates delivery status.
    /// </summary>
    Task UpdateStatusAsync(string messageId, DeliveryStatus status, string? statusDetails = null);

    /// <summary>
    /// Gets delivery status for message.
    /// </summary>
    Task<DeliveryStatusRecord?> GetStatusAsync(string messageId);

    /// <summary>
    /// Gets all messages for correlation ID.
    /// </summary>
    Task<IEnumerable<DeliveryStatusRecord>> GetByCorrelationIdAsync(string correlationId);
}
```

---

### TR-2: Message Types
**Base Interface:**
```csharp
public interface IMessage
{
    string MessageId { get; }
    string? CorrelationId { get; }
    DateTimeOffset Timestamp { get; }
    IDictionary<string, object> Metadata { get; }
}
```

**Email Message:**
```csharp
public interface IEmailMessage : IMessage
{
    string[] To { get; }
    string[] Cc { get; }
    string[] Bcc { get; }
    string From { get; }
    string? ReplyTo { get; }
    string Subject { get; }
    string? TextContent { get; }
    string? HtmlContent { get; }
    IEnumerable<Attachment>? Attachments { get; }
}
```

**SMS Message:**
```csharp
public interface ISmsMessage : IMessage
{
    string To { get; }
    string From { get; }
    string Content { get; }
}
```

**Push Notification:**
```csharp
public interface IPushMessage : IMessage
{
    string[] DeviceTokens { get; }
    string Title { get; }
    string Body { get; }
    IDictionary<string, string> Data { get; }
}
```

---

### TR-3: Send Options
```csharp
public class SendOptions
{
    public string? CorrelationId { get; set; }
    public DateTimeOffset? ScheduledDeliveryTime { get; set; }
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public bool TrackOpens { get; set; } = false;
    public bool TrackClicks { get; set; } = false;
}
```

---

### TR-4: Send Result
```csharp
public class SendResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

---

### TR-5: Webhook Result
```csharp
public class WebhookResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int MessagesProcessed { get; set; }
    public IEnumerable<string> MessageIds { get; set; } = Array.Empty<string>();
}
```

---

### TR-6: Delivery Status
```csharp
public enum DeliveryStatus
{
    Queued,
    Sent,
    Delivered,
    Failed,
    Bounced,
    Opened,
    Clicked
}

public class DeliveryStatusRecord
{
    public string MessageId { get; set; }
    public string ChannelName { get; set; }
    public string? CorrelationId { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? StatusDetails { get; set; }
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public DateTimeOffset? FailedAt { get; set; }
}

public class DeliveryStatusUpdate
{
    public string MessageId { get; set; }
    public DeliveryStatus OldStatus { get; set; }
    public DeliveryStatus NewStatus { get; set; }
    public string? StatusDetails { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
```

---

### TR-7: Performance Requirements
- **Send latency:** < 200ms (excluding provider API time)
- **Webhook processing:** < 100ms
- **Polling interval:** Configurable (default: 60 seconds)
- **Batch send:** Support 100+ messages in parallel
- **Concurrent webhooks:** Handle 1000+ concurrent webhook requests

---

### TR-8: Error Handling
**Retry Strategy:**
- Transient errors: Retry with exponential backoff
- Permanent errors: No retry, mark as failed
- Max retries: 3 (configurable)
- Retry delay: 30s, 60s, 120s (exponential)

**Provider Errors:**
```csharp
public class SendException : Exception
{
    public string? MessageId { get; }
    public string? ChannelName { get; }
    public bool IsTransient { get; }

    public SendException(string message, string? messageId = null, bool isTransient = false)
        : base(message)
    {
        MessageId = messageId;
        IsTransient = isTransient;
    }
}
```

---

## Non-Functional Requirements

### NFR-1: Reliability
- Guarantee at-least-once delivery (with retries)
- Idempotent operations (duplicate detection)
- Message queue for resilience

### NFR-2: Scalability
- Handle 1000+ sends per second
- Horizontal scaling support
- Load balancing across providers

### NFR-3: Security
- Validate webhook signatures
- Encrypt sensitive message content
- Audit all send/receive operations

---

## Constraints

### C-1: Message Size Limits
- Email: 10 MB (including attachments)
- SMS: 1600 characters (segmented)
- Push: 4 KB payload

### C-2: Rate Limits
- Provider-specific rate limits (e.g., SendGrid: 100 emails/second)
- Respect provider quotas
- Queue messages if rate limit exceeded

### C-3: Delivery Guarantees
- Best-effort delivery (not guaranteed)
- Retries for transient failures only
- Provider-dependent delivery confirmation

---

## Success Criteria

- ✅ Send email, SMS, push, Slack, Teams messages
- ✅ Receive messages via webhooks and polling
- ✅ Track delivery status (Queued → Sent → Delivered)
- ✅ Message correlation across channels
- ✅ Retry failed sends (exponential backoff)
- ✅ 85%+ test coverage
- ✅ < 200ms send latency (excluding provider)

---

## Out of Scope

- ❌ Message composition (handled by Epic 10, Epic 11)
- ❌ User preferences (separate feature)
- ❌ Multi-channel routing (separate feature)
- ❌ Message templating (handled by Epic 10)

---

## Dependencies

### Internal
- Channel Abstraction (Feature 1) - Channel lookup and providers

### External
- SendGrid API - Email delivery
- Twilio API - SMS delivery
- Firebase/APNs - Push notifications
- Slack/Teams APIs - Chat messages
- Message queue (RabbitMQ, Azure Service Bus) - Reliable delivery

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Channel Abstraction](../ChannelAbstraction/requirements.md)
- [Epic 2 Overview](../README-REVISED.md)
