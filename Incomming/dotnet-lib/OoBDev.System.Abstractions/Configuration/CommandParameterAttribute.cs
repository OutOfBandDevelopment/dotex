using System;

namespace OoBDev.System.Configuration;

/// <summary>
/// Specifies that a property represents a command parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CommandParameterAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the short representation of the command parameter.
    /// </summary>
    public string? Short { get; set; }

    /// <summary>
    /// Gets or sets the value of the command parameter.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets a unique identifier for this attribute.
    /// </summary>
    public override object TypeId => this;
}
