using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications;

/// <summary>Reads channels from <see cref="SendReceiveOptions.Channels"/>; names are case-insensitive.</summary>
public sealed class ConfigurationChannelRepository(IOptions<SendReceiveOptions> options) : IChannelRepository
{
    /// <inheritdoc />
    public Task<IChannel?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => Task.FromResult<IChannel?>(options.Value.Channels.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)));
}
