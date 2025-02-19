using Microsoft.Extensions.Diagnostics.HealthChecks;
using OllamaSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Ollama;

/// <summary>
/// Represents a health check implementation for the Ollama service.
/// </summary>
public class OllamaHealthCheck : IHealthCheck
{
    private readonly IOllamaApiClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The Ollama API client used for health checks.</param>
    public OllamaHealthCheck(
        IOllamaApiClient client
        ) => _client = client;

    /// <summary>
    /// Checks the health of the Ollama service asynchronously.
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
            var result = await _client.ListLocalModels(cancellationToken);
            return HealthCheckResult.Healthy(description: $"Loaded models: {string.Join("; ", result.Select(m => m.Name))}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(exception: ex);
        }
    }
}
