using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Told when a scheduled item's callback throws, for example to publish it to a dead-letter queue once <see cref="ScheduleItem.Attempts"/> reaches the store's limit.</summary>
public interface IScheduleFailureHandler
{
    /// <summary>Handles one failed attempt; an exception thrown here is swallowed so the next item still runs.</summary>
    /// <param name="item">The item, with the attempt count of this lease.</param>
    /// <param name="exception">What the callback threw.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task OnFailedAsync(ScheduleItem item, Exception exception, CancellationToken cancellationToken);
}
