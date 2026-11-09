namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217PurgeDataMessage(Rfc2217PurgeTarget Target, bool IsFromServer) : IRfc2217Message;
