Here is the documentation for the given source code files:

**OpenAIClientOptions.cs**

This class represents options for configuring the OpenAI service. It has three properties: `APIKey`, `Model`, and `EmbeddingModel`, which are all required.

### Class Diagram in Plant UML:
```
@startuml
class OpenAIClientOptions {
  - APIKey: string (required)
  - Model: string (required)
  - EmbeddingModel: string (required)
}

@enduml
```

**OpenAIManager.cs**

This class provides methods for interacting with the OpenAI language model. It implements the `ILanguageModelProvider` interface.

### Class Diagram in Plant UML:
```
@startuml
class OpenAIManager {
  + IOptions<OpenAIClientOptions> _config
  + OpenAIClient api
  + GetResponseAsync(string, string, CancellationToken) -> Task<string>
  + GetStreamedResponseAsync(string, string, CancellationToken) -> IAsyncEnumerable<string>
  + GetStreamedContextResponseAsync(string, List<string>, List<string>, CancellationToken) -> IAsyncEnumerable<string>
  + GetEmbeddedResponseAsync(string, CancellationToken) -> Task<float[]>
  + GetContextResponseAsync(string, List<string>, List<string>, CancellationToken) -> Task<string>
  + GetRAGResponseAsync(string, string, string, CancellationToken) -> Task<string>
  + GetRAGResponseCitiationsAsync(List<KeyValuePairModel>, string, CancellationToken) -> IAsyncEnumerable<string>
}

interface ILanguageModelProvider {
  + GetResponseAsync(string, string, CancellationToken) -> Task<string>
  + GetStreamedResponseAsync(string, string, CancellationToken) -> IAsyncEnumerable<string>
  + ...
}

@enduml
```

The `OpenAIManager` class has several methods for interacting with the OpenAI language model, including `GetResponseAsync`, `GetStreamedResponseAsync`, `GetStreamedContextResponseAsync`, `GetEmbeddedResponseAsync`, `GetContextResponseAsync`, `GetRAGResponseAsync`, and `GetRAGResponseCitiationsAsync`. These methods take various parameters, such as the prompt details and user input, and return responses in the form of tasks or async enumerables.

Note: The `ILanguageModelProvider` interface defines the methods that must be implemented by the `OpenAIManager` class.