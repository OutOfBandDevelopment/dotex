# OoBDev.Azure.StorageAccount


## Class: Azure.StorageAccount.AzureStorageGlobals
Contains global constants related to Azure Storage. 

### Fields

#### MessageProviderKey
The key associated with the Azure Storage message provider.

## Class: Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider
Represents a provider for storing and searching content in Azure Blob storage. 

### Properties

#### ContainerName
Container name for this instance
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*class with the specified 
*client*and 
*collectionName*. 


##### Parameters
* *client:* The BlobServiceClient used to connect to the Azure Blob storage.
* *collectionName:* The name of the collection in the Azure Blob storage.
* *options:* The configuration options to the azure container service.
* *loggerFactory:* ILoggerFactory instance.




#### GetContentAsync(System.String)
Retrieves the content of the specified file from Azure Blob storage. 


##### Parameters
* *file:* The name of the file to retrieve.




##### Return value
A ContentReference object representing the retrieved content.



#### GetContentMetaDataAsync(System.String)
Retrieves content metadata asynchronously. 


##### Parameters
* *path:* The path to the content.




##### Return value
A task representing the asynchronous operation. Returns the content metadata if it exists, otherwise returns null.



#### StoreContentAsync(OoBDev.Documents.Models.ContentReference,System.Collections.Generic.Dictionary{System.String,System.String},System.Boolean)
Stores content asynchronously. 


##### Parameters
* *reference:* The reference to the content.
* *metadata:* The metadata associated with the content.
* *overwrite:* Determines whether to overwrite existing content with the same name.




##### Return value
A task representing the asynchronous operation.



#### StoreContentMetaDataAsync(OoBDev.Documents.Models.ContentMetaDataReference)
Stores content metadata asynchronously. 


##### Parameters
* *reference:* The reference to the content metadata.




##### Return value
A task representing the asynchronous operation. Returns true if the metadata is stored successfully, otherwise false.



#### QueryContent
Queries content metadata. 


##### Return value
An IQueryable representing the content metadata.



#### DeleteContentAsync(System.String)
Deletes content asynchronously. 


##### Parameters
* *path:* The path to the content to be deleted.




##### Return value
A task representing the asynchronous operation.



#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*class with the specified dependencies. 


##### Parameters
* *serviceProvider:* The service provider.
* *options:* The service configurations.




#### 
Creates a new instance of 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*based on the specified collection name. 


##### Parameters
* *containerName:* The name of the collection.




##### Return value
A new instance of .



## Class: Azure.StorageAccount.BlobStorage.AzureBlobContainerProviderFactory
Represents a factory for creating instances of 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*class with the specified dependencies. 


##### Parameters
* *serviceProvider:* The service provider.
* *options:* The service configurations.




#### Create(System.String)
Creates a new instance of 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*based on the specified collection name. 


##### Parameters
* *containerName:* The name of the collection.




##### Return value
A new instance of .



## Class: Azure.StorageAccount.BlobStorage.AzureBlobProviderOptions
Options for configuring Azure Blob storage provider. 

### Properties

#### ConnectionString
Gets or sets the connection string for Azure Blob storage.
#### EnsureContainerExists
if true the system will create a container if not exists

## Class: Azure.StorageAccount.BlobStorage.AzureBlobServiceClientFactory
Represents a factory for creating instances of 
 *See: T:Azure.Storage.Blobs.BlobServiceClient*. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobServiceClientFactory*class with the specified configuration. 


##### Parameters
* *config:* The Azure Blob provider options.




#### Create
Creates a new instance of 
 *See: T:Azure.Storage.Blobs.BlobServiceClient*. 


##### Return value
A new instance of .



## Class: Azure.StorageAccount.BlobStorage.IBlobServiceClientFactory
Interface for a factory that creates instances of 
 *See: T:OoBDev.Azure.StorageAccount.BlobStorage.AzureBlobContainerProvider*. 

### Methods


#### Create
Creates a new instance of 
 *See: T:Azure.Storage.Blobs.BlobServiceClient*. 


##### Return value
The created .



## Class: Azure.StorageAccount.MessageQueueing.AzureStorageQueueMapper
Provides functionality to map objects to and from Azure Storage Queue messages. 

### Methods


#### Wrap(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Wraps the provided message and message context into a 
 *See: T:OoBDev.MessageQueueing.Services.WrappedQueueMessage*. 


##### Parameters
* *message:* The message object to wrap.
* *context:* The message context.




##### Return value
A wrapped queue message.



#### EnsureQueueExists(Microsoft.Extensions.Configuration.IConfiguration)
Ensures that the Azure Storage Queue exists based on the provided configuration. 


##### Parameters
* *configuration:* The configuration containing information about whether to ensure the queue exists.




