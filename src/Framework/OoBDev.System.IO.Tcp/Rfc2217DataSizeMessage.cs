namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217DataSizeMessage(ComPortDataSize DataSize, bool IsFromServer) : IRfc2217Message;
