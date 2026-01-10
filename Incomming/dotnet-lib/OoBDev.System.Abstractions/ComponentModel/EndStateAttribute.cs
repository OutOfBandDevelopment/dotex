using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// this attribute tags valid end states for enum based state machines
/// </summary>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EndStateAttribute : Attribute
{
}
