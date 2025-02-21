# Swagger Description - OoBDev.WebApi

*Version*: 1.0.0.0

## Endpoints

### /api/AI


Generate an LLM Response based on the prompt and user input

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




### /api/AI/Streamed


Generate an AbstractAI Streamed Response based on the prompt and user input

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




### /api/AI/Context


Generate an LLM Response based on the prompt and user input

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenAiContextRequestModel*




### /api/AI/Embeddings


Generate embeddings

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




Response: #/components/schemas/System.ReadOnlyMemory-System.Single

### /api/AI/RaggedAnne


Testing Rag endpoint for context

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




### /api/Communications/public


Sends an email publicly without requiring authentication.

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.Communications.Models.EmailMessageModel*

The email message model.



### /api/Communications/queued


Enqueues an email message to be processed asynchronously.

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.Communications.Models.EmailMessageModel*

The email message model.



### /api/Communications/private


Sends an email with authorization, requiring the caller to be authenticated.

HTTP Method: *post* \
Anonymous:   *False*



Request:     *#/components/schemas/OoBDev.Communications.Models.EmailMessageModel*

The email message model.



### /Document/Download/{file}


Downloads the specified file.

HTTP Method: *get* \
Anonymous:   *True*





### /Document/Text/{file}


Retrieves the text of the specified file.

HTTP Method: *get* \
Anonymous:   *True*





### /Document/Html/{file}


Retrieves the html of the specified file.

HTTP Method: *get* \
Anonymous:   *True*





### /Document/Pdf/{file}


Retrieves the pdf of the specified file.

HTTP Method: *get* \
Anonymous:   *True*





### /Document/Summary/{file}


Retrieves the summary of the specified file.

HTTP Method: *get* \
Anonymous:   *True*





### /Document/Upload/{file}


Upload file content

HTTP Method: *post* \
Anonymous:   *True*





### /Document/Convert


Document Converter

HTTP Method: *post* \
Anonymous:   *True*





### /api/GroqCloud/Completion


executes a completion request

HTTP Method: *post* \
Anonymous:   *False*



Request:     *#/components/schemas/OoBDev.AI.Models.CompletionRequest*

completion request



Response: #/components/schemas/OoBDev.AI.Models.CompletionResponse

### /api/MessageQueueing/public


Sends a message to the queue publicly without requiring authentication.

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.ExampleMessageModel*

The example message model.



### /api/MessageQueueing/private


Sends a message to the queue with authorization, requiring the caller to be authenticated.

HTTP Method: *post* \
Anonymous:   *False*



Request:     *#/components/schemas/OoBDev.WebApi.Models.ExampleMessageModel*

The example message model.



### /api/Ollama


Generate an LLM Response based on the prompt and user input

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




Retrieves the embedding vector for the given text.

HTTP Method: *get* \
Anonymous:   *False*





Response: #/components/schemas/System.ReadOnlyMemory-System.Single

### /api/Ollama/Streamed


Generate an AbstractAI Streamed Response based on the prompt and user input

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




### /api/Ollama/Completion


executes a completion request

HTTP Method: *post* \
Anonymous:   *False*



Request:     *#/components/schemas/OoBDev.AI.Models.CompletionRequest*

completion request



Response: #/components/schemas/OoBDev.AI.Models.CompletionResponse

### /api/SBERT


Retrieves the embedding vector for the given text.

HTTP Method: *get* \
Anonymous:   *False*





Response: #/components/schemas/System.ReadOnlyMemory-System.Single

### /api/TextTemplate/SupportedTemplates


Gets the list of supported template file types.

HTTP Method: *post* \
Anonymous:   *True*





### /api/TextTemplate/Apply


Applies a text template with the specified name and data.

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/System.Text.Json.Nodes.JsonNode*

The JSON data used for template processing.



### /api/User/claims


Gets the claims associated with the current user.

HTTP Method: *get* \
Anonymous:   *False*





### /health




HTTP Method: *get* \
Anonymous:   **





## Models

### OoBDev.AI.Models.CompletionRequest


#### Properties
| Name | Type | other |
|------|------|-------|
| model | string? | Gets or initializes the model for the completion request. | 
| context | integer[]? | Gets or initializes the context for the completion request. | 
| images | string[]? | Gets or initializes the images for the completion request. | 
| prompt | string? | Gets or initializes the prompt for the completion request. | 
| system | string? | Gets or initializes the system for the completion request. | 
| template | string? | Gets or initializes the template for the completion request. | 


