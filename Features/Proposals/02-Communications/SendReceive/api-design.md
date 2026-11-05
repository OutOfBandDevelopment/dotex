# Send & Receive - API Design

**Epic:** 2 - Communications Platform
**Feature:** Send & Receive
**Last Updated:** 2026-11-05

This document fixes the public surface of Send & Receive. The behaviour is in [requirements](./requirements.md) and [architecture](./architecture.md); the tests that pin it are in [testing-strategy](./testing-strategy.md).

---

## Packages and namespaces

| Package | Namespace | Holds |
|---------|-----------|-------|
| `OoBDev.Communications.Abstractions` | `OoBDev.Communications.Abstractions` | Interfaces, message and result types, enums (no implementation, no tests) |
| `OoBDev.Communications` | `OoBDev.Communications.SendReceive` | `SendService`, `ReceiveService`, `DeliveryTracker`, retry policy, `ServiceCollectionExtensions` |
| Provider adapters (`OoBDev.Communications.SendGrid`, `.Twilio`, ...) | `OoBDev.Communications.{Vendor}` | `IChannelProvider` and `IWebhookProcessor` per vendor; keys in `{Vendor}Globals` |

Adapters reference the Abstractions package only. The registration class is always `ServiceCollectionExtensions`.

---

## Contracts

The interfaces `ISendService`, `IReceiveService` and `IDeliveryTracker` are as written in requirements TR-1. The decisions below change that draft, because the framework rules differ from it.

| Draft | Final | Reason |
|-------|-------|--------|
| `Task<SendResult> SendAsync(string, IMessage, SendOptions?)` | adds a trailing `CancellationToken cancellationToken = default` to every async method | Cancellation on every I/O call (coding standards) |
| `object webhookPayload` | `WebhookRequest` (raw body bytes, headers, query, received time) | Signature checks need the exact bytes; a parsed object cannot be verified |
| `OnMessageReceived(Func<IMessage, Task>)` | `IMessageReceivedHandler` and `IDeliveryStatusHandler` registered in DI | The framework handler pattern; delegates cannot be retested or ordered |
| `DateTimeOffset.UtcNow` in the tracker | `TimeProvider` | Deterministic tests |
| `SendException(isTransient)` | `SendFailure` record on `SendResult` (`Kind`: `Transient`, `Permanent`, `RateLimited`, `Rejected`) | Failures are results, not exceptions; the caller branches on data |
| `SendResult` settable properties | init-only record | Immutable results |

```csharp
namespace OoBDev.Communications.Abstractions;

public interface ISendService
{
    Task<SendResult> SendAsync(string channelName, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default);
    Task<SendResult> SendToUserAsync(Guid userId, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SendResult>> SendToMultipleChannelsAsync(IReadOnlyCollection<string> channelNames, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default);
}

public interface IReceiveService
{
    Task<WebhookResult> HandleWebhookAsync(string channelName, WebhookRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IMessage>> PollAsync(string channelName, int maxMessages = 10, CancellationToken cancellationToken = default);
}

public interface IMessageReceivedHandler
{
    Task HandleAsync(IMessage message, CancellationToken cancellationToken = default);
}

public interface IDeliveryStatusHandler
{
    Task HandleAsync(DeliveryStatusUpdate update, CancellationToken cancellationToken = default);
}

public interface IWebhookProcessor
{
    string Provider { get; }
    bool ValidateSignature(IChannel channel, WebhookRequest request);
    Task<IReadOnlyList<ChannelEvent>> ProcessAsync(IChannel channel, WebhookRequest request, CancellationToken cancellationToken = default);
}

public interface IWebhookProcessorFactory
{
    IWebhookProcessor? GetProcessor(string provider);
}
```

### Records

```csharp
public sealed record WebhookRequest(
    ReadOnlyMemory<byte> Body,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    DateTimeOffset ReceivedAt);

public sealed record SendResult(
    bool Success,
    string MessageId,
    string? CorrelationId,
    string? ProviderMessageId,
    DateTimeOffset Timestamp,
    SendFailure? Failure = null);

public sealed record SendFailure(SendFailureKind Kind, string Message, int? StatusCode = null, TimeSpan? RetryAfter = null);

public enum SendFailureKind { Transient, Permanent, RateLimited, Rejected }

public sealed record WebhookResult(bool Success, int MessagesProcessed, IReadOnlyList<string> MessageIds, string? ErrorMessage = null);

public abstract record ChannelEvent(string MessageId);
public sealed record MessageReceivedEvent(string MessageId, IMessage Message) : ChannelEvent(MessageId);
public sealed record DeliveryStatusEvent(string MessageId, DeliveryStatusUpdate Update) : ChannelEvent(MessageId);
```

