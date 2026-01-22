# Send & Receive - Architecture

**Epic:** 2 - Communications Platform
**Feature:** Send & Receive
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Send & Receive implementation uses the **Command Pattern** for sending messages and **Observer Pattern** for receiving messages via webhooks and polling.

```
┌─────────────────────────────────────────────────────────────┐
│                  Application Service                        │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┴───────────┐
         ↓                       ↓
┌──────────────────┐   ┌──────────────────┐
│   ISendService   │   │ IReceiveService  │
│                  │   │                  │
│ - SendAsync()    │   │ - HandleWebhook()│
│ - SendMultiple() │   │ - PollAsync()    │
└────────┬─────────┘   └────────┬─────────┘
         │                      │
         ↓                      ↓
┌──────────────────┐   ┌──────────────────┐
│ IChannelProvider │   │IWebhookProcessor │
│                  │   │                  │
│ - SendAsync()    │   │ - Process()      │
└────────┬─────────┘   └────────┬─────────┘
         │                      │
         ↓                      ↓
┌──────────────────┐   ┌──────────────────┐
│IDeliveryTracker  │   │  Event Emitter   │
│                  │   │                  │
│ - RecordSent()   │   │ - OnReceived()   │
│ - UpdateStatus() │   │ - OnStatusUpdate()│
└──────────────────┘   └──────────────────┘
```

---

## Core Components

### 1. SendService (Command Pattern)

**Responsibilities:**
- Send messages via channels
- Validate message before sending
- Handle retries with exponential backoff
- Track delivery status
- Correlation ID management

**Key Design Decisions:**
- **Retry with exponential backoff** - Handles transient failures
- **Idempotent operations** - Duplicate detection via message ID
- **Async execution** - Non-blocking send operations

**Implementation Pattern:**
```csharp
public class SendService : ISendService
{
    private readonly IChannelRepository _channelRepository;
    private readonly IChannelRegistry _channelRegistry;
    private readonly IDeliveryTracker _deliveryTracker;
    private readonly ILogger<SendService> _logger;

    public async Task<SendResult> SendAsync(
        string channelName,
        IMessage message,
        SendOptions? options = null)
    {
        // 1. Get channel
        var channel = await _channelRepository.GetByNameAsync(channelName);
        if (channel == null)
        {
            throw new ChannelNotFoundException(channelName);
        }

        // 2. Get provider
        var provider = _channelRegistry.GetProvider(channel.Protocol, channel.Provider);
        if (provider == null)
        {
            throw new ProviderNotFoundException(channel.Protocol, channel.Provider);
        }

        // 3. Validate message
        if (!await provider.CanSendAsync(channel, message))
        {
            return new SendResult
            {
                Success = false,
                ErrorMessage = "Provider cannot send this message"
            };
        }

        // 4. Send with retry
        var result = await SendWithRetryAsync(provider, channel, message, options);

        // 5. Track delivery
        await _deliveryTracker.RecordSentAsync(result.MessageId, channelName, result);

        return result;
    }

    private async Task<SendResult> SendWithRetryAsync(
        IChannelProvider provider,
        IChannel channel,
        IMessage message,
        SendOptions? options)
    {
        var retryCount = options?.RetryCount ?? 3;
        var retryDelay = options?.RetryDelay ?? TimeSpan.FromSeconds(30);

        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            try
            {
                var result = await provider.SendAsync(channel, message);

                if (result.Success)
                {
                    return result;
                }

                // Check if error is transient
                if (!IsTransientError(result) || attempt == retryCount)
                {
                    return result;
                }

                // Wait before retry (exponential backoff)
                var delay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * Math.Pow(2, attempt));
                _logger.LogWarning("Send failed, retrying in {Delay}ms", delay.TotalMilliseconds);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send attempt {Attempt} failed", attempt + 1);

                if (attempt == retryCount)
                {
                    return new SendResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                }
            }
        }

        return new SendResult
        {
            Success = false,
            ErrorMessage = "All retry attempts failed"
        };
    }

    private bool IsTransientError(SendResult result)
    {
        // 5xx status codes = transient
        // 4xx status codes (except 429 rate limit) = permanent
        return (result.StatusCode >= 500 && result.StatusCode < 600) ||
               result.StatusCode == 429;  // Rate limit
    }
}
```

---

### 2. ReceiveService (Observer Pattern)

**Responsibilities:**
- Handle webhook callbacks
- Poll channels for messages
- Validate webhook signatures
- Emit events for received messages
- Update delivery status

