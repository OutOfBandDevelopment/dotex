using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

public enum TcpEndpointMode
{
    /// <summary>Connect out to a remote host:port.</summary>
    Client,

    /// <summary>Bind a local port and accept one inbound connection.</summary>
    Listener,
}
