# OoBDev.AI.Abstractions


## Class: AI.IEmbeddingProvider
Represents a provider for word embeddings. 

### Properties

#### Length
Gets the length of the embeddings.
### Methods


#### GetEmbeddingAsync(System.String,System.String)
Retrieves the embedding vector for the given content. 


##### Parameters
* *content:* The content for which to retrieve the embedding.
* *model:* The model for which to retrieve the embedding.




##### Return value
A task representing the asynchronous operation. The task result contains the embedding vector as an array of single-precision floats.



## Class: AI.ILanguageModelProvider
Represents a provider for a language model. 

### Methods


#### GetResponseAsync(System.String,System.String,System.Threading.CancellationToken)
Gets a response asynchronously based on the provided prompt details and user input. 


##### Parameters
* *promptDetails:* The details of the prompt.
* *userInput:* The user input.
* *cancellationToken:* The Cancellation Token.




##### Return value
A task representing the asynchronous operation that returns the response.



#### GetStreamedResponseAsync(System.String,System.String,System.Threading.CancellationToken)
Gets a streamed response asynchronously based on the provided prompt details and user input. 


##### Parameters
* *promptDetails:* The details of the prompt.
* *userInput:* The user input.
* *cancellationToken:* The Cancellation Token.




##### Return value
An asynchronous enumerable of strings representing the streamed response.



#### GetStreamedContextResponseAsync(System.String,System.Collections.Generic.List{System.String},System.Collections.Generic.List{System.String},System.Threading.CancellationToken)
Gets a response asynchronously based on the provided prompt details and user input. 


##### Parameters
* *assistantConfinment:* The confinment of the AI Assistant
* *systemInteractions:* The previous generated responses by the AI
* *userInput:* The user input including any previous messages in the chat
* *cancellationToken:* The Cancellation Token.




##### Return value
A task representing the asynchronous operation that returns the response.



#### GetEmbeddedResponseAsync(System.String,System.Threading.CancellationToken)
Gets the embedded response for the data provided. 


##### Parameters
* *data:* The data to be embedded there is a token limit of 2048 tokens.
* *cancellationToken:* The Cancellation Token.




##### Return value
Float array with the embedding data



#### GetContextResponseAsync(System.String,System.Collections.Generic.List{System.String},System.Collections.Generic.List{System.String},System.Threading.CancellationToken)
Gets a response asynchronously based on the provided prompt details and user input. 


##### Parameters
* *assistantConfinment:* The confinement of the AI Assistant
* *systemInteractions:* The previous generated responses by the AI
* *userInput:* The user input including any previous messages in the chat
* *cancellationToken:* The Cancellation Token.




##### Return value
A task representing the asynchronous operation that returns the response.



#### GetRAGResponseAsync(System.String,System.String,System.String,System.Threading.CancellationToken)
Gets a response asynchronously based on the provided prompt details and user input. 


##### Parameters
* *assistantConfinment:* The confinement of the AI Assistant
* *ragData:* The data to restrict the response
* *userInput:* The user input.
* *cancellationToken:* The Cancellation Token.




##### Return value
A task representing the asynchronous operation that returns the response.



#### GetRAGResponseCitiationsAsync(System.Collections.Generic.List{OoBDev.AI.Models.KeyValuePairModel},System.String,System.Threading.CancellationToken)
Gets a response asynchronously based on the ragData and userQuery 


##### Parameters
* *ragData:* The details of the prompt.
* *userQuery:* The userQuery
* *cancellationToken:* The cancellation token.




##### Return value
A task representing the asynchronous operation. The task result contains the generated response.



## Class: AI.IMessageCompletion
Represents a provider for message completion. 

### Methods


#### GetCompletionAsync(System.String,System.String)
Retrieves a completion for the given prompt from the specified model. 


##### Parameters
* *modelName:* The name of the model used for completion.
* *prompt:* The prompt for which to retrieve the completion.




##### Return value
A task representing the asynchronous operation. The task result contains the completion for the prompt.



#### GetCompletionAsync(OoBDev.AI.Models.CompletionRequest)
Retrieves a completion for the given prompt from the specified model. 


##### Parameters
* *model:* Completion request model




##### Return value
Resulting response object



## Class: AI.Models.CompletionRequest
Represents a completion request. 

### Properties

#### Model
Gets or initializes the model for the completion request.
#### Context
Gets or initializes the context for the completion request.
#### Images
Gets or initializes the images for the completion request.
#### Prompt
Gets or initializes the prompt for the completion request.
#### System
Gets or initializes the system for the completion request.
#### Template
Gets or initializes the template for the completion request.

## Class: AI.Models.CompletionResponse
Represents a completion response. 

### Properties

#### Context
Gets or sets the context for the completion response.
#### Response
Gets or sets the response generated by the completion process.

## Class: AI.Models.KeyValuePairModel
Base level KeyValuePairModel class 

### Properties

#### Key
Gets or sets the Key to be used.
#### Value
Gets or sets the Value to be used.