namespace OoBDev.Keycloak.Identity;

/// <summary>
/// Settings for reading a Keycloak realm through the admin REST API with a service account (client credentials).
/// The client needs the <c>view-users</c> and <c>view-realm</c> realm-management roles.
/// </summary>
public class KeycloakIdentityOptions
{
    /// <summary>Gets or sets the server root, for example <c>http://localhost:8081</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the realm to read.</summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>Gets or sets the service account client id.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Gets or sets the service account client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;
}
