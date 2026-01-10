using System;

namespace Eliassen.System.ComponentModel.Search;

/// <summary>
/// exclude from string comparison replacement visitor
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreStringComparisonReplacementAttribute : Attribute
{
}
