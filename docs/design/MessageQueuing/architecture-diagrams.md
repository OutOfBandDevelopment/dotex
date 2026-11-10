# Message Queue Architecture Diagrams

**Date:** 2026-01-20
**Pattern:** Context-Based Message Queue Provider Architecture

---

## Component Architecture

### High-Level Component Diagram

```plantuml
@startuml Message Queue Component Architecture

!define FRAMEWORK_COLOR #E1F5FE
!define PROVIDER_COLOR #FFF3E0
!define APPLICATION_COLOR #F3E5F5
!define EXTERNAL_COLOR #E8F5E9

package "Application Layer" APPLICATION_COLOR {
    component [OrderService] as OrderSvc
    component [PaymentService] as PaymentSvc
    component [NotificationService] as NotifySvc
}

package "OoBDev.MessageQueueing.Abstractions" FRAMEWORK_COLOR {
    interface IMessageSenderProvider
    interface IMessageReceiverProvider
    interface IMessageContext
    interface IMessageContextFactory
    interface IMessageSenderProviderFactory
}

package "OoBDev.MessageQueueing" FRAMEWORK_COLOR {
    component [MessageContextFactory]
    component [MessageSenderProviderFactory]
}

package "OoBDev.System" FRAMEWORK_COLOR {
    interface IJsonSerializer
    interface ILogger
    interface IConfiguration
}

package "Provider: RabbitMQ" PROVIDER_COLOR {
    component [RabbitMQQueueMessageProvider] as RabbitMQ
    component [QueueClientFactory] as RabbitFactory
    interface IQueueClientFactory as RabbitIFactory
}

package "Provider: AWS SQS" PROVIDER_COLOR {
    component [AmazonSqsMessageProvider] as SQS
    component [SqsClientFactory] as SqsFactory
    interface ISqsClientFactory as SqsIFactory
}

package "Provider: Azure Service Bus" PROVIDER_COLOR {
    component [AzureServiceBusMessageProvider] as ServiceBus
    component [ServiceBusClientFactory] as SbFactory
    interface IServiceBusClientFactory as SbIFactory
}

package "External Services" EXTERNAL_COLOR {
    component [RabbitMQ Server] as RabbitMQExt
    component [AWS SQS] as SQSExt
    component [Azure Service Bus] as ServiceBusExt
}

' Application dependencies
OrderSvc ..> IMessageSenderProvider : uses
OrderSvc ..> IMessageContextFactory : uses
PaymentSvc ..> IMessageSenderProvider : uses
NotifySvc ..> IMessageSenderProviderFactory : uses

' Framework implementations
MessageContextFactory ..|> IMessageContextFactory
MessageSenderProviderFactory ..|> IMessageSenderProviderFactory

' Provider implementations
RabbitMQ ..|> IMessageSenderProvider
RabbitMQ ..|> IMessageReceiverProvider
RabbitMQ ..> RabbitIFactory : uses
RabbitFactory ..|> RabbitIFactory

SQS ..|> IMessageSenderProvider
SQS ..> SqsIFactory : uses
SqsFactory ..|> SqsIFactory

ServiceBus ..|> IMessageSenderProvider
ServiceBus ..> SbIFactory : uses
SbFactory ..|> SbIFactory

' Provider dependencies on framework
RabbitMQ ..> IJsonSerializer : uses
RabbitMQ ..> ILogger : uses
SQS ..> IJsonSerializer : uses
SQS ..> ILogger : uses
ServiceBus ..> IJsonSerializer : uses
ServiceBus ..> ILogger : uses

RabbitFactory ..> IConfiguration : uses
SqsFactory ..> IConfiguration : uses
SbFactory ..> IConfiguration : uses

' External communication
RabbitMQ --> RabbitMQExt : sends/receives
SQS --> SQSExt : sends/receives
ServiceBus --> ServiceBusExt : sends/receives

note right of IMessageSenderProvider
  Non-generic interface:
  SendAsync(object message,
            IMessageContext context)
end note

note right of IMessageContext
  Contains:
  - CorrelationId
  - Headers (Dictionary)
  - Config (IConfigurationSection)
end note

@enduml
```

---

## Sequence Diagrams

### Send Message Flow (Generic)

