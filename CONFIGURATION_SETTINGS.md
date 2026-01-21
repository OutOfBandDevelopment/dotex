# OoBDev Framework - Configuration Reference

**Last Updated:** 2026-01-21
**Framework Version:** net10.0
**Total Configuration Points:** 157+

---

## Table of Contents

- [Overview](#overview)
- [Configuration Hierarchy](#configuration-hierarchy)
- [Options Pattern Classes](#options-pattern-classes)
  - [Framework Layer](#framework-layer)
  - [External Services Layer](#external-services-layer)
  - [Extensions Layer](#extensions-layer)
  - [Tools & CLI Layer](#tools--cli-layer)
- [Direct Configuration Keys](#direct-configuration-keys)
- [Environment Variables](#environment-variables)
  - [Runtime Environment](#runtime-environment)
  - [Framework-Specific](#framework-specific)
  - [Database & Storage](#database--storage)
  - [Message Queues](#message-queues)
  - [Cloud Services & Emulators](#cloud-services--emulators)
  - [Identity & Authentication](#identity--authentication)
  - [AI & ML Services](#ai--ml-services)
  - [Docker & Infrastructure](#docker--infrastructure)
- [Connection Strings](#connection-strings)
- [Test Configuration](#test-configuration)
- [Configuration Best Practices](#configuration-best-practices)
- [Validation Rules](#validation-rules)
- [Migration Guide](#migration-guide)

---

## Overview

This document provides a comprehensive reference for all configuration settings used across the OoBDev framework. Configuration follows the .NET Options Pattern and supports multiple configuration sources.

### Configuration Statistics

| Category | Count |
|----------|-------|
| Options Pattern Classes | 31 |
| Direct Configuration Keys | 24 |
| Environment Variables | 102 |
| Test Parameters | 30+ (see TEST_VARIABLES.md) |
| **Total Configuration Points** | **157+** |

### Configuration Sources (Priority Order)

1. Command-line arguments
2. Environment variables
3. User secrets (development only)
4. appsettings.{Environment}.json
5. appsettings.json
6. Default values in Options classes

### Configuration Naming Conventions

- **Options Classes**: `{Feature}Options` or `{Feature}Settings`
- **Configuration Sections**: Match the feature name (e.g., `MongoDB`, `Redis`, `Caching`)
- **Environment Variables**: UPPERCASE_WITH_UNDERSCORES
- **Configuration Keys**: PascalCase or colon-separated sections

---

## Configuration Hierarchy

```
appsettings.json
├── AllMiniLmL6V2Embedding
│   └── PercentageOfParallelism
├── ApacheTikaClientOptions
│   └── Url
├── Azure
│   ├── EventHub/Default/DefaultHubName
│   └── ServiceBus
│       ├── ConnectionString
│       ├── QueueName
│       └── TopicName
├── AzureBlobProviderOptions
│   ├── ConnectionString
│   └── EnsureContainerExists
├── Census/Geocoding
│   ├── BenchmarkId
│   ├── VintageId
│   └── UrlFormatter
├── ConnectionStrings
│   └── {ConnectionStringName}
├── DacPac
│   ├── Template/Path
│   ├── Source/Path
│   ├── Source/Patterns
│   ├── Target/{Path|Description|Name|BuildVersion|Version}
│   └── Setting/ModelOptionSource
├── FileTemplatingOptions
│   ├── TemplatePath
│   ├── SandboxPath
│   └── Priority
├── Google/Maps
│   └── ApiKey
├── GroqCloudApiClientOptions
│   ├── ApiKey
│   └── Model
├── MailKitSmtpClientOptions
│   ├── Host
│   ├── Port
│   ├── SecureSocketOption
│   └── {Uri|Password|UserName|DefaultFromEmailAddress}
├── MailKitImapClientOptions
│   ├── Host
│   ├── Port
│   └── {SecureSocketOption|Uri|Password|UserName}
├── MessageQueue
│   ├── {TargetName}/{MessageName}/{Config|Provider}
│   ├── {MessageName}/{Config|Provider}
│   ├── {TargetName}/{Config|Provider}
│   └── Default/{Config|Provider}
├── MicrosoftIdentityOptions
│   ├── ClientID
│   ├── Issuer
│   ├── ClientSecret
│   └── Tenant
├── MongoDatabaseOptions
│   ├── ConnectionString
│   └── DatabaseName
├── OAuth2SwaggerOptions
│   ├── UserReadApiClaim
│   ├── AuthorizationUrl
│   └── TokenUrl
├── OllamaApiClientOptions
│   ├── Url
│   └── DefaultModel
├── OoBDev
│   ├── Communications/EmailMessageComposer/EnableTracing
│   └── ServiceKeys/{FullTypeName}
├── OpenSearchOptions
│   ├── HostName
│   ├── Port
│   ├── IndexName
│   ├── UserName
│   └── Password
├── QdrantOptions
│   ├── Url
│   ├── CollectionName
│   └── EnsureCollectionExists
├── Redis/ConnectionMultiplexer
│   └── Config
├── SentenceEmbeddingOptions
│   └── Url
└── Twilio
    ├── SendGrid/Default/{From/Email|Subject}
    └── SmsMessaging/{AccountSid|AuthToken|Default/From}
```

---

## Options Pattern Classes

### Framework Layer

#### FileTemplatingOptions

**Namespace:** `OoBDev.System.Text.Templating`
**Configuration Section:** `FileTemplatingOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| TemplatePath | string | No | "./" | Template source path |
| SandboxPath | string? | No | null | Sandbox root path (security boundary) |
| Priority | int | No | 100 | Template resolution priority |

**Usage:**
```csharp
services.Configure<FileTemplatingOptions>(options =>
{
    options.TemplatePath = "./templates";
    options.SandboxPath = "./sandbox";
    options.Priority = 100;
});
```

**JSON Configuration:**
```json
{
  "FileTemplatingOptions": {
    "TemplatePath": "./templates",
    "SandboxPath": "./sandbox",
    "Priority": 100
  }
}
```

---

#### OAuth2SwaggerOptions

**Namespace:** `OoBDev.AspNetCore.JwtAuthentication.SwaggerGen`
**Configuration Section:** `OAuth2SwaggerOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| UserReadApiClaim | string | Yes | - | Claim used to determine API access |
| AuthorizationUrl | string | Yes | - | OAuth2 authorization endpoint URL |
| TokenUrl | string | Yes | - | OAuth2 token endpoint URL |

**Validation:**
- All properties required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<OAuth2SwaggerOptions>(options =>
{
    options.UserReadApiClaim = "api_access";
    options.AuthorizationUrl = "https://auth.example.com/oauth2/authorize";
    options.TokenUrl = "https://auth.example.com/oauth2/token";
});
```

---

### External Services Layer

#### ApacheTikaClientOptions

**Namespace:** `OoBDev.Apache.Tika`
**Configuration Section:** `ApacheTikaClientOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Url | string | Yes | - | Apache Tika server URL |

**Usage:**
```csharp
services.Configure<ApacheTikaClientOptions>(options =>
{
    options.Url = "http://localhost:9998";
});
```

**See Also:**
- Environment Variable: `TIKA_URL` (test configuration)

---

#### GroqCloudApiClientOptions

**Namespace:** `OoBDev.GroqCloud`
**Configuration Section:** `GroqCloudApiClientOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| ApiKey | string? | No | - | Groq Cloud API key (falls back to environment variable) |
| Model | string | No | "llama3-8b-8192" | Default LLM model identifier |

**Environment Variable Fallback:**
- `API_Key_Groq` (User-scoped environment variable)

**Usage:**
```csharp
services.Configure<GroqCloudApiClientOptions>(options =>
{
    options.ApiKey = "your-api-key"; // Or set API_Key_Groq env var
    options.Model = "llama3-8b-8192";
});
```

---

#### KeycloakIdentityOptions

**Namespace:** `OoBDev.Keycloak.Identity`
**Configuration Section:** `KeycloakIdentityOptions`

Currently empty placeholder for future configuration.

---

#### MailKitSmtpClientOptions

**Namespace:** `OoBDev.MailKit.Services`
**Configuration Section:** `MailKitSmtpClientOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Host | string | Yes | - | SMTP server host address |
| Port | int | No | 25 | SMTP server port number |
| SecureSocketOption | SecureSocketOptions | No | None | SSL/TLS configuration |
| Uri | Uri? | No | null | Alternative: SMTP server URI |
| Password | string? | No | null | SMTP authentication password |
| UserName | string? | No | null | SMTP authentication username |
| DefaultFromEmailAddress | string? | No | null | Default sender email address |

**Validation:**
- Host: Required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<MailKitSmtpClientOptions>(options =>
{
    options.Host = "smtp.example.com";
    options.Port = 587;
    options.SecureSocketOption = SecureSocketOptions.StartTls;
    options.UserName = "user@example.com";
    options.Password = "password";
});
```

---

#### MailKitImapClientOptions

**Namespace:** `OoBDev.MailKit.Services`
**Configuration Section:** `MailKitImapClientOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Host | string | Yes | - | IMAP server host address |
| Port | int | No | 143 | IMAP server port number |
| SecureSocketOption | SecureSocketOptions | No | None | SSL/TLS configuration |
| Uri | Uri? | No | null | Alternative: IMAP server URI |
| Password | string? | No | null | IMAP authentication password |
| UserName | string? | No | null | IMAP authentication username |

**Validation:**
- Host: Required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<MailKitImapClientOptions>(options =>
{
    options.Host = "imap.example.com";
    options.Port = 993;
    options.SecureSocketOption = SecureSocketOptions.SslOnConnect;
    options.UserName = "user@example.com";
    options.Password = "password";
});
```

---

#### MongoDatabaseOptions

**Namespace:** `OoBDev.MongoDB.Extensions`
**Configuration Section:** `MongoDatabaseOptions`
**Implements:** `IMongoSettings`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| ConnectionString | string | Yes | - | MongoDB connection string |
| DatabaseName | string | Yes | - | MongoDB database name |

**Validation:**
- Both properties required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<MongoDatabaseOptions>(options =>
{
    options.ConnectionString = "mongodb://localhost:27017";
    options.DatabaseName = "myapp";
});
```

**See Also:**
- Environment Variables: `MONGODB_CONNECTION_STRING`, `MONGODB_DATABASE_NAME`

---

#### OllamaApiClientOptions

**Namespace:** `OoBDev.Ollama`
**Configuration Section:** `OllamaApiClientOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Url | string | Yes | - | Ollama API URL |
| DefaultModel | string | Yes | - | Default model identifier (e.g., "phi3") |

**Validation:**
- Both properties required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<OllamaApiClientOptions>(options =>
{
    options.Url = "http://localhost:11434";
    options.DefaultModel = "phi3";
});
```

**See Also:**
- Environment Variables: `OLLAMA_URL`, `OLLAMA_MODEL`

---

#### OpenSearchOptions

**Namespace:** `OoBDev.OpenSearch`
**Configuration Section:** `OpenSearchOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| HostName | string | Yes | - | OpenSearch server hostname |
| Port | int | Yes | 9200 | OpenSearch server port |
| IndexName | string | Yes | - | Index name for operations |
| UserName | string? | No | null | Authentication username |
| Password | string? | No | null | Authentication password |

**Validation:**
- HostName, Port, IndexName: Required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<OpenSearchOptions>(options =>
{
    options.HostName = "localhost";
    options.Port = 9200;
    options.IndexName = "my-index";
    options.UserName = "admin";
    options.Password = "IntegrationTest123!";
});
```

**See Also:**
- Environment Variables: `OPENSEARCH_URL`, `OPENSEARCH_USERNAME`, `OPENSEARCH_PASSWORD`

---

#### QdrantOptions

**Namespace:** `OoBDev.Qdrant`
**Configuration Section:** `QdrantOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Url | string | Yes | - | Qdrant server URL |
| CollectionName | string | Yes | - | Collection name for vectors |
| EnsureCollectionExists | bool | No | false | Auto-create collection if missing |

**Validation:**
- Url, CollectionName: Required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<QdrantOptions>(options =>
{
    options.Url = "http://localhost:6333";
    options.CollectionName = "embeddings";
    options.EnsureCollectionExists = true;
});
```

**See Also:**
- Environment Variable: `QDRANT_URL`

---

#### SentenceEmbeddingOptions

**Namespace:** `OoBDev.SBert`
**Configuration Section:** `SentenceEmbeddingOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| Url | string | Yes | - | SBert HTTP API endpoint |

**Example:** `http://sbert.example.com:5080`

**Validation:**
- Url: Required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<SentenceEmbeddingOptions>(options =>
{
    options.Url = "http://localhost:5080";
});
```

**See Also:**
- Environment Variable: `SBERT_URL`

---

#### AllMiniLmL6V2EmbeddingOptions

**Namespace:** `OoBDev.SBert.AllMiniLML6v2Sharp`
**Configuration Section:** `AllMiniLmL6V2Embedding` (via ConfigPrefix constant)

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| PercentageOfParallelism | double | No | 0.75 | Fraction of CPU cores to use (0-1) |

**Usage:**
```csharp
services.Configure<AllMiniLmL6V2EmbeddingOptions>(options =>
{
    options.PercentageOfParallelism = 0.75; // 75% of available cores
});
```

**JSON Configuration:**
```json
{
  "AllMiniLmL6V2Embedding": {
    "PercentageOfParallelism": 0.75
  }
}
```

---

#### MicrosoftIdentityOptions

**Namespace:** `OoBDev.Microsoft.Azure.B2C.Identity`
**Configuration Section:** `MicrosoftIdentityOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| ClientID | string | Yes | - | Azure AD B2C client ID |
| Issuer | string | Yes | - | Azure AD B2C issuer URL |
| ClientSecret | string | Yes | - | Azure AD B2C client secret |
| Tenant | string | Yes | - | Azure AD B2C tenant identifier |

**Validation:**
- All properties required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<MicrosoftIdentityOptions>(options =>
{
    options.ClientID = "your-client-id";
    options.Issuer = "https://login.microsoftonline.com/your-tenant/v2.0";
    options.ClientSecret = "your-client-secret";
    options.Tenant = "your-tenant.onmicrosoft.com";
});
```

---

#### AzureBlobProviderOptions

**Namespace:** `OoBDev.Microsoft.Azure.StorageAccount.BlobStorage`
**Configuration Section:** `AzureBlobProviderOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| ConnectionString | string | Yes | - | Azure Blob Storage connection string |
| EnsureContainerExists | bool | No | false | Auto-create container if missing |

**Validation:**
- ConnectionString: Required (compile-time via `required` modifier)

**Usage:**
```csharp
services.Configure<AzureBlobProviderOptions>(options =>
{
    options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...";
    options.EnsureContainerExists = true;
});
```

**See Also:**
- Environment Variable: `AZURITE_CONNECTION_STRING` (development/testing)

---

### Extensions Layer

#### EmbeddingSentenceTransformerQueueReaderOptions

**Namespace:** `OoBDev.Data.Vectors.Hosting`
**Configuration Section:** `EmbeddingSentenceTransformerQueueReaderOptions`

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| MaximumReadLength | int | No | 100 | Maximum items to read per batch |
| ReadWaitTimeout | TimeSpan | No | 00:05:00 | Wait timeout between reads (5 minutes) |
| ErrorWaitTimeout | TimeSpan | No | 00:00:30 | Wait timeout after error (30 seconds) |

**Usage:**
```csharp
services.Configure<EmbeddingSentenceTransformerQueueReaderOptions>(options =>
{
    options.MaximumReadLength = 100;
    options.ReadWaitTimeout = TimeSpan.FromMinutes(5);
    options.ErrorWaitTimeout = TimeSpan.FromSeconds(30);
});
```

---

### Tools & CLI Layer

#### DocumentConverterOptions

**Namespace:** `OoBDev.DocumentConverter.Cli`
**Usage:** Command-line tool configuration

| Property | Type | Command Parameter | Description |
|----------|------|-------------------|-------------|
| InputPath | string? | `--input` | Path to input document |
| OutputPath | string? | `--output` | Path to output file |

**CLI Usage:**
```bash
dotnet run --input document.docx --output document.pdf
```

---

#### FileRagEngineOptions

**Namespace:** `OoBDev.FileRagEngine.Cli`
**Usage:** Command-line tool configuration

| Property | Type | Command Parameter | Required | Description |
|----------|------|-------------------|----------|-------------|
| InputPath | string? | `--input` | No | Path to input files |
| Template | string | (config) | Yes | Template content |
| OutputPath | string? | `--output` | No | Path to output file |
| LanguageModelType | string? | `--llm` | No | LLM provider type |
| IncludeRawOutput | bool | `--raw` | No | Include raw LLM output |
| IncludePrompt | bool | `--include-prompt` | No | Include prompt in output |

**Validation:**
- Template: Required (compile-time via `[Required]` and `required` modifier)

---

#### TemplateEngineOptions

**Namespace:** `OoBDev.TemplateEngine.Cli`
**Usage:** Command-line tool configuration

| Property | Type | Command Parameter | Required | Description |
|----------|------|-------------------|----------|-------------|
| InputFile | string? | `--input` | No | Path to input data file |
| Template | string | (config) | Yes | Template content |
| InputFileType | FileTypes? | `--input-type` | No | Input file format (JSON, XML, etc.) |
| OutputFile | string? | `--output` | No | Path to output file |

**Validation:**
- Template: Required (compile-time via `[Required]` and `required` modifier)

---

#### DacPacBuilderEngineOptions

**Namespace:** `OoBDev.DacPacCompiler.Cli`
**Usage:** SQL Server Database Project compiler

| Property | Type | Command Parameter | Required | Default | Description |
|----------|------|-------------------|----------|---------|-------------|
| Tool | DacPackTools | `--tool` | No | SqlClr | Tool mode (SqlClr, Merge) |
| ProjectVersion | string? | `--version` | No | null | Project version |
| ProjectName | string? | `--project` | No | null | Project name |
| DacpacFile | string? | `--dacpac` | No | null | DacPac file path |
| AssemblyPdbFramework | string? | `--pdb` | No | null | PDB file path |
| AssemblyFileFramework | string | `--sqlclr` | Yes | - | Assembly file path |

**Validation:**
- AssemblyFileFramework: Required (compile-time via `required` modifier)

---

## Direct Configuration Keys

These configuration keys are accessed directly via `IConfiguration` indexer or methods, not through Options classes.

### Message Queueing

#### RabbitMQ Connection Factory

**Configuration Section:** Passed as `IConfigurationSection`

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `ConnectionFactory.HostName` | string | Yes | - | RabbitMQ host |
| `ConnectionFactory.Port` | int | No | 5672 | AMQP port |
| `ConnectionFactory.UserName` | string | No | "guest" | Username |
| `ConnectionFactory.Password` | string | No | "guest" | Password |
| `ConnectionFactory.RequestedConnectionTimeout` | int (ms) | No | 30000 | Connection timeout |
| `ConnectionFactory.HandshakeContinuationTimeout` | int (ms) | No | 10000 | Handshake timeout |
| `QueueName` | string | Yes | - | Queue name |

**Usage:**
```csharp
var section = configuration.GetSection("RabbitMQ");
var factory = new QueueClientFactory(section);
```

**See Also:**
- Environment Variables: `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USERNAME`, `RABBITMQ_PASSWORD`

---

#### Azure Service Bus

**Configuration Section:** Passed as `IConfigurationSection`

| Key | Type | Required | Description |
|-----|------|----------|-------------|
| `ConnectionString` | string | Yes | Service Bus connection string |
| `QueueName` | string | Conditional | Queue name (required if TopicName not set) |
| `TopicName` | string | Conditional | Topic name (required if QueueName not set) |

**Usage:**
```csharp
var section = configuration.GetSection("ServiceBus");
var factory = new ServiceBusSenderFactory(section);
```

**See Also:**
- Environment Variable: `SERVICEBUS_CONNECTION_STRING`

---

#### AWS SQS

**Configuration Section:** Passed as `IConfigurationSection`

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `Region` | string | No | "us-east-1" | AWS region |
| `AccessKeyId` | string | No | null | AWS access key (uses credential chain if null) |
| `SecretAccessKey` | string | No | null | AWS secret key |
| `QueueUrl` | string | Conditional | - | Direct queue URL (required if QueueName not set) |
| `QueueName` | string | Conditional | - | Queue name (required if QueueUrl not set) |

**Usage:**
```csharp
var section = configuration.GetSection("Sqs");
var factory = new SqsClientFactory(section);
```

**See Also:**
- Environment Variables: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_DEFAULT_REGION`

---

#### Azure Storage Queue

**Configuration Section:** Passed as `IConfigurationSection`

| Key | Type | Required | Description |
|-----|------|----------|-------------|
| `ConnectionString` | string | Yes | Azure Storage connection string |
| `QueueName` | string | Yes | Queue name |

**Additional Mapper Configuration:**

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `EnsureQueueExists` | bool | No | false | Auto-create queue |
| `WaitDelay` | int (ms) | No | 1000 | Polling delay |

---

#### Message Queue Resolver (Cascading Hierarchy)

**Configuration Pattern:** `MessageQueue:{TargetName}:{MessageName}`

The framework uses a 4-level cascading fallback:

1. `MessageQueue:{TargetName}:{MessageName}`
2. `MessageQueue:{MessageName}`
3. `MessageQueue:{TargetName}`
4. `MessageQueue:Default`

Within each section:
- `Config` (sub-section) - Provider-specific configuration
- `Provider` (string) - Provider key identifier

**Example:**
```json
{
  "MessageQueue": {
    "Default": {
      "Provider": "rabbitmq",
      "Config": {
        "ConnectionFactory.HostName": "localhost",
        "QueueName": "default-queue"
      }
    },
    "OrderProcessing": {
      "Provider": "servicebus",
      "Config": {
        "ConnectionString": "...",
        "QueueName": "orders"
      }
    }
  }
}
```

---

### Database

#### SQL Server Connection Strings

**Configuration Key:** `ConnectionStrings:{ConnectionStringName}`
**Access Method:** `configuration.GetConnectionString("name")`

**Optional Timeout Configuration:**
**Configuration Key:** `CommandTimeouts:{ConnectionStringName}`
**Type:** int (seconds)

**Example:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=mydb;User Id=sa;Password=***"
  },
  "CommandTimeouts": {
    "DefaultConnection": 120
  }
}
```

**See Also:**
- Environment Variable: `SQL_CONNECTION_STRING` (testing)

---

#### SQL Server DacFx (Database Project Compiler)

**Configuration Keys:**

| Key | Type | Description |
|-----|------|-------------|
| `DacPac:Template:Path` | string | Template DacPac path |
| `DacPac:Source:Path` | string | Source files directory |
| `DacPac:Source:Patterns` | string (semicolon-delimited) | File patterns to include |
| `DacPac:Target:Path` | string | Output DacPac file path |
| `DacPac:Target:Description` | string | DacPac description |
| `DacPac:Target:Name` | string | DacPac name |
| `DacPac:Target:BuildVersion` | string | Build version |
| `DacPac:Target:Version` | string | DacPac version |
| `DacPac:Setting:ModelOptionSource` | ModelOptionSource (enum) | Model option source |

**Command-Line Mapping:**
- `--template` → `DacPac:Template:Path`
- `--source` → `DacPac:Source:Path`
- `--output` → `DacPac:Target:Path`

---

### Caching

#### Redis Connection Multiplexer

**Configuration Key:** `Redis:ConnectionMultiplexer:Config`
**Type:** string (StackExchange.Redis connection string format)

**Example:**
```json
{
  "Redis": {
    "ConnectionMultiplexer": {
      "Config": "localhost:6379,password=mypassword"
    }
  }
}
```

**See Also:**
- Environment Variable: `REDIS_CONNECTION_STRING`

---

### Service Selection

#### Keyed Service Selection

**Configuration Key:** `OoBDev::ServiceKeys::{FullTypeName}`
**Type:** string (service key)
**Purpose:** Dynamic service key selection for keyed DI services

**Example:**
```json
{
  "OoBDev": {
    "ServiceKeys": {
      "MyApp.Services.IPaymentProcessor": "stripe"
    }
  }
}
```

---

### External Services

#### Twilio SMS

**Configuration Keys:**

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `Twilio:SmsMessaging:AccountSid` | string | Yes | - | Account SID |
| `Twilio:SmsMessaging:AuthToken` | string | Yes | - | Auth token |
| `Twilio:SmsMessaging:Default:From` | string | No | null | Default sender phone number |

**See Also:**
- Environment Variables: `TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_PHONE_NUMBER`

---

#### Twilio SendGrid

**Configuration Keys:**

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `Twilio:SendGrid:Default:From:Email` | string | No | null | Default from address |
| `Twilio:SendGrid:Default:Subject` | string | No | "Message for you" | Default subject |

**See Also:**
- Environment Variable: `SENDGRID_API_KEY`

---

#### Google Maps Geocoding

**Configuration Key:** `Google:Maps:ApiKey`
**Type:** string (API key)
**Required:** Yes

---

#### Census Geocoding

**Configuration Keys:**

| Key | Type | Required | Default | Description |
|-----|------|----------|---------|-------------|
| `Census:Geocoding:BenchmarkId` | int | No | 4 | Benchmark ID (4 = Current) |
| `Census:Geocoding:VintageId` | int | No | 4 | Vintage ID (4 = Current) |
| `Census:Geocoding:UrlFormatter` | string | No | null | Custom URL format string |

---

### Complex Events

#### Event Hub Default Hub Name

**Configuration Key:** `Azure:EventHub:Default:DefaultHubName`
**Type:** string
**Purpose:** Default Event Hub name when not specified in attribute
**Fallback:** Uses `type.FullName` if not configured

---

### Communications

#### Email Message Composer Tracing

**Configuration Key:** `OoBDev:Communications:EmailMessageComposer:EnableTracing`
**Type:** bool
**Default:** false
**Purpose:** Enable/disable email composition debug tracing

---

## Environment Variables

### Runtime Environment

| Variable | Type | Default | Platform | Description |
|----------|------|---------|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | string | Production | All | ASP.NET Core environment (Development, Staging, Production) |
| `MSBUILDTERMINALLOGGER` | string | off | All | MSBuild logging control (reduces CI/CD noise) |

---

### Framework-Specific

#### API Keys

| Variable | Type | Platform | Description |
|----------|------|----------|-------------|
| `API_Key_Groq` | string | Windows (User) | Groq LLM API authentication |

---

### Database & Storage

#### SQL Server

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `SQL_SA_PASSWORD` | string | IntegrationTest123! | SQL Server sa password |
| `SQL_CONNECTION_STRING` | string | Server=localhost,1433;... | SQL Server connection |
| `WINDIR` | path | C:\Windows | Windows directory (used for .NET Framework refs) |

#### MongoDB

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `MONGODB_CONNECTION_STRING` | string | mongodb://localhost:27017 | MongoDB connection URI |
| `MONGODB_DATABASE_NAME` | string | integration_tests | Database name |

#### Redis

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `REDIS_CONNECTION_STRING` | string | localhost:6379 | Redis connection |
| `REDIS_HOST` | string | localhost | Redis host |
| `REDIS_PORT` | int | 6379 | Redis port |

---

### Message Queues

#### RabbitMQ

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `RABBITMQ_HOST` | string | localhost | RabbitMQ host |
| `RABBITMQ_PORT` | int | 5673 | AMQP port (changed from 5672 to avoid Service Bus conflict) |
| `RABBITMQ_MANAGEMENT_PORT` | int | 15672 | Management UI port |
| `RABBITMQ_USERNAME` | string | guest | Username |
| `RABBITMQ_PASSWORD` | string | guest | Password |
| `RABBITMQ_CONNECTION_STRING` | string | amqp://guest:guest@localhost:5673/ | Full connection URI |
| `RABBITMQ_DEFAULT_USER` | string | guest | Docker container default user |
| `RABBITMQ_DEFAULT_PASS` | string | guest | Docker container default password |

#### Azure Service Bus

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `SERVICEBUS_CONNECTION_STRING` | string | Endpoint=sb://localhost:5672/... | Service Bus connection |
| `SERVICEBUS_HOST` | string | localhost | Service Bus host |
| `SERVICEBUS_PORT` | int | 5672 | AMQP port |
| `SERVICEBUS_TEST_QUEUE` | string | integration-test-queue | Test queue name |
| `SERVICEBUS_TEST_TOPIC` | string | integration-test-topic | Test topic name |

---

### Cloud Services & Emulators

#### Azurite (Azure Storage Emulator)

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `AZURITE_URL` | string | http://localhost:10000 | Blob service URL |
| `AZURITE_BLOB_URL` | string | http://localhost:10000 | Blob endpoint |
| `AZURITE_QUEUE_URL` | string | http://localhost:10001 | Queue endpoint |
| `AZURITE_TABLE_URL` | string | http://localhost:10002 | Table endpoint |
| `AZURITE_CONNECTION_STRING` | string | DefaultEndpointsProtocol=http;... | Full connection string |
| `AZURITE_BLOB_SERVICE_URL` | string | http://127.0.0.1:10000/devstoreaccount1 | Direct blob endpoint |

**Note:** Connection string uses standard development account key (not secret).

#### LocalStack (AWS Emulator)

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `LOCALSTACK_URL` | string | http://localhost:4566 | Unified endpoint |
| `LOCALSTACK_HOST` | string | localhost | Host |
| `LOCALSTACK_PORT` | int | 4566 | Edge port |
| `LOCALSTACK_EDGE_PORT` | int | 4566 | Unified API port |
| `AWS_ACCESS_KEY_ID` | string | test | Dummy AWS credential |
| `AWS_SECRET_ACCESS_KEY` | string | test | Dummy AWS credential |
| `AWS_DEFAULT_REGION` | string | us-east-1 | Default region |
| `AWS_REGION` | string | us-east-1 | AWS region |
| `SQS_ENDPOINT` | string | http://localhost:4566 | SQS endpoint |
| `SQS_TEST_QUEUE` | string | integration-test-queue | Test queue name |

---

### Identity & Authentication

#### Keycloak

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `KEYCLOAK_URL` | string | http://localhost:8081 | Keycloak server URL |
| `KEYCLOAK_ADMIN` | string | admin | Admin username (Docker) |
| `KEYCLOAK_ADMIN_PASSWORD` | string | admin | Admin password |
| `KEYCLOAK_ADMIN_USERNAME` | string | admin | Admin user |
| `KEYCLOAK_REALM` | string | local-dev | Realm name |
| `KEYCLOAK_CLIENT_ID` | string | test-client | OAuth client ID |
| `KEYCLOAK_CLIENT_SECRET` | string | test-client-secret-12345 | OAuth client secret |
| `KEYCLOAK_TEST_USERNAME` | string | testuser | Test user |
| `KEYCLOAK_TEST_PASSWORD` | string | testpassword | Test password |

#### Identity Provider Selection

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `IDENTITY_PROVIDER` | enum | None | Identity provider (None, Keycloak) |
| `SWAGGER_ONLY` | bool | false | Skip hosting services, API-only mode |

---

### AI & ML Services

#### OpenSearch

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `OPENSEARCH_URL` | string | https://localhost:9200 | Cluster URL |
| `OPENSEARCH_HOST` | string | localhost | Host |
| `OPENSEARCH_PORT` | int | 9200 | Port |
| `OPENSEARCH_USERNAME` | string | admin | Username |
| `OPENSEARCH_PASSWORD` | string | IntegrationTest123! | Password |
| `OPENSEARCH_USE_HTTPS` | bool | true | Enable HTTPS |
| `OPENSEARCH_JAVA_OPTS` | string | -Xms512m -Xmx512m | JVM options |

#### Qdrant

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `QDRANT_URL` | string | http://localhost:6333 | HTTP API URL |
| `QDRANT_HOST` | string | localhost | Host |
| `QDRANT_PORT` | int | 6333 | HTTP port |
| `QDRANT_GRPC_URL` | string | http://localhost:6334 | gRPC API URL |

#### SBert

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `SBERT_URL` | string | http://localhost:5000 | HTTP API endpoint |
| `SBERT_HOST` | string | localhost | Host |
| `SBERT_PORT` | int | 5000 | Port |
| `SBERT_MODEL` | string | sentence-transformers/all-mpnet-base-v2 | HuggingFace model ID |

**Note:** Port discrepancy - `.runsettings` uses 5000, `.env.integration` uses 5080.

#### Ollama

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `OLLAMA_URL` | string | http://localhost:11434 | API URL |
| `OLLAMA_HOST` | string | localhost | Host |
| `OLLAMA_PORT` | int | 11434 | Port |
| `OLLAMA_MODEL` | string | phi3 | Default model name |

---

### Docker & Infrastructure

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `COMPOSE_PROJECT_NAME` | string | oobd-integration-tests | Docker Compose project name |
| `DOCKER_NETWORK` | string | oobd-integration-test-net | Docker network name |
| `DOCKER_COMPOSE_FILE` | string | docker-compose.integration-tests.yml | Compose file path |
| `DOCKER_HOST` | string | unix:///var/run/docker.sock | Docker daemon socket |
| `ACCEPT_EULA` | bool (Y/N) | Y | EULA acceptance (SQL Server, Keycloak) |

#### Health Check Configuration

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `TEST_TIMEOUT` | int (seconds) | 30 | Test execution timeout |
| `HEALTHCHECK_RETRIES` | int | 5 | Health check retry count |
| `HEALTHCHECK_INTERVAL` | duration | 10s | Time between health checks |
| `HEALTHCHECK_TIMEOUT` | duration | 5s | Health check timeout |

---

## Connection Strings

### Standard Connection Strings

Connection strings follow the .NET `ConnectionStrings` configuration section pattern.

**Access Method:**
```csharp
var connectionString = configuration.GetConnectionString("name");
```

**Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...",
    "MongoDB": "mongodb://...",
    "Redis": "localhost:6379"
  }
}
```

### Provider-Specific Formats

#### SQL Server
**Format:** `Server=host,port;Database=dbname;User Id=user;Password=pass;TrustServerCertificate=True;Encrypt=False`

**Example:**
```
Server=localhost,1433;User Id=sa;Password=IntegrationTest123!;TrustServerCertificate=True;Encrypt=False
```

#### MongoDB
**Format:** `mongodb://[username:password@]host:port[/database][?options]`

**Example:**
```
mongodb://localhost:27017/myapp
```

#### Redis (StackExchange.Redis)
**Format:** `host:port[,host:port],password=pass`

**Example:**
```
localhost:6379,password=mypassword
```

#### Azure Storage (Azurite)
**Format:** `DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=...;BlobEndpoint=...;QueueEndpoint=...;TableEndpoint=...`

**Development Account Key (Standard):**
```
Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==
```

#### Azure Service Bus
**Format:** `Endpoint=sb://[namespace:port]/;SharedAccessKeyName=...;SharedAccessKey=...`

**Emulator Example:**
```
Endpoint=sb://localhost:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=...
```

#### RabbitMQ (AMQP)
**Format:** `amqp://[username:password@]host:port[/vhost]`

**Example:**
```
amqp://guest:guest@localhost:5673/
```

---

## Test Configuration

Test configuration parameters are documented separately in [TEST_VARIABLES.md](./TEST_VARIABLES.md).

**Quick Reference:**
- **Total Test Parameters:** 30+
- **Docker-based Integration Tests:** 14 services
- **Live Cloud Services:** Azure B2C, App Insights, Groq

**Test Property Access Patterns:**

```csharp
// REQUIRED VALUES - URLs, credentials, connection strings
var url = TestContext.GetRequiredProperty<string>("MYSERVICE_URL");
var username = TestContext.GetRequiredProperty<string>("MYSERVICE_USERNAME");

// OPTIONAL VALUES WITH INDUSTRY DEFAULTS - port numbers
var port = TestContext.GetPropertyOrDefault("MONGODB_PORT", 27017);
```

**See Also:**
- [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Complete test property reference
- [Testing Guidelines](./docs/architecture/testing-guidelines.md) - Testing strategies
- [Docker Infrastructure](./containers/testing/README.md) - Integration test infrastructure

---

## Configuration Best Practices

### 1. Use Options Pattern

**Prefer:**
```csharp
public class MyService
{
    private readonly MyOptions _options;

    public MyService(IOptions<MyOptions> options)
    {
        _options = options.Value;
    }
}
```

**Avoid:**
```csharp
public class MyService
{
    private readonly IConfiguration _configuration;

    public MyService(IConfiguration configuration)
    {
        _configuration = configuration;
        var value = _configuration["My:Setting"]; // Direct access
    }
}
```

### 2. Use Strong Typing

**Prefer:**
```csharp
public class MyOptions
{
    public required string ApiKey { get; set; }
    public int Timeout { get; set; } = 30;
}
```

**Avoid:**
```csharp
var apiKey = configuration["ApiKey"]; // string, no type safety
var timeout = int.Parse(configuration["Timeout"] ?? "30"); // manual parsing
```

### 3. Provide Defaults

**Prefer:**
```csharp
public class MyOptions
{
    public int Port { get; set; } = 8080; // Sensible default
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

### 4. Document Required Settings

**Use XML documentation:**
```csharp
/// <summary>
/// Gets or sets the API key for authentication.
/// </summary>
/// <remarks>
/// This setting is required. Obtain from https://example.com/api-keys
/// </remarks>
public required string ApiKey { get; set; }
```

### 5. Use Environment-Specific Configuration

**Structure:**
```
appsettings.json          # Shared defaults
appsettings.Development.json  # Development overrides
appsettings.Production.json   # Production overrides
```

### 6. Never Commit Secrets

**Use:**
- User Secrets (development): `dotnet user-secrets set "ApiKey" "value"`
- Environment Variables (production)
- Azure Key Vault / AWS Secrets Manager
- Docker secrets

**Never:**
- Commit passwords, API keys, connection strings to source control
- Use hardcoded secrets in code

### 7. Validate Configuration

**Use Data Annotations:**
```csharp
public class MyOptions
{
    [Required]
    [Url]
    public required string ApiEndpoint { get; set; }

    [Range(1, 60)]
    public int TimeoutSeconds { get; set; } = 30;
}
```

**Or implement validation:**
```csharp
public class MyOptions : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrEmpty(ApiKey))
            yield return new ValidationResult("ApiKey is required");
    }
}
```

### 8. Use Hierarchical Configuration

**Organize by feature:**
```json
{
  "Database": {
    "ConnectionString": "...",
    "CommandTimeout": 30,
    "Retry": {
      "MaxAttempts": 3,
      "Delay": "00:00:05"
    }
  }
}
```

### 9. Support Multiple Environments

**Use environment variables for overrides:**
```bash
export Database__ConnectionString="Server=prod-server;..."
export Database__CommandTimeout="60"
```

**Note:** Double underscore (`__`) maps to configuration hierarchy.

---

## Validation Rules

### Common Validation Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Value must be provided | `[Required] public string ApiKey { get; set; }` |
| `[Range(min, max)]` | Numeric range | `[Range(1, 60)] public int Timeout { get; set; }` |
| `[StringLength(max)]` | String length | `[StringLength(100)] public string Name { get; set; }` |
| `[RegularExpression(pattern)]` | Pattern match | `[RegularExpression(@"^\d{3}-\d{3}-\d{4}$")]` |
| `[EmailAddress]` | Valid email format | `[EmailAddress] public string Email { get; set; }` |
| `[Url]` | Valid URL format | `[Url] public string Endpoint { get; set; }` |

### C# 11 Required Properties

**Compile-time validation:**
```csharp
public class MyOptions
{
    public required string ApiKey { get; set; } // Compiler error if not set
}
```

**Usage:**
```csharp
var options = new MyOptions
{
    ApiKey = "value" // Required, compiler enforces
};
```

### Custom Validation

**Implement IValidatableObject:**
```csharp
public class MyOptions : IValidatableObject
{
    public string? QueueUrl { get; set; }
    public string? QueueName { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrEmpty(QueueUrl) && string.IsNullOrEmpty(QueueName))
        {
            yield return new ValidationResult(
                "Either QueueUrl or QueueName must be specified",
                new[] { nameof(QueueUrl), nameof(QueueName) }
            );
        }
    }
}
```

---

## Migration Guide

### From Environment Variables to Options Pattern

**Before:**
```csharp
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
var endpoint = Environment.GetEnvironmentVariable("API_ENDPOINT");
```

**After:**
```csharp
// 1. Create Options class
public class ApiOptions
{
    public required string ApiKey { get; set; }
    public required string Endpoint { get; set; }
}

// 2. Configure in Startup/Program
services.Configure<ApiOptions>(configuration.GetSection("Api"));

// 3. Inject in service
public class MyService
{
    private readonly ApiOptions _options;

    public MyService(IOptions<ApiOptions> options)
    {
        _options = options.Value;
    }
}

// 4. Add to appsettings.json
{
  "Api": {
    "ApiKey": "...",
    "Endpoint": "https://api.example.com"
  }
}
```

### From Direct IConfiguration to Options

**Before:**
```csharp
public class MyService
{
    private readonly string _host;
    private readonly int _port;

    public MyService(IConfiguration config)
    {
        _host = config["Database:Host"];
        _port = int.Parse(config["Database:Port"] ?? "5432");
    }
}
```

**After:**
```csharp
// 1. Create Options class
public class DatabaseOptions
{
    public required string Host { get; set; }
    public int Port { get; set; } = 5432;
}

// 2. Configure
services.Configure<DatabaseOptions>(configuration.GetSection("Database"));

// 3. Inject
public class MyService
{
    private readonly DatabaseOptions _options;

    public MyService(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }
}
```

### Configuration Key Name Changes

| Old Pattern | New Pattern | Version | Notes |
|-------------|-------------|---------|-------|
| `ConnectionMultiplexerFactory:Source` | `Redis:ConnectionMultiplexer:Config` | 1.0 | Redis caching |
| Environment variable access | TestContext.GetRequiredProperty | 1.0 | Test configuration |

**Breaking Changes:** None currently documented.

---

## Related Documentation

- [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Test configuration reference
- [Architecture Documentation](./docs/architecture/README.md) - Framework architecture
- [Testing Guidelines](./docs/architecture/testing-guidelines.md) - Testing strategies
- [Docker Infrastructure](./containers/testing/README.md) - Integration test stack
- [Migration Guide](./docs/migration/README.md) - Version migration guides

---

**Last Generated:** 2026-01-21
**Generated By:** Configuration Documentation Protocol v1.0.0
**Total Configuration Points:** 157+ (31 Options classes + 24 direct keys + 102 environment variables)
