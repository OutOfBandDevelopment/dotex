using System.Collections.Generic;

namespace OoBDev.Data.DesiredState;

/// <summary>Which ensure scripts ran and which one-time scripts were skipped because their hash was already recorded.</summary>
public sealed record EnsureReport(IReadOnlyList<string> Ran, IReadOnlyList<string> Skipped);
