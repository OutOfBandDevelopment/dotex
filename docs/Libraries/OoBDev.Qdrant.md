# OoBDev.Qdrant


## Class: Qdrant.IPointStructFactory
Interface for a factory that creates instances of 
 *See: T:Qdrant.Client.Grpc.PointStruct*. 

### Methods


#### CreateFileChunk(OoBDev.System.IO.FileMetaData,OoBDev.System.IO.ContentChunk,System.IO.FileInfo,System.ReadOnlyMemory{System.Single})
Creates a new instance of 
 *See: T:Qdrant.Client.Grpc.PointStruct*for a file chunk. 


##### Parameters
* *metadata:* Metadata associated with the file.
* *chunk:* Content chunk of the file.
* *fileInfo:* Information about the file.
* *vector:* Vector representation of the content chunk.




##### Return value
A new instance of representing a file chunk.



#### CreateQuestion(System.Guid,System.String,System.ReadOnlyMemory{System.Single},System.String)
Creates a new instance of 
 *See: T:Qdrant.Client.Grpc.PointStruct*for a question. 


##### Parameters
* *uuid:* Unique identifier of the question.
* *question:* The question text.
* *vector:* Vector representation of the question.
* *type:* Optional type of the question.




##### Return value
A new instance of representing a question.



#### CreateServiceReference(System.Guid,System.Type,System.String,System.ReadOnlyMemory{System.Single})
Creates a new instance of 
 *See: T:Qdrant.Client.Grpc.PointStruct*for a service reference. 


##### Parameters
* *uuid:* Unique identifier of the service reference.
* *serviceType:* Type of the service.
* *description:* Description of the service.
* *vector:* Vector representation of the service reference.




##### Return value
A new instance of representing a service reference.



## Class: Qdrant.IQdrantGrpcClientFactory
Interface for a factory that creates instances of 
 *See: T:Qdrant.Client.Grpc.QdrantGrpcClient*. 

### Methods


#### Create
Creates a new instance of 
 *See: T:Qdrant.Client.Grpc.QdrantGrpcClient*. 


##### Return value
A new instance of .



## Class: Qdrant.PointStructFactory
Factory for creating instances of 
 *See: T:Qdrant.Client.Grpc.PointStruct*. 

### Fields

#### ServiceInstanceType
Represents the constant value indicating service instance type.
### Methods


#### CreateFileChunk(OoBDev.System.IO.FileMetaData,OoBDev.System.IO.ContentChunk,System.IO.FileInfo,System.ReadOnlyMemory{System.Single})
Creates a 
 *See: T:Qdrant.Client.Grpc.PointStruct*instance representing a file chunk. 


##### Parameters
* *metaData:* The metadata of the file.
* *chunk:* The content chunk.
* *fileInfo:* Information about the file.
* *vector:* The vector representing the file chunk.




##### Return value
The created .



#### CreateServiceReference(System.Guid,System.Type,System.String,System.ReadOnlyMemory{System.Single})
Creates a 
 *See: T:Qdrant.Client.Grpc.PointStruct*instance representing a service reference. 


##### Parameters
* *uuid:* The UUID of the service reference.
* *serviceType:* The type of the service.
* *description:* The description of the service reference.
* *vector:* The vector representing the service reference.




##### Return value
The created .



#### CreateQuestion(System.Guid,System.String,System.ReadOnlyMemory{System.Single},System.String)
Creates a 
 *See: T:Qdrant.Client.Grpc.PointStruct*instance representing a question. 


##### Parameters
* *uuid:* The UUID of the question.
* *question:* The question text.
* *vector:* The vector representing the question.
* *type:* The type of the question.




##### Return value
The created .



## Class: Qdrant.QdrantGrpcClientExtensions
Extension methods for Qdrant gRPC client. 

### Methods


#### EnsureCollectionExistsAsync(Qdrant.Client.Grpc.QdrantGrpcClient,System.String,System.UInt64,Qdrant.Client.Grpc.Distance,Microsoft.Extensions.Logging.ILogger)
Ensures that the specified collection exists in Qdrant. If it doesn't exist, creates the collection with the specified parameters. 


