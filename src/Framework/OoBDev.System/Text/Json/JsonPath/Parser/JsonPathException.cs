using System;

namespace OoBDev.System.Text.Json.JsonPath.Parser;

/// <summary>
/// Exception thrown when a JSON path parsing error occurs.
/// </summary>
public class JsonPathException : Exception
{
    /// <summary>
    /// Initializes a new instance of the JsonPathException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the JSON path parsing error.</param>
    public JsonPathException(string message) : base(message)
    {
    }
}