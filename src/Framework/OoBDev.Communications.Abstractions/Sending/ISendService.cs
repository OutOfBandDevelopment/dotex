using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Sends messages through channels with retry and delivery tracking.</summary>
public interface ISendService
{
    /// <summary>Sends <paramref name="message"/> through the named channel.</summary>
    /// <exception cref="ChannelNotFoundException">The channel does not exist.</exception>
    /// <exception cref="ProviderNotFoundException">No provider is registered for the channel.</exception>
    Task<SendResult> SendAsync(string channelName, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Sends through several channels in parallel; results are in the order of <paramref name="channelNames"/>.</summary>
    Task<IReadOnlyList<SendResult>> SendToMultipleChannelsAsync(IReadOnlyCollection<string> channelNames, IMessage message, SendOptions? options = null, CancellationToken cancellationToken = default);
}
