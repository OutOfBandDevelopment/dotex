# OoBDev.Ollama

OoBDev.Ollama provides functionality for interacting with the Ollama API, including text embedding, message completion, and language model responses. Let's explore its features:

## IOllamaApiClientFactory

Represents a factory for creating instances of the OllamaApiClient.

### Methods

- **Build**: Builds an instance of the OllamaApiClient for the specified host.

## OllamaApiClientExtensions

Provides extension methods for the `OllamaSharp.OllamaApiClient` class.

### Methods

- **EnsureModelExistsAsync(ollama, modelName, logger)**: Ensures that the specified model exists in the Ollama API client.
- **GetEmbeddingSingle(client, text, modelName)**: Retrieves the embedding for the specified text using the specified model synchronously.
- **GetEmbeddingSingleAsync(client, text, modelName)**: Retrieves the embedding for the specified text using the specified model asynchronously.
- **GetEmbeddingDouble(client, text, modelName)**: Retrieves the embedding for the specified text using the specified model synchronously.
- **GetEmbeddingDoubleAsync(client, text, modelName)**: Retrieves the embedding for the specified text using the specified model asynchronously.

## OllamaApiClientFactory

Factory class for creating instances of the `OllamaSharp.OllamaApiClient`.

### Methods

- **Constructor(options)**: Initializes a new instance of the `OoBDev.Ollama.OllamaApiClientFactory` class with the specified options.
- **Build**: Builds a new instance of the `OllamaSharp.OllamaApiClient` with the specified host.

## OllamaApiClientOptions

Represents the configuration options for the Ollama API client.

### Properties

- **Url**: Gets or initializes the URL of the Ollama API.
- **DefaultModel**: Gets or initializes the default model to use with the Ollama API.

## OllamaMessageCompletion

Responsible for generating message completions using the Ollama API.

### Methods

- **Constructor(client)**: Initializes a new instance of the `OoBDev.Ollama.OllamaMessageCompletion` class.
- **GetCompletionAsync(modelName, prompt)**: Generates a completion for the given prompt using the specified model asynchronously.
- **GetResponseAsync(promptDetails, userInput)**: Retrieves a response from the language model based on the provided prompt details and user input asynchronously.
- **GetStreamedResponseAsync(promptDetails, userInput, cancellationToken)**: Gets a streamed response asynchronously based on the provided prompt details and user input.

## ServiceCollectionExtensions

Provides extension methods for configuring services related to Ollama.

### Methods

- **TryAddOllamaServices(services, configuration, ollamaApiClientOptionSection)**: Configures services for Ollama.

OoBDev.Ollama facilitates easy integration with the Ollama API, enabling various text-related tasks such as embedding, completion, and language model responses.
