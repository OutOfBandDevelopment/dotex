using System.ComponentModel.DataAnnotations;

namespace OoBDev.Communications.Unsubscribe;

/// <summary>Options of the one-click unsubscribe links (RFC 8058).</summary>
public sealed class UnsubscribeOptions
{
    /// <summary>The HTTPS endpoint that receives the unsubscribe request; the token is added as the <c>t</c> query value.</summary>
    [Required]
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>The secret that signs tokens (at least 32 characters). Changing it invalidates links already sent.</summary>
    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;
}
