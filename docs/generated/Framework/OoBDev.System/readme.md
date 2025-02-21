**README**

**Summary**

The OoBDev.System package provides a comprehensive set of classes and utilities for various system-related functionalities, including security, serialization, templating, and providers. This package is designed to be used with the Microsoft.Extensions.DependencyInjection framework to provide a wide range of features for building robust and scalable applications.

**Technical Summary**

The OoBDev.System package uses various design patterns and architectural patterns to achieve its functionality. These patterns include:

* **Service-Provider Pattern**: The package uses the Service-Provider pattern to register and resolve instances of various services, such as hash algorithms, serializers, and templating engines.
* **Builder Pattern**: The `SystemExtensionBuilder` class uses the Builder pattern to provide a fluent API for configuring system extensions.
* **singleton pattern**: The package uses the Singleton pattern to provide a centralized instance of various services, such as date time provider, guid provider, and temp file factory.

**Component Diagram**

Here is the component diagram for the OoBDev.System package using PlantUML:
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-remote/plantuml-remote/master/src/main/svg/bootstrap.min.svg

class ServiceProvider {
  - services: Service[]
}

class Service {
  - name: string
  - instance: object
}

class HashTypes {
  - Md5: HashType
  - Sha256: HashType
  - Sha512: HashType
}

class SerializerTypes {
  - Json: SerializerType
  - Bson: SerializerType
  - Xml: SerializerType
}

class SystemExtensionBuilder {
  - fileTemplatingConfigurationSection: string
  - defaultHashType: HashType
  - defaultSerializerType: SerializerType
}

SystemExtensionBuilder --* ServiceProvider
ServiceProvider --* Service[]
Service --* HashTypes
Service --* SerializerTypes

@enduml
```
This diagram shows the relationships between the various components of the OoBDev.System package, including the `ServiceProvider`, `Service`, `HashTypes`, `SerializerTypes`, and `SystemExtensionBuilder` classes.