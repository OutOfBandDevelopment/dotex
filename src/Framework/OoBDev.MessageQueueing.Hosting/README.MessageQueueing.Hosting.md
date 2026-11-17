# OoBDev.MessageQueueing.Hosting

Hosted service that runs every message receiver provider of the application, so inbound queue handlers start with the host and stop with it.

## Use

```csharp
services.TryAddMessageQueueingHosting();
```

This adds `MessageReceiverHost` as a hosted service. It needs `IMessageReceiverProviderFactory` (registered by `TryAddMessageQueueingServices()` in [OoBDev.MessageQueueing](../OoBDev.MessageQueueing/README.MessageQueueing.md), which also decides per channel which keyed receiver provider runs). Queue adapters such as `OoBDev.Amazon.Sqs` and `OoBDev.Microsoft.Azure.ServiceBus` add their receiver providers.

## How it behaves

- `StartAsync` asks the factory for all providers and runs each one's `RunAsync` on its own task. It throws `InvalidOperationException` when called twice without a stop in between.
- When a provider throws, the host logs the error (message at error level, exception at debug level), waits `MessageReceiverHostOptions.RestartDelay` (default 10 seconds; bind it with `services.Configure<MessageReceiverHostOptions>(...)`) and runs the provider again. When `RunAsync` returns without an exception it is called again straight away, so a provider must run until its token is cancelled.
- `StopAsync` and `Dispose` cancel the shared token; `StopAsync` also waits for every provider task to finish.
- Start, stop, restart and failure messages use `[LoggerMessage]` event ids 1 to 11.

The host is covered by `MessageReceiverHostTests` in `OoBDev.MessageQueueing.Tests`.

## Outbox dispatcher

`services.TryAddOutboxDispatcherHosting()` adds `OutboxDispatcherHost`, a background service that calls `OutboxDispatcher.DispatchAsync` and waits `OutboxDispatcherHostOptions.PollInterval` (default 5 seconds) when nothing was sent. A failed send is logged and retried on the next run; entries stay in the store until a send succeeds. Register the real `IMessageQueueSender` for the dispatcher and a durable `IOutboxStore` for production (the in-memory store is the default). Covered by `OutboxDispatcherHostTests`.
