Here is the documentation for the source code files:

**Swagger Description**

This is the Swagger documentation for the OoBDev.WebApi project. It provides a description of the API endpoints, models, and schema definitions used throughout the API.

**Endpoints**

The API has several endpoints, each with its own HTTP method, anonymous access, and request schema. Here's a brief summary of each endpoint:

1. **/api/AI**: Generates an LLM Response based on the prompt and user input.
2. **/api/Communications/public**: Sends an email publicly without requiring authentication.
3. **/api/Communications/queued**: Enqueues an email message to be processed asynchronously.
4. **/api/Communications/private**: Sends an email with authorization, requiring the caller to be authenticated.
5. **/Document/Download/{file}**: Downloads the specified file.
6. **/Document/Text/{file}**: Retrieves the text of the specified file.
7. **/Document/Html/{file}**: Retrieves the HTML content of the specified file.
8. **/Document/Pdf/{file}**: Retrieves the PDF content of the specified file.
9. **/Document/Summary/{file}**: Retrieves the summary of the specified file.
10. **/api/MessageQueueing/public**: Sends a message to the queue publicly without requiring authentication.
11. **/api/MessageQueueing/private**: Sends a message to the queue with authorization, requiring the caller to be authenticated.
12. **/Search/List**: Retrieves a list of all available search results.
13. **/Search/SemanticSearch**: Performs a semantic search with the given query.
14. **/Search/LexicalSearch**: Performs a lexical search with the given query.
15. **/Search/HybridSearch**: Performs a hybrid search with the given query.
16. **/api/TextTemplate/SupportedTemplates**: Gets the list of supported template file types.
17. **/api/TextTemplate/Apply**: Applies a text template with the specified name and data.
18. **/api/User/claims**: Gets the claims associated with the current user.

**Models**

The API uses several models to represent data in its requests and responses. Here's a brief summary of each model:

1. **OoBDev.Communications.Models.AttachmentReferenceModel**: Represents an attachment reference in an email message.
2. **OoBDev.Communications.Models.EmailMessageModel**: Represents an email message, including its subject, body, and attachments.
3. **OoBDev.Search.Models.SearchResultModel**: Represents a search result, including its score, path hash, and file content.
4. **OoBDev.Search.Models.SearchTypes**: Enumerates the types of search results.
5. **OoBDev.System.Text.Templating.FileType**: Represents a file type, including its extension, content type, and whether it's a template type.
6. **OoBDev.WebApi.Models.ClaimModel**: Represents a claim, including its issuer, value, and type.
7. **OoBDev.WebApi.Models.ExampleMessageModel**: Represents an example message, including its input and JSON data.
8. **OoBDev.WebApi.Models.GenerativeAiRequestModel**: Represents a generative AI request, including its prompt details, user input, and API key.
9. **System.Text.Json.Nodes.JsonNode**: Represents a JSON node, including its options, parent, and root.
10. **System.Text.Json.Nodes.JsonNodeOptions**: Represents the options for the JSON node.

**Class Diagrams (Plant UML)**

Here are the class diagrams for the models mentioned above:

```plantuml
@startuml
class AttachmentReferenceModel {
  - containerName: string?
  - documentKey: string?
}

class EmailMessageModel {
  - referenceId: string?
  - fromAddress: string?
  - toAddresses: string[]?
  - ccAddresses: string[]?
  - bccAddresses: string[]?
  - subject: string?
  - textContent: string?
  - htmlContent: string?
  - headers: object?
  - attachments: AttachmentReferenceModel[]?
}

class SearchResultModel {
  - score: number
  - pathHash: string?
  - file: string?
  - content: string?
  - type: SearchTypes
}

enum SearchTypes {
  SemanticSearch
  LexicalSearch
  HybridSearch
}

class FileType {
  - extension: string?
  - contentType: string?
  - isTemplateType: boolean
}

class ClaimModel {
  - issuer: string?
  - value: string?
  - valueType: string?
  - type: string?
  - originalIssuer: string?
  - subjectName: string?
  - subjectLabel: string?
  - subjectAuthenticationType: string?
}

class ExampleMessageModel {
  - input: string?
  - data: JsonNode
}

class GenerativeAiRequestModel {
  - promptDetails: string