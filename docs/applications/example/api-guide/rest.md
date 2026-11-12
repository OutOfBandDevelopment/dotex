# Example web API: REST endpoints

Generated from the application's OpenAPI document by `ApiGuideTests`; do not edit by hand.

**Document:** OoBDev.Example.WebApi current

## Operations

### GET `/api/AllMiniLM`

Access: authenticated

Parameters:

- `text` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Cache`

Access: authenticated

Parameters:

- `key` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### PUT `/api/Cache`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### DELETE `/api/Cache`

Access: authenticated

Parameters:

- `key` (query, string)

Responses:

- `200` OK

### GET `/api/CgiPrograms`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Communications/private`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)
- `502` Bad Gateway (`text/plain`, `application/json`, `text/json`)

### POST `/api/Communications/public`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)
- `502` Bad Gateway (`text/plain`, `application/json`, `text/json`)

### POST `/api/Communications/queued`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/GroqCloud/Completion`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Identity/roles`

Access: authenticated; rights: `identity.read`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Identity/users`

Access: authenticated; rights: `identity.read`

Parameters:

- `search` (query, string)
- `max` (query)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Images/classify`

Access: anonymous

Request body: `multipart/form-data`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Images/embed`

Access: anonymous

Request body: `multipart/form-data`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/MessageQueueing/channels`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/MessageQueueing/private`

Access: authenticated

Parameters:

- `correlationId` (query, string)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/MessageQueueing/public`

Access: anonymous

Parameters:

- `correlationId` (query, string)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/MessageQueueing/received`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/ModelCompare/models`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/ModelCompare/similarity`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Ollama`

Access: authenticated

Parameters:

- `text` (query, string)
- `model` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Ollama`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Ollama/Completion`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Ollama/Streamed`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Operations/info`

Access: authenticated

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Rag`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Rights/check`

Access: anonymous

Parameters:

- `rights` (query, array)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Rights/endpoints`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Rights/protected`

Access: anonymous; rights: `rights.demo`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/SBERT`

Access: authenticated

Parameters:

- `text` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/SBERTDefault`

Access: authenticated

Parameters:

- `text` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Search`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Telemetry/recent`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTemplate/Apply`

Access: anonymous

Parameters:

- `templateName` (query, string)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### GET `/api/TextTemplate/List`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTemplate/Preview`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### GET `/api/TextTemplate/Read`

Access: anonymous

Parameters:

- `file` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### PUT `/api/TextTemplate/Save`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### POST `/api/TextTemplate/SupportedTemplates`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTools/markdown`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTools/xpath`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Toolbox/algorithms`

Access: authenticated

Responses:

- `200` OK

### POST `/api/Toolbox/calculate`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/hash`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/hmac`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/otp`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/vector-distance`

Access: authenticated

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/UiDefinitions`

Access: anonymous

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/UiDefinitions/{key}`

Access: anonymous

Parameters:

- `key` (path, string, required)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/UiDefinitions/{key}/evaluate`

Access: anonymous

Parameters:

- `key` (path, string, required)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/User/claims`

Access: authenticated

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/VectorLab/matrix`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/VectorStore/index`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/VectorStore/query`

Access: anonymous

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/VectorStore/{collection}`

Access: anonymous

Parameters:

- `collection` (path, string, required)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/asyncapi/{document}`

Access: authenticated

Parameters:

- `document` (path, string, required)

Responses:

- `200` OK

### GET `/asyncapi/{document}.json`

Access: authenticated

Parameters:

- `document` (path, string, required)

Responses:

- `200` OK

### GET `/health`

Responses:

- `200` Health report (`application/json`)

### POST `/webhooks/{channel}`

Access: anonymous

Parameters:

- `channel` (path, string, required)

Responses:

- `204` No Content
- `400` Bad Request (`text/plain`, `application/json`, `text/json`)
- `404` Not Found (`text/plain`, `application/json`, `text/json`)

