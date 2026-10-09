# OoBDev.Common.Extensions

This library provides a set of extension methods and classes for configuring common external services and identity providers in .NET applications.

## Classes

### `Common.Extensions.ExternalExtensionBuilder`

Represents a builder for configuring external extensions.

### `Common.Extensions.IdentityExtensionBuilder`

Represents a builder for configuring identity extensions.

### `Common.Extensions.IdentityProviders`

Specifies different identity providers for authentication.

### `Common.Extensions.ServiceCollectionExtensions`

Provides extension methods for configuring common external services.

## Methods

### `TryCommonExternalExtensions`

Tries to add common external services to the specified `IServiceCollection`.

* Parameters:
	+ `services`: The instance of `IServiceCollection`.
	+ `configuration`: The configuration containing settings for external services.
	+ `identityBuilder`: Optional builder for configuring identity extensions. Default is null.
	+ `externalBuilder`: Optional builder for configuring external extensions. Default is null.
* Return value: The updated instance of `IServiceCollection`.

**Properties**
-------------

### `MongoDatabaseConfigurationSection`

Gets or sets the configuration section name for MongoDB database options. Default is "MongoDatabaseOptions".

### `AzureBlobProviderOptionSection`

Gets or sets the configuration section name for Azure Blob Container options. Default is "AzureBlobContainerOptions".

### `SmtpConfigurationSection`

Gets or sets the configuration section name for SMTP client options (MailKit). Default is "MailKitSmtpClientOptions".

### `ImapConfigurationSection`

Gets or sets the configuration section name for IMAP client options (MailKit). Default is "MailKitImapClientOptions".

### `SentenceEmbeddingOptionSection`

Gets or sets the configuration section name for SentenceEmbeddingOptions.

### `QdrantOptionSection`

Gets or sets the configuration section name for Qdrant options.

### `OpenSearchOptionSection`

Gets or sets the configuration section name for OpenSearch options.

### `OllamaApiClientOptionSection`

Gets or sets the configuration section name for Ollama Api Client options.

### `GroqCloudApiClientOptionSection`

Gets or sets the configuration section name for GroqCloud Api Client options.

### `ApacheTikaClientOptionSection`

Gets or sets the configuration section name of Apache Tika Client options.

### `IdentityProvider`

Gets or sets the identity provider to use. Specifies the identity provider for authentication. The default value is `None`.

### `MicrosoftIdentityConfigurationSection`

Gets or sets the configuration section name for Microsoft Identity options. Default is "MicrosoftIdentityOptions".

### `KeycloakIdentityConfigurationSection`

Gets or sets the configuration section name for Keycloak identity options. Default is "KeycloakIdentityOptions".

**Identity Providers**
--------------------

### `None`

Represents no specific identity provider.

### `AzureB2C`

Represents the Azure B2C identity provider.

### `Keycloak`

Represents the Keycloak identity provider.

**Getting Started**
-------------------

To use this library, add the following NuGet package to your project:
```
Install-Package OoBDev.Common.Extensions
```
Then, in your startup class, use the `TryCommonExternalExtensions` method to configure the external services and identity providers:
```
public void ConfigureServices(IServiceCollection services)
{
    services.TryCommonExternalExtensions(Configuration, new IdentityExtensionBuilder(), new ExternalExtensionBuilder());
}
```
This library is designed to be flexible and extensible, so feel free to modify or extend it to suit your specific needs.