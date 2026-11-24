using System;
using System.Collections.Generic;

namespace OoBDev.AspNetCore.AuthGateway;

public sealed class AuthGatewayOptions
{
    public string ApiKeyHeader { get; set; } = "X-Api-Key";

    public TimeSpan TokenSkew { get; set; } = TimeSpan.FromSeconds(30);

    public IDictionary<string, AuthGatewayTarget> Targets { get; set; } = new Dictionary<string, AuthGatewayTarget>(StringComparer.OrdinalIgnoreCase);

    public IList<ApiKeyEntry> ApiKeys { get; set; } = [];

    public IList<ClientCertificateEntry> ClientCertificates { get; set; } = [];
}
