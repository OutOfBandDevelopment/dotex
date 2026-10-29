# SQL Server Service Broker queue provider

**Date:** 2026-10-29

New adapter `OoBDev.Microsoft.SqlServer.ServiceBroker` (ExternalServices/Microsoft, with `.Tests`): `ServiceBrokerQueueMessageProvider` implements `IMessageSenderProvider` and `IMessageReceiverProvider` under the key `sql-service-broker`, with `TryAddServiceBrokerServices()`, an AsyncAPI contributor (host and queue only, never the connection string) and optional idempotent creation of the message type, contract, queue and services (`EnsureObjects`). Receive runs one transaction per message; a failing handler rolls back and the message returns to the queue, and after `MaxRetries` (1 to 4, because SQL Server disables a queue after five consecutive rollbacks) the conversation is ended with an error. Retry counts are in memory, so a restart resets them.

Request: [docs/features/request/2026-10-28-sql-service-broker-tools.md](../features/request/2026-10-28-sql-service-broker-tools.md), now `done`. Design: [docs/design/SqlServiceBroker](../design/SqlServiceBroker/README.md). Readme: `README.Microsoft.SqlServer.ServiceBroker.md`.

**Verified:** Unit, 21 passed (options, scripts, registration, AsyncAPI). Integration against the test-stack SQL Server, 5 passed: send and receive once, two failures then success, poison ends the conversation and the queue stays enabled, `EnsureObjects` twice, broker disabled throws. Cancelling a `WAITFOR` surfaces as `SqlException` and is treated as a normal stop. Line coverage was not measured.
