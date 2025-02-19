using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// include field to be searchable for "SearchTerm"
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class SearchableAttribute : Attribute
{
    /// <summary>
    /// mark a virtual property as searchable
    /// </summary>
    /// <param name="targetName"></param>
    public SearchableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// mark a property as searchable
    /// </summary>
    public SearchableAttribute() { }

    /// <summary>
    /// Target name required only if this is used on the class
    /// </summary>
    public string? TargetName { get; }
}
