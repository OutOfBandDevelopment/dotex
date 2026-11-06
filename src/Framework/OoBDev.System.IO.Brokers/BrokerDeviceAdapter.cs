using System;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.System.IO.Pipelines;
namespace OoBDev.System.IO.Brokers;

/// <summary>
/// An <see cref="IDeviceAdapter"/> over one AMQP or STOMP broker connection. Topic addressing is carried as text so no
/// Session/Pipeline change is needed: each inbound message becomes one line, <c>topic&lt;TAB&gt;payload\n</c>;
/// a sent line is published to <see cref="BrokerDeviceAdapterOptions.PublishTopic"/>, or to its own topic when
/// written as <c>topic&lt;TAB&gt;payload</c>.
/// </summary>
public sealed class BrokerDeviceAdapter : IDeviceAdapter
{
    private readonly IBrokerConnectionFactory _factory;
    private readonly BrokerDeviceAdapterOptions _options;
    private readonly object _pipeLock = new();
    private IBrokerConnection? _connection;
    private Pipe _pipe = new();
    private PipeWriter? _output;
    private ConnectionState _state = ConnectionState.Closed;

    /// <summary>Creates the adapter.</summary>
    public BrokerDeviceAdapter(IBrokerConnectionFactory factory, IOptions<BrokerDeviceAdapterOptions> options)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(options);

        _factory = factory;
        _options = options.Value;
    }

    /// <inheritdoc />
    public string Type => "Broker";

    /// <inheritdoc />
    public string Path => $"{_options.Host}:{_options.Port}";

    /// <inheritdoc />
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create((data, ct) => WriteAsync(data, ct));

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
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        lock (_pipeLock)
        {
            _pipe = new Pipe();
        }

        State = ConnectionState.Opening;
        var connection = _factory.Create();
        connection.MessageReceived += OnMessage;
        connection.Disconnected += OnDisconnected;
        _connection = connection;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_options.TimeoutMs);
            await connection.ConnectAsync(_options, timeout.Token).ConfigureAwait(false);
            foreach (var topic in _options.SubscribeTopics.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                await connection.SubscribeAsync(topic.Trim(), timeout.Token).ConfigureAwait(false);
            }
        }
        catch
        {
            connection.MessageReceived -= OnMessage;
            connection.Disconnected -= OnDisconnected;
            await connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
            State = ConnectionState.Faulted;
            throw;
        }

        State = ConnectionState.Open;
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        var connection = _connection;
        _connection = null;
        if (connection is not null)
        {
            State = ConnectionState.Closing;
            connection.MessageReceived -= OnMessage;
            connection.Disconnected -= OnDisconnected;
            try
            {
                await connection.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Already gone: closing is idempotent.
            }

            await connection.DisposeAsync().ConfigureAwait(false);
        }

        PipeWriter writer;
        lock (_pipeLock)
        {
            writer = _pipe.Writer;
        }

        await writer.CompleteAsync().ConfigureAwait(false);
        State = ConnectionState.Closed;
    }

    internal async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var line = Encoding.UTF8.GetString(data.Span).TrimEnd('\r', '\n');
        if (line.Length == 0)
        {
            return;
        }

        var connection = _connection ?? throw new InvalidOperationException("The broker connection is not open.");
        string topic;
        string payload;
        var tab = line.IndexOf('\t', StringComparison.Ordinal);
        if (tab > 0)
        {
            topic = line[..tab];
            payload = line[(tab + 1)..];
        }
        else if (!string.IsNullOrWhiteSpace(_options.PublishTopic))
        {
            topic = _options.PublishTopic;
            payload = line;
        }
        else
        {
            throw new InvalidOperationException("No publish address is configured. Set one, or send 'topic<TAB>payload'.");
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.TimeoutMs);
        try
        {
            await connection.PublishAsync(topic, Encoding.UTF8.GetBytes(payload), timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Publishing to '{topic}' timed out after {_options.TimeoutMs} ms.");
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await CloseAsync();

    private void OnMessage(string topic, byte[] payload)
    {
        lock (_pipeLock)
        {
            var prefix = Encoding.UTF8.GetBytes(topic + "\t");
            var line = new byte[prefix.Length + payload.Length + 1];
            prefix.CopyTo(line, 0);
            payload.CopyTo(line, prefix.Length);
            line[^1] = (byte)'\n';
            _ = _pipe.Writer.WriteAsync(line).AsTask();
        }
    }

    private void OnDisconnected(Exception? error)
    {
        if (_connection is null)
        {
            return;
        }

        lock (_pipeLock)
        {
            _pipe.Writer.Complete(error);
        }

        State = error is null ? ConnectionState.Closed : ConnectionState.Faulted;
    }
}
