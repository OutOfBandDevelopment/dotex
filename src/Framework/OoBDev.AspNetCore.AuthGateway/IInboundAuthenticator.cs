using Microsoft.AspNetCore.Http;

namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>Recognises one inbound credential kind.</summary>
public interface IInboundAuthenticator
{
    public InboundScheme Scheme { get; }

    /// <summary>The caller, or null when the request carries no valid credential of this kind.</summary>
    public GatewayCaller? Authenticate(HttpContext context);
}
