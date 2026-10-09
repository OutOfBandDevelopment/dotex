using System;

namespace OoBDev.System.Providers;

/// <summary>
/// Provides date and time functionality.
/// </summary>
/// <remarks>
/// Kept for compatibility; delegates to <see cref="TimeProvider"/>. Prefer injecting <see cref="TimeProvider"/>.
/// </remarks>
#pragma warning disable CS0618 // IDateTimeProvider is obsolete
public class DateTimeProvider : IDateTimeProvider
{
    private readonly TimeProvider _time;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeProvider"/> class using the system clock.
    /// </summary>
    public DateTimeProvider() : this(TimeProvider.System) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeProvider"/> class.
    /// </summary>
    /// <param name="time">The time provider.</param>
    public DateTimeProvider(TimeProvider time) => _time = time;

    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current local date and time.
    /// </remarks>
    public DateTimeOffset Now => _time.GetLocalNow();

    /// <summary>
    /// Gets the current Coordinated Universal Time (UTC) date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current Coordinated Universal Time (UTC) date and time.
    /// </remarks>
    public DateTimeOffset UtcNow => _time.GetUtcNow();
}
