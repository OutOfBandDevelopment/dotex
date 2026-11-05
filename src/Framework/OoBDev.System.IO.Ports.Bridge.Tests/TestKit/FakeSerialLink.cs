using System.IO.Pipelines;
using System.IO.Ports;

namespace OoBDev.System.IO.Ports.Bridge.Tests.TestKit;

/// <summary>In-memory serial device. The "device" side is what a physical gadget would see.</summary>
public sealed class FakeSerialLink : ISerialLink
{
    private readonly Pipe _deviceToHost = new();
    private readonly Pipe _hostToDevice = new();
    private int _disposed;

    public FakeSerialLink()
    {
        Stream = new DuplexStream(_deviceToHost.Reader.AsStream(), _hostToDevice.Writer.AsStream());
    }

    public Stream Stream { get; }

    public int BaudRate { get; set; } = 115200;

    public int DataBits { get; set; } = 8;

    public global::System.IO.Ports.Parity Parity { get; set; } = global::System.IO.Ports.Parity.None;

    public global::System.IO.Ports.StopBits StopBits { get; set; } = global::System.IO.Ports.StopBits.One;

    public Handshake Handshake { get; set; } = Handshake.None;

    public bool DtrEnable { get; set; }

    public bool RtsEnable { get; set; }

    public bool BreakState { get; set; }

    public bool CtsHolding { get; set; }

    public bool DsrHolding { get; set; }

    public bool CDHolding { get; set; }

    public int InPurges { get; private set; }

    public int OutPurges { get; private set; }

    public void DiscardInBuffer() => InPurges++;

    public void DiscardOutBuffer() => OutPurges++;

    public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>Bytes the device "transmits" towards the host.</summary>
    public async Task DeviceSendAsync(byte[] data) => await _deviceToHost.Writer.WriteAsync(data);

    /// <summary>Reads exactly <paramref name="count"/> bytes the host wrote to the device.</summary>
    public async Task<byte[]> DeviceReceiveAsync(int count, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var stream = _hostToDevice.Reader.AsStream();
        var buffer = new byte[count];
        var read = 0;
        while (read < count)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read), cts.Token);
            if (n == 0)
            {
                break;
            }

            read += n;
        }

        return buffer[..read];
    }

    /// <summary>Simulates a USB unplug: the host's next read fails.</summary>
    public void Unplug() => _deviceToHost.Writer.Complete(new IOException("Device unplugged."));

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _deviceToHost.Writer.Complete();
    }
}
