namespace OoBDev.AspNetCore.AuthGateway;

/// <summary>A client certificate the gateway accepts, by thumbprint.</summary>
public sealed class ClientCertificateEntry
{
    public string Thumbprint { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
}
