using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>
/// Obtains an <see cref="ITcpConnection"/> for either TCP mode: dialing out (Client) or
/// accepting one inbound peer (Listener). See docs/design/Transports/transports.md.
/// </summary>
public interface ITcpConnectionSource
{
    Task<ITcpConnection> ConnectAsync(TcpEndpointOptions options, CancellationToken cancellationToken);

    Task<ITcpConnection> AcceptAsync(TcpEndpointOptions options, CancellationToken cancellationToken);
}
