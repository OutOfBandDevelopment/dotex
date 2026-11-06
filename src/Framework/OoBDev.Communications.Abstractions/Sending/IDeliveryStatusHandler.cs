using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Reacts to a delivery status change; handlers run in registration order.</summary>
public interface IDeliveryStatusHandler
{
    /// <summary>Handles the update.</summary>
    Task HandleAsync(DeliveryStatusUpdate update, CancellationToken cancellationToken = default);
}
