using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>Server-pushed UART line-status change. Only ever server-originated, so <see cref="IsFromServer"/> is always <c>true</c>.</summary>
internal sealed record Rfc2217NotifyLineStateMessage(Rfc2217LineState LineState) : IRfc2217Message
{
    public bool IsFromServer => true;
}
