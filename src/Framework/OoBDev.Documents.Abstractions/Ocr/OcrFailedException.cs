using System;

namespace OoBDev.Documents.Ocr;

/// <summary>The OCR service took the document and reported failure; repeating the call will not help.</summary>
public sealed class OcrFailedException : Exception
{
    /// <summary>Creates the exception.</summary>
    public OcrFailedException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What the service reported.</param>
    public OcrFailedException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and a cause.</summary>
    /// <param name="message">What the service reported.</param>
    /// <param name="innerException">The cause.</param>
    public OcrFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
