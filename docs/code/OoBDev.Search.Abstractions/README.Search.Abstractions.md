# OoBDev.Search.Abstractions

OoBDev.Search.Abstractions provides interfaces and models for storing and querying vectors efficiently. Here's a breakdown of its key components:

## IVectorStore
Interface for storing and querying vectors.

### Methods
- **StoreVectorsAsync**: Stores embeddings and metadata.
- **ListAsync**: Lists stored vectors.
- **FindNeighborsAsync**: Finds neighbors of a specified vector.
- **FindNeighborsAsync**: Finds neighbors of a specified vector, grouped by a key.

## IVectorStore`1
Generic interface for storing and querying vectors.

## SearchResultModel
Represents a search result model containing information about a search result.

### Properties
- **Score**: The score of the search result.
- **ItemId**: The item's ID value.
- **MetaData**: The item's metadata.

## SearchTypes
Specifies types of search.

## IVectorStoreFactory
Interface for creating instances of IVectorStore.

### Methods
- **Create**: Creates a new instance of IVectorStore with the specified container name.
- **Create``1**: Creates a new instance of IVectorStore of the specified type.

## IVectorStoreProvider
Interface for a vector store provider that implements IVectorStore.

### Properties
- **ContainerName**: The name of the container.

### Methods
- **Create**: Creates a new instance of IVectorStoreProvider with the specified container name.

## IVectorStoreProviderFactory
Interface for creating instances of IVectorStoreProvider.

### Methods
- **Create**: Creates a new instance of IVectorStoreProvider with the specified container name.

## VectorStoreAttribute
Attribute for specifying the container name for a vector store.

### Properties
- **ContainerName**: The name of the container.

OoBDev.Search.Abstractions offers a robust framework for vector storage and querying, facilitating efficient retrieval and manipulation of vector data.
