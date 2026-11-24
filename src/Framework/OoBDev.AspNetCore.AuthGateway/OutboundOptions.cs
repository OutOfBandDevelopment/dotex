using System.ComponentModel.DataAnnotations;

namespace OoBDev.AspNetCore.AuthGateway;

public sealed class OutboundOptions
{
    public OutboundGrant Grant { get; set; } = OutboundGrant.ClientCredentials;

    [Required]
    public string TokenEndpoint { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    public string? ClientSecret { get; set; }

    public string? Scope { get; set; }

    public string? Audience { get; set; }
}