##### Parameters
* *qdrant:* The Qdrant gRPC client.
* *collectionName:* The name of the collection to ensure exists.
* *vectorSize:* The size of the vectors in the collection.
* *distanceCalculation:* The distance calculation method for the collection.
* *logger:* The logger to use for logging messages.




##### Return value
The Qdrant gRPC client.



## Class: Qdrant.QdrantGrpcClientFactory
Factory for creating instances of the Qdrant gRPC client. 

### Methods


#### Constructor
Creates a new instance of the Qdrant gRPC client. 


##### Return value
A new instance of the Qdrant gRPC client.



#### Create
Creates a new instance of the Qdrant gRPC client. 


##### Return value
A new instance of the Qdrant gRPC client.



## Class: Qdrant.QdrantOptions
Options for configuring Qdrant. 

### Properties

#### Url
Gets or sets the URL for Qdrant.
#### CollectionName
Gets or sets the collection name for Qdrant.
#### EnsureCollectionExists
is this is true the system will create the collection if not exists

## Class: Qdrant.QdrantVectorStoreProvider
Provider for using Qdrant as a vector store. 

### Properties

#### CollectionName
current container name
### Methods


#### Constructor
constructor for QdrantVectorStoreProvider 


##### Parameters
* *client:* instance of client
* *options:* configuration options for Qdrant
* *containerName:* name of container




#### StoreVectorsAsync(System.Collections.Generic.IEnumerable{System.ReadOnlyMemory{System.Single}},System.Collections.Generic.Dictionary{System.String,System.Object})
Gets or sets the name of the container. 


#### StoreVectorsAsync(System.Collections.Generic.IEnumerable{System.ValueTuple{System.ReadOnlyMemory{System.Single},System.Collections.Generic.Dictionary{System.String,System.Object}}},System.Collections.Generic.Dictionary{System.String,System.Object})
Gets or sets the name of the container. 


#### ListAsync
Finds nearest neighbors for a given vector. 


##### Return value
An asynchronous enumerable collection of search results representing nearest neighbors.



#### FindNeighborsAsync(System.ReadOnlyMemory{System.Single})
Finds nearest neighbors for a given vector. 


##### Parameters
* *find:* The vector to search for neighbors.




##### Return value
An asynchronous enumerable collection of search results representing nearest neighbors.



#### FindNeighborsAsync(System.ReadOnlyMemory{System.Single},System.String)
Finds nearest neighbors for a given vector, grouped by a specified field. 


##### Parameters
* *find:* The vector to search for neighbors.
* *groupBy:* The field to group the results by.




##### Return value
An asynchronous enumerable collection of search results representing nearest neighbors grouped by the specified field.



#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Qdrant.QdrantVectorStoreProvider*class with the specified dependencies. 


##### Parameters
* *serviceProvider:* The service provider.




#### 
Creates a new instance of 
 *See: T:OoBDev.Qdrant.QdrantVectorStoreProvider*based on the specified collection name. 


##### Parameters
* *containerName:* The name of the collection.




##### Return value
A new instance of .



## Class: Qdrant.QdrantVectorStoreProviderFactory
Represents a factory for creating instances of 
 *See: T:OoBDev.Qdrant.QdrantVectorStoreProvider*. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Qdrant.QdrantVectorStoreProvider*class with the specified dependencies. 


##### Parameters
* *serviceProvider:* The service provider.




#### Create(System.String)
Creates a new instance of 
 *See: T:OoBDev.Qdrant.QdrantVectorStoreProvider*based on the specified collection name. 


##### Parameters
* *containerName:* The name of the collection.




##### Return value
A new instance of .



## Class: Qdrant.ServiceCollectionExtensions
Provides extension methods for configuring services related to Qdrant. 

### Methods


#### TryAddQdrantServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for Qdrant. 


##### Parameters
* *services:* The to add the services to.
* *configuration:* The to bind Qdrant options from.
* *qdrantOptionSection:* The configuration section name containing Qdrant options.




##### Return value
The modified .

