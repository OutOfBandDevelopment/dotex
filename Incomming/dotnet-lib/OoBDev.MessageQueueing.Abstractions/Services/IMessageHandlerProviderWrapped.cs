using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Provides a internal extensions for mechanism for handling queue messages.
/// </summary>
internal interface IMessageHandlerProviderWrapped : IMessageHandlerProvider
{
    /// <summary>
    /// Sets the collection of message queue handlers for the provider.
    /// </summary>
    /// <param name="handlers">The collection of message queue handlers.</param>
    /// <returns>The updated message handler provider.</returns>
    IMessageHandlerProviderWrapped SetHandlers(IEnumerable<IMessageQueueHandler> handlers);

    /// <summary>
    /// Sets the type of the message channel for the provider.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <returns>The updated message handler provider.</returns>
    IMessageHandlerProviderWrapped SetChannelType(Type channelType);

    /// <summary>
    /// Sets the configuration section for the provider.
    /// </summary>
    /// <param name="config">The configuration section.</param>
    /// <returns>The updated message handler provider.</returns>
    IMessageHandlerProviderWrapped SetConfig(IConfigurationSection config);
}
