# OoBDev.Microsoft.SqlServer.ServiceBroker

Queue provider for SQL Server Service Broker. It implements `IMessageSenderProvider` and `IMessageReceiverProvider`, so a queue is selected by configuration like the RabbitMQ, SQS and Service Bus providers.

## Description

```csharp
services.TryAddServiceBrokerServices();
```

Provider key: `sql-service-broker` (`ServiceBrokerGlobals.MessageProviderKey`). Settings go under the queue's `Config` section:

| Key | Default | Meaning |
|-----|---------|---------|
| `ConnectionString` | required | Database that holds the queue |
| `QueueName` | required | Queue, optionally `schema.name`; letters, digits and `_` only |
| `ServiceName` | required | Service bound to the queue; the sending service |
| `TargetServiceName` | `ServiceName` | Service that receives |
| `ContractName` | `//OoBDev/MessageQueueing/Contract` | Dialog contract |
| `MaxRetries` | 3 (1 to 4) | Failed handler runs before the conversation is ended with an error |
| `ReceiveTimeoutSeconds` | 5 | One `WAITFOR (RECEIVE ...)` wait |
| `EnsureObjects` | false | Create the message type, contract, queue and services when missing |

Behavior:

- Send opens a dialog, sends the same JSON envelope (`WrappedQueueMessage`) as the other providers and ends the sending side, in one transaction.
- Receive runs one transaction per message. A handler that completes commits, so the body is delivered once. A handler that throws rolls back and the message returns to the queue; at `MaxRetries` failures the conversation is ended with an error and logged. SQL Server disables a queue after five consecutive rollbacks, hence the limit of four. Failure counts are kept in memory, so a restart starts counting again.
- Service Broker must be enabled for the database (`ALTER DATABASE ... SET ENABLE_BROKER`); the provider checks and throws `InvalidOperationException` otherwise. Ordering across messages is not guaranteed beyond one conversation, and each send uses its own conversation.
- The AsyncAPI contributor lists the server name and queue only; the connection string is never copied.

Tests: Unit tests cover options, scripts, registration and the contributor; the send/receive round trip is Integration (SQL Server container, `SQLSERVER_CONNECTION_STRING`).

Design: [docs/design/SqlServiceBroker](../../../../docs/design/SqlServiceBroker/README.md).
