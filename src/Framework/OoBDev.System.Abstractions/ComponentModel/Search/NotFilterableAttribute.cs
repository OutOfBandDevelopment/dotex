using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// Specifies that a property or class should be explicitly excluded from filter selection.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class NotFilterableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFilterableAttribute"/> class with a target name.
    /// </summary>
    /// <param name="targetName">The target name to be excluded from filter selection.</param>
    public NotFilterableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFilterableAttribute"/> class without a target name.
    /// </summary>
    public NotFilterableAttribute()
    {
    }

    /// <summary>
    /// Gets the target name to be excluded from filter selection.
    /// </summary>
    public string? TargetName { get; }
}
