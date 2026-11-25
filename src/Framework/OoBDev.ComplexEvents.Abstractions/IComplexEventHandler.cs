using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Handles events of one type.</summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public interface IComplexEventHandler<in TEvent>
{
    /// <summary>Handles the event; must be idempotent because a lease can expire and repeat.</summary>
    /// <param name="event">The event.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
