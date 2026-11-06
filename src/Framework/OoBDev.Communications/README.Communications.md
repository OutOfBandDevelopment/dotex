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

### Receiving webhooks

`IReceiveService.HandleWebhookAsync(channel, WebhookRequest)` finds the channel and the provider's `IWebhookProcessor`
(`IWebhookProcessorFactory`, case-insensitive key), rejects a call older or newer than `WebhookClockTolerance`
(default 5 minutes), checks the signature over the raw body, parses it into `MessageReceivedEvent` and
`DeliveryStatusEvent`, then runs the registered `IMessageReceivedHandler` and `IDeliveryStatusHandler` instances in
registration order. A throwing handler is logged and the others still run. Bad input comes back as a failed
`WebhookResult`, never an exception. Status events move the `IDeliveryTracker` record forward only
(`Queued`, `Sent`, `Delivered`/`Failed`/`Bounced`, `Opened`, `Clicked`); a late `Sent` after `Bounced` is ignored.
Settings are validated at start-up (retry count 0 to 100, delays positive, factor at least 1). Polling (`PollAsync`) and the anonymous `POST /webhooks/{channel}` route are not built yet.

### Polling

`IReceiveService.PollAsync(channel, maxMessages)` asks a provider that implements `IPollableChannelProvider` for unread messages and hands each to the
`IMessageReceivedHandler` instances. A message is acknowledged (marked read at the vendor) only after every handler returned, so a failing handler
loses nothing and the message comes back on the next poll. It returns the messages that were handled; an unknown channel or provider throws, and a
provider that cannot be polled throws `NotSupportedException`. The caller (a hosted service on `SendReceiveOptions.PollInterval`) decides when to poll.
