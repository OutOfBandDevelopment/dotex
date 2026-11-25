# OoBDev.SendGrid

Sends email through the SendGrid v3 mail API as the `sendgrid` provider of the `email` protocol (an `IChannelProvider` of the Communications channel abstraction). It references the Communications Abstractions only and calls the REST API with `HttpClient`, so no vendor SDK is needed.

## Use

```csharp
services.TryAddSendGridProvider(configuration);   // section OoBDev:SendGrid; nothing is registered without an ApiKey
```

```json
{ "OoBDev": { "SendGrid": { "ApiKey": "<user secret>", "DefaultFrom": "noreply@example.org" } } }
```

A channel with `Protocol = "email"` and `Provider = "sendgrid"` then sends any `IEmailMessage` (to, cc, bcc, reply-to, text, HTML, attachments and inline content ids).

## Behaviour

- Success returns the vendor id from the `X-Message-Id` header as `ProviderMessageId`.
- Failures are returned, not thrown: 429 is `RateLimited` (with `Retry-After`), 5xx, 408 and network errors are `Transient`, other 4xx are `Permanent`, a missing sender or recipient is `Rejected` before any call.
- The failure message holds only the status code, never the key, addresses or the body.
- The API key comes from configuration or user secrets, never from a file in the repository.

## Tests

`OoBDev.SendGrid.Tests` (Unit) uses a fake `HttpMessageHandler`. Live accounts are not used by any automated test.
