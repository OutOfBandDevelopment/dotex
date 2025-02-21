## Overview
The OoBDev.Search.Semantic namespace contains classes and interfaces for working with vector stores. A vector store is a data structure that stores and retrieves vectors (dense numerical representations) associated with metadata.

## Classes and Interfaces
### VectorStoreFactory
The `VectorStoreFactory` class is responsible for creating instances of `IVectorStore` based on a specified collection name or type. It uses dependency injection to resolve the underlying provider for the vector store.

### WrappedVectorStore
The `WrappedVectorStore<T>` class is a wrapper around an `IVectorStore` instance. It provides a way to wrap an existing vector store with additional functionality. The `WrappedVectorStore` class implements the `IVectorStore<T>` interface.

### IVectorStore
The `IVectorStore<T>` interface defines the methods for working with a vector store. The methods include:

* `FindNeighborsAsync(ReadOnlyMemory<float> find)`: Finds nearest neighbors asynchronously based on the specified vector.
* `FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)`: Finds nearest neighbors asynchronously based on the specified vector and groups the results by a specified field.
* `ListAsync()`: Retrieves all items asynchronously.
* `StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, Dictionary<string, object> metadata)`: Stores vectors asynchronously along with their associated metadata.
* `StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata)> items, Dictionary<string, object> metadata)`: Stores the specified embeddings and metadata.

## UML Diagrams

### Class Diagram
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/CClassDiagram/trunk/CClassDiagram.puml

class VectorStoreFactory {
  - _serviceProvider: IServiceProvider
  - _factories: IEnumerable<IVectorStoreProviderFactory>

  -+ Create(string collectionName): IVectorStore
  -+ Create<T>() : IVectorStore
}

class WrappedVectorStore<T> {
  - _wrapped: IVectorStore

  -+ FindNeighborsAsync(ReadOnlyMemory<float> find): IAsyncEnumerable<SearchResultModel>
  -+ FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy): IAsyncEnumerable<SearchResultModel>
  -+ ListAsync(): IAsyncEnumerable<SearchResultModel>
  -+ StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, Dictionary<string, object> metadata): Task<string[]>
  -+ StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata)> items, Dictionary<string, object> metadata): Task<string[]>
}

interface IVectorStore<T> {
  -+ FindNeighborsAsync(ReadOnlyMemory<float> find): IAsyncEnumerable<SearchResultModel>
  -+ FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy): IAsyncEnumerable<SearchResultModel>
  -+ ListAsync(): IAsyncEnumerable<SearchResultModel>
  -+ StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, Dictionary<string, object> metadata): Task<string[]>
  -+ StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata)> items, Dictionary<string, object> metadata): Task<string[]>
}

@enduml
```

### Component Model
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/CComponentModel/trunk/CComponentModel.puml

class DataPortal {
  - _vectorStoreFactory: VectorStoreFactory
}

class SearchService {
  - _wrappedVectorStore: WrappedVectorStore<T>
}
```

### Sequence Diagram
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/CSequenceDiagram/trunk/CSequenceDiagram.puml

actor User
participant DataPortal as DP
participant SearchService as SS
participant WrappedVectorStore as WVS

activate User

User ->> DP: Find nearest neighbors
DP ->> WVS: Find nearest neighbors
WVS ->> DP: SearchResultModel
DP ->> User: SearchResultModel

User ->> DP: List all
DP ->> WVS: List all
WVS ->> DP: SearchResultModel
DP ->> User: SearchResultModel

User ->> DP: Store vectors
DP ->> WVS: Store vectors
WVS ->> DP: string[]
DP ->> User: string[]

deactivate User
@enduml
```