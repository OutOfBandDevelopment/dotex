# OoBDev.AI

Framework implementations on top of [OoBDev.AI.Abstractions](../OoBDev.AI.Abstractions/README.AI.Abstractions.md). Namespace `OoBDev.AI`; references the abstractions only, so it works with any adapter (Ollama, Groq, ...).

## Estimated token usage

| Type | Role |
|------|------|
| `TokenEstimate` | `Of(text)`: characters divided by four, rounded up |
| `EstimatingMessageCompletion`, `EstimatingStreamedCompletion` | Decorators for `IMessageCompletion` and `IStreamedCompletion`; the interfaces do not change. A streamed call is recorded when the stream ends or the caller stops reading |
| `LlmUsageMeter` | Adds up calls and estimated prompt and completion tokens per model (`Snapshot()` returns `LlmUsage` rows) and feeds the `oobdev.ai.estimated_tokens` counter on the `OoBDev.AI` meter, so an OpenTelemetry meter list that names `LlmUsageMeter.MeterName` exports it |
| `ServiceCollectionExtensions.TryAddEstimatedLlmUsage()` | Registers the meter and wraps every already registered completion service, plain or keyed, once; call it after the adapter registrations |

```csharp
services.TryAddOllamaServices(...);
services.TryAddEstimatedLlmUsage();
```

The numbers are estimates, not what a provider bills: label them approximate wherever they are shown. A provider usage field, when an adapter exposes one, would replace the estimate in a later change (that needs an interface change, which was declined for now, owner 2026-11-16). `ILanguageModelProvider` is not wrapped.
