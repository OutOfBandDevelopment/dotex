using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Indicates that a property to assist in mapping responses.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class QueryResultAttribute : Attribute
{
    /// <summary>
    /// if the position is set to this value the ordinal based mapping will not be attempted
    /// </summary>
    public const int UndefinedPosition = -1;

    /// <summary>
    /// Indicates that a property to assist in mapping responses.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Indicates that a property to assist in mapping responses.
    /// </summary>
    public int Position { get; set; } = UndefinedPosition;
}
