using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Keeps the delivery status of sent messages.</summary>
public interface IDeliveryTracker
{
    /// <summary>Records the outcome of a send.</summary>
    Task RecordSentAsync(string messageId, string channelName, SendResult result, CancellationToken cancellationToken = default);

    /// <summary>Gets the stored result of an earlier send of the same message on the channel, or <c>null</c>.</summary>
    Task<SendResult?> GetSentResultAsync(string messageId, string channelName, CancellationToken cancellationToken = default);

    /// <summary>Gets the status record, or <c>null</c>.</summary>
    Task<DeliveryStatusRecord?> GetStatusAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>Moves the status forward (Queued, Sent, Delivered/Failed/Bounced, Opened, Clicked); a stale or unknown update is ignored.</summary>
    /// <returns><c>true</c> when the record changed.</returns>
    Task<bool> UpdateStatusAsync(DeliveryStatusUpdate update, CancellationToken cancellationToken = default);

    /// <summary>Gets every record of a correlation id, oldest first.</summary>
    Task<IReadOnlyList<DeliveryStatusRecord>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
}
