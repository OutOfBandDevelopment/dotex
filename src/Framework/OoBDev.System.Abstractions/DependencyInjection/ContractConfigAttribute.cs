using System;

namespace OoBDev.System.DependencyInjection;

/// <summary>
/// Specifies configuration options for dependency injection contracts.
/// </summary>
/// <remarks>
/// This attribute is used to mark interfaces with DI configuration metadata.
/// Migrated from SharedFramework - functionality to be implemented.
/// </remarks>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class ContractConfigAttribute : Attribute
{
    /// <summary>
    /// Gets or sets whether a default implementation is allowed.
    /// </summary>
    public bool AllowDefault { get; set; }

    /// <summary>
    /// Gets or sets the configuration key for selecting implementations.
    /// </summary>
    public string? ConfigKey { get; set; }
}
