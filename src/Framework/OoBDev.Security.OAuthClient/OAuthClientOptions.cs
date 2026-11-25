using System;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.Security.OAuthClient;

/// <summary>Settings for the OAuth client: an issuer and a client id are all a public client needs.</summary>
public sealed class OAuthClientOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "OoBDev:OAuthClient";

    /// <summary>The issuer address; discovery reads <c>{Issuer}/.well-known/openid-configuration</c>.</summary>
    [Required]
    public Uri? Issuer { get; set; }

    /// <summary>The public client id registered with the provider (no secret).</summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>The scopes to ask for; <c>offline_access</c> is what yields a refresh token.</summary>
    public string[] Scopes { get; set; } = ["openid", "offline_access"];

    /// <summary>A token that expires within this time is refreshed before use.</summary>
    public TimeSpan Skew { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>How long the loopback listener waits for the browser to come back.</summary>
    public TimeSpan LoopbackTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>The name the tokens are stored under in the <see cref="ISecretStore"/>.</summary>
    [Required]
    public string SecretName { get; set; } = "oauth-tokens";

    /// <summary>The folder for <see cref="FileSecretStore"/>; defaults to <c>%LOCALAPPDATA%/OoBDev/oauth</c> (or the platform equivalent).</summary>
    public string? SecretFolder { get; set; }
}