**Key Design Decisions:**
- **Event-driven architecture** - Decoupled event processing
- **Provider-specific webhook processors** - Extensible for new providers
- **Signature validation** - Security best practice

**Implementation Pattern:**
```csharp
public class ReceiveService : IReceiveService
{
    private readonly IChannelRepository _channelRepository;
    private readonly IChannelRegistry _channelRegistry;
    private readonly IWebhookProcessorFactory _webhookProcessorFactory;
    private readonly IDeliveryTracker _deliveryTracker;
    private readonly List<Func<IMessage, Task>> _messageHandlers = new();
    private readonly List<Func<DeliveryStatusUpdate, Task>> _statusHandlers = new();

    public async Task<WebhookResult> HandleWebhookAsync(
        string channelName,
        object webhookPayload)
    {
        // 1. Get channel
        var channel = await _channelRepository.GetByNameAsync(channelName);
        if (channel == null)
        {
            return new WebhookResult
            {
                Success = false,
                ErrorMessage = $"Channel '{channelName}' not found"
            };
        }

        // 2. Get webhook processor for provider
        var processor = _webhookProcessorFactory.GetProcessor(channel.Provider);
        if (processor == null)
        {
            return new WebhookResult
            {
                Success = false,
                ErrorMessage = $"Webhook processor not found for provider '{channel.Provider}'"
            };
        }

        // 3. Process webhook
        var events = await processor.ProcessAsync(channel, webhookPayload);

        // 4. Handle events
        foreach (var evt in events)
        {
            if (evt is MessageReceivedEvent messageEvent)
            {
                await OnMessageReceivedAsync(messageEvent.Message);
            }
            else if (evt is DeliveryStatusEvent statusEvent)
            {
                await OnDeliveryStatusUpdatedAsync(statusEvent.Update);
            }
        }

        return new WebhookResult
        {
            Success = true,
            MessagesProcessed = events.Count(),
            MessageIds = events.Select(e => e.MessageId).ToList()
        };
    }

    public void OnMessageReceived(Func<IMessage, Task> handler)
    {
        _messageHandlers.Add(handler);
    }

    public void OnDeliveryStatusUpdated(Func<DeliveryStatusUpdate, Task> handler)
    {
        _statusHandlers.Add(handler);
    }

    private async Task OnMessageReceivedAsync(IMessage message)
    {
        foreach (var handler in _messageHandlers)
        {
            await handler(message);
        }
    }

    private async Task OnDeliveryStatusUpdatedAsync(DeliveryStatusUpdate update)
    {
        await _deliveryTracker.UpdateStatusAsync(update.MessageId, update.NewStatus, update.StatusDetails);

        foreach (var handler in _statusHandlers)
        {
            await handler(update);
        }
    }
}
```

---

### 3. DeliveryTracker (Repository Pattern)

**Responsibilities:**
- Store delivery records
- Update delivery status
- Query delivery history
- Correlation tracking

**Implementation Pattern:**
```csharp
public class DeliveryTracker : IDeliveryTracker
{
    private readonly CommunicationsDbContext _dbContext;
    private readonly ILogger<DeliveryTracker> _logger;

    public async Task RecordSentAsync(string messageId, string channelName, SendResult result)
    {
        var record = new DeliveryStatusRecord
        {
            MessageId = messageId,
            ChannelName = channelName,
            CorrelationId = result.CorrelationId,
            Status = result.Success ? DeliveryStatus.Sent : DeliveryStatus.Failed,
            QueuedAt = DateTimeOffset.UtcNow,
            SentAt = result.Success ? DateTimeOffset.UtcNow : null,
            StatusDetails = result.ErrorMessage
        };

        _dbContext.DeliveryRecords.Add(record);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Recorded delivery for message {MessageId}, status: {Status}",
            messageId, record.Status);
    }

    public async Task UpdateStatusAsync(string messageId, DeliveryStatus status, string? statusDetails = null)
    {
        var record = await _dbContext.DeliveryRecords
            .FirstOrDefaultAsync(r => r.MessageId == messageId);

        if (record != null)
        {
            var oldStatus = record.Status;
            record.Status = status;
            record.StatusDetails = statusDetails;

            if (status == DeliveryStatus.Delivered)
            {
                record.DeliveredAt = DateTimeOffset.UtcNow;
            }
            else if (status == DeliveryStatus.Failed || status == DeliveryStatus.Bounced)
            {
                record.FailedAt = DateTimeOffset.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated delivery status for message {MessageId}: {OldStatus} → {NewStatus}",
                messageId, oldStatus, status);
        }
        else
        {
            _logger.LogWarning("Delivery record not found for message {MessageId}", messageId);
        }
    }

    public async Task<DeliveryStatusRecord?> GetStatusAsync(string messageId)
    {
        return await _dbContext.DeliveryRecords
            .FirstOrDefaultAsync(r => r.MessageId == messageId);
    }

    public async Task<IEnumerable<DeliveryStatusRecord>> GetByCorrelationIdAsync(string correlationId)
    {
        return await _dbContext.DeliveryRecords
            .Where(r => r.CorrelationId == correlationId)
            .OrderBy(r => r.QueuedAt)
            .ToListAsync();
    }
}
```

