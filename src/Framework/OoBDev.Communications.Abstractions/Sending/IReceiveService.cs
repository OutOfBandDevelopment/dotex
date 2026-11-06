using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Handles inbound provider calls.</summary>
public interface IReceiveService
{
    /// <summary>Verifies, parses and dispatches a webhook call. Bad input is returned as a failed result, not thrown.</summary>
    Task<WebhookResult> HandleWebhookAsync(string channelName, WebhookRequest request, CancellationToken cancellationToken = default);
}
