namespace OoBDev.System.IO.Tcp;

internal sealed record Rfc2217FlowControlResumeMessage(bool IsFromServer) : IRfc2217Message;
