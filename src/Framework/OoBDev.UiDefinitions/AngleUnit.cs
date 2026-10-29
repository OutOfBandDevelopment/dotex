using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>The unit a <see cref="CoordinateSystem.Polar"/> angle arrives in.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AngleUnit>))]
public enum AngleUnit
{
    Degrees,
    Radians,
}
