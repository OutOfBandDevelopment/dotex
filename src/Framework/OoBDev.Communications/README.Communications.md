# OoBDev.Communications

## Summary

The OoBDev Communications libraries are an abstraction designed to asynchronously send 
messages over a dedicated channel such as Email and SMS

### Example

The `ISendService` sends an `IMessage` through a named channel (`OoBDev:Communications:Channels`); the channel's
provider (for example `OoBDev.MailKit` for `email`) does the delivery. Details are in `README.Communications.md`.

```csharp
public class ExampleClass(ISendService send)
{
    public Task<SendResult> SendAsync(EmailMessage message) =>
        send.SendAsync("email", message);
}
```

## Sending with channels (Phase 1)

`ISendService` sends an `IMessage` through a named `IChannel`. `SendService` looks up the channel
(`IChannelRepository`) and its provider (`IChannelRegistry`), returns the stored result when the same
`MessageId` already succeeded on that channel, and retries `Transient` and `RateLimited` failures with
`RetryDelay * BackoffFactor^attempt`, capped by `MaxRetryDelay`; a provider `RetryAfter` wins when longer.
`Permanent` and `Rejected` failures are returned at once. A missing channel or provider throws
`ChannelNotFoundException` or `ProviderNotFoundException`. Outcomes are recorded in `IDeliveryTracker`
(`InMemoryDeliveryTracker` by default; state is lost on restart).

```csharp
services.TryAddSendReceive();   // settings: OoBDev:Communications (RetryCount, RetryDelay, BackoffFactor, MaxRetryDelay)
// channels: OoBDev:Communications:Channels:0:Name / Protocol / Provider (ConfigurationChannelRepository);
// the host registers the IChannelProvider implementations; ChannelRegistry matches provider key and protocol
```

### Suppression list

`AddEmailSuppression()` (after `TryAddSendReceive`) wraps `ISendService` in `SuppressingSendService`: addresses on the `ISuppressionList` (case-insensitive; `InMemorySuppressionList` by default, register a durable one first) are removed from To, Cc and Bcc, and a message with no To left is not sent and returns a `Rejected` failure. Non-email messages pass through. `SuppressionStatusHandler` (registered by the same call) feeds the list from delivery status updates: the recipients of a `Permanent` bounce and of a `Complained` update are added; a transient bounce is not. `DeliveryStatusUpdate.Recipients` carries the addresses; the SES processor fills it from `bouncedRecipients` and `complainedRecipients`, and maps a complaint to the new `DeliveryStatus.Complained` (last in the order, so it still applies after an open or click). Covered by `SuppressingSendServiceTests`.

### Receiving webhooks

`IReceiveService.HandleWebhookAsync(channel, WebhookRequest)` finds the channel and the provider's `IWebhookProcessor`
(`IWebhookProcessorFactory`, case-insensitive key), rejects a call older or newer than `WebhookClockTolerance`
(default 5 minutes), checks the signature over the raw body, parses it into `MessageReceivedEvent` and
`DeliveryStatusEvent`, then runs the registered `IMessageReceivedHandler` and `IDeliveryStatusHandler` instances in
registration order. A throwing handler is logged and the others still run. Bad input comes back as a failed
`WebhookResult`, never an exception. Status events move the `IDeliveryTracker` record forward only
(`Queued`, `Sent`, `Delivered`/`Failed`/`Bounced`, `Opened`, `Clicked`); a late `Sent` after `Bounced` is ignored.
Settings are validated at start-up (retry count 0 to 100, delays positive, factor at least 1).

### Polling

`IReceiveService.PollAsync(channel, maxMessages)` asks a provider that implements `IPollableChannelProvider` for unread messages and hands each to the
`IMessageReceivedHandler` instances. A message is acknowledged (marked read at the vendor) only after every handler returned, so a failing handler
loses nothing and the message comes back on the next poll. It returns the messages that were handled; an unknown channel or provider throws, and a
provider that cannot be polled throws `NotSupportedException`. `services.TryAddCommunicationsHosting()` (`OoBDev.Communications.Hosting`) adds `ReceivePollingHost`, which polls every channel that sets `PollInterval` (and `PollBatchSize`, default 10) on a `PeriodicTimer`; a failed poll is logged and retried on the next tick. Without it the caller decides when to poll.

## Decisions

- Options (`SendReceiveOptions` and the others) are validated with data annotations and `ValidateOnStart` instead of `AddValidatedOptions<T>()`, so this library does not reference `OoBDev.System` (owner, 2026-11-05).
- Attachments will not be Stream-only: temp file, `byte[]` or zero-copy `ReadOnlyMemory<byte>`; see the [requirements](../../../docs/design/Communications/SendReceive/requirements.md#decisions-2026-11-05).
- Provider adapters (AWS SES and SNS first) are tested against emulators only, and their documents say so.

## One-click unsubscribe (RFC 8058)

`AddUnsubscribeLinks()` registers `UnsubscribeLinks` (options `OoBDev:Communications:Unsubscribe`: `EndpointUrl`, `SigningKey` of 32+ characters). `CreateHeaders(address)` returns `List-Unsubscribe` (`<url?t=token>`) and `List-Unsubscribe-Post: List-Unsubscribe=One-Click` to add to a bulk message. The token is the lower-cased address plus an HMAC-SHA512 signature: it cannot be forged for another address, needs no storage and never expires. The host maps an HTTPS endpoint that calls `UnsubscribeAsync(token)` for the POST (and a GET confirmation page); a valid token adds the address to the `ISuppressionList` with reason `unsubscribe`. Pair it with `OoBDev.Communications.EntityFrameworkCore` for a durable list. Changing the key invalidates links already sent.
