# OoBDev.Communications

## Summary

The OoBDev Communications libraries are an abstraction designed to asynchronously send 
messages over a dedicated channel such as Email and SMS

### Example

The `ICommunicationSender<>` interface is provided as a means to interact with various 
channel providers.  Example channels are hosted for `EmailMessageModel` and `SmsMessageModel`

```csharp

//Include ICommunicationSender<EmailMessageModel> in your service class

public class ExampleClass (
    ICommunicationSender<EmailMessageModel> email
    )
{
    public async Task<string> SendAsync(EmailMessageModel message) =>
        await email.SendAsync(message);
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
// the host registers IChannelRepository, IChannelRegistry and the IChannelProvider implementations
```

`ICommunicationSender<>` and the message models stay until the MailKit adapter, the example WebApi and Common are ported.

### Receiving webhooks

`IReceiveService.HandleWebhookAsync(channel, WebhookRequest)` finds the channel and the provider's `IWebhookProcessor`
(`IWebhookProcessorFactory`, case-insensitive key), rejects a call older or newer than `WebhookClockTolerance`
(default 5 minutes), checks the signature over the raw body, parses it into `MessageReceivedEvent` and
`DeliveryStatusEvent`, then runs the registered `IMessageReceivedHandler` and `IDeliveryStatusHandler` instances in
registration order. A throwing handler is logged and the others still run. Bad input comes back as a failed
`WebhookResult`, never an exception. Status events move the `IDeliveryTracker` record forward only
(`Queued`, `Sent`, `Delivered`/`Failed`/`Bounced`, `Opened`, `Clicked`); a late `Sent` after `Bounced` is ignored.
Polling (`PollAsync`) and the anonymous `POST /webhooks/{channel}` route are not built yet.
