using System.IO.Ports;

namespace OoBDev.System.IO.Ports.Bridge;

public sealed class SystemSerialLink : ISerialLink
{
    private readonly SerialPort _port;
    private int _disposed;

    public SystemSerialLink(BridgeOptions options)
    {
        _port = new SerialPort(options.ComPort, options.BaudRate, options.Parity, options.DataBits, options.StopBits)
        {
            Handshake = Handshake.None,
            ReadTimeout = Timeout.Infinite,
            WriteTimeout = Timeout.Infinite,
        };

        try
        {
            _port.Open();
        }
        catch
        {
            _port.Dispose();
            throw;
        }
    }

    public Stream Stream => _port.BaseStream;

    public int BaudRate { get => _port.BaudRate; set => _port.BaudRate = value; }

    public int DataBits { get => _port.DataBits; set => _port.DataBits = value; }

    public Parity Parity { get => _port.Parity; set => _port.Parity = value; }

    public StopBits StopBits { get => _port.StopBits; set => _port.StopBits = value; }

    public Handshake Handshake { get => _port.Handshake; set => _port.Handshake = value; }

    public bool DtrEnable { get => _port.DtrEnable; set => _port.DtrEnable = value; }

    public bool RtsEnable { get => _port.RtsEnable; set => _port.RtsEnable = value; }

    public bool BreakState { get => _port.BreakState; set => _port.BreakState = value; }

    public bool CtsHolding => _port.CtsHolding;

    public bool DsrHolding => _port.DsrHolding;

    public bool CDHolding => _port.CDHolding;

    public void DiscardInBuffer() => _port.DiscardInBuffer();

    public void DiscardOutBuffer() => _port.DiscardOutBuffer();

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            _port.Dispose();
        }
        catch (IOException)
        {
            // Device already gone (USB unplug); nothing left to release.
        }
    }
}
