using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// explicitly exclude properties from search
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class NotSearchableAttribute : Attribute
{
    /// <summary>
    /// explicitly exclude properties from search
    /// </summary>
    /// <param name="targetName">virtual property to target</param>
    public NotSearchableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// explicitly exclude properties from search
    /// </summary>
    public NotSearchableAttribute() { }

    /// <summary>
    /// Target name required only if this is used on the class
    /// </summary>
    public string? TargetName { get; }
}
