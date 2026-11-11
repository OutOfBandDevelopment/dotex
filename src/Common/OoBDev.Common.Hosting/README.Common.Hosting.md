# OoBDev.Common.Hosting

Aggregator for the hosted-service projects (`*.Hosting` in Framework, Extensions and ExternalServices) with one registration that switches them on.

## Use

```csharp
services.TryCommonHosting(configuration);                  // builder optional in release builds
services.TryCommonHosting(configuration, new HostingBuilder { DisableVectorHosting = true });
```

**Table 1 — `HostingBuilder` options**

| Property | Default | Effect |
|----------|---------|--------|
| `DisableCommunications` | false | Skips `TryAddCommunicationsHosting` |
| `DisableMessageQueueing` | false | Skips `TryAddMessageQueueingHosting` ([receiver host](../../Framework/OoBDev.MessageQueueing.Hosting/README.MessageQueueing.Hosting.md)) |
| `DisableVectorHosting` | false | Skips `TryAddVectorHosting` |
| `EmbeddingSentenceTransformerQueueReaderConfigurationSection` | `EmbeddingSentenceTransformerQueueReaderOptions` | Configuration section the vector hosting reads |

In Debug builds the builder parameter is required, in Release it defaults to null (all hosts on).
