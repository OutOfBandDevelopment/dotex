using System;

namespace OoBDev.System.Time;

/// <summary>
/// Assigns and merges <see cref="HybridLogicalClock"/> values for one site. Register as a singleton:
/// the counter must be monotonic across every event the process produces. Thread safe.
/// </summary>
public sealed class HybridLogicalClockGenerator
{
    private readonly TimeProvider _time;
    private readonly long _maxDriftTicks;
    private readonly object _lock = new();
    private long _lastPhysical;
    private int _counter;

    /// <summary>Creates a generator.</summary>
    /// <param name="time">Source of wall-clock time.</param>
    /// <param name="maxDrift">Largest accepted distance a received clock may be ahead of local time; null accepts any.</param>
    public HybridLogicalClockGenerator(TimeProvider? time = null, TimeSpan? maxDrift = null)
    {
        _time = time ?? TimeProvider.System;
        _maxDriftTicks = maxDrift?.Ticks ?? long.MaxValue;
    }

    /// <summary>The next value for a local event; strictly greater than every earlier value.</summary>
    /// <exception cref="OverflowException">The logical counter is exhausted.</exception>
    public HybridLogicalClock Next()
    {
        lock (_lock)
        {
            var now = _time.GetUtcNow().UtcTicks;
            if (now > _lastPhysical)
            {
                _lastPhysical = now;
                _counter = 0;
            }
            else
            {
                _counter = checked(_counter + 1);
            }
            return new HybridLogicalClock(_lastPhysical, _counter);
        }
    }

    /// <summary>Folds the clock of a received event into this one and returns the value for the receive event.</summary>
    /// <exception cref="ArgumentException">The received clock is further ahead of local time than the allowed drift.</exception>
    /// <exception cref="OverflowException">The logical counter is exhausted.</exception>
    public HybridLogicalClock Merge(HybridLogicalClock received)
    {
        lock (_lock)
        {
            var now = _time.GetUtcNow().UtcTicks;
            if (received.PhysicalTicks - now > _maxDriftTicks)
            {
                throw new ArgumentException("The received clock is ahead of local time by more than the allowed drift.", nameof(received));
            }

            var max = global::System.Math.Max(now, global::System.Math.Max(_lastPhysical, received.PhysicalTicks));
            if (max == _lastPhysical && max == received.PhysicalTicks)
            {
                _counter = checked(global::System.Math.Max(_counter, received.LogicalCounter) + 1);
            }
            else if (max == _lastPhysical)
            {
                _counter = checked(_counter + 1);
            }
            else if (max == received.PhysicalTicks)
            {
                _counter = checked(received.LogicalCounter + 1);
            }
            else
            {
                _counter = 0;
            }
            _lastPhysical = max;
            return new HybridLogicalClock(_lastPhysical, _counter);
        }
    }
}
