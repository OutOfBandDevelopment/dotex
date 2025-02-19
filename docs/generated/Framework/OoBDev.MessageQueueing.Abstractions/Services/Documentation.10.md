**Documentation for WrappedQueueMessage.cs**

## Introduction

The `WrappedQueueMessage` class is used to represent a message that is being processed in a message queueing system. It provides a way to wrap a payload with additional information such as content type, correlation ID, and properties.

## Class Diagram

```plantuml
@startuml
class WrappedQueueMessage {
  - contentType: string
  - correlationId: string
  - payload: object
  - payloadType: string
  - properties: Dictionary<string, object?>
}
WrappedQueueMessage -- implements IQueueMessage
@enduml
```

## Description

### Properties

* `ContentType`: Gets or initializes the content type of the message.
* `CorrelationId`: Gets or initializes the correlation ID of the message.
* `Payload`: Gets or initializes the payload of the message.
* `PayloadType`: Gets or initializes the payload type of the message.
* `Properties`: Gets or initializes the properties associated with the message.

### Implementations

`WrappedQueueMessage` implements the `IQueueMessage` interface, which provides a contract for message queueing messages.

## Sequence Diagram

```plantuml
@startuml
sequenceDiagram
    participant Queue as "Message Queue"
    participant WrappedQueueMessage as "WrappedQueueMessage"

    note left: Create and enqueue message
    WrappedQueueMessage->>Queue: Enqueue(WrappedQueueMessage)

    note right: Process message
    Queue->>WrappedQueueMessage: Dequeue()
    WrappedQueueMessage->>Processor: Process()

    note left: Process message failed
    WrappedQueueMessage->>Processor: Fail()

@enduml
```

In this sequence diagram, we see the creation and enqueuing of a `WrappedQueueMessage` instance, followed by its processing by a processor. If the processing fails, the message is considered failed.

**Class Member Explanation**

* `ContentType` is used to specify the type of content in the message.
* `CorrelationId` is used to track the correlation between different messages.
* `Payload` is the actual message payload.
* `PayloadType` is used to specify the type of payload.
* `Properties` is used to store additional properties associated with the message.