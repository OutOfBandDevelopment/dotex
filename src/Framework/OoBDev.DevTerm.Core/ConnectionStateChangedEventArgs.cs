using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm;

public sealed class ConnectionStateChangedEventArgs(ConnectionState previous, ConnectionState current) : EventArgs
{
    public ConnectionState Previous { get; } = previous;
    public ConnectionState Current { get; } = current;
}
