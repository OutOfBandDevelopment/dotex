using GroqNet;
using GroqNet.ChatCompletions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.GroqCloud;

/// <summary>
/// Represents a health check implementation for the GroqCloud service.
/// </summary>
public class GroqCloudHealthCheck : IHealthCheck
{
    private readonly GroqClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroqCloudHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The GroqCloud API client used for health checks.</param>
    public GroqCloudHealthCheck(
        GroqClient client
        ) => _client = client;

    /// <summary>
    /// Checks the health of the GroqCloud service asynchronously.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous health check operation.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // The Groq client has no cancellation overload, so the wait is cancelled instead
#pragma warning disable CA2016
            var response = await _client.GetChatCompletionsAsync(new GroqChatHistory() { new("Hello") }).WaitAsync(cancellationToken);
#pragma warning restore CA2016
            var result = _client.CurrentRateLimits;
            return HealthCheckResult.Healthy(description: $"Message: \"{response.Choices[0].Message.Content}\"");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(exception: ex);
        }
    }
}
