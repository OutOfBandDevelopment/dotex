# OoBDev.PostgreSql.Pgmq

Message-queue adapter for [pgmq](https://github.com/pgmq/pgmq), the PostgreSQL extension that keeps queues as tables. It plugs into the normal `IMessageQueueSender`, `IMessageQueueHandler` and `IMessageContext` pattern (`OoBDev.MessageQueueing`), the same as the RabbitMQ, Azure and AWS adapters.

- `services.TryAddPgmqQueueServices()` registers the providers; the provider key is `pgmq` (`PgmqGlobals.MessageProviderKey`).
- Queue configuration (`MessageQueue:{name}:Config`): `ConnectionString` and `QueueName` (required; the queue is created on first use), `VisibilityTimeoutSeconds` (30), `PollIntervalMilliseconds` (500), `BatchSize` (10).
- Sending calls `pgmq.send`. Receiving calls `pgmq.read` with the visibility timeout; after the handler succeeds the message is archived with `pgmq.archive`; when it throws the message is left and arrives again after the timeout (at-least-once, so handlers are idempotent).
- The database needs `CREATE EXTENSION pgmq` (the test stack image `ghcr.io/pgmq/pg17-pgmq` does it at first start).
- Not yet: an AsyncAPI contributor, dead-lettering after a read-count limit.
