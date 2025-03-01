using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Indicates that a property should be included as a query parameter in a request.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class QueryParameterAttribute : Attribute
{
    /// <summary>
    /// Indicates that a property should be included as a query parameter in a request.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Indicates to the mapping provide that this property should be serialized as JSON for the request
    /// </summary>
    public bool IsJson { get; set; } = false;
}
