using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.EventSourcing.Web;

/// <summary>Maps the CloudEvents HTTP edge for the event store.</summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps <c>POST {pattern}</c> for one structured CloudEvent (<c>application/cloudevents+json</c>) that is appended to the selected event store.
    /// Answers 202 when accepted, 400 for an invalid event, 409 when the event does not follow its stream and 415 for another content type, all as problem details.
    /// The endpoint carries no policy: chain <c>RequireAuthorization</c> on the result.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, <c>/events</c> by default.</param>
    public static RouteHandlerBuilder MapCloudEvents(this IEndpointRouteBuilder endpoints, string pattern = "/events")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints
            .MapPost(pattern, Accept)
            .WithName("PostCloudEvent")
            .Produces(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status415UnsupportedMediaType);
    }

    private static async Task<IResult> Accept(HttpRequest request, [FromKeyedServices(EventSourcingGlobals.EventStoreSelectedKey)] IEventStore store, CancellationToken cancellationToken)
    {
        if (!(request.ContentType?.StartsWith(CloudEventMapper.ContentType, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return Results.Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, title: "Unsupported media type", detail: $"Send a structured CloudEvent as {CloudEventMapper.ContentType}.");
        }

        EventRecord record;
        try
        {
            using var reader = new StreamReader(request.Body, Encoding.UTF8);
            record = CloudEventMapper.FromCloudEvent(await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
        }
        catch (Exception e) when (e is FormatException or JsonException or ArgumentException)
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid CloudEvent", detail: e.Message);
        }

        var result = await store.AppendAsync(record.StreamId, record.Sequence - 1, [new NewEvent(record.Type, record.Payload, record.Metadata)], cancellationToken).ConfigureAwait(false);
        return result.Succeeded
            ? Results.Accepted()
            : Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Event does not follow its stream", detail: $"{record.StreamId}:{record.Sequence} does not follow the stream (last sequence {result.LastSequence}).");
    }
}
