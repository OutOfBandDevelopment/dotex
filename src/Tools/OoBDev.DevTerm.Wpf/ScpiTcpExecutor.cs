using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OoBDev.Scpi;
using OoBDev.System.IO.Tcp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Sends SCPI requests to an instrument's raw socket (usually port 5025) and reads one line for each query.</summary>
/// <param name="host">The instrument host.</param>
/// <param name="port">The instrument TCP port.</param>
public sealed class ScpiTcpExecutor(string host, int port = 5025) : IDisposable
{
    private readonly TcpDeviceAdapter _adapter = new(host, port, TimeSpan.FromSeconds(3));
    private readonly global::System.Threading.Lock _gate = new();

    /// <summary>Sends the request and returns the reply line for a query, or null for a command.</summary>
    /// <param name="request">The request.</param>
    /// <returns>The reply text without its terminator, or null.</returns>
    /// <exception cref="IOException">The instrument cannot be reached or closed the connection.</exception>
    public Task<string?> ExecuteAsync(ScpiRequest request) => Task.Run(() => Execute(request));

    /// <summary>Closes the connection.</summary>
    public void Dispose() => _adapter.Dispose();

    private string? Execute(ScpiRequest request)
    {
        lock (_gate)
        {
            if (!_adapter.TryOpen(out var stream) || stream is null)
            {
                throw new IOException($"Cannot connect to {_adapter.Path}.");
            }

            var bytes = Encoding.ASCII.GetBytes(request.WireText);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
            if (!request.IsQuery) return null;

            var reply = new StringBuilder();
            while (true)
            {
                var next = stream.ReadByte();
                if (next < 0) throw new IOException("The instrument closed the connection before it replied.");
                if (next == '\n') return reply.ToString().TrimEnd('\r');
                reply.Append((char)next);
            }
        }
    }
}
