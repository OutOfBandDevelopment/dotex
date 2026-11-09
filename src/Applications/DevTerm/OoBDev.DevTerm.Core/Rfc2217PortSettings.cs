using System.IO.Ports;

namespace OoBDev.DevTerm;

/// <summary>The serial settings a remote RFC 2217 client has asked for.</summary>
public sealed record Rfc2217PortSettings(int BaudRate = 9600, int DataBits = 8, Parity Parity = Parity.None, StopBits StopBits = StopBits.One, bool Dtr = true, bool Rts = true);
