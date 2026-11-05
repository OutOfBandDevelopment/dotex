namespace OoBDev.System.IO.Ports.Bridge;

public enum BridgeProtocol
{
    /// <summary>Plain byte stream, no negotiation.</summary>
    Raw,

    /// <summary>Telnet with the RFC 2217 COM-PORT-OPTION: the client can change baud rate, DTR/RTS, etc.</summary>
    Rfc2217,
}
