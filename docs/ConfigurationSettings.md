# OoBDev - Configuration Settings

## Summary

This document contains a full list of configuration settings and how they are used

## Settings

| Setting                                           | CLI Mapping               | Default   | Function                                                                          |
|---------------------------------------------------|---------------------------|-----------|-----------------------------------------------------------------------------------|
| ApacheTikaClientOptions:Url                       |                           |           | URL of the Apache Tika server                                                     |
| OAuth2SwaggerOptions:UserReadApiClaim             |                           |           | claim that Swagger will use to determine the authenticated user's API access      |
| OAuth2SwaggerOptions:AuthorizationUrl             |                           |           | URL for the authorization endpoint                                                |
| OAuth2SwaggerOptions:TokenUrl                     |                           |           | URL for the token endpoint.                                                       |
| AzureBlobProviderOptions:ConnectionString         |                           |           | connection string for Azure Blob storage                                          |
| AzureBlobProviderOptions:EnsureContainerExists    | false                     |           | if true the system will create a container when not found                         |
| MailKitSmtpClientOptions:Host                     |                           |           | host address of the SMTP server                                                   |
| MailKitSmtpClientOptions:Port                     | 25                        |           | port number of the SMTP server                                                    |
| MailKitSmtpClientOptions:SecureSocketOption       | None                      |           | secure socket options for the SMTP connection                                     |
| MailKitSmtpClientOptions:Uri                      |                           |           | URI of the SMTP server.                                                           |
| MailKitSmtpClientOptions:Password                 |                           |           | password used for authentication with the SMTP server                             |
| MailKitSmtpClientOptions:UserName                 |                           |           | username used for authentication with the SMTP server                             |
| MailKitSmtpClientOptions:DefaultFromEmailAddress  |                           |           | default email address to use as the sender in outgoing emails                     |
| MailKitImapClientOptions:Host                     |                           |           | host address of the IMAP server                                                   |
| MailKitImapClientOptions:Port                     | 143                       |           | port number of the IMAP server                                                    |
| MailKitImapClientOptions:SecureSocketOption       | None                      |           | secure socket options for the IMAP connection                                     |
| MailKitImapClientOptions:Uri                      |                           |           | URI of the IMAP server.                                                           |
| MailKitImapClientOptions:Password                 |                           |           | password used for authentication with the IMAP server                             |
| MailKitImapClientOptions:UserName                 |                           |           | username used for authentication with the IMAP server                             |
| MicrosoftIdentityOptions:ClientID                 |                           |           | key for the Azure AD B2C client ID configuration                                  |
| MicrosoftIdentityOptions:Issuer                   |                           |           | key for the Azure AD B2C issuer configuration                                     |
| MicrosoftIdentityOptions:ClientSecret             |                           |           | key for the Azure AD B2C client secret configuration                              |
| MicrosoftIdentityOptions:Tenant                   |                           |           | key for the Azure AD B2C tenant configuration                                     |
| MongoDatabaseOptions:ConnectionString             |                           |           | connection string for the MongoDB database                                        |
| MongoDatabaseOptions:DatabaseName                 |                           |           | the name of the MongoDB database                                                  |
| OllamaApiClientOptions:Url                        |                           |           | the URL of the Ollama API                                                         |
| OllamaApiClientOptions:DefaultModel               |                           |           | default model to use with the Ollama API                                          |
| OpenAIClientOptions:APIKey                        |                           |           | APIKey to be used                                                                 |
| OpenAIClientOptions:DeploymentName                |                           |           | deployment model to be used for the text generation                               |
| OpenAIClientOptions:EmbeddingModel                |                           |           | deployment model to be used for the embedding generation                          |
| OpenSearchOptions:HostName                        |                           |           | hostname of the OpenSearch server                                                 |
| OpenSearchOptions:Port                            | 9200                      |           | port number of the OpenSearch server                                              |
| OpenSearchOptions:IndexName                       |                           |           | index name used for OpenSearch operations                                         |
| OpenSearchOptions:UserName                        |                           |           | username for authentication (if required)                                         |
| OpenSearchOptions:Password                        |                           |           | password for authentication (if required)                                         |
| QdrantOptions:Url                                 |                           |           | URL for Qdrant                                                                    |
| QdrantOptions:CollectionName                      |                           |           | collection name for Qdrant                                                        |
| QdrantOptions:EnsureCollectionExists              | false                     |           | system will create the collection if not exists                                   |
| SentenceEmbeddingOptions:Url                      |                           |           | URL for SBert                                                                     |
| FileTemplateSource:TemplatePath                   | --file-template-path      | "./"      | Path to file templates                                                            |
| FileTemplateSource:SandboxPath                    | --file-template-sandbox   | null      | Base sandbox path for templates                                                   |
| FileTemplateSource:Priority                       | --file-template-priority  | 100       | weight priority for template source                                               |
| TemplateEngineOptions:InputFile                   | --input                   |           | path to the input file                                                            |
| TemplateEngineOptions:Template                    |                           |           | template content                                                                  |
| TemplateEngineOptions:InputFileType               | --input-type              |           | the type of the input file                                                        |
| TemplateEngineOptions:OutputFile                  | --output                  |           | path to the output file                                                           |

## External Settings

* [JwtBearerOptions](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.jwtbeareroptions)