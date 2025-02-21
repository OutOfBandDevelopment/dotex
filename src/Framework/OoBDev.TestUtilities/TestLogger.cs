using Microsoft.Extensions.Logging;
using System.Threading;

namespace OoBDev.TestUtilities;

/// <summary>
/// Provides functionality for creating logger instances for testing purposes.
/// </summary>
public static class TestLogger
{
    private readonly static AsyncLocal<ILoggerFactory> _factory = new();

    /// <summary>
    /// Gets the logger factory instance.
    /// </summary>
    public static ILoggerFactory Factory =>
        _factory.Value ??= LoggerFactory.Create(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Trace)
        );

    /// <summary>
    /// Creates a logger instance for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create the logger for.</typeparam>
    /// <returns>A logger instance for the specified type.</returns>
    public static ILogger<T> CreateLogger<T>() => Factory.CreateLogger<T>();
}
