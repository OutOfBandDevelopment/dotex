namespace OoBDev.WebApi.Models;

/// <summary>A directory user with the roles Keycloak holds and the application rights those roles translate to.</summary>
public class IdentityUserModel
{
    /// <summary>Gets or sets the directory id.</summary>
    public required string Id { get; set; }

    /// <summary>Gets or sets the sign-in name.</summary>
    public required string UserName { get; set; }

    /// <summary>Gets or sets the email address.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the display name (first and last name).</summary>
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets whether the account is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets the realm roles.</summary>
    public string[] Roles { get; set; } = [];

    /// <summary>Gets or sets the application rights the roles map to.</summary>
    public string[] Rights { get; set; } = [];
}
