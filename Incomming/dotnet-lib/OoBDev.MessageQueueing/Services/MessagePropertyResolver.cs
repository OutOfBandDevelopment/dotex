using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Utility class for resolving properties related to message queue handling.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MessagePropertyResolver"/> class with the specified configuration.
/// </remarks>
/// <param name="configuration">The configuration used for resolving message queue properties.</param>
public class MessagePropertyResolver(IConfiguration configuration) : IMessagePropertyResolver
{
    private const string ConfigRootKey = "MessageQueue";
    private const string Default = nameof(Default);

    private static (string target, string message) GetSimpleNames(Type channelType, Type messageType) =>
        (
            channelType == typeof(object) ? Default :
                channelType.GetCustomAttribute<MessageQueueAttribute>()?.SimpleName ?? channelType.Name,
            messageType.GetCustomAttribute<MessageQueueAttribute>()?.SimpleName ?? messageType.Name
        );

    /// <summary>
    /// Gets the root configuration section along with simple target and message names.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>
    /// A tuple containing the root configuration section, simple target name, simple message name, and the configuration path.
    /// </returns>
    private (IConfigurationSection? config, string simpleTargetName, string simpleMessageName, string? configPath) RootConfiguration(Type channelType, Type messageType)
    {
        var (simpleTargetName, simpleMessageName) = GetSimpleNames(channelType, messageType);

        var keys = new[]
        {
            $"{ConfigRootKey}:{simpleTargetName}:{simpleMessageName}",
            $"{ConfigRootKey}:{simpleMessageName}",
            $"{ConfigRootKey}:{simpleTargetName}",
            $"{ConfigRootKey}:{Default}",
        };

        var configs = from key in keys
                      let config = configuration.GetSection(key)
                      where config != null
                      where config.GetChildren().Any()
                      select config;

        var selected = configs.FirstOrDefault();

        return (selected, simpleTargetName, simpleMessageName, selected?.Path);
    }

    /// <summary>
    /// Retrieves the safe configuration section along with simple target and message names.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>
    /// A tuple containing the safe configuration section, simple target name, simple message name, and the configuration path.
    /// </returns>
    public virtual (IConfigurationSection? configurationSection, string simpleTargetName, string simpleMessageName, string? configPath) ConfigurationSafe(Type channelType, Type messageType)
    {
        var (config, simpleTargetName, simpleMessageName, _) = RootConfiguration(channelType, messageType);
        var selected = config?.GetSection("Config") ?? config;
        return (selected, simpleTargetName, simpleMessageName, selected?.Path);
    }

    /// <summary>
    /// Retrieves the configuration section along with simple target and message names.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>The configuration section.</returns>
    public virtual IConfigurationSection Configuration(Type channelType, Type messageType)
    {
        var (config, simpleTargetName, simpleMessageName, _) = ConfigurationSafe(channelType, messageType);
        return config ?? throw new ApplicationException($"No configuration found for \"MessageQueue:{simpleTargetName}:{simpleMessageName}\"");
    }

    /// <summary>
    /// Resolves the message ID, generating a new one if not provided.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="messageId">The provided message ID.</param>
    /// <returns>The resolved message ID.</returns>
    public virtual string MessageId(Type channelType, Type messageType, string? messageId) =>
        string.IsNullOrWhiteSpace(messageId) ? GenerateId(channelType, messageType) : messageId;

    /// <summary>
    /// Generates a new message ID.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>The generated message ID.</returns>
    public virtual string GenerateId(Type channelType, Type messageType) =>
        Guid.NewGuid().ToString();

    /// <summary>
    /// Retrieves the safe provider information along with simple target and message names.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>
    /// A tuple containing the provider key, simple target name, simple message name, and the configuration path.
    /// </returns>
    public virtual (string? providerKey, string simpleTargetName, string simpleMessageName, string? configPath) ProviderSafe(Type channelType, Type messageType)
    {
        var (config, simpleTargetName, simpleMessageName, configPath) = RootConfiguration(channelType, messageType);
        var providerKey = config?["Provider"];
        return (providerKey, simpleTargetName, simpleMessageName, configPath);
    }

    /// <summary>
    /// Retrieves the provider key along with simple target and message names.
    /// </summary>
    /// <param name="channelType">The type of the message queue channel.</param>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>The provider key.</returns>
    public virtual string Provider(Type channelType, Type messageType)
    {
        var (providerKey, simpleTargetName, simpleMessageName, _) = ProviderSafe(channelType, messageType);
        return providerKey ?? throw new ApplicationException($"No provider found for \"MessageQueue:{simpleTargetName}:{simpleMessageName}:Provider\"");
    }
}
