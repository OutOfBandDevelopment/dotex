using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Builds the message for one channel from a request.</summary>
public interface IMessageComposer
{
    /// <summary>Gets the channel name this composer serves, matched to <see cref="TargetPreference.Channels"/> without regard to case.</summary>
    string ChannelName { get; }

    /// <summary>Composes the message, or returns <c>null</c> when the request has nothing to say on this channel (for example the target has no address).</summary>
    Task<IMessage?> ComposeAsync(CommunicationRequest request, TargetPreference preference, CancellationToken cancellationToken = default);
}
