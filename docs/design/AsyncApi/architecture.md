# AsyncApi — Architecture

[← Requirements](requirements.md) · [API design →](api-design.md)

## Contents

- [Components](#components)
- [Discovery flow](#discovery-flow)
- [Adapter contributions](#adapter-contributions)
- [Viewer](#viewer)

## Components

```plantuml
@startuml
skinparam componentStyle rectangle
rectangle "Application" as App <<container>> {
  rectangle "OoBDev.AsyncApi" as Model <<component>> {
    rectangle "AsyncApiDocument model" as M
    rectangle "AsyncApiDocumentBuilder" as B
    rectangle "AsyncApiJsonWriter" as W
  }
  rectangle "OoBDev.AspNetCore.Mvc" as Web <<component>> {
    rectangle "MapAsyncApi" as Map
    rectangle "Viewer page" as V
  }
  rectangle "Queue adapters (Sqs, ServiceBus, RabbitMQ)" as Ad <<component>> {
    rectangle "IAsyncApiContributor" as C
  }
}
rectangle "IConfiguration (MessageQueue)" as Cfg <<config>>
rectangle "IServiceCollection registrations" as Reg <<config>>
B --> Cfg : channels and providers
B --> Reg : handler and sender types
C --> B : server and bindings
B --> M
W --> M
Map --> B
Map --> W
Map --> V
@enduml
```

*Figure 1 — Components*

## Discovery flow

```plantuml
@startuml
participant "MapAsyncApi" as Map
participant "AsyncApiDocumentBuilder" as B
participant "MessageQueue config" as Cfg
participant "IAsyncApiContributor" as C
Map -> B : Build(name)
B -> B : find sender and handler service types
loop each (channel, message) pair
  B -> Cfg : resolve section by the standard order
  B -> C : Describe(providerKey, section)
  C --> B : server, channel address, bindings
  B -> B : add channel, message, operation (send or receive)
end
B --> Map : AsyncApiDocument
Map -> Map : write JSON
@enduml
```

*Figure 2 — Building a document*

The builder takes the registered `IMessageQueueHandler` instances (receive channels) and reads the `MessageQueue` configuration for queues that no handler covers (send channels): `IMessageQueueSender<T>` is an open generic, so senders cannot be enumerated by type. It builds the document on each request from those inputs. Document names match OpenAPI: `all` plus one per assembly that registers handlers or senders, matched case-insensitively.

## Adapter contributions

**Table 1 — Providers**

| Provider key | Server | Channel address | Bindings |
|--------------|--------|-----------------|----------|
| `sqs` | host from `ServiceUrl` or the regional endpoint, protocol `sqs` | `QueueName` or the last segment of `QueueUrl` | `sqs` queue name; FIFO when the name ends `.fifo` (`MessageGroupId`) |
| `servicebus` | namespace host parsed from `ConnectionString` (endpoint only), protocol `amqp` | `QueueName` or `TopicName` | queue or topic; `SessionId` noted |
| `rabbit-mq` | `HostName`:`Port`, protocol `amqp` | `QueueName` | `amqp` queue |
| `in-process` | none (local) | message simple name | none |

Contributors live in the adapter projects, implement `IAsyncApiContributor` from `OoBDev.AsyncApi`, and are registered by each adapter's `TryAdd...` method with `TryAddEnumerable`. Adapters therefore reference only the small model project, and the Abstractions projects are untouched.

## Viewer

`/asyncapi/{name}` returns a small HTML page that loads the AsyncAPI web component from a pinned CDN version and points it at `/asyncapi/{name}.json`. The CDN URL is a setting so offline installs can serve the script locally.

[← Requirements](requirements.md) · [API design →](api-design.md)
