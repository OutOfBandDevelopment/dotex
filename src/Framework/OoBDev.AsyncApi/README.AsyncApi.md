# OoBDev.AsyncApi

Builds an AsyncAPI 3.0 document for the message queue surfaces from the registered `IMessageQueueHandler<TChannel,TMessage>` types and the `MessageQueue` configuration. Queue adapters describe their server and channel by implementing `IAsyncApiContributor`; credentials are never emitted. Serve it with `MapAsyncApi()` from `OoBDev.AspNetCore.Mvc`.

```csharp
services.TryAddAsyncApiServices();
var json = AsyncApiJsonWriter.Write(provider.GetRequiredService<AsyncApiDocumentBuilder>().Build("all")!);
```

Queues that only send (`IMessageQueueSender<T>` is an open generic, so it cannot be enumerated) appear when they are configured under `MessageQueue` and no handler covers them.

Design: [AsyncApi](../../../docs/design/AsyncApi/README.md).
