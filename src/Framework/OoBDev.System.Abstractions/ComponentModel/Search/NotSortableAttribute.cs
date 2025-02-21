using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// Specifies that a property or class should not be sortable.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class NotSortableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotSortableAttribute"/> class.
    /// </summary>
    /// <param name="targetName">The name of the target property that should not be sortable.</param>
    public NotSortableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotSortableAttribute"/> class.
    /// </summary>
    public NotSortableAttribute() { }

    /// <summary>
    /// Gets the name of the target property that should not be sortable.
    /// </summary>
    public string? TargetName { get; }
}
