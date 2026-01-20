using System;

namespace OoBDev.ComplexEvents.Abstractions.Schedulers
{
    public interface IReleaseScheduleInstance
    {
        /// <summary>
        /// Reference key for looking up the scheduler instance
        /// </summary>
        string ReferenceKey { get; }

        /// <summary>
        /// Resolved type being used as the scheduled instance
        /// </summary>
        Type Scheduler { get; }

        /// <summary>
        /// Calculated time for next execution based on passed in values
        /// </summary>
        DateTimeOffset? NextStart { get; }

        /// <summary>
        /// Error message if any from scheduler
        /// </summary>
        string? ErrorMessage { get; }
    }
}
