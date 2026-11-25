namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>An authenticated caller: the scheme that accepted it and its subject.</summary>
public sealed record GatewayCaller(InboundScheme Scheme, string Subject);
