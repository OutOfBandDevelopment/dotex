# OoBDev.Ollama


## Class: Ollama.IOllamaApiClientFactory
Represents a factory for creating instances of the OllamaApiClient. 

### Methods


#### Build
Builds an instance of the OllamaApiClient for the specified host. 


##### Return value
An instance of the OllamaApiClient.



## Class: Ollama.IOllamaModelMapper
Represents a mapper interface for llama models. 

### Methods


#### Map(OoBDev.AI.Models.CompletionRequest)
Maps a 
 *See: T:OoBDev.AI.Models.CompletionRequest*model to a 
 *See: T:OllamaSharp.Models.GenerateCompletionRequest*object. 


##### Parameters
* *request:* The completion request model to map.




##### Return value
A generated completion request object.



#### Map(OllamaSharp.ConversationContextWithResponse)
Maps a 
 *See: T:OllamaSharp.ConversationContextWithResponse*object to a 
 *See: T:OoBDev.AI.Models.CompletionResponse*object. 


##### Parameters
* *response:* The conversation context with response to map.




##### Return value
A completion response object.



## Class: Ollama.OllamaApiClientExtensions
Provides extension methods for the 
 *See: T:OllamaSharp.OllamaApiClient*class. 

### Methods


#### EnsureModelExistsAsync(OllamaSharp.IOllamaApiClient,System.String,Microsoft.Extensions.Logging.ILogger)
Ensures that the specified model exists in the Ollama API client. If the model does not exist locally, it is copied from the 'llama2' model. 


##### Parameters
* *ollama:* The Ollama API client.
* *modelName:* The name of the model to ensure exists.
* *logger:* The logger instance for logging information.




##### Return value
The Ollama API client.



#### GetEmbeddingSingle(OllamaSharp.IOllamaApiClient,System.String,System.String)
Synchronously retrieves the embedding for the specified text using the specified model. 


##### Parameters
* *client:* The Ollama API client.
* *text:* The text for which the embedding is requested.
* *modelName:* The name of the model to use for embedding.




##### Return value
The embedding as an array of floats.



#### GetEmbeddingSingleAsync(OllamaSharp.IOllamaApiClient,System.String,System.String)
Asynchronously retrieves the embedding for the specified text using the specified model. 


##### Parameters
* *client:* The Ollama API client.
* *text:* The text for which the embedding is requested.
* *modelName:* The name of the model to use for embedding.




##### Return value
A task representing the asynchronous operation. The task result contains the embedding as an array of floats.



#### GetEmbeddingDoubleAsync(OllamaSharp.IOllamaApiClient,System.String,System.String)
Asynchronously retrieves the embedding for the specified text using the specified model. 


##### Parameters
* *client:* The Ollama API client.
* *text:* The text for which the embedding is requested.
* *modelName:* The name of the model to use for embedding.




##### Return value
A task representing the asynchronous operation. The task result contains the embedding as an array of doubles.



#### GetEmbeddingDouble(OllamaSharp.IOllamaApiClient,System.String,System.String)
Synchronously retrieves the embedding for the specified text using the specified model. 


##### Parameters
* *client:* The Ollama API client.
* *text:* The text for which the embedding is requested.
* *modelName:* The name of the model to use for embedding.




##### Return value
The embedding as an array of doubles.



## Class: Ollama.OllamaApiClientFactory
Factory class for creating instances of the 
 *See: T:OllamaSharp.OllamaApiClient*. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Ollama.OllamaApiClientFactory*class with the specified options. 


##### Parameters
* *options:* The options for configuring the Ollama API client.




#### Build
Builds a new instance of the 
 *See: T:OllamaSharp.OllamaApiClient*with the specified host. 


##### Return value
A new instance of the .



## Class: Ollama.OllamaApiClientOptions
Represents the configuration options for the Ollama API client. 

### Properties

#### Url
Gets or initializes the URL of the Ollama API.
#### DefaultModel
Gets or initializes the default model to use with the Ollama API.

## Class: Ollama.OllamaHealthCheck
Represents a health check implementation for the Ollama service. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Ollama.OllamaHealthCheck*class. 


##### Parameters
* *client:* The Ollama API client used for health checks.




#### CheckHealthAsync(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext,System.Threading.CancellationToken)
Checks the health of the Ollama service asynchronously. 


##### Parameters
* *context:* The health check context.
* *cancellationToken:* The cancellation token.




##### Return value
A task representing the asynchronous health check operation.



## Class: Ollama.OllamaMessageCompletion
Represents a class responsible for generating message completions using the Ollama API. 

### Properties

#### Length
Gets the length of the embeddings.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Ollama.OllamaMessageCompletion*class. 


##### Parameters
* *client:* The OllamaApiClient instance used for communication with the Ollama API.
* *mapper:* Model mapper for Ollama request/response.




#### GetEmbeddingAsync(System.String,System.String)
Retrieves the embedding vector for the given content. 


##### Parameters
* *content:* The content for which to retrieve the embedding.
* *model:* The model for which to retrieve the embedding.




##### Return value
A task representing the asynchronous operation. The task result contains the embedding vector as an array of single-precision floats.



#### GetCompletionAsync(System.String,System.String)
Generates a completion for the given prompt using the specified model. 


##### Parameters
* *modelName:* The name of the model to use for completion.
* *prompt:* The prompt for which completion is requested.




##### Return value
A task representing the asynchronous operation. The task result contains the completion response.



#### GetCompletionAsync(OoBDev.AI.Models.CompletionRequest)
Retrieves a completion for the given prompt from the specified model. 


##### Parameters
* *model:* Completion request model




##### Return value
Resulting response object



#### GetResponseAsync(System.String,System.String,System.Threading.CancellationToken)
Retrieves a response from the language model based on the provided prompt details and user input. 


##### Parameters
* *promptDetails:* Details of the prompt or context.
* *userInput:* The user input or query.
* *cancellationToken:* The Cancellation Token.




##### Return value
A task representing the asynchronous operation. The task result contains the response from the language model.



#### GetStreamedResponseAsync(System.String,System.String,System.Threading.CancellationToken)
Gets a streamed response asynchronously based on the provided prompt details and user input. 


##### Parameters
* *promptDetails:* The details of the prompt.
* *userInput:* The user input.
* *cancellationToken:* The Cancellation Token.




##### Return value
An asynchronous enumerable of strings representing the streamed response.



## Class: Ollama.OllamaModelMapper
Represents a mapper interface for llama models. 

### Methods


#### Map(OoBDev.AI.Models.CompletionRequest)
Maps a 
 *See: T:OoBDev.AI.Models.CompletionRequest*model to a 
 *See: T:OllamaSharp.Models.GenerateCompletionRequest*object. 


##### Parameters
* *request:* The completion request model to map.




##### Return value
A generated completion request object.



#### Map(OllamaSharp.ConversationContextWithResponse)
Maps a 
 *See: T:OllamaSharp.ConversationContextWithResponse*object to a 
 *See: T:OoBDev.AI.Models.CompletionResponse*object. 


##### Parameters
* *response:* The conversation context with response to map.




##### Return value
A completion response object.



## Class: Ollama.ServiceCollectionExtensions
Provides extension methods for configuring services related to Ollama. 

### Methods


#### TryAddOllamaServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for Ollama. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The configuration containing the Ollama API client options.
* *ollamaApiClientOptionSection:* The name of the configuration section containing the Ollama API client options.




##### Return value
The modified .

