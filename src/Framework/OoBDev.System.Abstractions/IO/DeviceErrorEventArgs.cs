using System;

namespace OoBDev.System.IO;

/// <summary>
/// Initializes a new instance of the <see cref="DeviceErrorEventArgs"/> class.
/// </summary>
/// <param name="exception">The exception that occurred.</param>
/// <param name="errorHandling">The error handling strategy.</param>
public class DeviceErrorEventArgs(Exception exception, ErrorHandling errorHandling) : EventArgs
{
    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; } = exception;

    /// <summary>
    /// Gets or sets the error handling strategy.
    /// </summary>
    public ErrorHandling ErrorHandling { get; set; } = errorHandling;
}