```plantuml
@startuml Send Message Sequence

actor Application
participant "OrderService" as Service
participant "IMessageContextFactory" as ContextFactory
participant "IMessageContext" as Context
participant "IMessageSenderProvider\n(RabbitMQ/SQS/ServiceBus)" as Provider
participant "IQueueClientFactory" as ClientFactory
participant "IJsonSerializer" as Serializer
participant "External Queue\n(RabbitMQ/SQS/ServiceBus)" as Queue

Application -> Service : ProcessOrder(order)
activate Service

Service -> ContextFactory : Create("OrderQueue", typeof(Order))
activate ContextFactory
ContextFactory --> Service : context
deactivate ContextFactory

Service -> Context : Headers["Priority"] = "High"
Service -> Context : CorrelationId = order.Id

Service -> Provider : SendAsync(order, context)
activate Provider

Provider -> ClientFactory : CreateAsync(context.Config)
activate ClientFactory
ClientFactory -> ClientFactory : Read config:\n- QueueName\n- ConnectionString\n- Provider-specific settings
ClientFactory -> Queue : Create connection
ClientFactory --> Provider : (connection, client, queueName)
deactivate ClientFactory

Provider -> Provider : Wrap message:\n- PayloadType\n- CorrelationId\n- Headers\n- Payload

Provider -> Serializer : SerializeAsync(wrappedMessage)
activate Serializer
Serializer --> Provider : messageBytes
deactivate Serializer

Provider -> Queue : Send message with:\n- Body (JSON)\n- Attributes/Headers\n- Provider-specific options
activate Queue
Queue --> Provider : messageId
deactivate Queue

Provider --> Service : messageId
deactivate Provider

Service --> Application : Order processed
deactivate Service

@enduml
```

### Receive Message Flow (Generic)

```plantuml
@startuml Receive Message Sequence

participant "BackgroundService" as BgService
participant "IMessageReceiverProvider\n(RabbitMQ/SQS/ServiceBus)" as Provider
participant "IMessageHandlerProvider" as Handler
participant "IQueueClientFactory" as ClientFactory
participant "IJsonSerializer" as Serializer
participant "External Queue\n(RabbitMQ/SQS/ServiceBus)" as Queue
participant "OrderProcessor" as Processor

BgService -> Provider : SetHandlerProvider(handler)
activate Provider
Provider -> Provider : Store handler reference
Provider --> BgService
deactivate Provider

BgService -> Provider : RunAsync(cancellationToken)
activate Provider

Provider -> ClientFactory : CreateAsync(config)
activate ClientFactory
ClientFactory --> Provider : (connection, client, queueName)
deactivate ClientFactory

loop Until Cancelled
    Provider -> Queue : Receive message
    activate Queue
    Queue --> Provider : messageBytes, attributes
    deactivate Queue

    Provider -> Serializer : DeserializeAsync<WrappedQueueMessage>(messageBytes)
    activate Serializer
    Serializer --> Provider : wrappedMessage
    deactivate Serializer

    Provider -> Provider : Extract:\n- Payload\n- CorrelationId\n- Headers

    Provider -> Handler : HandleAsync(wrappedMessage, correlationId)
    activate Handler

    Handler -> Processor : ProcessOrder(order)
    activate Processor
    Processor --> Handler : completed
    deactivate Processor

    Handler --> Provider : Task completed
    deactivate Handler

    Provider -> Queue : Acknowledge message
    activate Queue
    Queue --> Provider : acknowledged
    deactivate Queue
end

deactivate Provider

@enduml
```

### Multi-Provider Bridge Flow

```plantuml
@startuml Multi-Provider Bridge

actor Application
participant "BridgeService" as Bridge
participant "SQS Receiver\n(IMessageReceiverProvider)" as SqsReceiver
participant "RabbitMQ Sender\n(IMessageSenderProvider)" as RabbitSender
participant "IMessageContextFactory" as ContextFactory
participant "AWS SQS" as SQS
participant "RabbitMQ Server" as RabbitMQ

Application -> Bridge : Start bridge service
activate Bridge

Bridge -> SqsReceiver : SetHandlerProvider(bridgeHandler)
Bridge -> SqsReceiver : RunAsync()
activate SqsReceiver

loop Until Cancelled
    SqsReceiver -> SQS : Receive message
    activate SQS
    SQS --> SqsReceiver : message
    deactivate SQS

    SqsReceiver -> SqsReceiver : Deserialize message

    SqsReceiver -> Bridge : HandleAsync(message, correlationId)
    activate Bridge

    Bridge -> ContextFactory : Create("RabbitMQ-Target-Queue", messageType)
    activate ContextFactory
    ContextFactory --> Bridge : rabbitContext
    deactivate ContextFactory

    Bridge -> Bridge : Copy headers\nAdd bridge metadata

    Bridge -> RabbitSender : SendAsync(payload, rabbitContext)
    activate RabbitSender

    RabbitSender -> RabbitMQ : Send message
    activate RabbitMQ
    RabbitMQ --> RabbitSender : messageId
    deactivate RabbitMQ

    RabbitSender --> Bridge : messageId
    deactivate RabbitSender

    Bridge --> SqsReceiver : Success
    deactivate Bridge

    SqsReceiver -> SQS : Acknowledge
end

deactivate SqsReceiver

@enduml
```

