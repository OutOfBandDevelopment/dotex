namespace OoBDev.Microsoft.B2C.Identity;

/// <summary>
/// Contains keys related to Azure Active Directory B2C configuration.
/// </summary>
public class MicrosoftIdentityOptions
{
    /// <summary>
    /// Represents the key for the Azure AD B2C client ID configuration.
    /// </summary>
    public required string ClientID { get; set; }

    /// <summary>
    /// Represents the key for the Azure AD B2C issuer configuration.
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// Represents the key for the Azure AD B2C client secret configuration.
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Represents the key for the Azure AD B2C tenant configuration.
    /// </summary>
    public required string Tenant { get; set; }
}
