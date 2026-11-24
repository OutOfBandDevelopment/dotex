using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Data.DesiredState;

/// <summary>Thrown by apply when a plan holds hazards the options do not permit; nothing was run.</summary>
public sealed class UnsafePlanException(IReadOnlyList<Hazard> hazards)
    : InvalidOperationException("The plan has steps that need an explicit flag: " + string.Join("; ", hazards.Select(h => $"{h.Kind}: {h.Statement}")))
{
    public IReadOnlyList<Hazard> Hazards { get; } = hazards;
}
