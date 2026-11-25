using System.IO.Ports;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Line settings and control signals of an open serial port (what RFC 2217 lets a client change).</summary>
public interface ISerialControl
{
    public int BaudRate { get; set; }

    public int DataBits { get; set; }

    public Parity Parity { get; set; }

    public StopBits StopBits { get; set; }

    public Handshake Handshake { get; set; }

    public bool DtrEnable { get; set; }

    public bool RtsEnable { get; set; }

    public bool BreakState { get; set; }

    public bool CtsHolding { get; }

    public bool DsrHolding { get; }

    public bool CDHolding { get; }

    public void DiscardInBuffer();

    public void DiscardOutBuffer();
}
