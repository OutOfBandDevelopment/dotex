using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OoBDev.UiDefinitions;

/// <summary>
/// A read-only vector/coordinate display: a point (plus a short fading trail) plotted from live
/// values — x/y, x/y/z, or r/theta per <see cref="Coordinates"/> — on axes spanning ±<see cref="Range"/>
/// (for polar, a radius of <see cref="Range"/>). Optionally colored from live hue/saturation/value
/// channels (<see cref="HueId"/> in degrees 0–360; <see cref="SaturationId"/>/<see cref="BrightnessId"/>
/// in 0–1, or 0–100 read as a percentage). Display-only like <see cref="IndicatorControl"/>.
/// </summary>
public sealed class VectorControl : UiControl
{
    public CoordinateSystem Coordinates { get; set; } = CoordinateSystem.XY;

    public string? XId { get; set; }

    public string? YId { get; set; }

    public string? ZId { get; set; }

    public string? RadiusId { get; set; }

    public string? AngleId { get; set; }

    public AngleUnit AngleUnit { get; set; } = AngleUnit.Degrees;

    /// <summary>Axis extent: every axis spans [-Range, Range] (a polar plot's outer ring is radius Range).</summary>
    public double Range { get; set; } = 1;

    /// <summary>How many recent points stay drawn behind the current one (0 for none).</summary>
    public int TrailLength { get; set; } = 20;

    public string? HueId { get; set; }

    public string? SaturationId { get; set; }

    public string? BrightnessId { get; set; }

    /// <summary>Every live value id this control reads, in the order x, y, z / r, theta, then h, s, v — unset ids skipped.</summary>
    public IEnumerable<string> ValueIds()
    {
        string?[] ids = Coordinates == CoordinateSystem.Polar
            ? [RadiusId, AngleId, HueId, SaturationId, BrightnessId]
            : [XId, YId, Coordinates == CoordinateSystem.XYZ ? ZId : null, HueId, SaturationId, BrightnessId];
        return ids.OfType<string>();
    }
}
