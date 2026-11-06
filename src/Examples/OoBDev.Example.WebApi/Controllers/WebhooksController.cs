using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.Communications.Abstractions;

namespace OoBDev.WebApi.Controllers;

/// <summary>Receives inbound provider calls (delivery status, inbound messages) for a configured channel.</summary>
/// <param name="receive">The receive service.</param>
/// <param name="timeProvider">The clock used to stamp the call.</param>
[ApiController]
[Route("webhooks")]
public class WebhooksController(IReceiveService receive, TimeProvider timeProvider) : ControllerBase
{
    /// <summary>Verifies and processes a provider call; the vendor signature is the credential, so the route is anonymous.</summary>
    /// <param name="channel">The channel name from <c>OoBDev:Communications:Channels</c>.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>204 when processed; 404 for an unknown channel; 400 for a bad signature, stale call or payload (problem details).</returns>
    [HttpPost("{channel}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Receive(string channel, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await Request.Body.CopyToAsync(buffer, cancellationToken);

        var request = new WebhookRequest(
            buffer.ToArray(),
            Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase),
            Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString(), StringComparer.Ordinal),
            timeProvider.GetUtcNow());

        var result = await receive.HandleWebhookAsync(channel, request, cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        var notFound = result.ErrorMessage?.Contains("not found", StringComparison.Ordinal) == true;
        return Problem(
            result.ErrorMessage,
            statusCode: notFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
            title: "Webhook rejected");
    }
}
