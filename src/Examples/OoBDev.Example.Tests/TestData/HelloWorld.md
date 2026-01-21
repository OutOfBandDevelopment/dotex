# Swagger Description - OoBDev.WebApi

*Version*: 0.1.72.13

## Endpoints

### /api/AI


Generate an LLM Response based on the prompt and user input

HTTP Method: *post* \
Anonymous:   *True*



Request:     *#/components/schemas/OoBDev.WebApi.Models.GenerativeAiRequestModel*




Retrieves the embedding vector for the given text.

HTTP Method: *get* \
Anonymous:   *False*





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



### /Search/List


Retrieves a list of all available search results.

HTTP Method: *get* \
Anonymous:   *True*





### /Search/SemanticSearch


Performs a semantic search with the given query.

HTTP Method: *get* \
Anonymous:   *True*





### /Search/LexicalSearch


Performs a lexical search with the given query.

HTTP Method: *get* \
Anonymous:   *True*





### /Search/HybridSearch


Performs a hybrid search with the given query.

HTTP Method: *get* \
Anonymous:   *True*





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





## Models

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


### OoBDev.Search.Models.SearchResultModel


#### Properties
| Name | Type | other |
|------|------|-------|
| score | number | Gets or initializes the score of the search result. | 
| pathHash | string? | Gets or initializes the hash of the path where the search result was found. | 
| file | string? | Gets or initializes the name of the file where the search result was found. | 
| content | string? | Gets or initializes the content of the file where the search result was found. | 
| type | OoBDev.Search.Models.SearchTypes |  | 


### OoBDev.Search.Models.SearchTypes


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
| input | string? | Gets or sets the input string. Default value is &quot;Default Value&quot;. | 
| data | System.Text.Json.Nodes.JsonNode |  | 


### OoBDev.WebApi.Models.GenerativeAiRequestModel


#### Properties
| Name | Type | other |
|------|------|-------|
| promptDetails | string? | Gets or sets the prompt details. | 
| userInput | string? | Gets or sets the user input. | 
| apiKey | string? | Gets or sets the input api key to use. | 


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


