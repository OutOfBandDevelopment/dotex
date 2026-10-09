# OoBDev.OpenSearch

OoBDev.OpenSearch provides classes and utilities for working with OpenSearch. Here's an overview of its key components:

## IOpenSearchClientFactory

### Methods
- **Create**: Creates a new instance of the `OpenSearch.Net.IOpenSearchLowLevelClient`.

## LexicalProvider

Provides functionality for storing and searching lexical content using OpenSearch.

## OpenSearchClientFactory

### Methods
- **Constructor**: Initializes a new instance of the `OoBDev.OpenSearch.OpenSearchClientFactory` class.
- **Create**: Creates a new instance of the `OpenSearch.Net.IOpenSearchLowLevelClient`.

## OpenSearchOptions

Configuration options for the OpenSearch client.
- **HostName**: Gets or sets the hostname of the OpenSearch server.
- **Port**: Gets or sets the port number of the OpenSearch server.
- **IndexName**: Gets or sets the index name used for OpenSearch operations.
- **UserName**: Gets or sets the username for authentication (if required).
- **Password**: Gets or sets the password for authentication (if required).

## ServiceCollectionExtensions

Provides extension methods for configuring services related to OpenSearch.

### Methods
- **TryAddOpenSearchServices**: Configures services for OpenSearch.

