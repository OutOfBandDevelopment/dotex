using OoBDev.System.IO;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.DevTerm.Transports.Loopback;

/// <summary>
/// An in-process <see cref="ITransport"/> that answers scripted commands deterministically, so the shells and tests run without hardware.
/// Pipe-backed with no pump: there is no real stream underneath.
/// </summary>
public sealed class LoopbackTransport : ITransport
{
    private readonly IReadOnlyList<LoopbackRule> _rules = LoopbackScript.Default();
    private readonly LoopbackTransportOptions _options;
    private Pipe _pipe = new();
    private ConnectionState _state = ConnectionState.Closed;

    /// <summary>Creates a transport with the default script.</summary>
    public LoopbackTransport(IOptions<LoopbackTransportOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    /// <inheritdoc />
    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (_state == value) return;
            var previous = _state;
            _state = value;
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, value));
        }
    }

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc />
    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        // Close completes the old writer for good, so every open needs a fresh pipe.
        _pipe = new Pipe();
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        State = ConnectionState.Closed;
    }

    /// <inheritdoc />
    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var line = Encoding.ASCII.GetString(data.Span).TrimEnd('\r', '\n');
        if (line.Length == 0) return;

        var rule = _rules.FirstOrDefault(r => r.Pattern.IsMatch(line));
        if (rule is null)
        {
            // Visible feedback for a typo rather than silence.
            await PushLineAsync($"? Unrecognized: {line}", cancellationToken).ConfigureAwait(false);
            return;
        }

        var paceLines = rule.Streaming && _options.SampleIntervalMs > 0;
        var first = true;
        foreach (var responseLine in rule.Respond(rule.Pattern.Match(line)))
        {
            if (paceLines && !first) await Task.Delay(_options.SampleIntervalMs, cancellationToken).ConfigureAwait(false);
            await PushLineAsync(responseLine, cancellationToken).ConfigureAwait(false);
            first = false;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await CloseAsync().ConfigureAwait(false);

    private async Task PushLineAsync(string text, CancellationToken cancellationToken) =>
        await _pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(text + "\r\n"), cancellationToken).ConfigureAwait(false);
}
