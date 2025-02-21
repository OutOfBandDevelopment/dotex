**README File**

**Overview**

The OoBDev.MessageQueueing.Abstractions library provides a comprehensive abstraction layer for interacting with message queues in .NET applications. It offers interfaces and classes for handling messages, sending messages, and receiving messages from various message queues. These abstractions enable developers to decouple their application logic from specific message queue implementations, facilitating easier testing, extensibility, and maintenance.

**Key Features**

- **Message Handling**: Provides generic interfaces for handling messages from message queues, allowing for flexible message processing logic.
- **Message Sending**: Offers generic interfaces for sending messages to message queues, supporting asynchronous message transmission.
- **Message Context**: Defines a message context interface to encapsulate metadata associated with messages, such as origin message ID, correlation ID, and message type.
- **Extensibility**: Enables developers to implement custom message queue handlers, senders, and receivers, supporting a wide range of message queue systems.
- **Configurability**: Allows for configuration-based setup of message queue providers, handlers, and senders, facilitating easy integration with different messaging platforms.
- **Ease of Testing**: Facilitates unit testing by providing interfaces that can be easily mocked or stubbed, allowing for isolated testing of message handling and sending logic.

**Getting Started**

To use OoBDev.MessageQueueing.Abstractions in your .NET project, simply install the library via NuGet Package Manager:

```bash
dotnet add package OoBDev.MessageQueueing.Abstractions
```

**Technical Summary**

The library relies on the following design patterns:

* **Dependency Injection**: Interfaces and classes are designed to be loosely coupled, allowing for easy replacement of dependencies.
* **Test-Driven Development (TDD)**: Interfaces and classes are designed with testability in mind, making it easier to write unit tests.

**Component Diagram**

```plantuml
@startuml
/component "OoBDev.MessageQueueing.Abstractions" as Abstractions {
  interface IMessageQueueHandler as MessageHandler
  interface IMessageQueueSender as MessageSender
  class MessageQueueAttribute as Attribute
  class MyMessageHandler as MyMessageHandler
}

/component "Microsoft.Extensions.Configuration.Abstractions" as ConfigAbstractions {
  interface IConfiguration as Configuration
}

OoBDev.MessageQueueing.Abstractions ..> Microsoft.Extensions.Configuration.Abstractions
Abstractions ..> MyMessageHandler
MyMessageHandler ..> MessageHandler
MessageHandler ..> MessageSender

@enduml
```

This component diagram illustrates the relationships between the interfaces, classes, and dependencies within the OoBDev.MessageQueueing.Abstractions library.