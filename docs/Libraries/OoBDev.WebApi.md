# OoBDev.WebApi


## Class: WebApi.ApplicationServiceCollectionBuilder
Provides extension methods for configuring application services in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### AddApplicationServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Adds application services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The instance.




##### Return value
The updated instance.



## Class: WebApi.Controllers.AIController
Controller for AI-related operations. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.AIController*class with the specified dependencies. 


##### Parameters
* *llmProvider:* The language model provider.




#### GetResponseAsync(OoBDev.WebApi.Models.GenerativeAiRequestModel)
Generate an LLM Response based on the prompt and user input 


##### Return value
The string response from the LLM



#### GetStreamedResponseAsync(OoBDev.WebApi.Models.GenerativeAiRequestModel)
Generate an AbstractAI Streamed Response based on the prompt and user input 


##### Return value
The streamed string responses from the AbstractAI



#### GetContextResponseAsync(OoBDev.WebApi.Models.GenAiContextRequestModel)
Generate an LLM Response based on the prompt and user input 


##### Return value
The string response from the LLM



#### GenerateEmbeddingsAsync(OoBDev.WebApi.Models.GenerativeAiRequestModel)
Generate embeddings 


##### Return value
The float response from the LLM



#### GetRAGResponseAsync(OoBDev.WebApi.Models.GenerativeAiRequestModel)
Testing Rag endpoint for context 


##### Return value
The float response from the LLM



## Class: WebApi.Controllers.CommunicationsController
Controller for handling communication-related operations, such as sending emails and messages to a queue. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Controller for handling communication-related operations, such as sending emails and messages to a queue. 


##### Parameters
* *email:* The email communication sender.
* *queue:* The message queue sender.




#### SendAnonymous(OoBDev.Communications.Models.EmailMessageModel)
Sends an email publicly without requiring authentication. 


##### Parameters
* *model:* The email message model.




##### Return value
A task representing the asynchronous operation and containing a string result.



#### Enqueue(OoBDev.Communications.Models.EmailMessageModel)
Enqueues an email message to be processed asynchronously. 


##### Parameters
* *model:* The email message model.




##### Return value
A task representing the asynchronous operation and containing a string result.



#### SendAuthorize(OoBDev.Communications.Models.EmailMessageModel)
Sends an email with authorization, requiring the caller to be authenticated. 


##### Parameters
* *model:* The email message model.




##### Return value
A task representing the asynchronous operation and containing a string result.



## Class: WebApi.Controllers.DocumentController
Controller for handling document-related operations. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.DocumentController*class. 


##### Parameters
* *converter:* The document conversion service.
* *documentTypes:* The document type tools.
* *logger:* The logger.
* *docs:* The blob container for documents.
* *summary:* The blob container for summaries.




#### Download(System.String)
Downloads the specified file. 


##### Parameters
* *file:* The path to the file.




#### Text(System.String)
Retrieves the text of the specified file. 


##### Parameters
* *file:* The path to the file.




#### Html(System.String)
Retrieves the html of the specified file. 


##### Parameters
* *file:* The path to the file.




#### Pdf(System.String)
Retrieves the pdf of the specified file. 


##### Parameters
* *file:* The path to the file.




#### Summary(System.String)
Retrieves the summary of the specified file. 


##### Parameters
* *file:* The path to the file.




#### Upload(Microsoft.AspNetCore.Http.IFormFile,System.String,System.String)
Upload file content 


##### Parameters
* *content:* upload file content
* *file:* 
* *sourceContentType:* optionally overload the provided MIME Type




##### Return value




#### Convert(Microsoft.AspNetCore.Http.IFormFile,System.String,System.String)
Document Converter 


##### Parameters
* *content:* upload file content
* *targetContentType:* define the target MIME type
* *sourceContentType:* optionally overload the provided MIME Type




##### Return value




## Class: WebApi.Controllers.DocumentController.Documents
Represents a container for documents. 


## Class: WebApi.Controllers.DocumentController.Summaries
Represents a container for documents. 


## Class: WebApi.Controllers.GroqCloudController
Controller for AI-related operations. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.AIController*class with the specified dependencies. 


##### Parameters
* *completion:* The completion provider.




#### Completion(OoBDev.AI.Models.CompletionRequest)
executes a completion request 


##### Parameters
* *model:* completion request




##### Return value
completion result



## Class: WebApi.Controllers.MessageQueueingController
Controller for handling message queueing operations. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Controller for handling message queueing operations. 


##### Parameters
* *provider:* The example message provider.




#### PublicSend(OoBDev.WebApi.Models.ExampleMessageModel,System.String)
Sends a message to the queue publicly without requiring authentication. 


##### Parameters
* *model:* The example message model.
* *correlationId:* Optional correlation ID for tracking purposes.




##### Return value
A task representing the asynchronous operation and containing a string result.



#### AuthenticatedSend(OoBDev.WebApi.Models.ExampleMessageModel,System.String)
Sends a message to the queue with authorization, requiring the caller to be authenticated. 


##### Parameters
* *model:* The example message model.
* *correlationId:* Optional correlation ID for tracking purposes.




##### Return value
A task representing the asynchronous operation and containing a string result.



## Class: WebApi.Controllers.OllamaController
Controller for AI-related operations. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.AIController*class with the specified dependencies. 


