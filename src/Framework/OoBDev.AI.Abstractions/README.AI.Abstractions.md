# OoBDev.AI.Abstractions

Contracts for embeddings and language models, so applications depend on these interfaces and the provider (Ollama, SBert, ONNX, vision models) is chosen by registration. Namespace `OoBDev.AI`; no implementation lives here.

## Contracts

**Table 1 — Interfaces**

| Interface | Members |
|-----------|---------|
| `IEmbeddingProvider` | `Length` (dimension count) and `GenerateEmbeddingAsync(content, model, cancellationToken)` returning a `ReadOnlyMemory<float>`. `model` may be null to use the provider's model; `model` and the token have defaults in release builds and are required in `DEBUG` builds (rule `coding-standards`). |
| `ILanguageModelProvider` | `GetResponseAsync(promptDetails, userInput)`; streamed variants `GetStreamedResponseAsync` and `GetStreamedContextResponseAsync(assistantConfinment, systemInteractions, userInput)`; `GetContextResponseAsync`; `GetRAGResponseAsync(assistantConfinment, ragData, userInput)` and `GetRAGResponseCitiationsAsync(IList<KeyValuePairModel>, userQuery)`; `GetEmbeddedResponseAsync(data)` returning `float[]`. |
| `IStreamedCompletion` | `GetStreamedResponseAsync(promptDetails, userInput, cancellationToken)` as an `IAsyncEnumerable<string>`. |
| `IMessageCompletion` | `GetCompletionAsync(modelName, prompt)`, and `GetCompletionAsync(CompletionRequest)` returning a `CompletionResponse`. |

**Table 2 — Models**

| Type | Fields |
|------|--------|
| `CompletionRequest` | `Prompt` (required), `Model`, `Context` (token context of an earlier turn), `Images`, `System`, `Template` |
| `CompletionResponse` | `Response`, `Context` (pass it back to continue the conversation) |
| `KeyValuePairModel` | `Key`, `Value`: one retrieved source for the RAG methods |

The `Citiations` spelling in `GetRAGResponseCitiationsAsync` is the current public name.

## Implementations

- Embeddings: [SBert](../../ExternalServices/SBert/OoBDev.SBert/README.SBert.md) (HTTP service), [Onnx.SentenceEmbeddings](../../ExternalServices/Onnx/OoBDev.Onnx.SentenceEmbeddings/README.Onnx.SentenceEmbeddings.md) (in process) and the Ollama provider.
- Completion: the Ollama adapter (`IMessageCompletion`); the `OoBDev.Spike.ExtensionsAI` spike wraps `Microsoft.Extensions.AI` chat clients as an `ILanguageModelProvider`.
