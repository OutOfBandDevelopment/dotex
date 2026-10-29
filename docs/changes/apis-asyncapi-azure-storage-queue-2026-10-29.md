# AsyncAPI contributor for Azure Storage queues (2026-10-29)

- `AzureStorageQueueAsyncApiContributor` (provider key `azure-storage-queue`) in `OoBDev.Microsoft.Azure.StorageAccount`, registered by `TryAddAzureStorageQueueServices()`.
- Host comes from `QueueEndpoint`, else `AccountName` plus `EndpointSuffix`, else `queue.core.windows.net`; the account key is never read. Channel address is `QueueName`.
- Tests: 3 Unit tests (account string without key, Azurite endpoint, no connection string).
