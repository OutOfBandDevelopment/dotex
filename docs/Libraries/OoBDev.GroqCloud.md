# OoBDev.GroqCloud


## Class: GroqCloud.GroqCloudApiClientFactory
Factory class for creating instances of the 
 *See: T:GroqNet.GroqClient*. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.GroqCloud.GroqCloudApiClientFactory*class with the specified options. 


##### Parameters
* *options:* The options for configuring the GroqCloud API client.




#### Build
Builds a new instance of the 
 *See: T:GroqNet.GroqClient*with the specified host. 


##### Return value
A new instance of the .



## Class: GroqCloud.GroqCloudApiClientOptions
Configuration options for the Groq Cloud API client. 

### Properties

#### ApiKey
The API key used to authenticate with the Groq Cloud service.
#### Model
The model identifier for the AI model to be used. Defaults to "llama3-8b-8192".

## Class: GroqCloud.GroqCloudHealthCheck
Represents a health check implementation for the GroqCloud service. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.GroqCloud.GroqCloudHealthCheck*class. 


##### Parameters
* *client:* The GroqCloud API client used for health checks.




#### CheckHealthAsync(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext,System.Threading.CancellationToken)
Checks the health of the GroqCloud service asynchronously. 


##### Parameters
* *context:* The health check context.
* *cancellationToken:* The cancellation token.




##### Return value
A task representing the asynchronous health check operation.



## Class: GroqCloud.GroqCloudMessageCompletion
Represents a class responsible for generating message completions using the GroqCloud API. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.GroqCloud.GroqCloudMessageCompletion*class. 


##### Parameters
* *client:* The GroqCloudApiClient instance used for communication with the GroqCloud API.
* *mapper:* Model mapper for GroqCloud request/response.




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



## Class: GroqCloud.GroqCloudModelMapper
Represents a mapper interface for llama models. 

### Methods


#### Map(OoBDev.AI.Models.CompletionRequest)
Maps a 
 *See: T:OoBDev.AI.Models.CompletionRequest*model to a 
 *See: T:GroqNet.ChatCompletions.GroqChatHistory*object. 


##### Parameters
* *request:* The completion request model to map.




##### Return value
A generated completion request object.



#### Map(GroqNet.ChatCompletions.GroqChatCompletions)
Maps a 
 *See: T:GroqNet.ChatCompletions.GroqChatCompletions*object to a 
 *See: T:OoBDev.AI.Models.CompletionResponse*object. 


##### Parameters
* *response:* The conversation context with response to map.




##### Return value
A completion response object.



## Class: GroqCloud.IGroqCloudApiClientFactory
Represents a factory for creating instances of the GroqCloudApiClient. 

### Methods


#### Build
Builds an instance of the GroqCloudApiClient for the specified host. 


##### Return value
An instance of the GroqCloudApiClient.



## Class: GroqCloud.IGroqCloudModelMapper
Represents a mapper interface for llama models. 

### Methods


#### Map(OoBDev.AI.Models.CompletionRequest)
Maps a 
 *See: T:OoBDev.AI.Models.CompletionRequest*model to a 
 *See: T:GroqNet.ChatCompletions.GroqChatHistory*object. 


##### Parameters
* *request:* The completion request model to map.




##### Return value
A generated completion request object.



#### Map(GroqNet.ChatCompletions.GroqChatCompletions)
Maps a 
 *See: T:GroqNet.ChatCompletions.GroqChatCompletions*object to a 
 *See: T:OoBDev.AI.Models.CompletionResponse*object. 


##### Parameters
* *response:* The conversation context with response to map.




##### Return value
A completion response object.



## Class: GroqCloud.ServiceCollectionExtensions
Provides extension methods for configuring services related to groqCloud. 

### Methods


#### TryAddGroqCloudServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for groqCloud. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The configuration containing the groqCloud API client options.
* *groqCloudApiClientOptionSection:* The name of the configuration section containing the groqCloud API client options.




##### Return value
The modified .

