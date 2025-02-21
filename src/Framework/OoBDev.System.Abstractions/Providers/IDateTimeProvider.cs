using System;

namespace OoBDev.System.Providers;

/// <summary>
/// Provides date and time functionality.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current local date and time.
    /// </remarks>
    DateTimeOffset Now { get; }

    /// <summary>
    /// Gets the current Coordinated Universal Time (UTC) date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current Coordinated Universal Time (UTC) date and time.
    /// </remarks>
    DateTimeOffset UtcNow { get; }
}
