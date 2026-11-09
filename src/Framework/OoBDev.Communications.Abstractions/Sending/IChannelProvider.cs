using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Sends messages for one vendor.</summary>
public interface IChannelProvider
{
    /// <summary>Gets the provider key that channels refer to (case-insensitive).</summary>
    string ProviderName { get; }

    /// <summary>Gets the protocols the provider handles.</summary>
    string[] SupportedProtocols { get; }

    /// <summary>Checks whether the provider can send <paramref name="message"/> through <paramref name="channel"/>.</summary>
    Task<bool> CanSendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default);

    /// <summary>Sends one message. Vendor outcomes are returned as a failed result, not thrown.</summary>
    Task<SendResult> SendAsync(IChannel channel, IMessage message, CancellationToken cancellationToken = default);
}
