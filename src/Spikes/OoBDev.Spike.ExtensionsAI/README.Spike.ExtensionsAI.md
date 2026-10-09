# OoBDev.Spike.ExtensionsAI

## Summary

Spike: can the existing `OoBDev.AI.Abstractions` contracts (`ILanguageModelProvider`, `IEmbeddingProvider`) sit on top of `Microsoft.Extensions.AI` (`IChatClient`, `IEmbeddingGenerator<string, Embedding<float>>`)? The contracts predate that library. Findings and recommendation are in [the spike report](../../../docs/patterns-discovery/08-spikes/01-extensions-ai.md).

This is throwaway evidence, not a product: it is not packaged and not part of `OoBDev.sln`.

## Contents

| File | Purpose |
|------|---------|
| `ChatClientLanguageModelProvider.cs` | `ILanguageModelProvider` implemented once over any `IChatClient` (plus an optional embedding generator) |
| `EmbeddingGeneratorProvider.cs` | `IEmbeddingProvider` over any `IEmbeddingGenerator` |
| `FakeClients.cs` | In-memory fakes of the platform interfaces |
| `ExtensionsAiSpikeTests.cs` | Six unit tests: role mapping, streaming, citations, embeddings, keyed registration, and the platform middleware pipeline |

## Running

```bash
cd src/Framework
dotnet test ../Spikes/OoBDev.Spike.ExtensionsAI
```

(Run from `src/Framework`: `Directory.Build.props` computes `SolutionDir` from the working directory.)
