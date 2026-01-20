using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="UserController"/> class.
/// </summary>
/// <param name="claimsPrincipal">The claims principal representing the current user.</param>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(
    ClaimsPrincipal claimsPrincipal
    )
{
    /// <summary>
    /// Gets the claims associated with the current user.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="ClaimModel"/> representing the user's claims.</returns>
    [HttpGet("claims")]
    public IEnumerable<ClaimModel> GetClaims() =>
         from claim in claimsPrincipal.Claims
         select new ClaimModel
         {
             Issuer = claim.Issuer,
             Value = claim.Value,
             ValueType = claim.ValueType,
             Type = claim.Type,
             OriginalIssuer = claim.OriginalIssuer,
             SubjectName = claim.Subject?.Name,
             SubjectLabel = claim.Subject?.Label,
             SubjectAuthenticationType = claim.Subject?.AuthenticationType,
         };
}
