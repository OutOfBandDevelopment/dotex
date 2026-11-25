namespace OoBDev.System.IO.Tcp;

public enum TcpEndpointMode
{
    /// <summary>Connect out to a remote host:port.</summary>
    Client,

    /// <summary>Bind a local port and accept one inbound connection.</summary>
    Listener,
}
