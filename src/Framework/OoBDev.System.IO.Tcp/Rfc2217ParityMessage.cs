namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217ParityMessage(ComPortParity Parity, bool IsFromServer) : IRfc2217Message;
