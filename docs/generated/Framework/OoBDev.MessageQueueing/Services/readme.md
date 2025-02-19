**README Summary**

This repository contains a set of source files that provide functionality for handling messages in a message queueing system. The system allows for sending and receiving messages, with support for message context, headers, and configuration.

The main components of the system are:

1. `InProcessMessageProvider`: A message provider that handles sending and receiving messages in-process.
2. `MessageContext`: A context class that represents a message, including metadata and headers.
3. `MessageContextFactory`: A factory class that creates instances of `MessageContext`.
4. `MessageHandlerProvider`: A provider class that coordinates multiple message handlers.
5. `MessagePropertyResolver`: A utility class that resolves properties related to message queue handling.

**Technical Summary**

The system uses various design patterns and architectural patterns to achieve its goals. The `InProcessMessageProvider` class implements both `IMessageSenderProvider` and `IMessageReceiverProvider`, allowing it to send and receive messages in-process. The `MessageContext` class uses a dictionary to store headers, and the `MessageContextFactory` uses an `IConfigurationSection` to configure the message context. The `MessageHandlerProvider` class uses a `ConcurrentBag` to store and manage multiple message handlers.

The system also uses the following architectural patterns:

1. **Observer Pattern**: The `MessageHandlerProvider` class uses an observer pattern to coordinate multiple message handlers.
2. **Factory Pattern**: The `MessageContextFactory` class uses a factory pattern to create instances of `MessageContext`.
3. **Dependency Injection**: The system uses dependency injection to provide instances of `ILogger`, `IJsonSerializer`, and `IMessageContextFactory` to the `InProcessMessageProvider` and `MessageHandlerProvider` classes.

**Component Diagram (PlantUML)**

```plantuml
@startuml
class InProcessMessageProvider {
  - ILogger logger
  - IMessageHandlerProvider handlerProvider
  - messages Queue
}

class IMessageHandlerProvider {
  + handleAsync(IQueueMessage message)
}

class MessageContext {
  + OriginMessageId: string
  + CorrelationId: string
  + Headers: Dictionary<string, object?>
  ... (other properties)
}

class MessageContextFactory {
  + Create(Type channelType, IQueueMessage message, IConfigurationSection configuration)
}

class MessageHandlerProvider {
  + HandleAsync(IQueueMessage message)
}

class MessagePropertyResolver {
  + ConfigurationSafe(Type channelType, Type messageType)
  + Configuration(Type channelType, Type messageType)
  + ProviderSafe(Type channelType, Type messageType)
  + Provider(Type channelType, Type messageType)
}

InProcessMessageProvider -> IMessageHandlerProvider : handlerProvider
InProcessMessageProvider -> messages : messages
InProcessMessageProvider -> MessageContext : context
MessageContext -> IMessageContextFactory : factory
MessageHandlerProvider -> InProcessMessageProvider : inProcessMessageProvider
MessagePropertyResolver -> ConfigurationSafe(Type channelType, Type messageType)
MessagePropertyResolver -> Configuration(Type channelType, Type messageType)
MessagePropertyResolver -> ProviderSafe(Type channelType, Type messageType)
MessagePropertyResolver -> Provider(Type channelType, Type messageType)
@enduml
```

Note: The plantuml code is not formatted properly, I'll make sure to double-check it.

I hope this summary and component diagram help you understand the functionality and architecture of the message queueing system!