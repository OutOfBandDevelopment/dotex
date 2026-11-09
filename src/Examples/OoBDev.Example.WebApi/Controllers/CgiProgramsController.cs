using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System;
using System.Collections.Generic;

namespace OoBDev.WebApi.Controllers;

/// <summary>Lists the CGI programs the example host can run (the programs themselves are served by the CGI middleware).</summary>
/// <param name="catalog">The program catalog.</param>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class CgiProgramsController(CgiProgramCatalog catalog) : ControllerBase
{
    /// <summary>Lists the runnable programs; Linux binaries are left out on Windows.</summary>
    /// <returns>The programs and their URL paths.</returns>
    [HttpGet]
    public ActionResult<IReadOnlyList<CgiProgramModel>> List() => Ok(catalog.List(OperatingSystem.IsWindows()));
}
