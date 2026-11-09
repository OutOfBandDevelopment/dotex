using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Handles inbound provider calls.</summary>
public interface IReceiveService
{
    /// <summary>Verifies, parses and dispatches a webhook call. Bad input is returned as a failed result, not thrown.</summary>
    Task<WebhookResult> HandleWebhookAsync(string channelName, WebhookRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches at most <paramref name="maxMessages"/> messages from a pollable channel and dispatches them to the
    /// <see cref="IMessageReceivedHandler"/> instances. A message is acknowledged only after every handler returned, so a failing handler loses nothing.
    /// </summary>
    /// <returns>The messages that were handled and acknowledged.</returns>
    /// <exception cref="ChannelNotFoundException">The channel is not configured.</exception>
    /// <exception cref="ProviderNotFoundException">No provider matches the channel.</exception>
    /// <exception cref="System.NotSupportedException">The provider cannot be polled.</exception>
    Task<IReadOnlyList<IMessage>> PollAsync(string channelName, int maxMessages = 10, CancellationToken cancellationToken = default);
}
