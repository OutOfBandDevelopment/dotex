using OoBDev.System.IO;
using System;
using System.Threading.Tasks;

namespace OoBDev.ZWave;

/// <summary>
/// Owns a stick's stream device: acknowledges every unit it receives (<see cref="ZWaveAckResponder"/>), keeps a
/// <see cref="ZWaveNodeTable"/> current and relays the frames, so a consumer only reads frames and sends requests.
/// Build the stream device with no minimum transmission delay, or an acknowledgement can wait behind it past the stick's timeout.
/// </summary>
public sealed class ZWaveSession : IDisposable
{
    private readonly IStreamDevice<ZWaveFrame> _device;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZWaveSession"/> class and starts handling received frames.
    /// </summary>
    /// <param name="device">The stream device that carries the stick's frames.</param>
    /// <param name="nodes">The table to keep current, or <see langword="null"/> for a new one.</param>
    public ZWaveSession(IStreamDevice<ZWaveFrame> device, ZWaveNodeTable? nodes = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        _device = device;
        Nodes = nodes ?? new ZWaveNodeTable();
        _device.MessageReceived += OnMessageReceived;
    }

    /// <summary>Gets the per-node facts built from the frames received so far.</summary>
    public ZWaveNodeTable Nodes { get; }

    /// <summary>Occurs for every unit received, after its acknowledgement is queued and the node table updated.</summary>
    public event EventHandler<ZWaveFrame>? FrameReceived;

    /// <summary>Occurs when an acknowledgement could not be sent; the stick will retransmit and then answer CAN.</summary>
    public event EventHandler<Exception>? AnswerFailed;

    /// <summary>Queues a request for the stick.</summary>
    /// <param name="request">A frame from <see cref="ZWaveRequests"/>.</param>
    /// <returns><see langword="true"/> when the device accepted the frame.</returns>
    public Task<bool> SendAsync(ZWaveFrame request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _device.Transmit(request);
    }

    /// <inheritdoc />
    public void Dispose() => _device.MessageReceived -= OnMessageReceived;

    private async void OnMessageReceived(object? sender, ZWaveFrame frame)
    {
        try
        {
            if (ZWaveAckResponder.Respond(frame) is { } answer && !await _device.Transmit(answer).ConfigureAwait(false))
            {
                AnswerFailed?.Invoke(this, new InvalidOperationException("The device did not accept the acknowledgement."));
            }
        }
        catch (Exception ex)
        {
            AnswerFailed?.Invoke(this, ex);
        }

        Nodes.Apply(frame);
        FrameReceived?.Invoke(this, frame);
    }
}
