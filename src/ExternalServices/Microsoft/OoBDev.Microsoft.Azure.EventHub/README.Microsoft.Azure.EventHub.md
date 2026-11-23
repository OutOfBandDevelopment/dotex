# OoBDev.Microsoft.Azure.EventHub

Azure Event Hubs implementation of `IEventHubClient` (from `OoBDev.ComplexEvents.Abstractions`). Publish with `EventHubPublisher` and consume with `EventHubConsumer` from `OoBDev.ComplexEvents`; both work against any `IEventHubClient`, so tests use `InMemoryEventHubClient`.

## Use

```csharp
services.TryAddAzureEventHub();   // binds OoBDev:Azure:EventHub, or pass an options delegate
```

```json
{ "OoBDev": { "Azure": { "EventHub": { "ConnectionString": "Endpoint=sb://...;EntityPath=hub", "EventHubName": "", "ConsumerGroup": "", "ReadFromEarliest": false } } } }
```

- The event type name travels in the `eventType` application property; the body is the JSON event. A received event without that property is ignored.
- `ReadAsync` reads every partition from `$Default` (or the named consumer group) and ends when the token is cancelled. There is no checkpointing: restart reads new events only, or from the start with `ReadFromEarliest`.

## Testing

One Unit test checks the registration (no network). There is no Event Hubs emulator in the test stack, so publish and consume against Azure are not covered; add a `LiveIntegration` test or an emulator service (`add-integration-service` skill) when a consumer needs it.
