**README File**

**Introduction**
OoBDev.MessageQueueing is a .NET library for message queueing, providing a simple and extensible way to send and receive messages between applications. The library includes a message sender, message receiver, and various factories and utilities for configuring and resolving message queueing services.

**Components**

The library consists of the following components:

1. **MessageSender**: A class that sends messages asynchronously to a specified communication channel.
2. **MessageContext**: A class that represents the context associated with a message, including metadata and headers.
3. **MessageContextFactory**: A factory for creating instances of MessageContext.
4. **MessageHandlerProvider**: A class that provides handling of queue messages by coordinating multiple IMessageQueueHandler instances.
5. **MessagePropertyResolver**: A utility class for resolving properties related to message queue handling.
6. **MessageReceiverProviderFactory**: A factory for creating instances of IMessageReceiverProvider based on configured message handlers.
7. **MessageSenderProviderFactory**: A factory for creating instances of IMessageSenderProvider based on channel and message types.

**Service Collection Extensions**
The library provides extension methods for configuring IoC (Inversion of Control) services to support all Message Queueing within this library. The extensions methods add instances of the above components to the service collection.

**Technical Summary**
The library employs several design patterns and architectural patterns, including:

1. **Dependency Injection**: The library uses dependency injection to inject dependencies between components, making it easier to test and maintain.
2. **IoC (Inversion of Control)**: The library uses IoC to control the creation and management of components, allowing for loose coupling and flexibility.
3. **Abstract Factory**: The library uses the abstract factory pattern to create instances of IMessageSenderProvider and IMessageReceiverProvider factories, allowing for flexibility and extensibility.

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-Builder/master/C4_Business_Process_Diagram.puml

BPMNProcess "Message Queueing Process" {
    task "Send Message" as T1
    task "Receive Message" as T2

    flow T1 ->> T2
}

@enduml
```

**Component Diagram**
The following component diagram shows the relationships between the components:
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-Builder/master/C4_Component_Diagram.puml

 Component "Message Sender" as MS {
    part "Message Sender Provider" as MSP
    part "Message Context" as MC
    part "Message Context Factory" as MCF
}

 Component "Message Receiver" as MR {
    part "Message Receiver Provider" as MRP
    part "Message Handler Provider" as MHP
}

 Component "Message Queueing Services" as MQS {
    part "MessagePropertyResolver" as MPR
    part "MessageHandlerProvider" as MHP
    part "MessageSenderProviderFactory" as MSPF
    part "MessageReceiverProviderFactory" as MRPF
}

 MS ->> MC
 MS ->> MCF
 MSP ->> MS
 MRP ->> MC
 MRP ->> MCF
 MHP ->> MC
 MHP ->> MCR
 MPR ->> MQS
 MHP ->> MQS
 MSPF ->> MQS
 MRPF ->> MQS

@enduml
```
This component diagram shows the relationships between the components, including the Message Sender, Message Receiver, and Message Queueing Services.