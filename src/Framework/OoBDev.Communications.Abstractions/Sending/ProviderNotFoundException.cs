using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>No provider is registered for a channel's protocol and provider (a configuration error).</summary>
public class ProviderNotFoundException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    /// <param name="protocol">The channel protocol.</param>
    /// <param name="provider">The channel provider key.</param>
    public ProviderNotFoundException(string protocol, string provider) : base($"No provider '{provider}' for protocol '{protocol}'.")
    {
        Protocol = protocol;
        Provider = provider;
    }

    /// <summary>Gets the protocol.</summary>
    public string Protocol { get; }

    /// <summary>Gets the provider key.</summary>
    public string Provider { get; }
}
