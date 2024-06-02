using System;

namespace OoBDev.System.Text.Json.JsonPath.Parser;

public class JsonPathException : Exception
{
    public JsonPathException(string message) : base(message)
    {
    }
}