### OoBDev.AI.Models.CompletionResponse


#### Properties
| Name | Type | other |
|------|------|-------|
| context | integer[]? | Gets or sets the context for the completion response. | 
| response | string? | Gets or sets the response generated by the completion process. | 


### OoBDev.Communications.Models.AttachmentReferenceModel


#### Properties
| Name | Type | other |
|------|------|-------|
| containerName | string? | Gets the name of the container where the attachment is stored. | 
| documentKey | string? | Gets the key or identifier of the document associated with the attachment. | 


### OoBDev.Communications.Models.EmailMessageModel


#### Properties
| Name | Type | other |
|------|------|-------|
| referenceId | string? | Gets or sets the reference ID of the email message. | 
| fromAddress | string? | Gets or sets the sender's email address. | 
| toAddresses | string[]? | Gets or sets the list of recipient email addresses. | 
| ccAddresses | string[]? | Gets or sets the list of carbon copy (CC) email addresses. | 
| bccAddresses | string[]? | Gets or sets the list of blind carbon copy (BCC) email addresses. | 
| subject | string? | Gets or sets the subject of the email message. | 
| textContent | string? | Gets or sets the plain text content of the email message. | 
| htmlContent | string? | Gets or sets the HTML content of the email message. | 
| headers | object? | Gets or sets the headers of the email message. | 
| attachments | array? | Gets or sets the list of attachment references in the email message. | 


### OoBDev.System.Text.Templating.FileType


#### Properties
| Name | Type | other |
|------|------|-------|
| extension | string? | Gets or sets the file extension associated with the file type. | 
| contentType | string? | Gets or sets the content type associated with the file type. | 
| isTemplateType | boolean | Gets or sets a value indicating whether the file type is a template type. | 


### OoBDev.WebApi.Models.ClaimModel


#### Properties
| Name | Type | other |
|------|------|-------|
| issuer | string? | Gets or sets the issuer of the claim. | 
| value | string? | Gets or sets the value of the claim. | 
| valueType | string? | Gets or sets the value type of the claim. | 
| type | string? | Gets or sets the type of the claim. | 
| originalIssuer | string? | Gets or sets the original issuer of the claim. | 
| subjectName | string? | Gets or sets the subject name associated with the claim. | 
| subjectLabel | string? | Gets or sets the subject label associated with the claim. | 
| subjectAuthenticationType | string? | Gets or sets the authentication type of the subject associated with the claim. | 


### OoBDev.WebApi.Models.ExampleMessageModel


#### Properties
| Name | Type | other |
|------|------|-------|
| input | string? | Gets or sets the input string. Default value is "Default Value". | 
| data | System.Text.Json.Nodes.JsonNode |  | 


### OoBDev.WebApi.Models.GenAiContextRequestModel


#### Properties
| Name | Type | other |
|------|------|-------|
| assistantConfinment | string? | Gets or sets the prompt details. | 
| promptDetails | string[]? | Gets or sets the prompt details. | 
| userInput | string[]? | Gets or sets the user input. | 


### OoBDev.WebApi.Models.GenerativeAiRequestModel


#### Properties
| Name | Type | other |
|------|------|-------|
| promptDetails | string? | Gets or sets the prompt details. | 
| data | string? | Gets or sets the prompt details. | 
| userInput | string? | Gets or sets the user input. | 


### System.ReadOnlyMemory-System.Single


#### Properties
| Name | Type | other |
|------|------|-------|
| length | integer |  | 
| isEmpty | boolean |  | 
| span | System.ReadOnlySpan-System.Single |  | 


### System.ReadOnlySpan-System.Single


#### Properties
| Name | Type | other |
|------|------|-------|
| length | integer |  | 
| isEmpty | boolean |  | 


### System.Text.Json.Nodes.JsonNode


#### Properties
| Name | Type | other |
|------|------|-------|
| options | System.Text.Json.Nodes.JsonNodeOptions |  | 
| parent | System.Text.Json.Nodes.JsonNode |  | 
| root | System.Text.Json.Nodes.JsonNode |  | 


### System.Text.Json.Nodes.JsonNodeOptions


#### Properties
| Name | Type | other |
|------|------|-------|
| propertyNameCaseInsensitive | boolean |  | 


