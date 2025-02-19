Here is the documentation for the provided source code files, including class diagrams in Plant UML:

**OoBDev.RabbitMQ.csproj**

This is a .NET Core project file that defines the structure and dependencies of the OoBDev.RabbitMQ library. The project references several NuGet packages, including Microsoft.Extensions.Configuration.Abstractions, Microsoft.Extensions.DependencyInjection.Abstractions, and RabbitMQ.Client. It also references two other project files, OoBDev.MessageQueueing.Abstractions and OoBDev.System.Abstractions.

**RabbitMQGlobals.cs**

This file defines a static class called RabbitMQGlobals that contains global constants related to RabbitMQ. The class has a single constant property called MessageProviderKey, which is a string that represents the key associated with the RabbitMQ message provider.

**Readme.RabbitMQ.md**

This is a Markdown file that serves as the package read-me file for the OoBDev.RabbitMQ library. The file provides a brief summary of the library, notes and references, and links to external resources.

**ServiceCollectionEx.cs**

This file defines a static class called ServiceCollectionEx that provides extension methods for configuring RabbitMQ services in the IServiceCollection. The class has two methods:

* TryAddRabbitMQServices: attempts to add RabbitMQ services, including blob and queue services, to the specified IServiceCollection.
* TryAddRabbitMQQueueServices: attempts to add RabbitMQ queue services to the specified IServiceCollection.

The class uses the TryAddTransient, TryAddKeyedTransient, and AddTransient methods from Microsoft.Extensions.DependencyInjection to add services to the IServiceCollection.

**Class Diagram in Plant UML**

Here is a class diagram in Plant UML that represents the structure of the OoBDev.RabbitMQ library:
```
@startuml
!pragma java_annotation false

class RabbitMQQueueMessageProvider {
  - messageSender: IMessenger
  - messageReceiver: IMessageReceiver
}

class QueueClientFactory {
  - queueClient: QueueClient
}

class IServiceCollection {
  -- services: ServiceCollection
}

class ServiceCollectionEx {
  - TryAddRabbitMQServices: IServiceCollection
  - TryAddRabbitMQQueueServices: IServiceCollection
}

class RabbitMQGlobals {
  - MessageProviderKey: string
}

RabbitMQQueueMessageProvider --|> IMessageSenderProvider
RabbitMQQueueMessageProvider --|> IMessageReceiverProvider

QueueClientFactory --|> IQueueClientFactory

IServiceCollection --|> ServiceCollectionEx

ServiceCollectionEx --|> RabbitMQQueueMessageProvider

@enduml
```
This diagram shows the relationships between the classes and interfaces in the OoBDev.RabbitMQ library. It includes the RabbitMQQueueMessageProvider class, which implements IMessageSenderProvider and IMessageReceiverProvider, and the QueueClientFactory class, which implements IQueueClientFactory. The ServiceCollectionEx class provides extension methods for adding RabbitMQ services to the IServiceCollection.

The diagram also shows the relationships between the classes and interfaces in the Microsoft.Extensions.DependencyInjection and RabbitMQ.Client NuGet packages. These packages are used by the OoBDev.RabbitMQ library to provide services and implement messaging functionality.