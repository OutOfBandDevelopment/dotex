# OoBDev.Communications.MessageQueueing

Queue handlers for communications.

## EmailMessageHandler

`IMessageQueueHandler<EmailMessageModel, EmailMessageModel>`. It maps the queued `EmailMessageModel` to an `EmailMessage`
(`ReferenceId`, else the queue correlation id, becomes the `MessageId`, so a redelivered message is not sent twice) and sends it with
`ISendService` on the channel named in `OoBDev:Communications:EmailQueue:ChannelName` (default `email`; the channel itself is declared under
`OoBDev:Communications:Channels`, see `README.Communications.md`). A failed send throws so the queue retries or dead-letters the message.
Without a registered `ISendService` the handler logs and drops the message. Attachments are not mapped yet.

## ServiceCollectionExtensions

`TryAddCommunicationQueueServices()` registers the handler and binds `EmailMessageHandlerOptions`. The host also calls `TryAddSendReceive()`
and registers a channel provider (for example `OoBDev.MailKit`).
