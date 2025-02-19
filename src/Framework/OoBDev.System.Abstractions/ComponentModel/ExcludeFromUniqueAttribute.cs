using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// Attribute used to exclude an enum or enum field from being considered for uniqueness checks.
/// </summary>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class ExcludeFromUniqueAttribute : Attribute
{
}
