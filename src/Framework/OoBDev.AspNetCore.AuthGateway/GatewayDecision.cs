namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>The outcome for one request: a bearer token to forward, or a refusal.</summary>
public sealed record GatewayDecision(int StatusCode, string Title, string? BearerToken = null, GatewayCaller? Caller = null)
{
    public bool Allowed => BearerToken is not null;
}
