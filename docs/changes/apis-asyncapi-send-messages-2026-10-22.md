# AsyncAPI send messages by type

**Date:** 2026-10-22 · **Design:** [AsyncApi](../design/AsyncApi/README.md)

## What changed

- `AddAsyncApiSendMessage<TChannel, TMessage>()` registers a message type the application sends. The document builder adds its channel, a JSON schema and a `send` operation, resolving server and bindings through the same provider contributors as receive channels.
- Queues that remain configuration-only are still listed from the `MessageQueue` section, without a schema.

## Verification

`Build_RegisteredSendMessage_AddsSendOperationWithSchema` (Unit); `OoBDev.AsyncApi.Tests` 10 of 10. The viewer was not checked in a browser (still in BACKLOG).
