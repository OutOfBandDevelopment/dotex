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

One Unit test checks the registration (no network). One Integration test (`oobd-test-eventhubs`, the emulator in the test stack with Azurite, host port 5674) publishes an event and reads it back with its type. Checkpointing is not built, so restart behaviour is not covered.
