using OoBDev.Communications.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications;

/// <summary>
/// Adds the recipients of a permanent bounce or a spam complaint to the <see cref="ISuppressionList"/>. A transient
/// bounce (full mailbox, server down) does not suppress, because the address may work later.
/// </summary>
/// <param name="suppressions">The suppression list.</param>
public class SuppressionStatusHandler(ISuppressionList suppressions) : IDeliveryStatusHandler
{
    /// <inheritdoc />
    public async Task HandleAsync(DeliveryStatusUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        var reason = update switch
        {
            { NewStatus: DeliveryStatus.Complained } => "complaint",
            { NewStatus: DeliveryStatus.Bounced, StatusDetails: "Permanent" } => "bounce",
            _ => null,
        };
        if (reason is null || update.Recipients is null)
        {
            return;
        }

        foreach (var address in update.Recipients)
        {
            await suppressions.AddAsync(address, reason, cancellationToken).ConfigureAwait(false);
        }
    }
}
