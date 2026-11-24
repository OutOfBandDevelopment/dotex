using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.SpatialServices;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Controllers;

/// <summary>Measures the great-circle distance and initial bearing between two coordinates with <see cref="GeoMath"/>; nothing leaves the server.</summary>
[ApiController]
[Route("api/[controller]")]
public class DistanceController : ControllerBase
{
    private const double _metersPerMile = 1609.344;

    /// <summary>Measures between two coordinates.</summary>
    /// <param name="request">The two coordinates.</param>
    /// <returns>Distance in three units and the bearing.</returns>
    [HttpPost]
    [ApplicationRight("geocode.use")]
    [ProducesResponseType<DistanceResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<DistanceResponseModel> Measure([FromBody] DistanceRequestModel request)
    {
        GlobalPosition from;
        GlobalPosition to;
        try
        {
            from = new GlobalPosition(request.FromLatitude, request.FromLongitude);
            to = new GlobalPosition(request.ToLatitude, request.ToLongitude);
        }
        catch (global::System.ArgumentOutOfRangeException ex)
        {
            return Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Coordinates refused");
        }

        var meters = GeoMath.DistanceMeters(from, to);
        return new DistanceResponseModel(meters, meters / 1000, meters / _metersPerMile, GeoMath.BearingDegrees(from, to));
    }
}
