using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.System.Security.Claims;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.WebApi.Controllers;

/// <summary>Shows the authorization model: which rights each endpoint needs and which the caller holds.</summary>
/// <param name="actions">The host's action descriptors.</param>
/// <param name="catalog">Builds the endpoint list.</param>
/// <param name="checker">The application right checker.</param>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class RightsController(
    IActionDescriptorCollectionProvider actions,
    EndpointRightCatalog catalog,
    IApplicationRightChecker checker) : ControllerBase
{
    /// <summary>Lists every endpoint with the access it needs.</summary>
    /// <returns>The endpoints ordered by route.</returns>
    [HttpGet("endpoints")]
    public ActionResult<IReadOnlyList<EndpointRightModel>> Endpoints() => Ok(catalog.Describe(actions.ActionDescriptors.Items));

    /// <summary>Checks the current user against the listed rights (any one is enough).</summary>
    /// <param name="rights">The rights to check.</param>
    /// <returns>The outcome with the rights the user holds.</returns>
    [HttpGet("check")]
    public ActionResult<RightCheckModel> Check([FromQuery] string[] rights) => Ok(new RightCheckModel
    {
        Required = rights,
        Held = [.. User.FindAll(CommonClaims.ApplicationRight).Select(c => c.Value).Order()],
        Result = checker.Check(User, rights).ToString(),
    });

    /// <summary>Needs the <c>rights.demo</c> right; the screen lists it and the check shows why a call is refused.</summary>
    /// <returns>A confirmation when the caller holds the right.</returns>
    [HttpGet("protected")]
    [ApplicationRight("rights.demo")]
    public ActionResult<string> Protected() => Ok("You hold the rights.demo right.");
}
