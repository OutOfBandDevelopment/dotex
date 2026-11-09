namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217StopSizeMessage(ComPortStopSize StopSize, bool IsFromServer) : IRfc2217Message;
