namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217ModemStateMaskMessage(Rfc2217ModemState Mask, bool IsFromServer) : IRfc2217Message;
