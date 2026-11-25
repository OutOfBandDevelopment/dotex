# OoBDev.Twilio

Sends text messages through the Twilio Messages API as the `twilio` provider of the `sms` protocol (an `IChannelProvider` of the Communications channel abstraction). It references the Communications Abstractions only and calls the REST API with `HttpClient`, so no vendor SDK is needed.

## Use

```csharp
services.TryAddTwilioProvider(configuration);   // section OoBDev:Twilio; nothing is registered without an AccountSid
```

```json
{ "OoBDev": { "Twilio": { "AccountSid": "AC...", "AuthToken": "<user secret>", "DefaultFrom": "+15550000000" } } }
```

A channel with `Protocol = "sms"` and `Provider = "twilio"` then sends any `ISmsMessage` (`SmsMessage` is the plain implementation: E.164 `To`, optional `From`, `Body`).

## Behaviour

- Success returns the message `sid` as `ProviderMessageId`.
- Failures are returned, not thrown: 429 is `RateLimited` (with `Retry-After`), 5xx, 408 and network errors are `Transient`, other 4xx are `Permanent`, a missing sender or recipient is `Rejected` before any call.
- The failure message holds only the status code, never the token, phone numbers or the text.
- Opt-out handling (STOP replies) is done by Twilio; delivery status arrives through the webhook processors of the Communications layer, not through this provider.

## Tests

`OoBDev.Twilio.Tests` (Unit) uses a fake `HttpMessageHandler`. Live accounts are not used by any automated test.