`DeliveryStatus`, `DeliveryStatusRecord`, `DeliveryStatusUpdate`, `IMessage`, `IEmailMessage`, `ISmsMessage` and `IPushMessage` keep the shapes of TR-2 and TR-6; the record fields become `init`.

### Options

```csharp
public sealed class SendOptions
{
    public string? CorrelationId { get; init; }
    public DateTimeOffset? ScheduledDeliveryTime { get; init; }
    public int? RetryCount { get; init; }
    public TimeSpan? RetryDelay { get; init; }
    public bool TrackOpens { get; init; }
    public bool TrackClicks { get; init; }
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

public sealed class SendReceiveOptions
{
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    public double BackoffFactor { get; set; } = 2.0;
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(60);
    public TimeSpan WebhookClockTolerance { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxParallelSends { get; set; } = 100;
}
```

`SendOptions` values override `SendReceiveOptions` per call (null means "use the configured value"). `SendReceiveOptions` binds from the `OoBDev:Communications:SendReceive` section and is validated with `AddValidatedOptions<T>()` (counts at least 0, delays positive, factor at least 1).

---

## Behaviour contract

| Operation | Contract |
|-----------|----------|
| `SendAsync` | Unknown channel throws `ChannelNotFoundException`; unknown provider throws `ProviderNotFoundException` (configuration errors). A provider refusal, a transient failure after the retries, a permanent failure: returns `Success = false` with a `SendFailure`. Never throws for provider outcomes. |
| Retry | Only `Transient` and `RateLimited` retry. Delay is `RetryDelay * BackoffFactor^attempt`, capped by `MaxRetryDelay`; a `RetryAfter` from the provider wins when longer. A cancelled token stops the loop and throws `OperationCanceledException`. |
| Idempotency | A repeated `MessageId` for the same channel returns the stored `SendResult` without a provider call. |
| `SendToMultipleChannelsAsync` | Sends in parallel (bounded by `MaxParallelSends`), results in the order of the input; one failure does not cancel the others. |
| `SendToUserAsync` | Resolves the channel from the user's preference (Epic 7); no preference returns a `Rejected` failure. |
| `ScheduledDeliveryTime` | The call records the message as `Queued` and hands it to the deferral queue (Core `IDeferralManager`); it does not wait. |
| `HandleWebhookAsync` | Unknown channel or provider returns `Success = false` (the endpoint must not throw on foreign input). A failed signature returns `Success = false` with `ErrorMessage = "invalid signature"` and no handler runs. A request older than `WebhookClockTolerance` is rejected when the provider signs a timestamp. Handlers run in registration order; an exception in one handler is logged and does not stop the others or fail the webhook. |
| `UpdateStatusAsync` | Status moves forward only along `Queued -> Sent -> Delivered`/`Failed`/`Bounced`, then `Opened`, `Clicked`; a stale event is ignored (logged at debug). An unknown message id is logged and ignored, never created. |
| `PollAsync` | Returns at most `maxMessages`, marks them read only after the handlers returned. |

---

## Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSendReceive(this IServiceCollection services, IConfiguration configuration);
    public static IServiceCollection TryAddDeliveryTracker<TTracker>(this IServiceCollection services) where TTracker : class, IDeliveryTracker;
    public static IServiceCollection TryAddMessageReceivedHandler<THandler>(this IServiceCollection services) where THandler : class, IMessageReceivedHandler;
    public static IServiceCollection TryAddDeliveryStatusHandler<THandler>(this IServiceCollection services) where THandler : class, IDeliveryStatusHandler;
}
```

`TryAddSendReceive` registers `ISendService`, `IReceiveService`, `IWebhookProcessorFactory` and the options. The default `IDeliveryTracker` is the in-memory tracker (Simulate tier); the persistent tracker comes from the persistence adapter. Webhook processors register as keyed services under the provider key.

## HTTP surface (host project, not the library)

| Route | Maps to | Notes |
|-------|---------|-------|
| `POST /webhooks/{channel}` | `IReceiveService.HandleWebhookAsync` | Anonymous (the provider cannot send a JWT); the signature is the credential. Reads the raw body once. Returns `204` on success, `400` problem details for a bad signature or payload, `404` for an unknown channel (RFC 9457). |
| `GET /messages/{messageId}/status` | `IDeliveryTracker.GetStatusAsync` | Right `communications.read`. |
| `GET /messages?correlationId=` | `IDeliveryTracker.GetByCorrelationIdAsync` | Right `communications.read`. |

These are documented in the host's OpenAPI document; the library has no ASP.NET reference.

## Telemetry

An `ActivitySource` named `OoBDev.Communications` records one `send` span per call (tags: channel, provider, attempt count, outcome, never the content) and one `webhook` span; counters `communications.sent`, `communications.failed`, `communications.webhooks` carry the channel and provider.

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Channel Abstraction API](../ChannelAbstraction/api-design.md)
- [Core API](../Core/api-design.md)
