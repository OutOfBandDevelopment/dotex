Here is the documentation for the provided source code in Markdown format:

# OoBDev.MessageQueueing.Services Tests

## Overview

This section provides documentation for the unit tests of the OoBDev.MessageQueueing.Services namespace.

### InProcessMessageProviderTests

```InProcessMessageProviderTests.cs
```

The `InProcessMessageProviderTests` class contains the unit tests for the `InProcessMessageProvider` class. This class is responsible for sending and processing messages in-process.

#### InOutTest

The `InOutTest` method tests the `SendAsync` method of the `InProcessMessageProvider` class. It sets up a mock handler and a message context, and then sends a message and verifies that it is processed correctly.

### Class Diagram

```plantuml
@startuml
class InProcessMessageProvider {
    - TestLogger testLogger
    - IMessageHandlerProvider handlerProvider
}
InProcessMessageProvider --* IMessageHandlerProvider
InProcessMessageProvider --* TestLogger
@enduml
```

### Component Model

```plantuml
@startuml
component process_message_provider
component message_handler_provider
component test_logger

process_message_provider --|> InProcessMessageProvider
message_handler_provider --|> IMessageHandlerProvider
test_logger --|> TestLogger
@enduml
```

### Sequence Diagram

```plantuml
@startuml
participant InProcessMessageProvider as ipmp
participant IMessageHandlerProvider as imhp
participant TestLogger as tl

note "Setup handler provider" as n1
imhp/plugin_inProcessMessageProvider:setHandlerProvider(ipmp)

note "Send message" as n2
ipmp/sendAsync(message:TestMessage):correlationId

note "Handle message" as n3
imhp/handleAsync(message:TestMessage):completedTask

note "Run" as n4
ipmp/runAsync(cancellationToken: CancellationToken):completedTask
@enduml
```

## MessageHandlerProviderTests

```MessageHandlerProviderTests.cs
```

The `MessageHandlerProviderTests` class contains the unit tests for the `MessageHandlerProvider` class. This class is responsible for providing message handlers.

### HandleAsyncTest

The `HandleAsyncTest` method tests the `HandleAsync` method of the `MessageHandlerProvider` class. It sets up a mock serializer, context factory, and message queue handler, and then handles a message and verifies that it is processed correctly.

### Class Diagram

```plantuml
@startuml
class MessageHandlerProvider {
    - IJsonSerializer jsonSerializer
    - IMessageContextFactory contextFactory
}
MessageHandlerProvider --* IJsonSerializer
MessageHandlerProvider --* IMessageContextFactory
@enduml
```

### Component Model

```plantuml
@startuml
component message_handler_provider
component json_serializer
component message_context_factory

message_handler_provider --|> MessageHandlerProvider
json_serializer --|> IJsonSerializer
message_context_factory --|> IMessageContextFactory
@enduml
```

### Sequence Diagram

```plantuml
@startuml
participant MessageHandlerProvider as mhp
participant IJsonSerializer as js
participant IMessageContextFactory as icf
participant IQueueMessage as im

note "Create context" as n1
icf/create(message: IQueueMessage):messageContext

note "Handle message" as n2
mhp/handleAsync(message: IQueueMessage, correlationId: string):completedTask

note "Serialize" as n3
js/serialize(message: IQueueMessage):serializedMessage

note "Process message" as n4
icf/setMessageContext(im, messageContext)

@enduml
```

## MessageReceiverProviderFactoryTests

```MessageReceiverProviderFactoryTests.cs
```

The `MessageReceiverProviderFactoryTests` class contains the unit tests for the `MessageReceiverProviderFactory` class. This class is responsible for creating message receiver providers.

### CreateTest

The `CreateTest` method tests the `Create` method of the `MessageReceiverProviderFactory` class. It sets up a mock handler, property resolver, and receiver provider, and then creates a message receiver provider and verifies that it is created correctly.

### Class Diagram

```plantuml
@startuml
class MessageReceiverProviderFactory {
    - IMessageQueueHandler handler
    - IMessagePropertyResolver propertyResolver
    - ServiceCollection serviceCollection
}
MessageReceiverProviderFactory --* IMessageQueueHandler
MessageReceiverProviderFactory --* IMessagePropertyResolver
MessageReceiverProviderFactory --* ServiceCollection
@enduml
```

### Component Model

```plantuml
@startuml
component message_receiver_provider_factory
component message_queue_handler
component message_property_resolver
component service_collection

message_receiver_provider_factory --|> MessageReceiverProviderFactory
message_queue_handler --|> IMessageQueueHandler
message_property_resolver --|> IMessagePropertyResolver
service_collection --|> ServiceCollection
@enduml
```

### Sequence Diagram

```plantuml
@startuml
participant MessageReceiverProviderFactory as mrpf
participant