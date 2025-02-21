using System;

namespace OoBDev.System.Providers;

/// <summary>
/// Provides date and time functionality.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current local date and time.
    /// </remarks>
    public DateTimeOffset Now => DateTimeOffset.Now;

    /// <summary>
    /// Gets the current Coordinated Universal Time (UTC) date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current Coordinated Universal Time (UTC) date and time.
    /// </remarks>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
