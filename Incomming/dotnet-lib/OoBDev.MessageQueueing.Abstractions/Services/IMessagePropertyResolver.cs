using Microsoft.Extensions.Configuration;
using System;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Resolves properties related to message handling, such as provider, message ID, and configuration.
/// </summary>
public interface IMessagePropertyResolver
{
    /// <summary>
    /// Retrieves provider information for the specified message channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>A tuple containing provider key, simple target name, simple message name, and configuration path.</returns>
    (string? providerKey, string simpleTargetName, string simpleMessageName, string? configPath) ProviderSafe(Type channelType, Type messageType);

    /// <summary>
    /// Retrieves the provider key for the specified message channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>The provider key.</returns>
    string Provider(Type channelType, Type messageType);

    /// <summary>
    /// Retrieves the message ID for the specified message channel, message type, and optional original message ID.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="messageId">The original message ID (optional).</param>
    /// <returns>The resolved message ID.</returns>
    string MessageId(Type channelType, Type messageType, string? messageId);

    /// <summary>
    /// Generates a unique ID for the specified message channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>The generated unique ID.</returns>
    string GenerateId(Type channelType, Type messageType);

    /// <summary>
    /// Retrieves configuration information for the specified message channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>A tuple containing the configuration section, simple target name, simple message name, and configuration path.</returns>
    (IConfigurationSection? configurationSection, string simpleTargetName, string simpleMessageName, string? configPath) ConfigurationSafe(Type channelType, Type messageType);

    /// <summary>
    /// Retrieves the configuration section for the specified message channel and message types.
    /// </summary>
    /// <param name="channelType">The type of the message channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>The configuration section.</returns>
    IConfigurationSection Configuration(Type channelType, Type messageType);
}
