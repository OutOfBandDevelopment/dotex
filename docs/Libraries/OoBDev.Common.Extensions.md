# OoBDev.Common.Extensions


## Class: Common.Extensions.ExternalExtensionBuilder
Represents a builder for configuring external extensions. 

### Properties

#### MongoDatabaseConfigurationSection
Gets or sets the configuration section name for MongoDB database options. The configuration section name for MongoDB database options. Default is "MongoDatabaseOptions".
#### AzureBlobProviderOptionSection
Gets or sets the configuration section name for Azure Blob Container options. The configuration section name for Azure Blob Container options. Default is "AzureBlobContainerOptions".
#### SmtpConfigurationSection
Gets or sets the configuration section name for SMTP client options (MailKit). The configuration section name for SMTP client options (MailKit). Default is "MailKitSmtpClientOptions".
#### ImapConfigurationSection
Gets or sets the configuration section name for IMAP client options (MailKit). The configuration section name for IMAP client options (MailKit). Default is "MailKitImapClientOptions".
#### SentenceEmbeddingOptionSection
Gets or sets the configuration section name for SentenceEmbeddingOptions.
#### QdrantOptionSection
Gets or sets the configuration section name for Qdrant options.
#### OpenSearchOptionSection
Gets or sets the configuration section name for OpenSearch options.
#### OllamaApiClientOptionSection
Gets or sets the configuration section name for Ollama Api Client options.
#### GroqCloudApiClientOptionSection
Gets or sets the configuration section name for GroqCloud Api Client options.
#### ApacheTikaClientOptionSection
gets or sets the configuration section name of Apache Tika Client options.

## Class: Common.Extensions.IdentityExtensionBuilder
Represents a builder for configuring identity extensions. 

### Properties

#### IdentityProvider
Gets or sets the identity provider to use. Specifies the identity provider for authentication. The default value is .
#### MicrosoftIdentityConfigurationSection
Gets or sets the configuration section name for Microsoft Identity options. The configuration section name for Microsoft Identity options. Default is "MicrosoftIdentityOptions".
#### KeycloakIdentityConfigurationSection
Gets or sets the configuration section name for Keycloak identity options. The configuration section name for Keycloak identity options. Default is "KeycloakIdentityOptions".

## Class: Common.Extensions.IdentityProviders
Specifies different identity providers for authentication. 

### Fields

#### None
Represents no specific identity provider.
#### AzureB2C
Represents the Azure B2C identity provider.
#### Keycloak
Represents the Keycloak identity provider.

## Class: Common.Extensions.ServiceCollectionExtensions
Provides extension methods for configuring common external services in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryCommonExternalExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,OoBDev.Common.Extensions.IdentityExtensionBuilder,OoBDev.Common.Extensions.ExternalExtensionBuilder)
Tries to add common external services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The instance.
* *configuration:* The configuration containing settings for external services.
* *identityBuilder:* Optional builder for configuring identity extensions. Default is null.
* *externalBuilder:* Optional builder for configuring external extensions. Default is null.




##### Return value
The updated instance.

