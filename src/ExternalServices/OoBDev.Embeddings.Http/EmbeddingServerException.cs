using System;
using System.Net;

namespace OoBDev.Embeddings.Http;

/// <summary>
/// The embedding server answered with an error or a response that does not fit the request. Input text and the response body are never included.
/// </summary>
public sealed class EmbeddingServerException : Exception
{
    /// <summary>Creates the exception.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="statusCode">The HTTP status, when there was a response.</param>
    public EmbeddingServerException(string message, HttpStatusCode? statusCode = null) : base(message) => StatusCode = statusCode;

    /// <summary>The HTTP status, when there was a response.</summary>
    public HttpStatusCode? StatusCode { get; }
}
