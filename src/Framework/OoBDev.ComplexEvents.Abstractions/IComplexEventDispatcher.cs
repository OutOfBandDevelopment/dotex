using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Runs every handler registered for an event.</summary>
public interface IComplexEventDispatcher
{
    /// <summary>Dispatches in order; a failing handler does not stop the rest and the errors are thrown together.</summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken);
}
