using OoBDev.MailKit.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MailKit;

/// <summary>
/// Represents a health check implementation for checking the connection status of a MailKit SMTP client.
/// </summary>
public class MailkitSmtpHealthCheck : IHealthCheck
{
    private readonly ISmtpClientFactory _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MailkitSmtpHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The MailKit SMTP client used for health checks.</param>
    public MailkitSmtpHealthCheck(
        ISmtpClientFactory client
        ) => _client = client;

    /// <summary>
    /// Checks the health of the MailKit SMTP client asynchronously.
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
            using var client = await _client.CreateAsync();
            return HealthCheckResult.Healthy(description: "Connected");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded(exception: ex);
        }
    }
}
