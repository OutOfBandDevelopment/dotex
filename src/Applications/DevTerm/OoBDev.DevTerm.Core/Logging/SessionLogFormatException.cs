using System;

namespace OoBDev.DevTerm.Logging;

/// <summary>A file (or one of its lines) isn't a valid dev-term session log.</summary>
public sealed class SessionLogFormatException : Exception
{
    public SessionLogFormatException(string message)
        : base(message)
    {
    }

    public SessionLogFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
