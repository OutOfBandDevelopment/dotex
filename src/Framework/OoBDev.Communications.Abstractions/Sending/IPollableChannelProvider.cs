using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>A provider that fetches inbound messages by asking the vendor (for example an IMAP inbox).</summary>
public interface IPollableChannelProvider : IChannelProvider
{
    /// <summary>Fetches at most <paramref name="maxMessages"/> unread messages without marking them read.</summary>
    Task<IReadOnlyList<PolledMessage>> PollAsync(IChannel channel, int maxMessages, CancellationToken cancellationToken = default);

    /// <summary>Marks the messages behind the receipts as read.</summary>
    Task AcknowledgeAsync(IChannel channel, IReadOnlyCollection<string> receipts, CancellationToken cancellationToken = default);
}
