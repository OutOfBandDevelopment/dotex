# OoBDev.Search.Abstractions


## Class: Search.IVectorStore
Interface for storing and querying vectors. 

### Methods


#### StoreVectorsAsync(System.Collections.Generic.IEnumerable{System.ReadOnlyMemory{System.Single}},System.Collections.Generic.Dictionary{System.String,System.Object})
Stores the specified embeddings and metadata. 


##### Parameters
* *embeddings:* The embeddings to store.
* *metadata:* The metadata to store.




##### Return value
A task representing the asynchronous operation.



#### StoreVectorsAsync(System.Collections.Generic.IEnumerable{System.ValueTuple{System.ReadOnlyMemory{System.Single},System.Collections.Generic.Dictionary{System.String,System.Object}}},System.Collections.Generic.Dictionary{System.String,System.Object})
Stores the specified embeddings and metadata. 


##### Parameters
* *items:* The embeddings to store.
* *metadata:* The metadata to store.




##### Return value
A task representing the asynchronous operation.



#### ListAsync
Lists the stored vectors. 


##### Return value
A sequence of search result models.



#### FindNeighborsAsync(System.ReadOnlyMemory{System.Single})
Finds the neighbors of the specified vector. 


##### Parameters
* *find:* The vector to find neighbors for.




##### Return value
A sequence of search result models.



#### FindNeighborsAsync(System.ReadOnlyMemory{System.Single},System.String)
Finds the neighbors of the specified vector, grouped by the specified key. 


##### Parameters
* *find:* The vector to find neighbors for.
* *groupBy:* The key to group the results by.




##### Return value
A sequence of search result models.



## Class: Search.IVectorStore`1
Interface for storing and querying vectors. 


## Class: Search.Models.SearchResultModel
Represents a search result model containing information about a search result. 

### Properties

#### Score
Gets or initializes the score of the search result.
#### ItemId
Gets or initializes the item's id value.
#### MetaData
Gets or initializes the item's metadata.

## Class: Search.Models.SearchTypes
Specifies the types of search. 

### Fields

#### None
Indicates no specific search type.
#### Semantic
Indicates a semantic search type.
#### Lexical
Indicates a lexical search type.
#### Hybrid
Indicates a hybrid search type combining semantic and lexical searches.

## Class: Search.Semantic.IVectorStoreFactory
Interface for creating instances of IVectorStore. 

### Methods


#### Create(System.String)
Creates a new instance of IVectorStore with the specified collection name. 


##### Parameters
* *collectionName:* The name of the collection.




##### Return value
A new instance of IVectorStore.



#### Create``1
Creates a new instance of IVectorStore of the specified type. 


##### Return value
A new instance of IVectorStore of the specified type.



## Class: Search.Semantic.IVectorStoreProvider
Interface for a vector store provider that implements IVectorStore. 

### Properties

#### CollectionName
Gets or sets the name of the container.
### Methods


#### 
Creates a new instance of IVectorStoreProvider with the specified container name. 


##### Parameters
* *containerName:* The name of the container.




##### Return value
A new instance of IVectorStoreProvider.



## Class: Search.Semantic.IVectorStoreProviderFactory
Interface for creating instances of IVectorStoreProvider. 

### Methods


#### Create(System.String)
Creates a new instance of IVectorStoreProvider with the specified container name. 


##### Parameters
* *containerName:* The name of the container.




##### Return value
A new instance of IVectorStoreProvider.



## Class: Search.VectorStoreAttribute
Attribute for specifying the container name for a vector store. 

This attribute can be applied to a class that implements IVectorStore.
### Properties

#### CollectionName
Gets or sets the name of the container.