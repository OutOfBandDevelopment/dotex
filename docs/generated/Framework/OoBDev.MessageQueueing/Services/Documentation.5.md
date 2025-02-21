# Message Receiver Provider Factory Documentation
====================================================

## Overview
-----------

The Message Receiver Provider Factory is responsible for creating instances of `IMessageReceiverProvider` based on configured message handlers. This factory uses the `IMessagePropertyResolver` to resolve the correct message receiver provider for each handler.

## Class Diagram
---------------

```plantuml
@startuml
class MessageReceiverProviderFactory {
  -private logger: ILogger
  +Create(): IEnumerable<IMessageReceiverProvider>
  +Receiver(string providerKey): IMessageReceiverProvider
}

class IMessageReceiverProvider {
  +HandleMessage()
}

class IMessageHandlerProvider {
  +GetRequiredService(): IServiceProvider
  +SetHandlers(): void
  +SetChannelType(): void
  +SetConfig(): void
}

class IMessagePropertyResolver {
  +ProviderSafe(channelType, messageType): IMessageReceiverProvider
  +ConfigurationSafe(channelType, messageType): IMessagePropertyResolver
}

class ILogger {
  +LogWarning(message, handlers): void
}

@enduml
```

## Method Descriptions
--------------------

### `Create()`

Creates instances of `IMessageReceiverProvider` based on configured message handlers.

### `Receiver(string providerKey)`

Resolves an instance of `IMessageReceiverProvider` using the provided provider key.

## Message Sender Provider Factory Documentation
================================================___

## Overview
-----------

The Message Sender Provider Factory is responsible for creating instances of `IMessageSenderProvider` based on channel and message types.

### Class Diagram
---------------

```plantuml
@startuml
class MessageSenderProviderFactory {
  +Sender(channelType, messageType): IMessageSenderProvider
}

class IMessageSenderProvider {
  +Send(channelType, messageType): void
}

class IMessagePropertyResolver {
  +Provider(channelType, messageType): string
}

class IServiceProvider {
  +GetRequiredService(): void
}

class ILogger {
  +LogWarning(message, handlers): void
}

@enduml
```

### Method Descriptions
--------------------

### `Sender(Type channelType, Type messageType)`

Creates an instance of `IMessageSenderProvider` based on the provided channel and message types.