Create documentation for the follow source code. 

The files should be in markdown.
When reasonable include class diagrams, component models and sequence diagrams in Plant UML.
PlantUML blocks should all be identified with "```plantuml" and closed with "```"

## Source Files

```IMessageReceiverProvider.cs
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Provides functionality for receiving messages from a message queue.
/// </summary>
public interface IMessageReceiverProvider
{
    /// <summary>
    /// Sets the handler provider for processing received messages.
    /// </summary>
    /// <param name="handlerProvider">The message handler provider.</param>
    /// <returns>The current instance of the message receiver provider.</returns>
    IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider);

    /// <summary>
    /// Runs the message receiver asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(CancellationToken cancellationToken = default);
}

```

```IMessageReceiverProviderFactory.cs
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Factory for creating instances of <see cref="IMessageReceiverProvider"/>.
/// </summary>
public interface IMessageReceiverProviderFactory
{
    /// <summary>
    /// Creates a collection of message receiver providers.
    /// </summary>
    /// <returns>An enumerable collection of message receiver providers.</returns>
    IEnumerable<IMessageReceiverProvider> Create();
}

```

```IMessageSenderProvider.cs
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents a provider for sending messages to a message queue.
/// </summary>
public interface IMessageSenderProvider
{
    /// <summary>
    /// Sends a message asynchronously to the message queue.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="context">The context associated with the message.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the unique identifier assigned to the sent message.</returns>
    Task<string?> SendAsync(object message, IMessageContext context);
}

```

```IMessageSenderProviderFactory.cs
using System;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents a factory for creating instances of <see cref="IMessageSenderProvider"/>.
/// </summary>
public interface IMessageSenderProviderFactory
{
    /// <summary>
    /// Creates an instance of <see cref="IMessageSenderProvider"/> for the specified channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>An instance of <see cref="IMessageSenderProvider"/>.</returns>
    IMessageSenderProvider Sender(Type channelType, Type messageType);
}

```

```IQueueMessage.cs
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;
/// <summary>
/// Represents a message within a message queue.
/// </summary>
public interface IQueueMessage
{
    /// <summary>
    /// Gets the content type of the message.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Gets the correlation identifier of the message.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Gets the payload object of the message.
    /// </summary>
    object Payload { get; }

    /// <summary>
    /// Gets the type of the payload object.
    /// </summary>
    string? PayloadType { get; }

    /// <summary>
    /// Gets the properties associated with the message.
    /// </summary>
    Dictionary<string, object?> Properties { get; }
}

```

