using OoBDev.MailKit.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MailKit;

/// <summary>
/// Represents a health check implementation for checking the connection status of a MailKit IMAP client.
/// </summary>
public class MailkitImapHealthCheck : IHealthCheck
{
    private readonly IImapClientFactory _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="MailkitImapHealthCheck"/> class.
    /// </summary>
    /// <param name="client">The MailKit IMAP client used for health checks.</param>
    public MailkitImapHealthCheck(
        IImapClientFactory client
        ) => _client = client;

    /// <summary>
    /// Checks the health of the MailKit IMAP client asynchronously.
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
