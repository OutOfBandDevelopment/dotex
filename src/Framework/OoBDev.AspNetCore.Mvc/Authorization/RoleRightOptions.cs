using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Role to application right mapping (configuration section <c>OoBDev:Authorization</c>).
/// </summary>
public class RoleRightOptions : IValidatableObject
{
    /// <summary>
    /// Claim types read as roles.
    /// </summary>
    [MinLength(1)]
    public string[] RoleClaimTypes { get; set; } = [ClaimTypes.Role, "roles", "role"];

    /// <summary>
    /// Role name to the rights it grants.
    /// </summary>
    public Dictionary<string, string[]> Roles { get; set; } = [];

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RoleClaimTypes.Any(string.IsNullOrWhiteSpace))
            yield return new ValidationResult("Role claim types must not be blank.", [nameof(RoleClaimTypes)]);
        foreach (var (role, rights) in Roles)
        {
            if (string.IsNullOrWhiteSpace(role))
                yield return new ValidationResult("Role names must not be blank.", [nameof(Roles)]);
            if (rights is null || rights.Any(string.IsNullOrWhiteSpace))
                yield return new ValidationResult($"Role '{role}' has a blank right.", [nameof(Roles)]);
        }
    }
}
