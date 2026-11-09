# Example web API: REST endpoints

Generated from the application's OpenAPI document by `ApiGuideTests`; do not edit by hand.

**Document:** OoBDev.Example.WebApi current

## Operations

### GET `/api/AllMiniLM`

Parameters:

- `text` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Cache`

Parameters:

- `key` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### PUT `/api/Cache`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### DELETE `/api/Cache`

Parameters:

- `key` (query, string)

Responses:

- `200` OK

### POST `/api/Communications/private`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)
- `502` Bad Gateway (`text/plain`, `application/json`, `text/json`)

### POST `/api/Communications/public`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)
- `502` Bad Gateway (`text/plain`, `application/json`, `text/json`)

### POST `/api/Communications/queued`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/GroqCloud/Completion`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Images/classify`

Request body: `multipart/form-data`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Images/embed`

Request body: `multipart/form-data`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/MessageQueueing/channels`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/MessageQueueing/private`

Parameters:

- `correlationId` (query, string)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/MessageQueueing/public`

Parameters:

- `correlationId` (query, string)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/MessageQueueing/received`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/ModelCompare/models`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/ModelCompare/similarity`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Ollama`

Parameters:

- `text` (query, string)
- `model` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Ollama`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Ollama/Completion`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Ollama/Streamed`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Operations/info`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Rag`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/SBERT`

Parameters:

- `text` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/SBERTDefault`

Parameters:

- `text` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Search`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Telemetry/recent`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTemplate/Apply`

Parameters:

- `templateName` (query, string)

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### GET `/api/TextTemplate/List`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTemplate/Preview`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK

### GET `/api/TextTemplate/Read`

Parameters:

- `file` (query, string)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTemplate/SupportedTemplates`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTools/markdown`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/TextTools/xpath`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/Toolbox/algorithms`

Responses:

- `200` OK

### POST `/api/Toolbox/calculate`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/hash`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/hmac`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/otp`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/Toolbox/vector-distance`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/User/claims`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/VectorLab/matrix`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/VectorStore/index`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### POST `/api/VectorStore/query`

Request body: `application/json`, `text/json`, `application/*+json`

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/api/VectorStore/{collection}`

Parameters:

- `collection` (path, string, required)

Responses:

- `200` OK (`text/plain`, `application/json`, `text/json`)

### GET `/asyncapi/{document}`

Parameters:

- `document` (path, string, required)

Responses:

- `200` OK

### GET `/asyncapi/{document}.json`

Parameters:

- `document` (path, string, required)

Responses:

- `200` OK

### GET `/health`

Responses:

- `200` Health report (`application/json`)

### POST `/webhooks/{channel}`

Parameters:

- `channel` (path, string, required)

Responses:

- `204` No Content
- `400` Bad Request (`text/plain`, `application/json`, `text/json`)
- `404` Not Found (`text/plain`, `application/json`, `text/json`)

