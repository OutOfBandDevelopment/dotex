# OoBDev.Azure.StorageAccount

## Summary

The OoBDev.Azure.StorageAccount library provides classes and methods for interacting with 
Azure Storage services, including Blob storage and message queueing. It offers functionalities 
such as storing and retrieving content in Blob storage, sending and receiving messages using 
Azure Storage Queues, and configuring Azure Storage services in .NET applications.

## Classes and Methods

### Azure.StorageAccount.AzureStorageGlobals

Contains global constants related to Azure Storage.

- **MessageProviderKey**: The key associated with the Azure Storage message provider.

### Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider

Represents a provider for storing and searching content in Azure Blob storage.

- **Properties**:
  - **ContainerName**: Container name for this instance

- **Methods**:
  - **Constructor**: Initializes a new instance of the `AzureBlobContainerProvider` class with 
    the specified client, collectionName, and loggerFactory.
  - **GetContentAsync**: Retrieves the content of the specified file from Azure Blob storage.
  - **TryStoreAsync**: Stores the specified content in Azure Blob storage.
  - **GetContentMetaDataAsync**: Retrieves content metadata asynchronously.
  - **StoreContentAsync**: Stores content asynchronously.
  - **StoreContentMetaDataAsync**: Stores content metadata asynchronously.
  - **QueryContent**: Queries content metadata.
  - **DeleteContentAsync**: Deletes content asynchronously.

### Azure.StorageAccount.BlobStorage.AzureBlobContainerProviderFactory

Represents a factory for creating instances of `AzureBlobContainerProvider`.

- **Methods**:
  - **Constructor**: Initializes a new instance of the `AzureBlobContainerProviderFactory` class 
    with the specified dependencies.
  - **Create**: Creates a new instance of `AzureBlobContainerProvider` based on the specified collection name.

### Azure.StorageAccount.BlobStorage.AzureBlobProviderOptions

Options for configuring Azure Blob storage provider.

- **Properties**:
  - **ConnectionString**: Gets or sets the connection string for Azure Blob storage.
  - **DocumentCollectionName**: Gets or sets the name of the collection for storing documents 
    in Azure Blob storage.
  - **SummaryCollectionName**: Gets or sets the name of the collection for storing document 
    summaries in Azure Blob storage.

### Azure.StorageAccount.BlobStorage.BlobServiceClientFactory

Represents a factory for creating instances of `BlobServiceClient`.

- **Methods**:
  - **Constructor**: Initializes a new instance of the `BlobServiceClientFactory` class with
    the specified configuration.
  - **Create**: Creates a new instance of `BlobServiceClient`.

### Azure.StorageAccount.BlobStorage.IBlobServiceClientFactory

Interface for a factory that creates instances of `BlobServiceClient`.

- **Methods**:
  - **Create**: Creates a new instance of `BlobServiceClient`.

### Azure.StorageAccount.MessageQueueing.AzureStorageQueueMessageProvider

Provides functionality for sending and receiving messages using Azure Storage Queues.

- **Methods**:
  - **Constructor**: Initializes a new instance of the `AzureStorageQueueMessageProvider` class.
  - **SendAsync**: Sends a message asynchronously to an Azure Storage Queue.
  - **SetHandlerProvider**: Sets the message handler provider for processing received messages.
  - **RunAsync**: Runs the message receiver asynchronously, continuously listening for incoming messages.

### Azure.StorageAccount.MessageQueueing.IQueueClientFactory

Factory for creating instances of `QueueClient` for Azure Storage Queues.

- **Methods**:
  - **Create**: Creates a new instance of `QueueClient` based on the provided configuration section.

### Azure.StorageAccount.MessageQueueing.QueueClientFactory

Factory for creating instances of `QueueClient` for Azure Storage Queues.

- **Methods**:
  - **Create**: Creates a new instance of `QueueClient` based on the provided configuration section.

### Azure.StorageAccount.ServiceCollectionExtensions

Provides extension methods for configuring Azure Storage services in the `IServiceCollection`.

- **Methods**:
  - **TryAddAzureStorageServices**: Tries to add Azure Storage services including blob and queue 
    services to the specified `IServiceCollection`.
  - **TryAddAzureStorageBlobServices**: Configures services for Azure Storage Blob.
  - **TryAddAzureStorageQueueServices**: Tries to add Azure Storage queue services to the specified `IServiceCollection`.

