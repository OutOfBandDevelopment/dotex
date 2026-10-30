using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

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
