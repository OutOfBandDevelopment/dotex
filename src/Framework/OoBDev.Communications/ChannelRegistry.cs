using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Finds a registered <see cref="IChannelProvider"/> by provider key and protocol (both case-insensitive).</summary>
public sealed class ChannelRegistry(IEnumerable<IChannelProvider> providers) : IChannelRegistry
{
    /// <inheritdoc />
    public IChannelProvider? GetProvider(string protocol, string providerName)
        => providers.FirstOrDefault(p =>
            string.Equals(p.ProviderName, providerName, StringComparison.OrdinalIgnoreCase)
            && p.SupportedProtocols.Contains(protocol, StringComparer.OrdinalIgnoreCase));
}
