using System;

namespace OoBDev.Documents.Scanning;

/// <summary>Thrown when content is refused because a scan found something or could not complete.</summary>
public class ContentRejectedException : Exception
{
    /// <summary>Creates the exception.</summary>
    public ContentRejectedException() { }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What was found.</param>
    public ContentRejectedException(string message) : base(message) { }

    /// <summary>Creates the exception with a message and cause.</summary>
    /// <param name="message">What failed.</param>
    /// <param name="innerException">The scanner failure.</param>
    public ContentRejectedException(string message, Exception innerException) : base(message, innerException) { }
}
