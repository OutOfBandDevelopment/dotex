using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO.Tcp;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Listens on a TCP port and turns every accepted peer into a <see cref="SessionTab"/>, raised through
/// <see cref="TabReady"/> for a front end to add (docs/design/Transports/transports.md, docs/design/MultiSessionUi/multi-session-ui.md).
/// </summary>
/// <param name="cliOptions">The options each accepted peer's session is composed from (presenters, parser, limits).</param>
/// <param name="host">The local address to bind; any address when empty.</param>
/// <param name="port">The local port to bind; 0 picks a free one.</param>
public sealed class PeerTabSource(CliOptions cliOptions, string? host, int port) : IAsyncDisposable
{
    private readonly TcpPeerListener _listener = new(host, port);
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private int _disposed;

    /// <summary>Raised for each accepted peer with its remote endpoint and ready (already open) tab. May be raised on a background thread.</summary>
    public event EventHandler<PeerTabEventArgs>? TabReady;

    /// <summary>The bound port, valid after <see cref="Start"/>.</summary>
    public int Port => _listener.Port;

    /// <summary>Starts accepting peers. Does nothing when already started.</summary>
    public void Start()
    {
        if (_loop is not null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _loop = Task.Run(() => AcceptAsync(token), CancellationToken.None);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (_cts is not null)
        {
            await _cts.CancelAsync().ConfigureAwait(false);
        }

        if (_loop is not null)
        {
            await _loop.ConfigureAwait(false);
        }

        _cts?.Dispose();
        await _listener.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async Task AcceptAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var peer in _listener.AcceptPeersAsync(cancellationToken).ConfigureAwait(false))
            {
                var tab = SessionTab.BuildForAdapter(cliOptions, peer.Adapter);
                TabReady?.Invoke(this, new PeerTabEventArgs(peer.RemoteEndPoint, tab));
            }
        }
        catch (OperationCanceledException)
        {
            // Disposal ends the loop.
        }
    }
}
