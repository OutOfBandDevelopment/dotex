# OoBDev.Data.Vectors.Hosting

Hosted service that answers embedding requests sent through a SQL Server Service Broker queue, so a database can ask for the embedding of a text without calling out itself.

## Registration

```csharp
services.TryAddVectorHosting(configuration);   // section name is required in DEBUG builds
```

This binds `EmbeddingSentenceTransformerQueueReaderOptions` from the `EmbeddingSentenceTransformerQueueReaderOptions` section (or the name passed), registers the transient `IEmbeddingSentenceTransformerQueueReader` and adds `EmbeddingSentenceTransformerQueueReaderHost` as a hosted service. An `IEmbeddingProvider`, `IJsonSerializer`, `IXmlSerializer` and `IDatabaseQuery<>` must be registered by the application, and the connection string is read from `ConnectionStrings:EmbeddingSentenceTransformer`.

```json
{
  "EmbeddingSentenceTransformerQueueReaderOptions": { "MaximumReadLength": 100, "ReadWaitTimeout": "00:05:00" },
  "ConnectionStrings": { "EmbeddingSentenceTransformer": "Server=...;Database=...;" }
}
```

**Table 1 — Queue reader options**

| Option | Default | Use |
|--------|---------|-----|
| `MaximumReadLength` | 100 | Messages received per `RECEIVE`. |
| `ReadWaitTimeout` | 5 minutes | `WAITFOR ... TIMEOUT` on the queue, and the pause after a failed batch. |
| `ErrorWaitTimeout` | 30 seconds | Declared but not used yet; the reader pauses for `ReadWaitTimeout` after an error. |

## Message flow

The reader receives from `[embedding].[oobdev://embedding/sentence-transformer/queue]` in a transaction and handles each message by type:

- `oobdev://embedding/sentence-transformer/request`: an XML element with `id`, `value` and `tableName` attributes; the reader generates the embedding of `value` and sends an `oobdev://embedding/sentence-transformer/response` element on the same conversation carrying the same attributes plus `embedding` (a JSON array).
- the Service Broker `EndDialog` message: ends the conversation.
- anything else: the batch fails, rolls back and is retried after the pause.

The host restarts the reader after an unhandled error, waiting 10 seconds (a TODO in the code to make it configurable), and cancels it on stop.

The queue, contracts and message types are created by the database project that ships with the vector store (`OoBDev.Data.Vectors.DB`).
