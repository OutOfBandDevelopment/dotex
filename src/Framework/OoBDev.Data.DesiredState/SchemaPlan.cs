using System.Collections.Generic;

namespace OoBDev.Data.DesiredState;

/// <summary>The statements a tool would run to reach the desired state, the hazards among them, and the drops the tool skipped by itself.</summary>
public sealed record SchemaPlan(string Engine, IReadOnlyList<string> Statements, IReadOnlyList<Hazard> Hazards, IReadOnlyList<string> Skipped)
{
    /// <summary>True when nothing would change.</summary>
    public bool IsEmpty => Statements.Count == 0;
}
