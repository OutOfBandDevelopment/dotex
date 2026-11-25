namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217LineStateMaskMessage(Rfc2217LineState Mask, bool IsFromServer) : IRfc2217Message;
