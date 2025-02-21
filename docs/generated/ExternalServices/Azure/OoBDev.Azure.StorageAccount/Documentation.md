**AzureStorageGlobals.cs Documentation**

Class Diagram:
```
@startuml
class AzureStorageGlobals {
  - MessageProviderKey: string
}
@enduml
```
Summary:
This is a static class that contains global constants related to Azure Storage.

Properties:
- **MessageProviderKey**: The key associated with the Azure Storage message provider.

**OoBDev.Azure.StorageAccount.csproj Documentation**

Project File:
This is a .NET Core 8.0 project file that defines the dependencies and configuration for the OoBDev.Azure.StorageAccount library.

Properties:
- **TargetFramework**: net8.0
- **Nullable**: enable
- **GenerateDocumentationFile**: True
- **PackageReadmeFile**: Readme.Azure.StorageAccount.md

Dependencies:
- **Azure.Storage.Blobs**: 12.20.0
- **Azure.Storage.Queues**: 12.18.0
- **Microsoft.Extensions.Configuration.Abstractions**: 8.0.0
- **Microsoft.Extensions.DependencyInjection.Abstractions**: 8.0.1
- **Microsoft.Extensions.Logging.Abstractions**: 8.0.1
- **Microsoft.Extensions.Options.ConfigurationExtensions**: 8.0.0

Project References:
- **OoBDev.Documents.Abstractions**: ..\..\..\Framework\OoBDev.Documents.Abstractions\OoBDev.Documents.Abstractions.csproj
- **OoBDev.Extensions**: ..\..\..\Framework\OoBDev.Extensions\OoBDev.Extensions.csproj
- **OoBDev.MessageQueueing.Abstractions**: ..\..\..\Framework\OoBDev.MessageQueueing.Abstractions\OoBDev.MessageQueueing.Abstractions.csproj
- **OoBDev.Search.Abstractions**: ..\..\..\Framework\OoBDev.Search.Abstractions\OoBDev.Search.Abtrasound.csproj
- **OoBDev.System.Abstractions**: ..\..\..\Framework\OoBDev.System.Abstractions\OoBDev.System.Abstractions.csproj

**Readme.Azure.StorageAccount.md Documentation**

Overview:
The OoBDev.Azure.StorageAccount library provides classes and methods for interacting with Azure Storage services, including Blob storage and message queueing.

Modules:
- **Azure.StorageAccount.AzureStorageGlobals**: Contains global constants related to Azure Storage.
- **Azure.StorageAccount.BlobStorage**: Provides classes and methods for interacting with Azure Blob storage.
- **Azure.StorageAccount.MessageQueueing**: Provides classes and methods for interacting with Azure Storage Queues.

**ServiceCollectionExtensions.cs Documentation**

Class Diagram:
```
@startuml
class ServiceCollectionExtensions {
  - TryAddAzureStorageServices(context: IServiceCollection, config: IConfiguration, azureBlobProviderOptionSection: string)
  - TryAddAzureStorageBlobServices(context: IServiceCollection, config: IConfiguration, azureBlobProviderOptionSection: string)
  - TryAddAzureStorageQueueServices(context: IServiceCollection)
}
@enduml
```
Summary:
This class provides extension methods for configuring Azure Storage services in the IServiceCollection.

Methods:
- **TryAddAzureStorageServices**: Tries to add Azure Storage services including blob and queue services to the specified IServiceCollection.
- **TryAddAzureStorageBlobServices**: Configures services for Azure Storage Blob.
- **TryAddAzureStorageQueueServices**: Tries to add Azure Storage queue services to the specified IServiceCollection.

**Class Diagrams**

```
@startuml
class AzureStorageGlobals {
  - MessageProviderKey: string
}

class AzureBlobContainerProvider {
  - ContainerName: string
  - GetContentAsync(file: File): FileContent
  - TryStoreAsync(content: FileContent): bool
  - GetContentMetaDataAsync(file: File): FileMetadata
  - StoreContentMetaD