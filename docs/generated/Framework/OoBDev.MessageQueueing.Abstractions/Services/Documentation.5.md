Here is the documentation for the source code in markdown format, with PlantUML diagrams as requested:

# Messaging Queueing Services

These services provide functionality for sending and receiving messages from a message queue.

## Interfaces

### IMessageReceiverProvider

Provides functionality for receiving messages from a message queue.

```plantuml
@startuml
class IMessageReceiverProvider {
  + SetHandlerProvider(IMessageHandlerProvider handlerProvider): IMessageReceiverProvider
  + RunAsync(CancellationToken cancellationToken = default): Task
}
@enduml
```

### IMessageReceiverProviderFactory

Factory for creating instances of IMessageReceiverProvider.

```plantuml
@startuml
class IMessageReceiverProviderFactory {
  + Create(): IEnumerable<IMessageReceiverProvider>
}
@enduml
```

### IMessageSenderProvider

Represents a provider for sending messages to a message queue.

```plantuml
@startuml
class IMessageSenderProvider {
  + SendAsync(object message, IMessageContext context): Task<string?>
}
@enduml
```

### IMessageSenderProviderFactory

Represents a factory for creating instances of IMessageSenderProvider.

```plantuml
@startuml
class IMessageSenderProviderFactory {
  + Sender(Type channelType, Type messageType): IMessageSenderProvider
}
@enduml
```

### IQueueMessage

Represents a message within a message queue.

```plantuml
@startuml
class IQueueMessage {
  + ContentType: string
  + CorrelationId: string?
  + Payload: object
  + PayloadType: string?
  + Properties: Dictionary<string, object?>
}
@enduml
```

## Sequence Diagram

Here is a sequence diagram showing the interaction between the message sender and receiver:

```plantuml
@startuml
participant MessageSender as sender
participant MessageQueue as queue
participant MessageReceiver as receiver

sender ->> queue: SendAsync(message)
queue ->> receiver: RunAsync()
ALT message received is processed successfully
  receiver ->> sender: CompletedAsync()
  receiver ->> queue: Acknowledge()
else
  receiver ->> sender: FaultedAsync()
  receiver ->> queue: Reject()
end
@enduml
```

This sequence diagram shows the interaction between the message sender, message queue, and message receiver. The sender sends a message to the queue, which is then processed by the receiver. The receiver then sends a completion async result back to the sender, and acknowledges the message if it was processed successfully, or faults the message if there was an error.