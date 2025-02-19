**README File**

This repository contains the code for a message queueing hosting system, which provides a framework for starting and stopping message receivers based on configured providers.

**Overview**

The system consists of the following components:

1. **MessageReceiverHost**: This is the hosted service responsible for starting and stopping message receivers based on the configured providers. It initializes a new instance of the class, disposing of resources when necessary, and providing methods for starting and stopping the message receiver host.
2. **ServiceCollectionExtensions**: This class provides extension methods for configuring IoC (Inversion of Control) services to support all Message Queueing within this library.

**Design Patterns and Architectural Patterns**

The **MessageReceiverHost** class implements the following design patterns:

* **Single Responsibility Principle** (SRP): Each method in the class has a single responsibility, making it easier to understand and maintain.
* **Factory Pattern**: The `IMessageReceiverProviderFactory` is used to create instances of message receiver providers.
* **CancellationToken** is used to cancel the start or stop operation.

**Component Diagram**

```plantuml
@startuml
class MessageReceiverHost {
  + IHostedService
  + IMessageReceiverProviderFactory
  + ILogger<MessageReceiverHost>
}

class IMessageReceiverProviderFactory {
  + Create(): IMessageReceiverProvider[]
}

class IMessageReceiverProvider {
  + RunAsync(CancellationToken)
}

class CancellationToken {
  - IsCancellationRequested
}

class Logger {
  + LogInformation(string)
  + LogError(string, Exception)
  + LogDebug(string, string)
}

ServiceCollectionExtensions -> MessageReceiverHost
MessageReceiverHost -> IMessageReceiverProviderFactory

note "This is a high-level diagram showing the main components and their interactions"
@enduml
```