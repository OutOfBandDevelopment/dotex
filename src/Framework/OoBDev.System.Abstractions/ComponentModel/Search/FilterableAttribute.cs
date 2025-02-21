using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// Allow tagging entity classes to enumerate filterable fields/properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class FilterableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FilterableAttribute"/> class.
    /// </summary>
    public FilterableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterableAttribute"/> class.
    /// </summary>
    public FilterableAttribute() { }

    /// <summary>
    /// column mapping override
    /// </summary>
    public string? TargetName { get; }
}
