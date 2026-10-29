using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>A <see cref="VectorControl"/>'s current point, trail, and optional color.</summary>
public sealed class VectorState : LiveDisplayState
{
    private readonly Dictionary<string, double> _values = new(StringComparer.Ordinal);
    private readonly Queue<(double X, double Y, double Z)> _trail = new();
    private (double X, double Y, double Z)? _pointBeforeBatch;
    private bool _coordinateChanged;

    public VectorState(VectorControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        Control = control;
        ValueIds = [.. control.ValueIds().Distinct(StringComparer.Ordinal)];
    }

    public VectorControl Control { get; }

    public override IReadOnlyList<string> ValueIds { get; }

    /// <summary>Whether every coordinate value (x/y[/z], or r/theta) has arrived at least once.</summary>
    public bool HasPoint => Control.Coordinates switch
    {
        CoordinateSystem.Polar => Has(Control.RadiusId) && Has(Control.AngleId),
        CoordinateSystem.XYZ => Has(Control.XId) && Has(Control.YId) && Has(Control.ZId),
        _ => Has(Control.XId) && Has(Control.YId),
    };

    /// <summary>The current point in Cartesian coordinates (polar converted; z is 0 unless XYZ).</summary>
    public (double X, double Y, double Z) Point
    {
        get
        {
            if (Control.Coordinates == CoordinateSystem.Polar)
            {
                var r = Get(Control.RadiusId);
                var theta = Get(Control.AngleId);
                var radians = Control.AngleUnit == AngleUnit.Degrees ? theta * Math.PI / 180 : theta;
                return (r * Math.Cos(radians), r * Math.Sin(radians), 0);
            }

            return (Get(Control.XId), Get(Control.YId), Control.Coordinates == CoordinateSystem.XYZ ? Get(Control.ZId) : 0);
        }
    }

    /// <summary>Earlier points, oldest first, not including the current one.</summary>
    public IReadOnlyList<(double X, double Y, double Z)> Trail => [.. _trail];

    /// <summary>The live h/s/v color when a hue channel is declared and has arrived (saturation/value default to 1); otherwise null.</summary>
    public (byte R, byte G, byte B)? Color
    {
        get
        {
            if (Control.HueId is null || !Has(Control.HueId))
            {
                return null;
            }

            static double Unit(double v) => v > 1 ? v / 100 : v;
            var s = Control.SaturationId is { } sid && Has(sid) ? Unit(Get(sid)) : 1;
            var v = Control.BrightnessId is { } vid && Has(vid) ? Unit(Get(vid)) : 1;
            return ChartPalette.FromHsv(Get(Control.HueId), s, v);
        }
    }

    /// <summary>A one-line readout, e.g. <c>x=0.5 y=-0.25</c> or <c>r=0.8 θ=45°</c> — "—" for a value not yet received.</summary>
    public string Readout()
    {
        string V(string? id) => id is not null && Has(id) ? ChartValue.Format(Get(id)) : "—";
        return Control.Coordinates switch
        {
            CoordinateSystem.Polar => $"r={V(Control.RadiusId)} θ={V(Control.AngleId)}{(Control.AngleUnit == AngleUnit.Degrees ? "°" : " rad")}",
            CoordinateSystem.XYZ => $"x={V(Control.XId)} y={V(Control.YId)} z={V(Control.ZId)}",
            _ => $"x={V(Control.XId)} y={V(Control.YId)}",
        };
    }

    protected override void OnBatchStarting()
    {
        _pointBeforeBatch = HasPoint ? Point : null;
        _coordinateChanged = false;
    }

    protected override bool ApplyNumber(string id, double value)
    {
        _coordinateChanged |= id != Control.HueId && id != Control.SaturationId && id != Control.BrightnessId;
        _values[id] = value;
        return true;
    }

    protected override bool OnBatchCompleted(bool changed)
    {
        // The point moved: where it was joins the trail (bounded by TrailLength).
        if (_coordinateChanged && _pointBeforeBatch is { } previous && Control.TrailLength > 0)
        {
            _trail.Enqueue(previous);
            while (_trail.Count > Control.TrailLength)
            {
                _trail.Dequeue();
            }
        }

        return changed;
    }

    private bool Has(string? id) => id is not null && _values.ContainsKey(id);

    private double Get(string? id) => id is not null && _values.TryGetValue(id, out var v) ? v : 0;
}
