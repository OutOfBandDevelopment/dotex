**Readme File**

Welcome to OoBDev.Documents!

This library provides a set of tools for managing documents, including storage, conversion, and type detection. It offers classes and factories for creating and interacting with blob containers, document conversion services, and document type tools.

**Summary**

The OoBDev.Documents library is designed to simplify document-related tasks in your applications. It provides a range of features, including:

* Blob container management with the BlobContainerFactory
* Document conversion using the DocumentConversion service
* Content type detection with the DocumentTypeTools
* Service configuration using the ServiceCollectionExtensions

**Technical Summary**

This library uses several design patterns and architectural patterns to achieve its goals. The ServiceCollectionExtensions class uses the Singleton pattern to ensure that only one instance of the DocumentConversion and BlobContainerFactory services are created. The TryAddDocumentServices method uses a combination of Singleton and Transient lifetime scopes to configure the services.

The library also uses dependency injection to provide the required services to the application.

**Component Diagram**

```plantuml
@startuml
class BlobContainerFactory {
  +BlobContainer createBlobContainer(string name)
}

class DocumentConversion {
  +DocumentConversionChainBuilder createDocumentConversionChain()
}

class DocumentTypeTools {
  +IDocumentType getDocumentType(string contentType)
  +IDocumentType getDocumentType(string fileExtension)
  +IDocumentType getDocumentType.Stream(Stream)
}

class ServiceCollection {
  +IServiceCollection TryAddDocumentServices()
}

ServiceCollection --+
                 |  BlobContainerFactory
                 |  DocumentConversion
                 |  DocumentTypeTools
@enduml
```
This component diagram shows the main classes and their relationships in the OoBDev.Documents library. The BlobContainerFactory, DocumentConversion, and DocumentTypeTools classes are provided as services to the application through the ServiceCollectionExtensions class.