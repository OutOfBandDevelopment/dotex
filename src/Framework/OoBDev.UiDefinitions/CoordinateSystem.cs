using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>Which live values a <see cref="VectorControl"/> reads, and how it plots them.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CoordinateSystem>))]
public enum CoordinateSystem
{
    /// <summary>x/y, from <see cref="VectorControl.XId"/>/<see cref="VectorControl.YId"/>.</summary>
    XY,

    /// <summary>x/y/z, adding <see cref="VectorControl.ZId"/> (drawn in an oblique projection).</summary>
    XYZ,

    /// <summary>r/theta, from <see cref="VectorControl.RadiusId"/>/<see cref="VectorControl.AngleId"/>.</summary>
    Polar,
}
