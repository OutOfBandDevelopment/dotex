using System;

namespace OoBDev.System.ComponentModel;

/// <summary>
/// output enum string as this value when serialized as json
/// </summary>

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EnumValueAttribute(string name) : Attribute
{

    /// <summary>
    /// value to output in place of Enum.ToString() with Json Serializer
    /// </summary>
    public string Name { get; init; } = name;
}