### Factory Pattern Details

```plantuml
@startuml Factory Pattern

participant "MessageProvider" as Provider
participant "IQueueClientFactory" as Factory
participant "IConfigurationSection" as Config
participant "Provider SDK\n(RabbitMQ.Client/\nAWS SDK/\nAzure SDK)" as SDK

Provider -> Factory : CreateAsync(context.Config)
activate Factory

Factory -> Config : Read required settings
activate Config

alt RabbitMQ
    Config --> Factory : HostName, Port, QueueName
    Factory -> SDK : new ConnectionFactory()
    Factory -> SDK : CreateConnectionAsync()
    activate SDK
    SDK --> Factory : IConnection
    Factory -> SDK : CreateChannelAsync()
    SDK --> Factory : IChannel
    deactivate SDK

else AWS SQS
    Config --> Factory : QueueUrl, Region, AccessKey
    Factory -> SDK : new AmazonSQSClient()
    activate SDK
    SDK --> Factory : IAmazonSQS
    deactivate SDK
    Factory -> SDK : GetQueueUrlAsync(queueName)
    activate SDK
    SDK --> Factory : queueUrl
    deactivate SDK

else Azure Service Bus
    Config --> Factory : ConnectionString, QueueName/TopicName
    Factory -> SDK : new ServiceBusClient()
    activate SDK
    SDK --> Factory : ServiceBusClient
    deactivate SDK
    Factory -> SDK : CreateSender(queueName)
    activate SDK
    SDK --> Factory : ServiceBusSender
    deactivate SDK
end

deactivate Config

Factory --> Provider : (connection, client, queueName)
deactivate Factory

@enduml
```

---

## Dependency Injection Flow

```plantuml
@startuml Dependency Injection

package "Startup/Program.cs" {
    component [ServiceCollection] as SC
}

package "Service Registration Extensions" {
    component [TryAddRabbitMQServices]
    component [TryAddAmazonSqsServices]
    component [TryAddAzureServiceBusServices]
}

package "Registered Services" {
    component [IMessageSenderProvider\n(non-keyed)] as Sender1
    component [IMessageSenderProvider\n(keyed: "rabbitmq")] as Sender2
    component [IMessageSenderProvider\n(keyed: "sqs")] as Sender3
    component [IMessageSenderProvider\n(keyed: "servicebus")] as Sender4
    component [IMessageContextFactory] as Factory1
    component [IJsonSerializer] as Json
}

package "Application Services" {
    component [OrderService] as App1
    component [BridgeService] as App2
}

SC -> TryAddRabbitMQServices : services.TryAddRabbitMQServices()
SC -> TryAddAmazonSqsServices : services.TryAddAmazonSqsServices()
SC -> TryAddAzureServiceBusServices : services.TryAddAzureServiceBusServices()

TryAddRabbitMQServices --> Sender1 : AddTransient<IMessageSenderProvider,\nRabbitMQQueueMessageProvider>()
TryAddRabbitMQServices --> Sender2 : AddKeyedTransient<IMessageSenderProvider,\nRabbitMQQueueMessageProvider>("rabbitmq")

TryAddAmazonSqsServices --> Sender3 : AddKeyedTransient<IMessageSenderProvider,\nAmazonSqsMessageProvider>("sqs")

TryAddAzureServiceBusServices --> Sender4 : AddKeyedTransient<IMessageSenderProvider,\nAzureServiceBusMessageProvider>("servicebus")

SC --> Factory1 : TryAddSingleton<IMessageContextFactory>
SC --> Json : TryAddSingleton<IJsonSerializer>

App1 ..> Sender1 : Inject (first registered)
App1 ..> Factory1 : Inject

App2 ..> Sender2 : Inject [FromKeyedServices("rabbitmq")]
App2 ..> Sender3 : Inject [FromKeyedServices("sqs")]
App2 ..> Sender4 : Inject [FromKeyedServices("servicebus")]

note right of App1
  Simple injection:
  Gets first registered provider
end note

note right of App2
  Multi-provider injection:
  Uses keyed services to get
  specific providers
end note

@enduml
```