##### Parameters
* *llmProvider:* The language model provider.
* *completion:* The completion provider.
* *embedding:* The embedding provider.




#### GetResponseAsync(OoBDev.WebApi.Models.GenerativeAiRequestModel)
Generate an LLM Response based on the prompt and user input 


##### Return value
The string response from the LLM



#### GetStreamedResponseAsync(OoBDev.WebApi.Models.GenerativeAiRequestModel)
Generate an AbstractAI Streamed Response based on the prompt and user input 


##### Return value
The streamed string responses from the AbstractAI



#### Embed(System.String,System.String)
Retrieves the embedding vector for the given text. 


##### Parameters
* *text:* The text for which to retrieve the embedding vector.
* *model:* The model for which to retrieve the embedding vector.




##### Return value
The embedding vector.



#### Completion(OoBDev.AI.Models.CompletionRequest)
executes a completion request 


##### Parameters
* *model:* completion request




##### Return value
completion result



## Class: WebApi.Controllers.SBERTController
Controller for AI-related operations. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.AIController*class with the specified dependencies. 


##### Parameters
* *embedding:* The embedding provider.




#### Embed(System.String)
Retrieves the embedding vector for the given text. 


##### Parameters
* *text:* The text for which to retrieve the embedding vector.




##### Return value
The embedding vector.



## Class: WebApi.Controllers.SearchController
Controller for handling search operations. 


## Class: WebApi.Controllers.TextTemplateController
Controller for handling text template operations. 
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.TextTemplateController*class. 

### Methods


#### Constructor
Controller for handling text template operations. 


##### Parameters
* *engine:* The template engine for processing text templates.
* *fileTypes:* The collection of file types supported by the controller.




#### SupportedTemplates
Gets the list of supported template file types. 


##### Return value
An enumeration of supported file types.



#### Apply(System.String,System.Text.Json.Nodes.JsonNode)
Applies a text template with the specified name and data. 


##### Parameters
* *templateName:* The name of the text template to apply.
* *data:* The JSON data used for template processing.




##### Return value
An action result containing the processed template content as a downloadable file.



## Class: WebApi.Controllers.UserController
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.UserController*class. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WebApi.Controllers.UserController*class. 


##### Parameters
* *claimsPrincipal:* The claims principal representing the current user.




#### GetClaims
Gets the claims associated with the current user. 


##### Return value
An enumerable collection of representing the user's claims.



## Class: WebApi.Models.ClaimModel
Represents a model for a claim. 

### Properties

#### Issuer
Gets or sets the issuer of the claim. This property is required.
#### Value
Gets or sets the value of the claim. This property is required.
#### ValueType
Gets or sets the value type of the claim. This property is required.
#### Type
Gets or sets the type of the claim. This property is required.
#### OriginalIssuer
Gets or sets the original issuer of the claim. This property is required.
#### SubjectName
Gets or sets the subject name associated with the claim. This property is optional.
#### SubjectLabel
Gets or sets the subject label associated with the claim. This property is optional.
#### SubjectAuthenticationType
Gets or sets the authentication type of the subject associated with the claim. This property is optional.

## Class: WebApi.Models.ExampleMessageModel
Represents an example message model. 

### Properties

#### Input
Gets or sets the input string. Default value is "Default Value".
#### Data
Gets or sets the JSON data associated with the message.

## Class: WebApi.Models.GenAiContextRequestModel
Represents the model used for generating AI responses. 

### Properties

#### AssistantConfinment
Gets or sets the prompt details.
#### PromptDetails
Gets or sets the prompt details.
#### UserInput
Gets or sets the user input.

## Class: WebApi.Models.GenerativeAiRequestModel
Represents the model used for generating AI responses. 

### Properties

#### PromptDetails
Gets or sets the prompt details.
#### Data
Gets or sets the prompt details.
#### UserInput
Gets or sets the user input.

## Class: WebApi.Program
primary entry point 

### Methods


#### Main(System.String[])
primary entry point 


## Class: WebApi.Provider.ExampleMessageProvider
Implementation of 
 *See: T:OoBDev.WebApi.Provider.IExampleMessageProvider*and 
 *See: T:OoBDev.MessageQueueing.IMessageQueueHandler`1*for handling and sending example messages. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Implementation of 
 *See: T:OoBDev.WebApi.Provider.IExampleMessageProvider*and 
 *See: T:OoBDev.MessageQueueing.IMessageQueueHandler`1*for handling and sending example messages. 


##### Parameters
* *sender:* The message queue sender for sending messages.
* *logger:* The logger for logging messages.




#### PostAsync(System.Object,System.String)
Posts an example message asynchronously. 


##### Parameters
* *message:* The message to be sent.
* *correlationId:* The optional correlation ID.




##### Return value
The message ID.



#### HandleAsync(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Handles an example message asynchronously. 


##### Parameters
* *message:* The received message.
* *context:* The message context.




##### Return value
A task representing the asynchronous operation.



## Class: WebApi.Provider.IExampleMessageProvider
Interface for providing asynchronous operations related to example messages. 

### Methods


#### PostAsync(System.Object,System.String)
Posts an example message asynchronously. 


##### Parameters
* *message:* The message object to be posted.
* *correlationId:* Optional correlation ID for tracking purposes.




##### Return value
A task representing the asynchronous operation and containing a string result.

