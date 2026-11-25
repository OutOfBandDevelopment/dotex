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

## Deferral release

`TryAddCommunicationsHosting()` also adds the orchestration services (`TryAddCommunicationOrchestration`) and `DeferralReleaseHost`: a `PeriodicTimer` loop that calls `ICommunicationOrchestrator.ReleaseDueAsync()` every `OoBDev:Communications:DeferralReleaseInterval` (default `00:01:00`, range 1 second to 1 day). A failed release is logged and retried on the next tick. Held requests stay in the registered `IDeferralStore` (in memory by default, so they are lost on restart). Covered by `DeferralReleaseHostTests`.
