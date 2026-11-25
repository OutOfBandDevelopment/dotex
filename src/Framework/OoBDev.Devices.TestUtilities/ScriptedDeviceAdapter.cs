using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.Devices.TestUtilities;

/// <summary>
/// An in-memory <see cref="IDeviceAdapter"/> that plays a device: each write is matched against the scripted rules and the
/// rule's reply is pushed back on <see cref="Input"/>, so request and response sequences run without hardware or a socket.
/// Every write is recorded in <see cref="Written"/>.
/// </summary>
public sealed class ScriptedDeviceAdapter : IDeviceAdapter
{
    private readonly List<(byte[] Request, byte[][] Replies)> _rules = [];
    private readonly Pipe _pipe = new();
    private PipeWriter? _output;
    private ConnectionState _state = ConnectionState.Closed;

    /// <inheritdoc/>
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <summary>Gets every payload written to the device, in order.</summary>
    public IList<byte[]> Written { get; } = [];

    /// <summary>Gets the writes that matched no rule.</summary>
    public IList<byte[]> Unmatched { get; } = [];

    /// <inheritdoc/>
    public string Type => "Scripted";

    /// <inheritdoc/>
    public string Path => "scripted";

    /// <inheritdoc/>
    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            var previous = _state;
            _state = value;
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, value));
        }
    }

    /// <inheritdoc/>
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc/>
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create(WriteAsync);

    /// <summary>Adds a rule: a write equal to <paramref name="request"/> is answered with each of <paramref name="replies"/> in turn. The first matching rule wins and a rule can match again.</summary>
    /// <param name="request">The exact bytes the device expects.</param>
    /// <param name="replies">The reply chunks pushed back, one per transport read.</param>
    /// <returns>This adapter, for chaining.</returns>
    public ScriptedDeviceAdapter When(byte[] request, params byte[][] replies)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(replies);
        _rules.Add((request, replies));
        return this;
    }

    /// <summary>Pushes bytes to the reader as if the device sent them unprompted.</summary>
    /// <param name="data">The bytes.</param>
    public async Task PushAsync(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        await _pipe.Writer.WriteAsync(data).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        State = ConnectionState.Closed;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() => await CloseAsync().ConfigureAwait(false);

    private async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        var payload = data.ToArray();
        Written.Add(payload);
        var rule = _rules.FirstOrDefault(r => r.Request.AsSpan().SequenceEqual(payload));
        if (rule.Replies is null)
        {
            Unmatched.Add(payload);
            return;
        }

        foreach (var reply in rule.Replies)
        {
            await _pipe.Writer.WriteAsync(reply, cancellationToken).ConfigureAwait(false);
        }
    }
}
