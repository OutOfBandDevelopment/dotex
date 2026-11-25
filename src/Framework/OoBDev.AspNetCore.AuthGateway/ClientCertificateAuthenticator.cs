using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;

namespace OoBDev.AspNetCore.AuthGateway;

public sealed class ClientCertificateAuthenticator(IOptions<AuthGatewayOptions> options) : IInboundAuthenticator
{
    public InboundScheme Scheme => InboundScheme.ClientCertificate;

    public GatewayCaller? Authenticate(HttpContext context)
    {
        var certificate = context.Connection.ClientCertificate;
        if (certificate is null)
        {
            return null;
        }

        foreach (var entry in options.Value.ClientCertificates)
        {
            if (string.Equals(entry.Thumbprint, certificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return new GatewayCaller(InboundScheme.ClientCertificate, entry.Subject);
            }
        }

        return null;
    }
}
