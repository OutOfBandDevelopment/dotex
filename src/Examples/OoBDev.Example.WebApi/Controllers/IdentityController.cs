using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.Keycloak.Identity;
using OoBDev.WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>Read-only view of the identity directory: users, roles and the rights each role translates to.</summary>
/// <param name="directory">The Keycloak admin reader.</param>
/// <param name="mapper">The role to right translation.</param>
[Route("api/[controller]")]
[ApiController]
public class IdentityController(KeycloakDirectoryReader directory, IRoleRightMapper mapper) : ControllerBase
{
    /// <summary>Lists users with their roles and rights.</summary>
    /// <param name="search">Optional text matched against name and email.</param>
    /// <param name="max">The most users to return.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The matching users ordered by sign-in name.</returns>
    [HttpGet("users")]
    [ApplicationRight("identity.read")]
    public async Task<ActionResult<IReadOnlyList<IdentityUserModel>>> Users([FromQuery] string? search = null, [FromQuery] int max = 50, CancellationToken cancellationToken = default)
    {
        var users = await directory.ListUsersAsync(search, max: max, cancellationToken: cancellationToken);
        return Ok(users.OrderBy(u => u.UserName, global::System.StringComparer.OrdinalIgnoreCase).Select(Map).ToList());
    }

    /// <summary>Lists the realm roles with the rights each one grants.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>One entry per role.</returns>
    [HttpGet("roles")]
    [ApplicationRight("identity.read")]
    public async Task<ActionResult<IReadOnlyList<IdentityRoleModel>>> Roles(CancellationToken cancellationToken = default)
    {
        var roles = await directory.ListRolesAsync(cancellationToken);
        return Ok(roles.Select(r => new IdentityRoleModel { Name = r, Rights = [.. mapper.MapRights([r]).Order(global::System.StringComparer.Ordinal)] }).ToList());
    }

    private IdentityUserModel Map(KeycloakUserSummary user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        DisplayName = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
        Enabled = user.Enabled,
        Roles = [.. user.Roles],
        Rights = [.. mapper.MapRights(user.Roles).Order(global::System.StringComparer.Ordinal)],
    };
}
