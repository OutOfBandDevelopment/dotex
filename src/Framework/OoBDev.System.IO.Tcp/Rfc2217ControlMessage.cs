namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217ControlMessage(Rfc2217ControlValue Value, bool IsFromServer) : IRfc2217Message;
