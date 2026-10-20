using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Shows the most recent traces this API sent to the Aspire dashboard, read back through the dashboard telemetry API.
/// Settings: <c>Telemetry:DashboardUrl</c> (server side), <c>Telemetry:PublicUrl</c> (link for the browser) and <c>Telemetry:ApiKey</c>.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class TelemetryController(IConfiguration configuration, IHttpClientFactory httpClientFactory) : ControllerBase
{
    /// <summary>Lists the most recent root spans of this service.</summary>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The dashboard link and the recent traces, newest first.</returns>
    [HttpGet("recent")]
    public async Task<ActionResult<TelemetryRecentModel>> Recent(CancellationToken cancellationToken)
    {
        var baseUrl = (configuration["Telemetry:DashboardUrl"] ?? "http://localhost:18888").TrimEnd('/');
        var publicUrl = (configuration["Telemetry:PublicUrl"] ?? baseUrl).TrimEnd('/');
        var service = configuration["OpenTelemetryOptions:ServiceName"];
        var model = new TelemetryRecentModel { DashboardUrl = publicUrl, Traces = [] };
        if (string.IsNullOrWhiteSpace(service))
            return model;

        try
        {
            using var client = httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/telemetry/traces?limit=200");
            request.Headers.Add("x-api-key", configuration["Telemetry:ApiKey"] ?? "");
            using var response = await client.SendAsync(request, cancellationToken);
            // The dashboard answers 404 until it has received telemetry for the service
            if (response.StatusCode == global::System.Net.HttpStatusCode.NotFound)
                return model;
            response.EnsureSuccessStatusCode();
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            model.Traces = ReadSpans(document.RootElement, service).OrderByDescending(t => t.Started).Take(20).ToList();
            return model;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return Problem(title: "The dashboard could not be read", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static IEnumerable<TelemetryTraceModel> ReadSpans(JsonElement root, string service)
    {
        if (!root.TryGetProperty("data", out var data) || !data.TryGetProperty("resourceSpans", out var resources)) yield break;
        foreach (var resource in resources.EnumerateArray())
        {
            if (!IsService(resource, service) || !resource.TryGetProperty("scopeSpans", out var scopes)) continue;
            foreach (var scope in scopes.EnumerateArray())
            {
                if (!scope.TryGetProperty("spans", out var spans)) continue;
                foreach (var span in spans.EnumerateArray())
                {
                    var start = long.Parse(span.GetProperty("startTimeUnixNano").GetString()!);
                    var end = long.Parse(span.GetProperty("endTimeUnixNano").GetString()!);
                    yield return new TelemetryTraceModel
                    {
                        TraceId = span.GetProperty("traceId").GetString()!,
                        Name = span.GetProperty("name").GetString()!,
                        Started = DateTimeOffset.FromUnixTimeMilliseconds(start / 1_000_000),
                        DurationMs = (end - start) / 1_000_000.0,
                    };
                }
            }
        }
    }

    private static bool IsService(JsonElement resource, string service) =>
        resource.TryGetProperty("resource", out var r) && r.TryGetProperty("attributes", out var attributes)
        && attributes.EnumerateArray().Any(a => a.GetProperty("key").GetString() == "service.name"
            && a.GetProperty("value").GetProperty("stringValue").GetString() == service);
}
