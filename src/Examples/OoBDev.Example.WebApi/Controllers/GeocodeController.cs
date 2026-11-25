using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.SpatialServices;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Controllers;

/// <summary>Looks up the coordinates of a US address through <see cref="ILocationServices"/> (the Census geocoder). The typed address leaves the server for that public service.</summary>
/// <param name="locations">The location service.</param>
[ApiController]
[Route("api/[controller]")]
public class GeocodeController(ILocationServices locations) : ControllerBase
{
    /// <summary>Geocodes one address.</summary>
    /// <param name="request">The address.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The match, or a result with <c>Matched</c> false.</returns>
    [HttpPost]
    [ApplicationRight("geocode.use")]
    [ProducesResponseType<GeocodeResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GeocodeResponseModel>> Geocode([FromBody] GeocodeRequestModel request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Line1))
        {
            return Problem("A street line is required.", statusCode: StatusCodes.Status400BadRequest, title: "Address refused");
        }

        var result = await locations.GeocodeAsync(new AddressQuery(request.Line1.Trim(), null, request.City, request.Region, request.PostalCode), cancellationToken);
        return new GeocodeResponseModel(result.Position is not null, result.Position?.Latitude, result.Position?.Longitude, result.Quality.ToString(), result.NormalizedAddress);
    }
}
