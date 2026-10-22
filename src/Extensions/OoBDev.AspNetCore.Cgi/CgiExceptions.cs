#pragma warning disable MA0048 // related small types share a file
using System;

namespace OoBDev.AspNetCore.Cgi;

/// <summary>
/// A CGI failure that maps to an HTTP status code.
/// </summary>
public class CgiException : Exception
{
    /// <summary>
    /// Creates the exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to answer with.</param>
    /// <param name="message">What went wrong.</param>
    public CgiException(int statusCode, string message) : base(message) => StatusCode = statusCode;

    /// <summary>
    /// The HTTP status code to answer with.
    /// </summary>
    public int StatusCode { get; }
}

/// <summary>
/// No process slot was free (503).
/// </summary>
public class CgiBusyException() : CgiException(503, "All CGI process slots are in use.");

/// <summary>
/// The request body was larger than <see cref="CgiOptions.MaxRequestBodyBytes"/> (413).
/// </summary>
public class CgiBodyTooLargeException() : CgiException(413, "The request body is too large.");

/// <summary>
/// The process ran past <see cref="CgiOptions.Timeout"/> (504).
/// </summary>
public class CgiTimeoutException() : CgiException(504, "The CGI program timed out.");

/// <summary>
/// The program output was not a valid CGI response (502).
/// </summary>
/// <param name="message">What was wrong with the output.</param>
public class CgiProtocolException(string message) : CgiException(502, message);

/// <summary>
/// The program could not start or exited with an error before producing output (500).
/// </summary>
/// <param name="message">What went wrong.</param>
public class CgiProcessFailedException(string message) : CgiException(500, message);

/// <summary>
/// The response grew past <see cref="CgiOptions.MaxResponseBytes"/> after it started, so it could only be aborted.
/// </summary>
public class CgiResponseTooLargeException() : CgiException(502, "The CGI response is too large.");
