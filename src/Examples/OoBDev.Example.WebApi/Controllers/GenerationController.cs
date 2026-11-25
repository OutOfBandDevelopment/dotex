using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.Generations;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Controllers;

/// <summary>Generates seeded sample data with <see cref="Generator"/>; the same seed and type always give the same graph, and nothing is real data.</summary>
[ApiController]
[Route("api/[controller]")]
public class GenerationController : ControllerBase
{
    private static readonly DateTimeOffset _fixedNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Generates one object graph.</summary>
    /// <param name="request">The sample type and the seed.</param>
    /// <returns>The graph as JSON.</returns>
    [HttpPost]
    [ApplicationRight("generation.use")]
    [ProducesResponseType<GenerationResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<GenerationResponseModel> Generate([FromBody] GenerationRequestModel request)
    {
        var generator = new Generator(request.Seed, new FixedClock(_fixedNow));
        object? graph = request.SampleType?.ToLowerInvariant() switch
        {
            "person" => generator.Create<SamplePerson>(),
            "customer" => generator.Create<SampleCustomer>(),
            "order" => generator.Create<SampleOrder>(),
            _ => null,
        };

        if (graph is null)
        {
            return Problem($"Unknown sample type '{request.SampleType}'; use person, customer or order.", statusCode: StatusCodes.Status400BadRequest, title: "Sample type refused");
        }

        return new GenerationResponseModel(request.SampleType!.ToLowerInvariant(), request.Seed, JsonSerializer.SerializeToElement(graph));
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
