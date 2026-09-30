# OoBDev.GroqCloud

The OoBDev.GroqCloud library provides a set of classes and interfaces for interacting with the Groq 
Cloud API. This library allows developers to easily integrate the Groq Cloud service into their 
applications and utilize its AI-powered features.

### Classes and Interfaces

The library consists of the following classes and interfaces:

* `GroqCloudApiClientFactory`: A factory class for creating instances of the `GroqClient` class.
* `GroqCloudApiClientOptions`: A class representing the configuration options for the GroqCloud API client.
* `GroqCloudHealthCheck`: A class representing a health check implementation for the GroqCloud service.
* `GroqCloudMessageCompletion`: A class responsible for generating message completions using the GroqCloud API.
* `GroqCloudModelMapper`: A mapper interface for llama models.
* `IGroqCloudApiClientFactory`: An interface representing a factory for creating instances of the GroqCloudApiClient.
* `IGroqCloudModelMapper`: An interface representing a mapper interface for llama models.
* `ServiceCollectionExtensions`: A class providing extension methods for configuring services related to GroqCloud.

### Methods and Properties

The classes and interfaces in the library provide the following methods and properties:

* `GroqCloudApiClientFactory`:
	+ `Constructor`: Initializes a new instance of the `GroqCloudApiClientFactory` class with the specified options.
	+ `Build`: Builds a new instance of the `GroqClient` class with the specified host.
* `GroqCloudApiClientOptions`:
	+ `ApiKey`: The API key used to authenticate with the Groq Cloud service.
	+ `Model`: The model identifier for the AI model to be used. Defaults to "llama3-8b-8192".
* `GroqCloudHealthCheck`:
	+ `Constructor`: Initializes a new instance of the `GroqCloudHealthCheck` class.
	+ `CheckHealthAsync`: Checks the health of the GroqCloud service asynchronously.
* `GroqCloudMessageCompletion`:
	+ `Constructor`: Initializes a new instance of the `GroqCloudMessageCompletion` class.
	+ `GetCompletionAsync`: Generates a completion for the given prompt using the specified model.
	+ `GetCompletionAsync`: Retrieves a completion for the given prompt from the specified model.
* `GroqCloudModelMapper`:
	+ `Map`: Maps a `CompletionRequest` model to a `GroqChatHistory` object.
	+ `Map`: Maps a `GroqChatCompletions` object to a `CompletionResponse` object.
* `IGroqCloudApiClientFactory`:
	+ `Build`: Builds an instance of the GroqCloudApiClient for the specified host.
* `IGroqCloudModelMapper`:
	+ `Map`: Maps a `CompletionRequest` model to a `GroqChatHistory` object.
	+ `Map`: Maps a `GroqChatCompletions` object to a `CompletionResponse` object.
* `ServiceCollectionExtensions`:
	+ `TryAddGroqCloudServices`: Configures services for GroqCloud.

### Usage

To use the OoBDev.GroqCloud library, you can add it to your .NET project and use the provided classes and interfaces to 
interact with the Groq Cloud API. You can also use the `ServiceCollectionExtensions` class to configure services related
to GroqCloud.

### Configuration

The library provides options for configuring the GroqCloud API client, including the API key and model identifier. You 
can configure these options using the `GroqCloudApiClientOptions` class.

### Health Checks

The library provides a health check implementation for the GroqCloud service using the `GroqCloudHealthCheck` class. 
You can use this class to check the health of the GroqCloud service asynchronously.

### Message Completions

The library provides a class for generating message completions using the GroqCloud API, `GroqCloudMessageCompletion`. 
You can use this class to generate completions for given prompts using the specified model.

### Model Mappers

The library provides a mapper interface for llama models, `IGroqCloudModelMapper`. You can use this interface to map 
`CompletionRequest` models to `GroqChatHistory` objects and vice versa.

### Service Collection Extensions

The library provides extension methods for configuring services related to GroqCloud, `ServiceCollectionExtensions`. 
You can use these methods to add services for GroqCloud to your service collection.

I hope this README file helps you understand how to use the OoBDev.GroqCloud library. If you have any questions or 
need further assistance, please don't hesitate to reach out.