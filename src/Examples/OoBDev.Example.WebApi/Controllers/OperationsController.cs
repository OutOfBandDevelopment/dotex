using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Runtime facts and the effective configuration for operators. Values whose key looks like a secret are masked.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OperationsController(
    IConfiguration configuration,
    IWebHostEnvironment environment
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


    private static string? Mask(string key, string? value) =>
        SecretWords.Any(w => key.Contains(w, StringComparison.OrdinalIgnoreCase)) ? "***" : value;
}
