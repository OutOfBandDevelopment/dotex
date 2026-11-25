using System;

namespace OoBDev.WorkQueue;

/// <summary>
/// Thrown by a stage handler when retrying cannot help; the item is dead-lettered at once.
/// </summary>
public sealed class NonRetryableWorkException : Exception
{
    /// <summary>Creates the exception.</summary>
    public NonRetryableWorkException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">The message.</param>
    public NonRetryableWorkException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception with a message and a cause.</summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The cause.</param>
    public NonRetryableWorkException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
