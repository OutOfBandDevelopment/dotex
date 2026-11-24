namespace OoBDev.Data.DesiredState;

/// <summary>The explicit permissions an apply needs for hazardous steps; everything is refused by default.</summary>
public sealed record ApplyOptions(bool AllowDataLoss = false, bool AllowTypeChange = false)
{
    /// <summary>True when this option set permits the hazard.</summary>
    public bool Permits(Hazard hazard) => hazard.Kind switch
    {
        HazardKind.TypeChange => AllowTypeChange,
        _ => AllowDataLoss,
    };
}
