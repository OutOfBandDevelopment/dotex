**README File**

The OoBDev Message Queueing Services is a collection of .NET libraries that provide a mechanism for handling messages within a message queue. The services provide a robust and scalable solution for building enterprise-grade message-based systems.

**Summary**

The OoBDev Message Queueing Services consist of several interfaces and classes that work together to handle messages within a message queue. The services provide functionality for creating and managing message contexts, resolving provider information, and handling message queue handlers.

**Technical Summary**

The OoBDev Message Queueing Services utilize various design patterns and architectural patterns to achieve their functionality. The following patterns are notable:

* **Factory Pattern**: The `IMessageContextFactory` interface is used to create instances of `IMessageContext`, which represents the context of a message being processed in a message queue.
* **Wrapper Pattern**: The `IMessageHandlerProviderWrapped` interface wraps the `IMessageHandlerProvider` interface, providing additional functionality for setting message queue handlers and configuration.
* **Repository Pattern**: The `IMessagePropertyResolver` interface is used to resolve properties related to message handling, such as provider information, message IDs, and configuration.

**Component Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/mladenk/devplants/master/uml-config.png

class IMessageContext {
  - originMessageId : string
  - correlationId : string
  - requestId : string
  - sentId : string
  - channelType : string
  - messageType : string
  - sentAt : DateTimeOffset
  - sentBy : string
  - sentFrom : string
  - headers : Dictionary<string, object>
  - config : IConfigurationSection
}

class IMessageContextFactory {
  - create(channelType, messageType, originMessageId, correlationId, requestId, configuration, caller, callerLine, callerFile) : IMessageContext
  - create(channelType, message, configuration) : IMessageContext
}

class IMessageHandlerProvider {
  - handleAsync(message, messageId) : Task
  - config : IConfigurationSection
}

class IMessageHandlerProviderWrapped {
  - setHandlers(handlers) : IMessageHandlerProviderWrapped
  - setChannelType(channelType) : IMessageHandlerProviderWrapped
  - setConfig(config) : IMessageHandlerProviderWrapped
}

class IMessagePropertyResolver {
  - providerSafe(channelType, messageType) : (providerKey, simpleTargetName, simpleMessageName, configPath)
  - provider(channelType, messageType) : string
  - messageId(channelType, messageType, messageId) : string
  - generateId(channelType, messageType) : string
  - configurationSafe(channelType, messageType) : (configurationSection, simpleTargetName, simpleMessageName, configPath)
  - configuration(channelType, messageType) : IConfigurationSection
}

IMessageContextFactory -->> IMessageContext
IMessageHandlerProvider -->> IMessageHandlerProviderWrapped
IMessageHandlerProviderWrapped -->> IMessageHandlerProvider
IMessagePropertyResolver -->> IMessageHandlerProviderProvider

@enduml
```

The component diagram shows the relationships between the various classes and interfaces in the OoBDev Message Queueing Services. The diagram includes the `IMessageContext`, `IMessageContextFactory`, `IMessageHandlerProvider`, `IMessageHandlerProviderWrapped`, and `IMessagePropertyResolver` classes, as well as their relationships with each other.