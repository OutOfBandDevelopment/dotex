namespace OoBDev.WebApi.Models;

/// <summary>A realm role and the application rights it grants.</summary>
public class IdentityRoleModel
{
    /// <summary>Gets or sets the role name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the rights the role maps to.</summary>
    public string[] Rights { get; set; } = [];
}
