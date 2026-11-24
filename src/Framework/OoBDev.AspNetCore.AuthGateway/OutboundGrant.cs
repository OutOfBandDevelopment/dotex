namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>How the gateway obtains the bearer token it sends to the target.</summary>
public enum OutboundGrant
{
    /// <summary>The gateway's own client (RFC 6749 section 4.4).</summary>
    ClientCredentials,

    /// <summary>The inbound bearer token is exchanged (RFC 8693).</summary>
    TokenExchange,
}
