namespace OoBDev.Communications.Abstractions;

/// <summary>Finds the provider for a channel.</summary>
public interface IChannelRegistry
{
    /// <summary>Gets the provider registered for the protocol and provider name, or <c>null</c>.</summary>
    IChannelProvider? GetProvider(string protocol, string providerName);
}
