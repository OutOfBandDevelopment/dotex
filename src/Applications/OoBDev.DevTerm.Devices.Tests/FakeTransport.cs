using System.IO.Pipelines;
using System.Text;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.DevTerm.Devices.Tests;

/// <summary>An in-memory <see cref="IDeviceAdapter"/> that records every write and can answer each one.</summary>
internal sealed class FakeTransport : IDeviceAdapter
{
    private readonly Pipe _pipe = new();
    private PipeWriter? _output;
    private ConnectionState _state = ConnectionState.Closed;

    public List<byte[]> WrittenPayloads { get; } = [];

    public Exception? FailWritesWith { get; set; }

    /// <summary>Text pushed back on the input after every write; null leaves the device silent.</summary>
    public string? ReplyToEveryWrite { get; set; }

    public IEnumerable<string> WrittenText => WrittenPayloads.Select(Encoding.ASCII.GetString);

    public ConnectionState State
    {
        get => _state;
        private set
        {
            var previous = _state;
            _state = value;
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, value));
        }
    }

    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    public PipeReader Input => _pipe.Reader;

    public string Type => "Fake";

    public string Path => "fake";

    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create(WriteAsync);

    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _pipe.Writer.CompleteAsync();
        State = ConnectionState.Closed;
    }

    public async ValueTask DisposeAsync() => await CloseAsync();

    private async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        if (FailWritesWith is { } failure)
        {
            throw failure;
        }

        WrittenPayloads.Add(data.ToArray());
        if (ReplyToEveryWrite is { } reply)
        {
            await _pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(reply + "\n"), CancellationToken.None);
        }
    }
}