---

## Data Flow

### Sequence: Send Message with Retry

```
┌─────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│   App   │    │SendService│   │  Provider│    │ Tracker  │    │   API    │
└────┬────┘    └─────┬────┘    └─────┬────┘    └─────┬────┘    └─────┬────┘
     │               │               │               │               │
     │ SendAsync()   │               │               │               │
     ├──────────────>│               │               │               │
     │               │ Get channel   │               │               │
     │               │ Get provider  │               │               │
     │               │               │               │               │
     │               │ SendAsync()   │               │               │
     │               ├──────────────>│               │               │
     │               │               │ Call API      │               │
     │               │               ├──────────────────────────────>│
     │               │               │               │               │
     │               │               │ 500 Error     │               │
     │               │               │<──────────────────────────────┤
     │               │               │               │               │
     │               │ Wait 30s (retry backoff)      │               │
     │               │               │               │               │
     │               │ SendAsync()   │               │               │
     │               ├──────────────>│               │               │
     │               │               │ Call API      │               │
     │               │               ├──────────────────────────────>│
     │               │               │               │               │
     │               │               │ 200 OK        │               │
     │               │               │<──────────────────────────────┤
     │               │               │               │               │
     │               │ Success       │               │               │
     │               │<──────────────┤               │               │
     │               │               │               │               │
     │               │ RecordSentAsync()             │               │
     │               ├──────────────────────────────>│               │
     │               │               │               │               │
     │ SendResult    │               │               │               │
     │<──────────────┤               │               │               │
     │               │               │               │               │
```

---

## Design Patterns

### 1. Command Pattern
- `ISendService.SendAsync()` encapsulates send command
- Retry logic wrapped in command execution
- Tracking and logging separated from execution

### 2. Observer Pattern
- Event handlers registered via `OnMessageReceived()`, `OnDeliveryStatusUpdated()`
- Webhook events trigger registered handlers
- Decoupled event processing

### 3. Repository Pattern
- `IDeliveryTracker` for delivery record persistence
- Abstraction over database access
- Query methods for delivery history

### 4. Factory Pattern
- `IWebhookProcessorFactory` creates provider-specific webhook processors
- Provider-specific payload parsing
- Extensible for new providers

---

## Performance Optimizations

### 1. Retry with Exponential Backoff
- Initial delay: 30 seconds
- Exponential factor: 2x
- Max retries: 3
- Avoids overwhelming provider APIs

### 2. Concurrent Sends
- Send to multiple channels in parallel
- Task-based async operations
- Scales to 1000+ concurrent sends

### 3. Webhook Batching
- Process webhook events in batches
- Reduces database roundtrips
- Improves throughput

---

## Error Handling

### Send Errors
```csharp
try
{
    var result = await _sendService.SendAsync("support-email", message);
}
catch (SendException ex) when (ex.IsTransient)
{
    // Transient error - will retry
    _logger.LogWarning(ex, "Transient send error");
}
catch (SendException ex)
{
    // Permanent error - no retry
    _logger.LogError(ex, "Permanent send error");
}
```

### Webhook Errors
```csharp
try
{
    var result = await _receiveService.HandleWebhookAsync("support-email", payload);
}
catch (WebhookValidationException ex)
{
    // Invalid signature - reject webhook
    _logger.LogError(ex, "Invalid webhook signature");
    return Unauthorized();
}
```

---

## Thread Safety

### Concurrency Strategy
- **SendService** - Stateless, thread-safe
- **ReceiveService** - Event handlers synchronized
- **DeliveryTracker** - Database transactions for atomicity

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 2 Overview](../README-REVISED.md)
