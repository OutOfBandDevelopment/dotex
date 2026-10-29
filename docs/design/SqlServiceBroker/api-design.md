# SQL Server Service Broker — API design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft · **Date:** 2026-10-29

## Public surface

Namespace `OoBDev.Microsoft.SqlServer.ServiceBroker`:

```csharp
public static class ServiceBrokerGlobals { public const string MessageProviderKey = "sql-service-broker"; }

public static class ServiceCollectionExtensions
{
    IServiceCollection TryAddServiceBrokerServices(this IServiceCollection services);
    IServiceCollection TryAddServiceBrokerQueueServices(this IServiceCollection services);
}

public class ServiceBrokerQueueMessageProvider : IMessageSenderProvider, IMessageReceiverProvider { … }
```

Registration mirrors RabbitMQ: transient providers, keyed by `MessageProviderKey`, `TryAddEnumerable` for the AsyncAPI contributor.

## Configuration

Per queue, from the message context config: `ConnectionString`, `Queue`, `Service`, `TargetService` (default the same service), `Contract`, `MaxRetries` (default 3), `ReceiveTimeoutSeconds` (default 5), `EnsureObjects` (default false). Option binding follows the other providers; validation of `Queue`, `Service` and `ConnectionString` is required.

## Errors

`ConfigurationMissingException` for missing settings or a database with `is_broker_enabled = 0`; `SqlException` surfaces from send; handler exceptions are logged, rolled back and retried per REQ-005.
