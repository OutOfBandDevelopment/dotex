using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert;

/// <summary>
/// Represents a health check implementation for SBERT (Sentence-BERT) service.
/// </summary>
public class SbertHealthCheck : IHealthCheck
{
    private readonly ISentenceEmbeddingClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SbertHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The SBERT client used for health checks.</param>
    public SbertHealthCheck(
        ISentenceEmbeddingClient client
        ) => _client = client;

    /// <summary>
    /// Checks the health of the SBERT service asynchronously.
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
            var result = await _client.GetHealthAsync();
            return HealthCheckResult.Healthy(description: result);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(exception: ex);
        }
    }
}
