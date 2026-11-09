# OoBDev.Communications.Hosting

Background work for `OoBDev.Communications`. `services.TryAddCommunicationsHosting()` (also called by `TryCommonHosting`) adds `ReceivePollingHost`:
one `PeriodicTimer` loop per configured channel that sets `PollInterval`, calling `IReceiveService.PollAsync(channel, PollBatchSize)`.
A failed poll is logged and retried on the next tick; channels without `PollInterval` are never polled.

```json
{ "OoBDev": { "Communications": { "Channels": [
  { "Name": "inbox", "Protocol": "email", "Provider": "mailkit-imap", "PollInterval": "00:01:00", "PollBatchSize": 10 }
] } } }
```

The IMAP connection settings are in `README.MailKit.md`. Webhook delivery needs no host (the WebApi route calls `IReceiveService.ReceiveAsync`).
