using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.DevTerm;

/// <summary>
/// A transport implemented by a child process that speaks newline-delimited JSON on stdin and stdout
/// (<c>hello</c>, <c>open</c>, <c>write</c>, <c>close</c> from the host; <c>opened</c>, <c>data</c>, <c>error</c>,
/// <c>closed</c> from the plugin). The host never trusts the child: a handshake or open that gets no
/// answer in the reply timeout, an exit, or a bad line faults the adapter.
/// </summary>
public sealed class ExternalProcessAdapter : IDeviceAdapter
{
    private readonly string _fileName;
    private readonly IReadOnlyList<string> _arguments;
    private readonly TimeSpan _replyTimeout;
    private readonly Lock _gate = new();
    private readonly PluginSandbox? _sandbox;
    private PluginChild? _process;
    private Pipe _pipe = NewPipe();
    private PipeWriter? _output;
    private TaskCompletionSource<string?> _opened = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ConnectionState _state = ConnectionState.Closed;

    /// <summary>Creates an adapter that starts <paramref name="fileName"/> when it is opened.</summary>
    /// <param name="name">The plugin name shown as the transport type.</param>
    /// <param name="fileName">The program to run.</param>
    /// <param name="arguments">Its arguments.</param>
    /// <param name="replyTimeout">How long the open may take.</param>
    public ExternalProcessAdapter(string name, string fileName, IReadOnlyList<string> arguments, TimeSpan replyTimeout, PluginSandbox? sandbox = null)
    {
        _sandbox = sandbox;
        Type = name;
        _fileName = fileName;
        _arguments = arguments;
        _replyTimeout = replyTimeout;
    }

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public string Type { get; }

    /// <inheritdoc />
    public string Path => $"plugin://{Type}";

    /// <summary>Why the adapter faulted, or null.</summary>
    public string? FaultReason { get; private set; }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc />
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create(WriteAsync);

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_process is not null)
        {
            return;
        }

        State = ConnectionState.Opening;
        FaultReason = null;
        _pipe = NewPipe();
        _opened = new(TaskCreationOptions.RunContinuationsAsynchronously);
        PluginChild process;
        try
        {
            process = PluginChild.Start(_fileName, _arguments, _sandbox);
        }
        catch (InvalidOperationException ex)
        {
            State = ConnectionState.Faulted;
            throw new IOException(ex.Message, ex);
        }

        _process = process;
        _ = Task.Run(() => ReadLoopAsync(process), CancellationToken.None);
        try
        {
            Send(process, new { type = "hello" });
            Send(process, new { type = "open" });
            var error = await _opened.Task.WaitAsync(_replyTimeout, cancellationToken).ConfigureAwait(false);
            if (error is not null)
            {
                throw new IOException(error);
            }

            State = ConnectionState.Open;
        }
        catch (Exception ex) when (ex is TimeoutException or IOException)
        {
            await FaultAsync(ex is TimeoutException ? $"no answer within {_replyTimeout.TotalMilliseconds:0} ms" : ex.Message).ConfigureAwait(false);
            throw new IOException($"Plugin '{Type}' failed to open: {FaultReason}", ex);
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        var process = _process;
        _process = null;
        if (process is not null)
        {
            State = ConnectionState.Closing;
            try
            {
                Send(process, new { type = "close" });
            }
            catch (IOException)
            {
                // The child is already gone.
            }

            process.Dispose();
        }

        await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        if (State != ConnectionState.Faulted)
        {
            State = ConnectionState.Closed;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await CloseAsync().ConfigureAwait(false);

    private static Pipe NewPipe() => new(new PipeOptions(pauseWriterThreshold: 0));

    private Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        var process = _process ?? throw new IOException("The plugin is not open.");
        Send(process, new { type = "write", hex = Convert.ToHexString(data.Span) });
        return Task.CompletedTask;
    }

    private void Send(PluginChild process, object message)
    {
        var line = JsonSerializer.Serialize(message);
        try
        {
            lock (_gate)
            {
                process.Input.WriteLine(line);
                process.Input.Flush();
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException)
        {
            throw new IOException("The plugin process is gone.", ex);
        }
    }

    private async Task ReadLoopAsync(PluginChild process)
    {
        try
        {
            while (await process.Output.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                switch (root.GetProperty("type").GetString())
                {
                    case "opened":
                        _opened.TrySetResult(null);
                        break;
                    case "data":
                        await _pipe.Writer.WriteAsync(Convert.FromHexString(root.GetProperty("hex").GetString() ?? string.Empty)).ConfigureAwait(false);
                        break;
                    case "error":
                        var message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "error" : "error";
                        _opened.TrySetResult(message);
                        await FaultAsync(message).ConfigureAwait(false);
                        return;
                    case "closed":
                        _opened.TrySetResult("the plugin closed");
                        await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
                        State = ConnectionState.Closed;
                        return;
                    default:
                        break;
                }
            }

            _opened.TrySetResult("the plugin exited");
            if (ReferenceEquals(_process, process))
            {
                await FaultAsync("the plugin exited").ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or FormatException or IOException or InvalidOperationException)
        {
            _opened.TrySetResult(ex.Message);
            await FaultAsync(ex.Message).ConfigureAwait(false);
        }
    }

    private async Task FaultAsync(string reason)
    {
        FaultReason ??= reason;
        var process = _process;
        _process = null;
        if (process is not null)
        {
            process.Kill();
        }

        await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        State = ConnectionState.Faulted;
    }

}
