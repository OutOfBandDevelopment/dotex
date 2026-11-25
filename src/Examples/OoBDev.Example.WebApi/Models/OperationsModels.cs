using System;
using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>One configuration setting; secret-looking values are masked.</summary>
public class ConfigurationEntryModel
{
    /// <summary>Gets or sets the configuration path (sections separated by colons).</summary>
    public required string Key { get; set; }

    /// <summary>Gets or sets the value, or <c>***</c> when the key looks like a secret.</summary>
    public string? Value { get; set; }
}

/// <summary>Runtime facts and the effective configuration.</summary>
public class OperationsInfoModel
{
    /// <summary>Gets or sets the hosting environment name.</summary>
    public required string Environment { get; set; }

    /// <summary>Gets or sets the application name.</summary>
    public required string Application { get; set; }

    /// <summary>Gets or sets the .NET runtime description.</summary>
    public required string Runtime { get; set; }

    /// <summary>Gets or sets the process start time.</summary>
    public DateTimeOffset Started { get; set; }

    /// <summary>Gets or sets the machine name.</summary>
    public required string Machine { get; set; }

    /// <summary>Gets or sets the effective configuration, sorted by key.</summary>
    public required IList<ConfigurationEntryModel> Configuration { get; set; }
}

/// <summary>One health check result.</summary>
public class HealthEntryModel
{
    /// <summary>Gets or sets the registered check name, for example <c>ollama</c>.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets <c>Healthy</c>, <c>Degraded</c> or <c>Unhealthy</c>.</summary>
    public required string Status { get; set; }

    /// <summary>Gets or sets the check description, when it gave one.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets how long the check took, in milliseconds.</summary>
    public double DurationMs { get; set; }
}

/// <summary>The combined health of the configured providers.</summary>
public class HealthReportModel
{
    /// <summary>Gets or sets the worst status of all checks.</summary>
    public required string Status { get; set; }

    /// <summary>Gets or sets one entry per registered check, sorted by name.</summary>
    public required IList<HealthEntryModel> Entries { get; set; }
}
