# OoBDev.MessageQueueing

## MessageSender`1

Represents a message sender for a specific communication channel.

### Methods

#### Constructor

Initializes a new instance of the class.

##### Parameters

- *context*: The message context factory.
- *provider*: The message sender provider factory.
- *resolver*: The message property resolver.
- *logger*: The logger.

#### SendAsync(System.Object,System.String)

Sends a message asynchronously to the specified communication channel.

##### Parameters

- *message*: The message to be sent.
- *correlationId*: The correlation ID associated with the message (optional).

##### Return value

The ID of the sent message.

## ServiceCollectionExtensions

Provides extension methods for configuring IoC (Inversion of Control) services for Message Queueing within this library.

### Methods

#### TryAddMessageQueueingServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)

Configures services for Message Queueing within this library.

##### Parameters

- *services*: The service collection to add the services to.

##### Return value

The modified service collection.

## Services

### InProcessMessageProvider

Represents an in-process message provider that implements both IMessageSenderProvider and IMessageReceiverProvider.

#### Fields

- **MessageProviderKey**: Gets the key associated with the in-process message provider.

### MessageContext

Represents the context associated with a message, including metadata and headers.

### MessageContextFactory

Factory for creating instances of IMessageContext.

### MessageHandlerProvider

Provides handling of queue messages by coordinating multiple IMessageQueueHandler instances.

### MessagePropertyResolver

Utility class for resolving properties related to message queue handling.

### MessageReceiverProviderFactory

Factory for creating instances of IMessageReceiverProvider based on configured message handlers.

### MessageSenderProviderFactory

Factory for creating instances of IMessageSenderProvider based on channel and message types.