##### Return value
True if the queue exists or should be ensured to exist; otherwise, false.



#### WaitDelay(Microsoft.Extensions.Configuration.IConfiguration)
Retrieves the wait delay value from the provided configuration. 


##### Parameters
* *configuration:* The configuration containing the wait delay value.




##### Return value
The wait delay value, in milliseconds.



## Class: Azure.StorageAccount.MessageQueueing.AzureStorageQueueMessageProvider
Provides functionality for sending and receiving messages using Azure Storage Queues. 
Initializes a new instance of the 
 *See: T:OoBDev.Azure.StorageAccount.MessageQueueing.AzureStorageQueueMessageProvider*class. 

### Methods


#### Constructor
Provides functionality for sending and receiving messages using Azure Storage Queues. 


##### Parameters
* *serializer:* The JSON serializer for message serialization and deserialization.
* *clientFactory:* The factory for creating Azure Storage Queue clients.
* *mapper:* The model mapper for Azure Storage Queues.
* *logger:* The logger for logging messages.




#### SendAsync(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Sends a message asynchronously to an Azure Storage Queue. 


##### Parameters
* *message:* The message to be sent.
* *context:* The message context containing additional information.




##### Return value
The message ID if the send operation is successful; otherwise, null.



#### SetHandlerProvider(OoBDev.MessageQueueing.Services.IMessageHandlerProvider)
Sets the message handler provider for processing received messages. 


##### Parameters
* *handlerProvider:* The message handler provider.




##### Return value
The current instance of .



#### RunAsync(System.Threading.CancellationToken)
Runs the message receiver asynchronously, continuously listening for incoming messages. 


##### Parameters
* *cancellationToken:* The cancellation token to stop the receiver.




##### Return value
A task representing the asynchronous operation.



## Class: Azure.StorageAccount.MessageQueueing.IAzureStorageQueueMapper
Provides functionality to map objects to and from Azure Storage Queue messages. 

### Methods


#### EnsureQueueExists(Microsoft.Extensions.Configuration.IConfiguration)
Ensures that the Azure Storage Queue exists based on the provided configuration. 


##### Parameters
* *configuration:* The configuration containing information about whether to ensure the queue exists.




##### Return value
True if the queue exists or should be ensured to exist; otherwise, false.



#### Wrap(System.Object,OoBDev.MessageQueueing.Services.IMessageContext)
Wraps the provided message and message context into a 
 *See: T:OoBDev.MessageQueueing.Services.WrappedQueueMessage*. 


##### Parameters
* *message:* The message object to wrap.
* *context:* The message context.




##### Return value
A wrapped queue message.



#### WaitDelay(Microsoft.Extensions.Configuration.IConfiguration)
Retrieves the wait delay value from the provided configuration. 


##### Parameters
* *configuration:* The configuration containing the wait delay value.




##### Return value
The wait delay value, in milliseconds.



## Class: Azure.StorageAccount.MessageQueueing.IQueueClientFactory
Factory for creating instances of 
 *See: T:Azure.Storage.Queues.QueueClient*for Azure Storage Queues. 

### Methods


#### Create(Microsoft.Extensions.Configuration.IConfigurationSection)
Creates a new instance of 
 *See: T:Azure.Storage.Queues.QueueClient*based on the provided configuration section. 


##### Parameters
* *config:* The configuration section containing connection string and queue name.




##### Return value
A new instance of for the specified Azure Storage Queue.



##### Exceptions

* *System.ApplicationException:* Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.




## Class: Azure.StorageAccount.MessageQueueing.QueueClientFactory
Factory for creating instances of 
 *See: T:Azure.Storage.Queues.QueueClient*for Azure Storage Queues. 

### Methods


#### Create(Microsoft.Extensions.Configuration.IConfigurationSection)
Creates a new instance of 
 *See: T:Azure.Storage.Queues.QueueClient*based on the provided configuration section. 


##### Parameters
* *config:* The configuration section containing connection string and queue name.




##### Return value
A new instance of for the specified Azure Storage Queue.



##### Exceptions

* *System.ApplicationException:* Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.




## Class: Azure.StorageAccount.ServiceCollectionExtensions
Provides extension methods for configuring Azure Storage services in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryAddAzureStorageServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Tries to add Azure Storage services including blob and queue services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add services to.
* *configuration:* The to add services to.
* *azureBlobProviderOptionSection:* The name for the ConfigurationSectionName.




##### Return value
The modified .



#### TryAddAzureStorageBlobServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for Azure Storage Blob. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The to bind Azure Blob Storage options from.
* *azureBlobProviderOptionSection:* The configuration section name containing Azure Blob Storage options.




##### Return value
The modified .



#### TryAddAzureStorageQueueServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Tries to add Azure Storage queue services to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The to add services to.




##### Return value
The modified .

