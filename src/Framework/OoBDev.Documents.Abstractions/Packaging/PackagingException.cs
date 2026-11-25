using System;
namespace OoBDev.Documents.Packaging;

/// <summary>The packaging request cannot be fulfilled.</summary>
public class PackagingException : Exception
{
    /// <summary>Creates the exception.</summary>
    public PackagingException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What is wrong.</param>
    public PackagingException(string message) : base(message)
    {
    }

    /// <summary>Creates the exception with a message and cause.</summary>
    /// <param name="message">What is wrong.</param>
    /// <param name="innerException">The cause.</param>
    public PackagingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
