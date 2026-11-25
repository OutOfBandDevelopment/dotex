# OoBDev.Common.Extensions

Aggregator for the external service adapters (`src/ExternalServices`) with one registration that adds them all. It is part of [OoBDev.Common.Complete](../OoBDev.Common.Complete/README.Common.Complete.md).

## Use

```csharp
services.TryCommonExternalExtensions(configuration);                 // namespace OoBDev.Common.Extensions
services.TryCommonExternalExtensions(configuration,
    new IdentityExtensionBuilder { IdentityProvider = IdentityProviders.None },
    new ExternalExtensionBuilder { QdrantOptionSection = "Vectors:Qdrant" });
```

In Debug builds both builders are required parameters, in Release they default to null (the `#if DEBUG` pattern). Every registration uses `TryAdd*`, so an earlier registration wins.

## What it registers

In order: MongoDB, Azure storage (blob), RabbitMQ, MailKit (SMTP and IMAP), OpenTelemetry, Keycloak (only when `IdentityProvider` has the `Keycloak` flag, the default), SBert, all-MiniLM-L6-v2 embeddings, Qdrant, OpenSearch, Ollama, GroqCloud, Apache Tika, Markdig, Handlebars, MysticMind, SQL Server, Microsoft caching and Redis caching. Each adapter's readme lists its own configuration. WkHtmlToPdf and HtmlToOpenXml are opt-in: set `IncludeWkHtmlToPdf` and `IncludeHtmlToOpenXml` on `ExternalExtensionBuilder` (Apache Tika already converts to PDF).

## Builders

**Table 1 — `ExternalExtensionBuilder` configuration sections (defaults are the options type names)**

| Property | Default section |
|----------|-----------------|
| `MongoDatabaseConfigurationSection` | `MongoDatabaseOptions` |
| `AzureBlobProviderOptionSection` | `AzureBlobProviderOptions` |
| `SmtpConfigurationSection`, `ImapConfigurationSection` | `MailKitSmtpClientOptions`, `MailKitImapClientOptions` |
| `SentenceEmbeddingOptionSection` | `SentenceEmbeddingOptions` (shared by SBert and all-MiniLM-L6-v2) |
| `OpenTelemetryOptionSection` | `OpenTelemetryOptions` |
| `QdrantOptionSection`, `OpenSearchOptionSection` | `QdrantOptions`, `OpenSearchOptions` |
| `OllamaApiClientOptionSection`, `GroqCloudApiClientOptionSection` | `OllamaApiClientOptions`, `GroqCloudApiClientOptions` |
| `ApacheTikaClientOptionSection` | `ApacheTikaClientOptions` |

`IdentityExtensionBuilder` has `IdentityProvider` (flags enum `IdentityProviders`: `None`, `Keycloak`; default `Keycloak`) and `KeycloakIdentityConfigurationSection` (default `KeycloakIdentityOptions`).
