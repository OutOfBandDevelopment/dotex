namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217FlowControlSuspendMessage(bool IsFromServer) : IRfc2217Message;
