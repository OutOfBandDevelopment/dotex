# OoBDev.Qdrant

OoBDev.Qdrant offers functionality for interacting with the Qdrant API, including managing collections, querying for nearest neighbors, and creating vector representations of various data types. Let's delve into its features:

## IPointStructFactory

Interface for a factory that creates instances of `Qdrant.Client.Grpc.PointStruct`.

### Methods

- **CreateFileChunk**: Creates a new instance of `Qdrant.Client.Grpc.PointStruct` for a file chunk.
- **CreateQuestion**: Creates a new instance of `Qdrant.Client.Grpc.PointStruct` for a question.
- **CreateServiceReference**: Creates a new instance of `Qdrant.Client.Grpc.PointStruct` for a service reference.

## IQdrantGrpcClientFactory

Interface for a factory that creates instances of `Qdrant.Client.Grpc.QdrantGrpcClient`.

### Methods

- **Create**: Creates a new instance of `Qdrant.Client.Grpc.QdrantGrpcClient`.

## PointStructFactory

Factory for creating instances of `Qdrant.Client.Grpc.PointStruct`.

### Methods

- **CreateFileChunk**: Creates an instance representing a file chunk.
- **CreateServiceReference**: Creates an instance representing a service reference.
- **CreateQuestion**: Creates an instance representing a question.

## QdrantGrpcClientExtensions

Extension methods for Qdrant gRPC client.

### Methods

- **EnsureCollectionExistsAsync**: Ensures that the specified collection exists in Qdrant.
- Other methods for querying and managing collections.

## QdrantGrpcClientFactory

Factory for creating instances of the Qdrant gRPC client.

### Methods

- **Create**: Creates a new instance of the Qdrant gRPC client.

## QdrantOptions

Options for configuring Qdrant.

### Properties

- **Url**: The URL for Qdrant.
- **CollectionName**: The collection name for Qdrant.

## QdrantVectorStoreProvider

Provider for using Qdrant as a vector store.

### Properties

- **ContainerName**: The current container name.

### Methods

- **StoreVectorsAsync**: Stores vectors in the container.
- **FindNeighborsAsync**: Finds nearest neighbors for a given vector.

## QdrantVectorStoreProviderFactory

Factory for creating instances of `QdrantVectorStoreProvider`.

### Methods

- **Create**: Creates a new instance of `QdrantVectorStoreProvider`.

## ServiceCollectionExtensions

Extension methods for configuring services related to Qdrant.

### Methods

- **TryAddQdrantServices**: Configures services for Qdrant.

OoBDev.Qdrant provides comprehensive support for utilizing the Qdrant API efficiently, empowering developers with advanced functionalities for managing collections, querying, and vector representations.
