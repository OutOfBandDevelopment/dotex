using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;

namespace OoBDev.WebApi.Controllers;

/// <summary>Schedules demo messages through <c>OoBDev.ComplexEvents</c>; a hosted runner handles each one when it is due.</summary>
/// <param name="demo">The schedule demo.</param>
[ApiController]
[Route("api/[controller]")]
public class ScheduleController(ScheduleDemo demo) : ControllerBase
{
    /// <summary>Lists pending and fired items.</summary>
    /// <returns>The state.</returns>
    [HttpGet]
    [ApplicationRight("schedule.use")]
    [ProducesResponseType<ScheduleStateModel>(StatusCodes.Status200OK)]
    public ActionResult<ScheduleStateModel> Get() => demo.State();

    /// <summary>Schedules an item, or moves the due time of the item with the same key.</summary>
    /// <param name="request">The item.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The state after the change.</returns>
    [HttpPost]
    [ApplicationRight("schedule.use")]
    [ProducesResponseType<ScheduleStateModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScheduleStateModel>> Schedule([FromBody] ScheduleRequestModel request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || request.InSeconds is < 0 or > 3600)
        {
            return Problem("A key is required and InSeconds must be 0 to 3600.", statusCode: StatusCodes.Status400BadRequest, title: "Item refused");
        }

        await demo.ScheduleAsync(request.Key.Trim(), request.InSeconds, request.Message, cancellationToken);
        return demo.State();
    }

    /// <summary>Removes a pending item.</summary>
    /// <param name="key">The key.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The state after the change, or 404 when nothing was pending.</returns>
    [HttpDelete("{key}")]
    [ApplicationRight("schedule.use")]
    [ProducesResponseType<ScheduleStateModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScheduleStateModel>> Release(string key, CancellationToken cancellationToken)
        => await demo.ReleaseAsync(key, cancellationToken) ? demo.State() : NotFound();
}
