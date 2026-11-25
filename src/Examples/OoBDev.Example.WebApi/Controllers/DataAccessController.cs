using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.DacFx;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;

namespace OoBDev.WebApi.Controllers;

/// <summary>A read-only SQL query runner: one SELECT, checked by a parser, run with the read-only intent, a row cap and a timeout, on an instance named in <c>OoBDev:DataAccess</c>.</summary>
/// <param name="runner">The query runner.</param>
[ApiController]
[Route("api/[controller]")]
public class DataAccessController(ReadOnlyQueryRunner runner) : ControllerBase
{
    /// <summary>Runs one SELECT.</summary>
    /// <param name="request">The query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The columns and rows, or a problem when the text is refused, the runner is off or the instance fails.</returns>
    [HttpPost("query")]
    [ApplicationRight("data.query")]
    [ProducesResponseType<DataQueryResultModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<DataQueryResultModel>> Query([FromBody] DataQueryRequestModel request, CancellationToken cancellationToken)
    {
        if (!runner.IsConfigured)
        {
            return Problem("Set OoBDev:DataAccess:ConnectionString to a test or reporting instance.", statusCode: StatusCodes.Status404NotFound, title: "Data access is not configured");
        }

        var check = SelectOnlyGuard.Check(request.Sql);
        if (!check.IsAllowed)
        {
            return Problem(check.Reason, statusCode: StatusCodes.Status400BadRequest, title: "Query refused");
        }

        try
        {
            return await runner.RunAsync(request.Sql, cancellationToken);
        }
        catch (Exception ex) when (ex is global::Microsoft.Data.SqlClient.SqlException or InvalidOperationException or TimeoutException)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status502BadGateway, title: "Query failed");
        }
    }
}
