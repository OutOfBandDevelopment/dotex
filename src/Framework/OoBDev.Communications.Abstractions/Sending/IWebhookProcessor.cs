using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Verifies and parses the webhook calls of one vendor.</summary>
public interface IWebhookProcessor
{
    /// <summary>Gets the provider key (case-insensitive).</summary>
    string Provider { get; }

    /// <summary>Checks the vendor signature over the raw body.</summary>
    bool ValidateSignature(IChannel channel, WebhookRequest request);

    /// <summary>Parses a verified call into events.</summary>
    Task<IReadOnlyList<ChannelEvent>> ProcessAsync(IChannel channel, WebhookRequest request, CancellationToken cancellationToken = default);
}
