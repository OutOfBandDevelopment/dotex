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
- **Create**: Creates a new instance of IVectorStore with the specified collection name.
- **Create``1**: Creates a new instance of IVectorStore of the specified type.

## IVectorStoreProvider
Interface for a vector store provider that implements IVectorStore.

### Properties
- **CollectionName**: The collection the provider reads and writes.

### Methods
- **Create**: Creates a new instance of IVectorStoreProvider with the specified collection name.

## IVectorStoreProviderFactory
Interface for creating instances of IVectorStoreProvider.

### Methods
- **Create**: Creates a new instance of IVectorStoreProvider with the specified collection name.

## VectorStoreAttribute
Attribute for specifying the collection name of a vector store.

### Properties
- **CollectionName**: The collection the provider reads and writes.

## Graph

`IGraphStore` (`UpsertNodeAsync`, `ConnectAsync`, `NeighborsAsync`, `ShortestPathAsync`, `DeleteNodeAsync`) over `GraphNode` (id, label, properties) is the graph contract; see the [GraphQueries](../../../docs/design/GraphQueries/README.md) design.

`SearchTypes` is a flags enum: `Semantic` (1), `Lexical` (2) and `Hybrid` (3, both). No lexical or hybrid provider exists yet.
