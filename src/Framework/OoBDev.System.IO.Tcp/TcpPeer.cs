namespace OoBDev.System.IO.Tcp;

/// <summary>One accepted peer: its remote endpoint text (<c>address:port</c>) and the open adapter the front end owns from here on.</summary>
/// <param name="RemoteEndPoint">The peer's address and port.</param>
/// <param name="Adapter">The open adapter; dispose it to drop the peer.</param>
public sealed record TcpPeer(string RemoteEndPoint, IDeviceAdapter Adapter);