---

## Configuration Flow

```plantuml
@startuml Configuration Flow

package "appsettings.json" {
    component [MessageQueuing] as Config
    component [AWS] as AwsConfig
    component [Azure] as AzureConfig
}

package "IConfiguration" {
    component [ConfigurationRoot] as Root
    component [IConfigurationSection] as Section
}

package "MessageContext" {
    component [IMessageContext] as Context
    component [Config Property] as ContextConfig
}

package "Provider Factory" {
    component [QueueClientFactory] as Factory
}

Config --> Root : Loaded at startup
AwsConfig --> Root
AzureConfig --> Root

Root --> Section : GetSection("MessageQueuing:OrderQueue")

Section --> Context : Set as context.Config

Context --> Factory : Passed to CreateAsync(context.Config)

note right of Config
  {
    "MessageQueuing": {
      "OrderQueue": {
        "Provider": "sqs",
        "QueueUrl": "https://sqs...",
        "MessageGroupId": "orders"
      }
    }
  }
end note

note right of Factory
  Reads from IConfigurationSection:
  - Required: QueueUrl/QueueName
  - Optional: Provider-specific settings
  - Throws ConfigurationMissingException
    if required settings missing
end note

@enduml
```

---

## Message Structure

```plantuml
@startuml Message Structure

class Order {
    +int Id
    +decimal Total
    +string CustomerId
}

class WrappedQueueMessage {
    +string ContentType
    +string PayloadType
    +string CorrelationId
    +object Payload
    +Dictionary<string, object?> Properties
}

class "IMessageContext" {
    +string? CorrelationId
    +Dictionary<string, object?> Headers
    +IConfigurationSection Config
    +string? OriginMessageId
    +string? MessageType
    +DateTimeOffset? SentAt
}

class "Queue Message\n(JSON Bytes)" {
    +byte[] Body
    +MessageAttributes
    +Headers/Properties
}

Order --> WrappedQueueMessage : Wrapped as Payload

WrappedQueueMessage --> IJsonSerializer : Serialized to JSON

IJsonSerializer --> "Queue Message\n(JSON Bytes)" : byte[]

IMessageContext ..> WrappedQueueMessage : Headers copied to Properties

note right of WrappedQueueMessage
  Envelope pattern:
  - Standard message wrapper
  - Contains metadata + payload
  - Type information for deserialization
  - Correlation tracking
end note

note bottom of "Queue Message\n(JSON Bytes)"
  Provider-specific format:
  - SQS: MessageBody + MessageAttributes
  - Service Bus: Body + ApplicationProperties
  - RabbitMQ: Body + Headers
end note

@enduml
```

---

## Error Handling Flow

```plantuml
@startuml Error Handling

participant "Application" as App
participant "MessageProvider" as Provider
participant "Factory" as Factory
participant "IConfigurationSection" as Config
participant "External Queue" as Queue
participant "ILogger" as Logger

App -> Provider : SendAsync(message, context)
activate Provider

Provider -> Factory : CreateAsync(context.Config)
activate Factory

Factory -> Config : Read "QueueUrl"
activate Config

alt Configuration Missing
    Config --> Factory : null
    deactivate Config
    Factory -> Factory : throw ConfigurationMissingException(\n  "MessageQueuing:OrderQueue:QueueUrl")
    Factory --> Provider : ConfigurationMissingException
    Provider -> Logger : LogError(exception)
    Provider --> App : Exception propagated

else Configuration Valid
    Config --> Factory : "https://sqs..."
    deactivate Config

    Factory -> Queue : Create client
    activate Queue

    alt Connection Failed
        Queue --> Factory : Exception
        deactivate Queue
        Factory --> Provider : Exception
        Provider -> Logger : LogError("Failed to create queue client")
        Provider --> App : Exception propagated

    else Connection Successful
        Queue --> Factory : client
        deactivate Queue
        Factory --> Provider : (connection, client, queueName)
        deactivate Factory

        Provider -> Queue : SendMessageAsync()
        activate Queue

        alt Send Failed
            Queue --> Provider : Exception
            deactivate Queue
            Provider -> Logger : LogError("Failed to send message")
            Provider --> App : Exception propagated

        else Send Successful
            Queue --> Provider : messageId
            deactivate Queue
            Provider -> Logger : LogInformation("Message sent: {messageId}")
            Provider --> App : messageId
        end
    end
end

deactivate Provider

@enduml
```

