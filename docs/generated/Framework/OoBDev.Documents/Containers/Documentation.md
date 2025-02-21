Here is the documentation for the source code in Markdown format:

**BlobContainerFactory**
======================

**Overview**
-----------

The `BlobContainerFactory` is a class that provides a way to create instances of `IBlobContainer` based on a specified container name.

**Constructor**
-------------

```plantuml
@startuml
class BlobContainerFactory {
  - _serviceProvider: IServiceProvider
  - _factories: IEnumerable<IBlobContainerProviderFactory>
  - _logger: ILogger
}
BlobContainerFactory -> IServiceProvider: serviceProvider
BlobContainerFactory -> IEnumerable<IBlobContainerProviderFactory>: factories
BlobContainerFactory -> ILogger: logger
@enduml
```

**Create**
---------

The `Create` method creates a blob container with the specified container name.

**Create** Sequence Diagram
-------------------------

```plantuml
@startuml
sequenceDiagram
    participant BlobContainerFactory as factory
    participant IBlobContainerProvider as provider
    note left: Service provider
    factory->>provider: Get keyed service
    alt provider exists
        provider->>factory: Obtain provider
    else
        factory->>factories: Select creator
        alt creator exists
            creator->>factory: Create provider
        else
            factory->>IServiceProvider: Get required service
    end
    factory->>provider: Set container name
    factory->>logger: Log information
    note right: Return provider
@enduml
```

**WrappedBlobContainer**
------------------------

**Overview**
-----------

The `WrappedBlobContainer` is a class that wraps an instance of `IBlobContainer` and provides a way to interact with it.

**Constructor**
-------------

```plantuml
@startuml
class WrappedBlobContainer<T> {
  - _wrapped: IBlobContainer
}
WrappedBlobContainer<T> -> IBlobContainer: wrapped
@enduml
```

**Methods**
---------

*   **DeleteContentAsync**: Deletes content asynchronously.
*   **GetContentAsync**: Retrieves content asynchronously.
*   **GetContentMetaDataAsync**: Retrieves content metadata asynchronously.
*   **QueryContent**: Queries content metadata.
*   **StoreContentAsync**: Stores content asynchronously.
*   **StoreContentMetaDataAsync**: Stores content metadata asynchronously.

I hope this documentation meets your requirements. Let me know if you need any further modifications.