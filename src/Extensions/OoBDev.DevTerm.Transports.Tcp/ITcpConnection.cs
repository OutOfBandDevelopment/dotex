using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Transports.Tcp;

/// <summary>
/// One established TCP connection, however it was obtained (dialed out, or accepted from a
/// listener). Abstracted so <see cref="TcpTransport"/> can be unit tested with a fake instead
/// of a real socket.
/// </summary>
public interface ITcpConnection : IDisposable
{
    /// <summary>The connection's byte stream, pumped into a pipe (see <see cref="StreamToPipePump"/>).</summary>
    Stream Stream { get; }

    Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
}