---

## Testing Architecture

```plantuml
@startuml Testing Architecture

package "Unit Tests" {
    component [ProviderTests] as UnitTests
    component [FactoryTests] as FactoryTests
    component [MockJsonSerializer] as MockJson
    component [MockConfiguration] as MockConfig
    component [MockQueueClient] as MockClient
}

package "Integration Tests\n(DevLocal)" {
    component [DevLocalTests] as DevTests
    component [RealJsonSerializer] as RealJson
    component [RealConfiguration] as RealConfig
    component [LocalQueue] as LocalQ
}

package "Integration Tests\n(Docker)" {
    component [DockerTests] as DockerTests
    component [DockerCompose] as Docker
    component [RabbitMQ Container] as RabbitDocker
    component [LocalStack Container] as LocalStack
}

package "System Under Test" {
    component [MessageProvider] as SUT
    component [QueueClientFactory] as SutFactory
}

UnitTests ..> MockJson : Inject
UnitTests ..> MockConfig : Inject
UnitTests ..> SUT : Test
FactoryTests ..> MockConfig : Inject
FactoryTests ..> SutFactory : Test

DevTests ..> RealJson : Use
DevTests ..> RealConfig : Load from appsettings
DevTests ..> SUT : Test
DevTests ..> LocalQ : Connect to local queue

DockerTests ..> RealJson : Use
DockerTests ..> RealConfig : Load from appsettings
DockerTests ..> SUT : Test
DockerTests ..> Docker : Start containers
Docker --> RabbitDocker : RabbitMQ on port 5672
Docker --> LocalStack : SQS on port 4566

note right of UnitTests
  Fast, isolated tests
  - Mock all dependencies
  - No external services
  - Verify behavior
end note

note right of DevTests
  Manual testing
  - Requires local services
  - Real serialization
  - Real queue operations
end note

note right of DockerTests
  CI/CD integration
  - Docker-based services
  - Automated in pipeline
  - Full stack testing
end note

@enduml
```

---

## Provider Comparison Matrix

```plantuml
@startmindmap Provider Features

* Message Queue\nProviders

** RabbitMQ
*** Exchanges
**** Direct
**** Fanout
**** Topic
**** Headers
*** Routing Keys
*** TTL
*** Priority
*** Persistence
*** Prefetch

** AWS SQS
*** Standard Queue
*** FIFO Queue
*** Message Groups
*** Deduplication
*** Delay Queues
*** Dead Letter
*** Visibility Timeout
*** Long Polling

** Azure Service Bus
*** Queues
*** Topics
*** Subscriptions
*** Sessions
*** Partitioning
*** Scheduled Messages
*** Dead Letter
*** Transactions
*** Auto-forwarding

@endmindmap
```

---

## Usage Notes

### Rendering PlantUML Diagrams

These diagrams use PlantUML syntax and can be rendered using:

1. **PlantUML Online Server:** http://www.plantuml.com/plantuml/uml/
2. **VS Code Extension:** PlantUML extension by jebbs
3. **IntelliJ/Rider Plugin:** PlantUML integration
4. **Command Line:** `plantuml architecture-diagrams.md`

### Diagram Purposes

| Diagram | Purpose | Audience |
|---------|---------|----------|
| **Component Architecture** | Overall system structure | Architects, developers |
| **Send Message Sequence** | Message sending flow | Developers implementing senders |
| **Receive Message Sequence** | Message receiving flow | Developers implementing receivers |
| **Multi-Provider Bridge** | Bridge pattern example | Integration developers |
| **Factory Pattern Details** | Factory implementation | Provider implementers |
| **Dependency Injection** | DI setup | Application developers |
| **Configuration Flow** | Config loading | DevOps, developers |
| **Message Structure** | Message format | All developers |
| **Error Handling** | Exception flow | Developers, support |
| **Testing Architecture** | Test strategy | QA, developers |
| **Provider Features** | Feature comparison | Architects, decision makers |

---

## See Also

- [Pattern Context-Based](./pattern-context-based.md) - Detailed pattern documentation
- [Migration Checklist](./migration-checklist.md) - Step-by-step migration guide
- [README](./README.md) - Overview and quick start

---

**Last Updated:** 2026-01-20
**Format:** PlantUML
**Status:** Active Documentation
