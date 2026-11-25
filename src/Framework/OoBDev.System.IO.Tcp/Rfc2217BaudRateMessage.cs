namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217BaudRateMessage(int BaudRate, bool IsFromServer) : IRfc2217Message;
