using OoBDev.System.Linq.Search;
using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// part of default sort for entity
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class DefaultSortAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSortAttribute"/> class.
    /// </summary>
    /// <param name="targetName">The property name to use for mapping.</param>
    /// <param name="priority">The sort column position priority.</param>
    /// <param name="order">The direction to order this mapped column.</param>
    public DefaultSortAttribute(
        string? targetName = default,
        int priority = default,
        OrderDirections order = OrderDirections.Ascending
        )
    {
        TargetName = targetName;
        Priority = priority;
        Order = order;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultSortAttribute"/> class.
    /// </summary>
    public DefaultSortAttribute() { }

    /// <summary>
    /// property name to use for mapping
    /// </summary>
    public string? TargetName { get; }

    /// <summary>
    /// sort column position priority 
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// direction to order this mapped column
    /// </summary>
    public OrderDirections Order { get; }
}
