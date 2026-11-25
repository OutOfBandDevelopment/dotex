using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Runtime facts and the effective configuration for operators. Values whose key looks like a secret are masked.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApplicationRight("operations.read")]
public class OperationsController(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    HealthCheckService health
    ) : ControllerBase
{
    private static readonly string[] SecretWords = ["secret", "password", "pwd", "key", "token", "connectionstring", "credential"];

    /// <summary>
    /// Gets the runtime facts and the configuration.
    /// </summary>
    /// <returns>The operations summary.</returns>
    [HttpGet("info")]
    public OperationsInfoModel Info() => new()
    {
        Environment = environment.EnvironmentName,
        Application = environment.ApplicationName,
        Runtime = RuntimeInformation.FrameworkDescription,
        Machine = global::System.Environment.MachineName,
        Started = new DateTimeOffset(Process.GetCurrentProcess().StartTime.ToUniversalTime()),
        Configuration = [.. configuration.AsEnumerable()
            .Where(kv => kv.Value is not null)
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ConfigurationEntryModel { Key = kv.Key, Value = Mask(kv.Key, kv.Value) })],
    };

    /// <summary>
    /// Runs the registered health checks (one per configured provider, such as Qdrant, SQL or mail) and reports each result.
    /// </summary>
    /// <param name="cancellationToken">Cancels the checks.</param>
    /// <returns>The combined status and one entry per check.</returns>
    [HttpGet("health")]
    public async Task<HealthReportModel> Health(CancellationToken cancellationToken)
    {
        var report = await health.CheckHealthAsync(cancellationToken);
        return new HealthReportModel
        {
            Status = report.Status.ToString(),
            Entries = [.. report.Entries
                .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                .Select(e => new HealthEntryModel
                {
                    Name = e.Key,
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    DurationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1),
                })],
        };
    }

    private static string? Mask(string key, string? value) =>
        SecretWords.Any(w => key.Contains(w, StringComparison.OrdinalIgnoreCase)) ? "***" : value;
}
