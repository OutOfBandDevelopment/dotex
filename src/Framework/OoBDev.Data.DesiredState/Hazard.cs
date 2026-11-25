namespace OoBDev.Data.DesiredState;

/// <summary>A plan statement that needs an explicit flag before it runs.</summary>
public sealed record Hazard(HazardKind Kind, string Statement, string Message);
