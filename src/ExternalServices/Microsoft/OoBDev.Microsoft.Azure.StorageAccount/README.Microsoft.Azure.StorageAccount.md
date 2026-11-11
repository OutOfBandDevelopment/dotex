# OoBDev.Microsoft.Azure.StorageAccount

Azure Storage adapters: a blob container provider (`IBlobContainerProvider`) and a storage queue message provider (`IMessageSenderProvider` and `IMessageReceiverProvider`).

## Contents

- [Registration](#registration)
- [Blob storage](#blob-storage)
- [Storage queues](#storage-queues)
- [Verification](#verification)

## Registration

```csharp
services.TryAddAzureStorageServices(configuration);          // blobs and queues
services.TryAddAzureStorageBlobServices(configuration);      // blobs only
services.TryAddAzureStorageQueueServices();                  // queues only
```

The registration class is `OoBDev.Microsoft.Azure.StorageAccount.ServiceCollectionExtensions`. The blob options bind to the `AzureBlobProviderOptions` section unless another name is passed (it is a required argument in DEBUG builds).

## Blob storage

**Table 1 — Blob types**

| Type | Role |
|------|------|
| `AzureBlobProviderOptions` | `ConnectionString` (required) and `EnsureContainerExists` (create the container on first use when true). |
| `IBlobServiceClientFactory`, `AzureBlobServiceClientFactory` | Create the `BlobServiceClient`; the `BlobServiceClient` is also registered as a transient service. |
| `AzureBlobContainerProviderFactory` | `IBlobContainerProviderFactory.Create(containerName)`; returns `null` when no connection string is set. |
| `AzureBlobContainerProvider` | `IBlobContainerProvider` over one container (the name is lower-cased): `GetContentAsync`, `GetContentMetaDataAsync`, `StoreContentAsync`, `StoreContentMetaDataAsync`, `QueryContent`, `DeleteContentAsync`. |

## Storage queues

`AzureStorageQueueMessageProvider` sends and receives messages. It is registered under the key `azure-storage-queue` (`AzureStorageGlobals.MessageProviderKey`) and as the default sender and receiver. The queue section holds `ConnectionString` and `QueueName`; a missing value throws `ConfigurationMissingException` naming the key. Messages are Base64 encoded. `IQueueClientFactory` creates the `QueueClient`, and `IAzureStorageQueueMapper` wraps a message and its context for the queue and reads the `EnsureQueueExists` and wait-delay settings.

`AzureStorageQueueAsyncApiContributor` describes the queue in the AsyncAPI document from the endpoint host in the connection string; keys are never read.

## Verification

Queue tests (`OoBDev.Microsoft.Azure.StorageAccount.Tests`): Unit for the mapper and the AsyncAPI contributor, Integration against Azurite in the integration stack (see `docs/architecture/testing/integration/`). Blob provider tests (`BlobStorage/AzureBlobContainerProviderIntegrationTests`, Integration) cover store, read, metadata, list, delete and the no-overwrite default.
