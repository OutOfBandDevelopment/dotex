namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>A credential kind the gateway accepts from callers.</summary>
public enum InboundScheme
{
    ApiKey,
    ClientCertificate,
}
