using System;
namespace OoBDev.Documents.Store;

/// <summary>The content failed validation and was not stored.</summary>
public class InvalidContentException : Exception
{
    /// <summary>Creates the exception.</summary>
    public InvalidContentException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">Why the content was refused.</param>
    public InvalidContentException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception with a message and cause.</summary>
    /// <param name="message">Why the content was refused.</param>
    /// <param name="innerException">The cause.</param>
    public InvalidContentException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
